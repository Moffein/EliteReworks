using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks
{
    public static class Util
    {
        public static void DebuffSphere(BuffIndex buff, TeamIndex team, Vector3 position, float radius, float debuffDuration, GameObject effect, GameObject hitEffect, bool ignoreImmunity)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (effect != null)
            {
                EffectManager.SpawnEffect(effect, new EffectData
                {
                    origin = position,
                    scale = radius
                }, true);
            }

            List<HealthComponent> hcList = new List<HealthComponent>();
            Collider[] array = Physics.OverlapSphere(position, radius, LayerIndex.entityPrecise.mask);
            for (int i = 0; i < array.Length; i++)
            {
                HurtBox hurtBox = array[i].GetComponent<HurtBox>();
                if (hurtBox)
                {
                    HealthComponent healthComponent = hurtBox.healthComponent;
                    ProjectileController pc = array[i].GetComponentInParent<ProjectileController>();
                    if (healthComponent && !pc && !hcList.Contains(healthComponent))
                    {
                        hcList.Add(healthComponent);
                        if (healthComponent.body && healthComponent.body.teamComponent && healthComponent.body.teamComponent.teamIndex != team)
                        {
                            if (healthComponent.body
                                && (ignoreImmunity|| (!healthComponent.body.HasBuff(RoR2Content.Buffs.Immune) && !healthComponent.body.HasBuff(RoR2Content.Buffs.HiddenInvincibility))))
                            {
                                healthComponent.body.AddTimedBuff(buff, debuffDuration);
                                if (hitEffect != null)
                                {
                                    EffectManager.SpawnEffect(hitEffect, new EffectData
                                    {
                                        origin = healthComponent.body.corePosition
                                    }, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void BuffSphere(BuffIndex buff, TeamIndex team, Vector3 position, float radius, float debuffDuration, GameObject effect)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (effect != null)
            {
                EffectManager.SpawnEffect(effect, new EffectData
                {
                    origin = position,
                    scale = radius
                }, true);
            }

            List<HealthComponent> hcList = new List<HealthComponent>();
            Collider[] array = Physics.OverlapSphere(position, radius, LayerIndex.entityPrecise.mask);
            for (int i = 0; i < array.Length; i++)
            {
                HurtBox hurtBox = array[i].GetComponent<HurtBox>();
                if (hurtBox)
                {
                    HealthComponent healthComponent = hurtBox.healthComponent;
                    ProjectileController pc = array[i].GetComponentInParent<ProjectileController>();
                    if (healthComponent && !pc && !hcList.Contains(healthComponent))
                    {
                        hcList.Add(healthComponent);
                        if (healthComponent.body && healthComponent.body.teamComponent)
                        {
                            if (healthComponent.body.teamComponent.teamIndex == team)
                            {
                                if (healthComponent.body)
                                {
                                    healthComponent.body.AddTimedBuff(buff, debuffDuration);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static CharacterBody FindElite(BuffIndex buff, TeamIndex team, Vector3 position, float radius)
        {
            CharacterBody body = null;
            List<HealthComponent> hcList = new List<HealthComponent>();
            Collider[] array = Physics.OverlapSphere(position, radius, LayerIndex.entityPrecise.mask);
            for (int i = 0; i < array.Length; i++)
            {
                HurtBox hurtBox = array[i].GetComponent<HurtBox>();
                if (hurtBox)
                {
                    HealthComponent healthComponent = hurtBox.healthComponent;
                    ProjectileController pc = array[i].GetComponentInParent<ProjectileController>();
                    if (healthComponent && !pc && !hcList.Contains(healthComponent))
                    {
                        hcList.Add(healthComponent);
                        if (healthComponent.body && healthComponent.body.teamComponent)
                        {
                            if (healthComponent.body.teamComponent.teamIndex == team)
                            {
                                CharacterBody cb = healthComponent.body;
                                if (cb && cb.healthComponent.alive && cb.HasBuff(buff))
                                {
                                    return cb;
                                }
                            }
                        }
                    }
                }
            }
            return body;
        }
    }
}
