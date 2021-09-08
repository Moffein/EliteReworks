using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using MonoMod.Cil;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using EliteReworks.Tweaks.T1.Components;
using R2API;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T1
{
    public static class AffixBlue
    {
		public static int baseMeatballCount = 5;
		public static float lightningDamageCoefficient = 0.8f;
		public static float lightningBlastRadius = 6f;
		public static GameObject lightningProjectilePrefab;

		public static GameObject impactEffectPrefab;
		public static GameObject triggerEffectPrefab;

        public static void Setup()
		{
			AffixBlue.lightningProjectilePrefab = BuildLightningProjectile();
			AffixBluePassiveLightning.lightningProjectilePrefab = AffixBlue.lightningProjectilePrefab;
			RemoveVanillaEffects();
		}

        private static GameObject BuildLightningProjectile()
        {
			AffixBlue.impactEffectPrefab = BuildLightningEffect();
			AffixBlue.triggerEffectPrefab = BuildLightningTriggerEffect();
			GameObject projectile = Resources.Load<GameObject>("prefabs/projectiles/ElectricWormSeekerProjectile").InstantiateClone("MoffeinEliteReworkOverloadinLightningProjectile",true);
			ProjectileImpactExplosion pie = projectile.GetComponent<ProjectileImpactExplosion>();
			pie.blastProcCoefficient = 0f;
			pie.blastRadius = lightningBlastRadius;
			pie.explosionEffect = AffixBlue.impactEffectPrefab;
			pie.destroyOnEnemy = false;
			pie.blastAttackerFiltering = AttackerFiltering.NeverHit;

			ProjectileController pc = projectile.GetComponent<ProjectileController>();
			pc.procCoefficient = 0f;

			AkEvent[] ae = projectile.GetComponentsInChildren<AkEvent>();
			foreach(AkEvent a in ae)
            {
				UnityEngine.Object.Destroy(a);
            }

			UnityEngine.Object.Destroy(projectile.GetComponent<AkGameObj>());

			ProjectileAPI.Add(projectile);
			return projectile;
        }

		private static GameObject BuildLightningEffect()
		{
			GameObject effect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/lightningstakenova"), "MoffeinEliteReworkOverloadinLightningEffect", false);
			EffectComponent ec = effect.GetComponent<EffectComponent>();
			ec.applyScale = true;
			ec.soundName = "Play_mage_m2_impac";//Play_item_use_lighningArm
			EffectAPI.AddEffect(effect);
			return effect;
		}

		private static GameObject BuildLightningTriggerEffect()
		{
			GameObject effect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/lightningstakenova"), "MoffeinEliteReworkOverloadinLightningTriggerEffect", false);
			EffectComponent ec = effect.GetComponent<EffectComponent>();
			ec.applyScale = true;
			ec.soundName = "Play_mage_m2_impact";
			EffectAPI.AddEffect(effect);
			return effect;
		}

		public static void RemoveVanillaEffects()
        {
			//Remove Shields
			if (EliteReworksPlugin.affixBlueRemoveShield)
			{
				IL.RoR2.CharacterBody.RecalculateStats += (il) =>
				{
					ILCursor c = new ILCursor(il);
					c.GotoNext(
						 x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AffixBlue")
						);
					c.Remove();
					c.Emit<EliteReworksPlugin>(OpCodes.Ldsfld, nameof(EliteReworksPlugin.EmptyBuff));
				};
			}

			//Remove vanilla on-hit effect
			IL.RoR2.GlobalEventManager.OnHitAll += (il) =>
			{
				ILCursor c = new ILCursor(il);
				c.GotoNext(
					 x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AffixBlue")
					);
				c.Remove();
				c.Emit<EliteReworksPlugin>(OpCodes.Ldsfld, nameof(EliteReworksPlugin.EmptyBuff));
			};
		}

		//Copypasted from Magma Worm
		public static void FireMeatballs(GameObject attacker, float damage, bool crit,
			Vector3 impactNormal, Vector3 impactPosition, Vector3 forward,
			int meatballCount, float meatballAngle, float meatballForce, float velocity)
		{
			EffectManager.SpawnEffect(triggerEffectPrefab, new EffectData { origin = impactPosition - Vector3.up, scale = 1.5f }, true);
			float num = 360f / (float)meatballCount;
			Vector3 normalized = Vector3.ProjectOnPlane(forward, impactNormal).normalized;
			Vector3 point = Vector3.RotateTowards(impactNormal, normalized, meatballAngle * 0.0174532924f, float.PositiveInfinity);
			for (int i = 0; i < meatballCount; i++)
			{
				Vector3 forward2 = Quaternion.AngleAxis(num * (float)i, impactNormal) * point;
				ProjectileManager.instance.FireProjectile(lightningProjectilePrefab, impactPosition, Util.QuaternionSafeLookRotation(forward2),
					attacker, damage, meatballForce, crit, DamageColorIndex.Default, null, velocity);
			}
		}
	}
}
