﻿using EliteReworks.Tweaks.T1.Components;
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
        public static GameObject hitEffectPrefab;

        public static void Setup()
        {
            explosionEffectPrefab = CreateExplosionEffect();
            hitEffectPrefab = CreateHitEffect();
            slow80alt = CreateAltSlowBuff();

            IL.RoR2.CharacterModel.UpdateOverlays += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "Slow80")
                    );
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, CharacterModel, bool>>((hasBuff, self) =>
                {
                    return hasBuff || (self.body.HasBuff(slow80alt));
                });
            };
        }

        public static BuffDef CreateAltSlowBuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.buffColor = new Color(165f/255f, 222f/255f, 237f/255f);
            buff.canStack = false;
            buff.isDebuff = true;
            buff.name = "EliteReworksSlow80";
            buff.iconSprite = LegacyResourcesAPI.Load<Sprite>("textures/bufficons/texBuffSlow50Icon");
            R2API.ContentAddition.AddBuffDef((buff));
            return buff;
        }

        public static GameObject CreateHitEffect()
        {
            GameObject effect = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashMageIceLarge").InstantiateClone("MoffeinEliteReworksGlacialHit", false);
            UnityEngine.Object.Destroy(effect.GetComponent<ShakeEmitter>());
            EffectComponent ec = effect.GetComponent<EffectComponent>();
            ec.soundName = "Play_mage_m2_iceSpear_shoot";
            ec.applyScale = false;
            R2API.ContentAddition.AddEffect(effect);
            return effect;
        }

        public static GameObject CreateExplosionEffect()
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
    }
}
