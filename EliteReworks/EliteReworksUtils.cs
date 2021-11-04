using EliteReworks.Tweaks.T2.Components;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks
{
    public static class EliteReworksUtils
    {
        public static void DebuffSphere(BuffIndex buff, TeamIndex team, Vector3 position, float radius, float debuffDuration, GameObject effect, GameObject hitEffect, bool ignoreImmunity, bool falloff = false)
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
            float radiusHalfwaySqr = radius * radius * 0.25f;
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
                                && (ignoreImmunity || (!healthComponent.body.HasBuff(RoR2Content.Buffs.Immune) && !healthComponent.body.HasBuff(RoR2Content.Buffs.HiddenInvincibility))))
                            {
                                float effectiveness = 1f;
                                if (falloff)
                                {
                                    float distSqr = (position - hurtBox.collider.ClosestPoint(position)).sqrMagnitude;
                                    if (distSqr > radiusHalfwaySqr)  //Reduce effectiveness when over half the radius away
                                    {
                                        effectiveness *= 0.5f;
                                    }
                                }
                                healthComponent.body.AddTimedBuff(buff, effectiveness * debuffDuration);
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

        public static void BuffSphere(CharacterBody ownerBody, BuffIndex buff, TeamIndex team, Vector3 position, float radius, float debuffDuration, GameObject effect, bool ignoreOwner)
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
                                if (healthComponent.body && !(ignoreOwner && healthComponent.body == ownerBody))
                                {
                                    healthComponent.body.AddTimedBuff(buff, debuffDuration);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
