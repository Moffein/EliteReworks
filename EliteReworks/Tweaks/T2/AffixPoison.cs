﻿using EliteReworks.Tweaks.T2.Components;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T2
{
    public static class AffixPoison
    {
		public static DamageAPI.ModdedDamageType malachiteDamage;
		public static GameObject spikePrefab;
		public static GameObject spikeOrbProjectile;
		public static float basePoisonTimer = 6f;
		public static float spikeHPPercent = 0.5f;

		public static BuffDef MalachiteBuildup;
		public static float malachiteBuildupTime = 4f;
		public static int malachiteBuildupMaxStacks = 5;

        public static void Setup()
        {
			AffixPoisonDebuffAura.refreshTime = malachiteBuildupTime / malachiteBuildupMaxStacks;
			AffixPoisonDebuffAura.buffDuration = AffixPoisonDebuffAura.refreshTime + 0.1f;

			malachiteDamage = DamageAPI.ReserveDamageType();
			spikePrefab = BuildSpikePrefab();
			spikeOrbProjectile = BuildSpikeOrb(spikePrefab);
			AffixPoisonDebuffAura.indicatorPrefab = BuildIndicator();//BuildIndicatorAlt();

			//Replace original code
			On.RoR2.CharacterBody.UpdateAffixPoison += (On.RoR2.CharacterBody.orig_UpdateAffixPoison orig, RoR2.CharacterBody self, float deltaTime) =>
            {
				if (self.HasBuff(RoR2Content.Buffs.AffixPoison))
				{
					if (self.healthComponent && self.healthComponent.isInFrozenState)
					{
						self.poisonballTimer = 0f;
					}
					else
					{
						SetStateOnHurt ssoh = self.gameObject.GetComponent<SetStateOnHurt>();
						if (ssoh)
						{
							Type state = ssoh.targetStateMachine.state.GetType();
							if (state == typeof(EntityStates.StunState) || state == typeof(EntityStates.ShockState))
							{
								self.poisonballTimer = 0f;
							}
						}
					}

					AffixPoisonDebuffAura apd = self.gameObject.GetComponent<AffixPoisonDebuffAura>();
					if (!apd)
                    {
						self.gameObject.AddComponent<AffixPoisonDebuffAura>();
                    }

					if (!apd.wardActive)
					{
						self.poisonballTimer = 0f;
					}
					else
					{
						self.poisonballTimer += deltaTime;
						if (self.poisonballTimer >= basePoisonTimer)
						{
							int num = 3 + (int)self.radius;
							self.poisonballTimer = 0f;
							Vector3 up = Vector3.up;
							float num2 = 360f / (float)num;
							Vector3 normalized = Vector3.ProjectOnPlane(self.transform.forward, up).normalized;
							Vector3 point = Vector3.RotateTowards(up, normalized, 0.436332315f, float.PositiveInfinity);
							for (int i = 0; i < num; i++)
							{
								Vector3 forward = Quaternion.AngleAxis(num2 * (float)i, up) * point;
								ProjectileManager.instance.FireProjectile(spikeOrbProjectile, self.corePosition, RoR2.Util.QuaternionSafeLookRotation(forward), self.gameObject, self.damage * 1f, 0f, RoR2.Util.CheckRoll(self.crit, self.master), DamageColorIndex.Default, null, -1f);
							}
						}
					}
				}
			};

			SetupDebuff();
        }

		private static void SetupDebuff()
        {
			Debug.Log("\n\n\n\n\n\n\n\n\n\nMalachite Color: " + RoR2Content.Buffs.HealingDisabled.buffColor);
			BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
			buff.buffColor = RoR2Content.Buffs.PulverizeBuildup.buffColor;
			buff.isDebuff = true;
			buff.name = "EliteReworksMalachiteBuildup";
			buff.iconSprite = RoR2Content.Buffs.HealingDisabled.iconSprite;
			BuffAPI.Add(new CustomBuff(buff));
			MalachiteBuildup = buff;

			On.RoR2.HealthComponent.Heal += (orig, self, amount, procChainMask, isRegen) =>
			{
				int malachiteCount = Mathf.Min(self.body.GetBuffCount(MalachiteBuildup), 5);
				if (malachiteCount > 0)
                {
					amount *= 1f - 0.2f * malachiteCount;
                }
				return orig(self, amount, procChainMask, isRegen);
			};

			On.RoR2.CharacterBody.AddBuff_BuffIndex += (orig, self, buffIndex) =>
			{
				orig(self, buffIndex);
				if (buffIndex == RoR2Content.Buffs.HealingDisabled.buffIndex && self.HasBuff(MalachiteBuildup.buffIndex))
                {
					self.ClearTimedBuffs(MalachiteBuildup.buffIndex);
                }
			};
		}

		private static GameObject BuildSpikePrefab()
        {
			GameObject projectile = Resources.Load<GameObject>("Prefabs/Projectiles/PoisonStakeProjectile").InstantiateClone("MoffeinEliteReworksPoisonStakeProjectile", true);

			DamageAPI.ModdedDamageTypeHolderComponent modDamage = projectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
			modDamage.Add(malachiteDamage);
			ProjectileAPI.Add(projectile);
			return projectile;
		}

		private static GameObject BuildSpikeOrb(GameObject projectileChild)
		{
			GameObject projectile = Resources.Load<GameObject>("Prefabs/Projectiles/PoisonOrbProjectile").InstantiateClone("MoffeinEliteReworksPoisonOrbProjectile", true);
			ProjectileImpactExplosion pie = projectile.GetComponent<ProjectileImpactExplosion>();
			pie.blastRadius = 0f;
			pie.blastDamageCoefficient = 0f;
			pie.blastProcCoefficient = 0f;
			pie.childrenProjectilePrefab = projectileChild;
			ProjectileAPI.Add(projectile);
			return projectile;
		}

		private static GameObject BuildIndicator()
        {
			GameObject indicator = Resources.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator").InstantiateClone("MoffeinEliteReworksPoisonIndicator", true);
			indicator.transform.localScale = AffixPoisonDebuffAura.wardRadius / 12.8f * Vector3.one;
			PrefabAPI.RegisterNetworkPrefab(indicator);
			return indicator;
		}

		//Should this go in the Component instead?
		public static void MalachiteSphere(TeamIndex team, Vector3 position, float radius, float debuffDuration)
		{
			if (!NetworkServer.active)
			{
				return;
			}

			List<HealthComponent> hcList = new List<HealthComponent>();
			Collider[] array = Physics.OverlapSphere(position, radius, LayerIndex.entityPrecise.mask);
			for (int i = 0; i < array.Length; i++)
			{
				HurtBox hurtBox = array[i].GetComponent<HurtBox>();
				if (hurtBox)
				{
					HealthComponent healthComponent = hurtBox.healthComponent;
					ProjectileController pc = array[i].GetComponentInParent<ProjectileController>();
					if (healthComponent && !pc && !hcList.Contains(healthComponent))
					{
						hcList.Add(healthComponent);
						if (healthComponent.body.teamComponent && healthComponent.body.teamComponent.teamIndex != team)
						{
							if (!healthComponent.body.HasBuff(RoR2Content.Buffs.HealingDisabled.buffIndex))
							{
								healthComponent.body.AddTimedBuff(MalachiteBuildup, debuffDuration);
								int buffCount = healthComponent.body.GetBuffCount(MalachiteBuildup);
								if (buffCount >= malachiteBuildupMaxStacks)
								{
									healthComponent.body.ClearTimedBuffs(MalachiteBuildup.buffIndex);
									healthComponent.body.AddTimedBuff(RoR2Content.Buffs.HealingDisabled.buffIndex, debuffDuration);
								}
								else
								{
									for (int j = 0; j < healthComponent.body.timedBuffs.Count; j++)
									{
										if (healthComponent.body.timedBuffs[j].buffIndex == MalachiteBuildup.buffIndex
											&& healthComponent.body.timedBuffs[j].timer < debuffDuration)
										{
											healthComponent.body.timedBuffs[j].timer = debuffDuration;

										}
									}
								}
							}
							else
							{
								healthComponent.body.AddTimedBuff(RoR2Content.Buffs.HealingDisabled.buffIndex, debuffDuration);
							}
						}
					}
				}
			}
		}
	}
}
