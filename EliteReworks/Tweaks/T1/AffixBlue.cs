using UnityEngine;
using RoR2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using EliteReworks.Tweaks.T1.Components;
using R2API;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;

namespace EliteReworks.Tweaks.T1
{
    public static class AffixBlue
    {
		public static int baseMeatballCount = 5;
		public static float lightningDamageCoefficient = 0.7f;

		public static GameObject lightningProjectilePrefab;
		public static GameObject lightningBossProjectilePrefab;

		public static GameObject lightningEffectPrefab;

		public static GameObject lightningBombV2Prefab;

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

			AffixBlue.lightningProjectilePrefab = BuildLightningProjectile();
			AffixBlue.lightningBossProjectilePrefab = BuildLightningBossProjectile();

			AffixBlue.lightningBombV2Prefab = BuildLightningBombV2();

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
				RemoveShields();
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
			//pie.explosionEffect = BuildLightningEffect();
			//pie.impactEffect = BuildLightningEffect();
			pie.destroyOnEnemy = false;
			pie.blastAttackerFiltering = AttackerFiltering.NeverHitSelf;
			pie.falloffModel = BlastAttack.FalloffModel.None;

			pie.impactEffect = AffixBlue.lightningEffectPrefab;

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
			toReturn.eventName = "Play_mage_m1_cast_lightning";// "Play_item_proc_chain_lightning";
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
			pie.impactEffect = AffixBlue.lightningEffectPrefab;
			ContentAddition.AddProjectile(toReturn);

			return toReturn;
        }

		private static void RemoveShields()
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
	}
}
