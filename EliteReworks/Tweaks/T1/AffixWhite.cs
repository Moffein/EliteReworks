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
        public static float baseRadius = 4f;
        public static float slowDuration = 3f;
        public static void Setup()
        {
            explosionEffect = CreateEffect();
        }

        public static GameObject CreateEffect()
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

        public static GameObject explosionEffect;
    }
}
