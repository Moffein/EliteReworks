using EliteReworks.Tweaks.T1.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T1
{
    public static class AffixWhite
    {
        public static BuffDef slow80alt;

        public static float baseRadius = 4f;
        public static float baseSlowDuration = 0.5f;
        public static float procSlowDuration = 2f;

        public static GameObject explosionEffectPrefab;
        public static NetworkSoundEventDef slowProcSound;

        public static void Setup()
        {
            explosionEffectPrefab = CreateExplosionEffect();
            slow80alt = CreateAltSlowBuff();
            slowProcSound = CreateSlowProcSound();

            //Remove vanilla on-hit effect so that all on-hits are handled via the OnHitAll hook
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += RemoveGlacialOnHitEffect;
            RecalculateStatsAPI.GetStatCoefficients += Slow80AltStats;
            IL.RoR2.CharacterModel.UpdateOverlays += Slow80AltOverlay;
        }

        private static void Slow80AltStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(AffixWhite.slow80alt) && !sender.HasBuff(RoR2Content.Buffs.Slow80))
            {
                args.moveSpeedReductionMultAdd += 0.8f;
            }
        }

        private static void Slow80AltOverlay(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(
                 x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "Slow80")
                ))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, CharacterModel, bool>>((hasBuff, self) =>
                {
                    return hasBuff || (self.body.HasBuff(slow80alt));
                });
            }
            else
            {
                Debug.LogError("EliteReworks: Slow80AltOverlay IL hook failed.");
            }
        }

        private static void RemoveGlacialOnHitEffect(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                 x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AffixWhite")
                ))
            {
                c.EmitDelegate<Func<BuffDef, BuffDef>>(orig => null);
            }
            else
            {
                Debug.LogError("EliteReworks: RemoveGlacialOnHitEffect IL hook failed.");
            }
        }

        private static BuffDef CreateAltSlowBuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.buffColor = new Color(165f/255f, 222f/255f, 237f/255f);
            buff.canStack = false;
            buff.isDebuff = true;
            buff.name = "EliteReworksSlow80";
            buff.iconSprite = LegacyResourcesAPI.Load<BuffDef>("BuffDefs/Slow80").iconSprite;
            R2API.ContentAddition.AddBuffDef((buff));
            return buff;
        }

        private static GameObject CreateExplosionEffect()
        {
            //prefabs/effects/impacteffects/AffixWhiteExplosion
            //prefabs/effects/muzzleflashes/MuzzleflashMageIceLarge
            GameObject effect = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/impacteffects/IceRingExplosion").InstantiateClone("MoffeinEliteReworksGlacialExplosion", false);
            UnityEngine.Object.Destroy(effect.GetComponent<ShakeEmitter>());
            EffectComponent ec = effect.GetComponent<EffectComponent>();
            ec.soundName = "";
            //ec.applyScale = true;
            ec.applyScale = false;

            ParticleSystemRenderer[] ps = effect.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (ParticleSystemRenderer p in ps)
            {
                switch (p.name)
                {
                    case "IceMesh":
                        UnityEngine.Object.Destroy(p);
                        break;
                    default:
                        break;
                }
                /*if (p.name == "Chunks")
                {
                    p.enabled = false;
                }*/
            }

            //effect.AddComponent<DestroyOnTimer>().duration = 0.5f;

            R2API.ContentAddition.AddEffect(effect);
            return effect;
        }

        private static NetworkSoundEventDef CreateSlowProcSound()
        {
            NetworkSoundEventDef toReturn = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            toReturn.eventName = "Play_mage_m2_iceSpear_shoot";
            (toReturn as UnityEngine.Object).name = "EliteReworksGlacialSlowNetworkSound";
            ContentAddition.AddNetworkSoundEventDef(toReturn);

            return toReturn;
        }
    }
}
