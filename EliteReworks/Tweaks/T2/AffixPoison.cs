using EliteReworks.Tweaks.T2.Components;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;
namespace EliteReworks.Tweaks.T2
{
    public static class AffixPoison
    {
		public static DamageAPI.ModdedDamageType malachiteDamage;
		public static GameObject spikePrefab;
		public static GameObject spikeOrbProjectile;
		public static float basePoisonTimer = 6f;
		public static float spikeHPPercent = 0.5f;

        public static void Setup()
        {
			malachiteDamage = DamageAPI.ReserveDamageType();
			spikePrefab = BuildSpikePrefab();
			spikeOrbProjectile = BuildSpikeOrb(spikePrefab);
			AffixPoisonDebuffAura.indicatorPrefab = BuildIndicator();

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
			};
        }

		private static GameObject BuildSpikePrefab()
        {
			GameObject projectile = Resources.Load<GameObject>("Prefabs/Projectiles/PoisonStakeProjectile").InstantiateClone("MoffeinEliteReworksPoisonStakeProjectile", true);

			//UnityEngine.Object.Destroy(projectile.GetComponent<ProjectileImpactExplosion>());
			//UnityEngine.Object.Destroy(projectile.GetComponent<TeamAreaIndicator>());
			//UnityEngine.Object.Destroy(projectile.GetComponent<MineProximityDetonator>());

			//projectile.AddComponent<DestroyOnTimer>().duration = basePoisonTimer * 1.5f;

			/*BuffWard poisonWard = projectile.AddComponent<BuffWard>();
			poisonWard.radius = 20f;
			poisonWard.interval = 0.5f;
			poisonWard.rangeIndicator = null;
			poisonWard.buffDef = RoR2Content.Buffs.HealingDisabled;
			poisonWard.buffDuration = 1f;
			poisonWard.floorWard = false;
			poisonWard.invertTeamFilter = true;
			poisonWard.expireDuration = 0;*/

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
    }
}
