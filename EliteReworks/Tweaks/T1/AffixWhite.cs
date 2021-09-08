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
        public static float baseRadius = 5f;
        public static float slowDuration = 1.5f;
        public static void Setup()
        {
            explosionEffect = CreateEffect();
            //On-Hit handled in shared hooks
        }

        public static GameObject CreateEffect()
        {
            GameObject effect = Resources.Load<GameObject>("prefabs/effects/impacteffects/AffixWhiteExplosion").InstantiateClone("MoffeinEliteReworkGlacialExplosion", false);
            UnityEngine.Object.Destroy(effect.GetComponent<ShakeEmitter>());
            EffectComponent ec = effect.GetComponent<EffectComponent>();
            ec.soundName = "";
            ec.applyScale = true;

            /*ParticleSystem[] ps = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem p in ps)
            {
                p.playbackSpeed *= 2f;
            }*/

            //effect.AddComponent<DestroyOnTimer>().duration = 0.5f;

            EffectAPI.AddEffect(effect);
            return effect;
        }

        public static GameObject explosionEffect;
    }
}
