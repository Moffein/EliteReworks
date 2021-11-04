using UnityEngine;
using RoR2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using EliteReworks.Tweaks.T1.Components;
using R2API;
using RoR2.Projectile;

namespace EliteReworks.Tweaks.T1
{
    public static class AffixBlue
    {
		public static int baseMeatballCount = 5;
		public static float lightningDamageCoefficient = 0.7f;
		public static float lightningBlastRadius = 3f;
		public static GameObject lightningProjectilePrefab;

		public static GameObject triggerEffectPrefab;
		public static GameObject triggerEffectBossPrefab;

		public static void Setup()
		{
			AffixBlue.triggerEffectPrefab = BuildLightningTriggerEffect();
			AffixBlue.triggerEffectBossPrefab = BuildLightningTriggerEffectBoss();

			AffixBlue.lightningProjectilePrefab = BuildLightningProjectile();
			AffixBluePassiveLightning.lightningProjectilePrefab = AffixBlue.lightningProjectilePrefab;

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

        private static GameObject BuildLightningProjectile()
        {
			GameObject projectile = Resources.Load<GameObject>("prefabs/projectiles/ElectricWormSeekerProjectile").InstantiateClone("MoffeinEliteReworkOverloadinLightningProjectile",true);

			ProjectileController pc = projectile.GetComponent<ProjectileController>();
			pc.procCoefficient = 0f;

			AkEvent[] ae = projectile.GetComponentsInChildren<AkEvent>();
			foreach(AkEvent a in ae)
            {
				UnityEngine.Object.Destroy(a);
            }

			UnityEngine.Object.Destroy(projectile.GetComponent<AkGameObj>());

			ProjectileImpactExplosion pie = projectile.GetComponent<ProjectileImpactExplosion>();
			pie.blastProcCoefficient = 0f;
			pie.blastRadius = lightningBlastRadius;
			//pie.explosionEffect = BuildLightningEffect();
			//pie.impactEffect = BuildLightningEffect();
			pie.destroyOnEnemy = false;
			pie.blastAttackerFiltering = AttackerFiltering.NeverHit;
			pie.falloffModel = BlastAttack.FalloffModel.None;

			ProjectileAPI.Add(projectile);
			return projectile;
        }

		private static GameObject BuildLightningEffect()
		{
			GameObject effect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/lightningstakenova"), "MoffeinEliteReworksOverloadinLightningEffect", false);
			EffectComponent ec = effect.GetComponent<EffectComponent>();
			ec.applyScale = true;
			ec.soundName = "Play_item_use_lighningArm";
			EffectAPI.AddEffect(effect);
			return effect;
		}

		private static GameObject BuildLightningTriggerEffect()
		{
			GameObject effect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/lightningstakenova"), "MoffeinEliteReworksOverloadinLightningTriggerEffect", false);
			EffectComponent ec = effect.GetComponent<EffectComponent>();
			ec.applyScale = true;
			ec.soundName = "Play_mage_m2_impact";
			EffectAPI.AddEffect(effect);
			return effect;
		}

		private static GameObject BuildLightningTriggerEffectBoss()
		{
			GameObject effect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/lightningstakenova"), "MoffeinEliteReworksOverloadinLightningTriggerBossEffect", false);
			EffectComponent ec = effect.GetComponent<EffectComponent>();
			ec.applyScale = true;
			ec.soundName = "Play_titanboss_shift_shoot";
			EffectAPI.AddEffect(effect);
			return effect;
		}

		public static void RemoveShields()
        {
			//Remove Shields
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

		//Copypasted from Magma Worm
		public static void FireMeatballs(GameObject attacker, bool isChampion, float damage, bool crit,
			Vector3 impactNormal, Vector3 impactPosition, Vector3 forward,
			int meatballCount, float meatballAngle, float meatballForce, float velocity, bool randomize = false)
		{
			EffectManager.SpawnEffect(isChampion ? triggerEffectBossPrefab : triggerEffectPrefab, new EffectData { origin = impactPosition - Vector3.up, scale = 1.5f }, true);
			float num = 360f / (float)meatballCount;
			float randomOffset = UnityEngine.Random.Range(0f, 360f);
			Vector3 normalized = Vector3.ProjectOnPlane(forward, impactNormal).normalized;
			Vector3 point = Vector3.zero;
			if (!randomize)
            {
				point = Vector3.RotateTowards(impactNormal, normalized, meatballAngle * 0.0174532924f, float.PositiveInfinity);
			}
			bool spawnedClose = false;
			for (int i = 0; i < meatballCount; i++)
			{
				if (randomize)
				{
					float angle = Random.Range(!spawnedClose? 0f : meatballAngle, meatballAngle * 1.2f);
					if (angle < meatballAngle)
                    {
						spawnedClose = true;
                    }
					else
                    {
						spawnedClose = false;
                    }
					point = Vector3.RotateTowards(impactNormal, normalized, angle * 0.0174532924f, float.PositiveInfinity);
				}
				Vector3 forward2 = Quaternion.AngleAxis(randomOffset + num * (float)i, impactNormal) * point;
                ProjectileManager.instance.FireProjectile(lightningProjectilePrefab, impactPosition, RoR2.Util.QuaternionSafeLookRotation(forward2),
					attacker, damage, meatballForce, crit, DamageColorIndex.Default, null, velocity);
			}
		}
	}
}
