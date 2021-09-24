using EliteReworks.Tweaks.T1.Components;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
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
        public static float slowDuration = 3f;

        public static GameObject explosionEffectPrefab;
        public static GameObject hitEffectPrefab;

        public static void Setup()
        {
            explosionEffectPrefab = CreateExplosionEffect();
            hitEffectPrefab = CreateHitEffect();
            slow80alt = CreateAltSlowBuff();
        }

        public static BuffDef CreateAltSlowBuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.buffColor = new Color(165f/255f, 222f/255f, 237f/255f);
            buff.canStack = false;
            buff.isDebuff = true;
            buff.name = "EliteReworksSlow80";
            buff.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffSlow50Icon");
            BuffAPI.Add(new CustomBuff(buff));
            return buff;
        }

        public static GameObject CreateHitEffect()
        {
            GameObject effect = Resources.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashMageIceLarge").InstantiateClone("MoffeinEliteReworksGlacialHit", false);
            UnityEngine.Object.Destroy(effect.GetComponent<ShakeEmitter>());
            EffectComponent ec = effect.GetComponent<EffectComponent>();
            ec.soundName = "Play_mage_m2_iceSpear_shoot";
            ec.applyScale = false;
            EffectAPI.AddEffect(effect);
            return effect;
        }

        public static GameObject CreateExplosionEffect()
        {
            //prefabs/effects/impacteffects/AffixWhiteExplosion
            //prefabs/effects/muzzleflashes/MuzzleflashMageIceLarge
            GameObject effect = Resources.Load<GameObject>("prefabs/effects/impacteffects/IceRingExplosion").InstantiateClone("MoffeinEliteReworksGlacialExplosion", false);
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

            EffectAPI.AddEffect(effect);
            return effect;
        }
    }
}
