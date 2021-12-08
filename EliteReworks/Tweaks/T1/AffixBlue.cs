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
	}
}
