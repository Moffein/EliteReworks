using UnityEngine;
using RoR2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using EliteReworks.Tweaks.T1.Components;
using R2API;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using System;

namespace EliteReworks.Tweaks.T1
{
    public static class AffixBlue
    {
		public static int baseMeatballCount = 5;
		public static float lightningDamageCoefficient = 0.5f;

		public static GameObject lightningProjectilePrefab;
		public static GameObject lightningBossProjectilePrefab;

		public static GameObject lightningEffectPrefab;
		public static GameObject lightningEffectQuietPrefab;
		public static GameObject lightningEffectOnHitPrefab;

		public static GameObject lightningBombV2Prefab;
		public static GameObject scatterProjectilePrefab;

		public static NetworkSoundEventDef triggerSound;
		public static NetworkSoundEventDef triggerBossSound;

		public static bool enablePassiveLightning = true;
		public static bool enableOnHitRework = true;

		public static void Setup()
		{
			lightningEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLightning/LightningStakeNova.prefab").WaitForCompletion().InstantiateClone("MoffeinEliteReworkOverloadinLightningVFX", false);
			EffectComponent ec = lightningEffectPrefab.GetComponent<EffectComponent>();
			ec.soundName = "Play_EliteReworks_Lightning";
			R2API.ContentAddition.AddEffect(lightningEffectPrefab);


			lightningEffectQuietPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLightning/LightningStakeNova.prefab").WaitForCompletion().InstantiateClone("MoffeinEliteReworkOverloadinLightningQuietVFX", false);
			EffectComponent ec2 = lightningEffectQuietPrefab.GetComponent<EffectComponent>();
			ec2.soundName = "Play_item_proc_chain_lightning";
			R2API.ContentAddition.AddEffect(lightningEffectQuietPrefab);

			lightningEffectOnHitPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLightning/LightningStakeNova.prefab").WaitForCompletion().InstantiateClone("MoffeinEliteReworkOverloadinLightningOnHitVFX", false);
			EffectComponent ec3 = lightningEffectOnHitPrefab.GetComponent<EffectComponent>();
			ec3.soundName = "Play_mage_m1_impact_lightning";
			R2API.ContentAddition.AddEffect(lightningEffectOnHitPrefab);

			AffixBlue.lightningProjectilePrefab = BuildLightningProjectile();
			AffixBlue.lightningBossProjectilePrefab = BuildLightningBossProjectile();

			if (AffixBluePassiveLightning.scatterBombs)
            {
				AffixBlue.scatterProjectilePrefab = BuildScatterProjectile();
			}
			else
			{
				AffixBlue.lightningBombV2Prefab = BuildLightningBombV2();
			}

			AffixBluePassiveLightning.lightningProjectilePrefab = AffixBlue.lightningProjectilePrefab;

			AffixBlue.triggerSound = BuildTriggerSound();
			AffixBlue.triggerBossSound = BuildTriggerBossSound();

			//Remove vanilla on-hit effect
			if (enableOnHitRework)
			{
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

			if (EliteReworksPlugin.affixBlueRemoveShield)
            {
                IL.RoR2.CharacterBody.RecalculateStats += RemoveShields;
            }
			else if (EliteReworksPlugin.affixBlueReduceShield)
            {
				IL.RoR2.CharacterBody.RecalculateStats += ReduceShields;
            }
		}

		private static GameObject BuildLightningProjectile()
		{
			GameObject projectile = LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/ElectricWormSeekerProjectile").InstantiateClone("MoffeinEliteReworkOverloadinLightningProjectile", true);

			ProjectileController pc = projectile.GetComponent<ProjectileController>();
			pc.procCoefficient = 0f;

			AkEvent[] ae = projectile.GetComponentsInChildren<AkEvent>();
			foreach (AkEvent a in ae)
			{
				UnityEngine.Object.Destroy(a);
			}

			UnityEngine.Object.Destroy(projectile.GetComponent<AkGameObj>());

			ProjectileImpactExplosion pie = projectile.GetComponent<ProjectileImpactExplosion>();
			pie.blastProcCoefficient = 0f;
			pie.blastRadius = 5f;
			pie.destroyOnEnemy = false;
			pie.blastAttackerFiltering = AttackerFiltering.NeverHitSelf;
			pie.falloffModel = BlastAttack.FalloffModel.None;

			pie.impactEffect = AffixBlue.lightningEffectQuietPrefab;

			R2API.ContentAddition.AddProjectile(projectile);
			return projectile;
		}
		private static GameObject BuildScatterProjectile()
		{
			GameObject projectile = LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/ElectricWormSeekerProjectile").InstantiateClone("MoffeinEliteReworkOverloadinScatterProjectile", true);

			ProjectileController pc = projectile.GetComponent<ProjectileController>();
			pc.procCoefficient = 0f;

			AkEvent[] ae = projectile.GetComponentsInChildren<AkEvent>();
			foreach (AkEvent a in ae)
			{
				UnityEngine.Object.Destroy(a);
			}

			UnityEngine.Object.Destroy(projectile.GetComponent<AkGameObj>());

			ProjectileImpactExplosion pie = projectile.GetComponent<ProjectileImpactExplosion>();
			pie.blastProcCoefficient = 0f;
			pie.blastRadius = 4f;
			pie.destroyOnEnemy = false;
			pie.blastAttackerFiltering = AttackerFiltering.NeverHitSelf;
			pie.falloffModel = BlastAttack.FalloffModel.None;

			pie.impactEffect = AffixBlue.lightningEffectOnHitPrefab;

			R2API.ContentAddition.AddProjectile(projectile);
			return projectile;
		}

		private static GameObject BuildLightningBossProjectile()
		{
			GameObject projectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElectricWorm/ElectricOrbProjectile.prefab").WaitForCompletion().InstantiateClone("MoffeinEliteReworkOverloadinLightningBossProjectile", true);

			ProjectileController pc = projectile.GetComponent<ProjectileController>();
			pc.procCoefficient = 0f;

			AkEvent[] ae = projectile.GetComponentsInChildren<AkEvent>();
			foreach (AkEvent a in ae)
			{
				UnityEngine.Object.Destroy(a);
			}

			UnityEngine.Object.Destroy(projectile.GetComponent<AkGameObj>());

			ProjectileImpactExplosion pie = projectile.GetComponent<ProjectileImpactExplosion>();
			pie.blastProcCoefficient = 0f;
			pie.blastRadius = 7f;
			//pie.explosionEffect = BuildLightningEffect();
			//pie.impactEffect = BuildLightningEffect();
			pie.destroyOnEnemy = false;
			pie.blastAttackerFiltering = AttackerFiltering.NeverHitSelf;
			pie.falloffModel = BlastAttack.FalloffModel.None;
			pie.fireChildren = false;


			R2API.ContentAddition.AddProjectile(projectile);
			return projectile;
		}

		private static NetworkSoundEventDef BuildTriggerSound()
		{
			NetworkSoundEventDef toReturn = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
			toReturn.eventName = "Play_EliteReworks_Lightning";//"Play_mage_m1_cast_lightning";// "Play_item_proc_chain_lightning";
			(toReturn as UnityEngine.Object).name = "EliteReworksOverloadingNetworkSound";
			ContentAddition.AddNetworkSoundEventDef(toReturn);

			return toReturn;
		}

		private static NetworkSoundEventDef BuildTriggerBossSound()
        {
			NetworkSoundEventDef toReturn = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
			toReturn.eventName = "Play_titanboss_shift_shoot";//"Play_titanboss_shift_shoot";
			(toReturn as UnityEngine.Object).name = "EliteReworksOverloadingBossNetworkSound";
			ContentAddition.AddNetworkSoundEventDef(toReturn);

			return toReturn;
		}

		private static GameObject BuildLightningBombV2()
        {
			GameObject toReturn = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/LightningStake").InstantiateClone("MoffeinEliteReworksOverloadingStakeV2",true);
			ProjectileImpactExplosion pie = toReturn.GetComponent<ProjectileImpactExplosion>();
			pie.blastRadius = 7f;
			pie.falloffModel = BlastAttack.FalloffModel.None;
			pie.impactEffect = AffixBlue.lightningEffectOnHitPrefab;
			ContentAddition.AddProjectile(toReturn);

			return toReturn;
        }

        private static void RemoveShields(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                 x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AffixBlue")
                );
            c.Remove();
            c.Emit<EliteReworksPlugin>(OpCodes.Ldsfld, nameof(EliteReworksPlugin.EmptyBuff));
        }

        private static void ReduceShields(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                 x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AffixBlue")
                );
			c.GotoNext(x => x.MatchLdcR4(0.5f));
			c.Remove();
			c.Emit(OpCodes.Ldc_R4, 0.25f);

			c.GotoNext(
				x => x.MatchCall(typeof(RoR2.CharacterBody), "get_maxHealth"),
				x => x.MatchAdd());
			c.Index++;
			c.EmitDelegate<Func<float, float>>(shieldToAdd =>
			{
				return shieldToAdd * 0.3333333333f;
			});
        }
    }
}
