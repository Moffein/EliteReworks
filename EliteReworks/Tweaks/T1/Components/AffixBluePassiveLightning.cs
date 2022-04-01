using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T1.Components
{
    [DisallowMultipleComponent]
    public class AffixBluePassiveLightning : MonoBehaviour
    {
        public static SetStateOnHurt ssoh;
        public static GameObject lightningProjectilePrefab;
        public static float baseLightningTimer = 6f;
        public static int baseMeatballCount = 5;
        public static float baseDamage = 40f;

        public static bool uncapOnHitLightning = false;
        public static int baseBodyMeatballStock = 20;
        public static float totalBodyMeatballRechargeInterval = 1.6f;
        public int bodyMeatballStock { get; private set; }
        private float bodyMeatballRechargeInterval;
        private float bodyMeatballRechargeStopwatch;
        public CharacterBody ownerBody;

        private float lightningStopwatch;

        private float stunDisableTimer = 0f;

        public bool OnHitReady(int meatballCount)
        {
            return meatballCount <= bodyMeatballStock || uncapOnHitLightning;
        }

        public void TriggerOnHit(int meatballCount)
        {
            bodyMeatballRechargeStopwatch = 0f;
            bodyMeatballStock -= meatballCount;
            if (bodyMeatballStock < 0)
            {
                bodyMeatballStock = 0;
            }
        }

        public void Awake()
        {
            ownerBody = base.GetComponent<CharacterBody>();
            lightningStopwatch = 0f;
            ssoh = base.GetComponent<SetStateOnHurt>();

            bodyMeatballStock = baseBodyMeatballStock;
            bodyMeatballRechargeInterval = totalBodyMeatballRechargeInterval / (float)baseBodyMeatballStock;
            bodyMeatballRechargeStopwatch = 0f;
        }

        public void FixedUpdate()
        {
            if (!NetworkServer.active) return;

            if (bodyMeatballStock < baseBodyMeatballStock)
            {
                bodyMeatballRechargeStopwatch += Time.fixedDeltaTime;
                if (bodyMeatballRechargeStopwatch >= bodyMeatballRechargeInterval)
                {
                    bodyMeatballRechargeStopwatch -= bodyMeatballRechargeInterval;
                    bodyMeatballStock++;
                }
            }
            else
            {
                bodyMeatballRechargeStopwatch = 0f;
            }

            if (!(ownerBody && ownerBody.HasBuff(RoR2Content.Buffs.AffixBlue.buffIndex) && ownerBody.healthComponent && ownerBody.healthComponent.alive))
            {
                return;
            }


            if (ownerBody.healthComponent && ownerBody.healthComponent.isInFrozenState)
            {
                lightningStopwatch = 0f;
                stunDisableTimer = EliteReworksPlugin.eliteStunDisableDuration;
            }
            else if (ssoh)
            {
                Type state = ssoh.targetStateMachine.state.GetType();
                if (state == typeof(EntityStates.StunState) || state == typeof(EntityStates.ShockState))
                {
                    lightningStopwatch = 0f;
                    stunDisableTimer = EliteReworksPlugin.eliteStunDisableDuration;
                }
            }

            if (stunDisableTimer <= 0f)
            {
                lightningStopwatch += Time.fixedDeltaTime;
                if (lightningStopwatch >= baseLightningTimer)
                {
                    lightningStopwatch = 0f;
                    int meatballCount = baseMeatballCount; //+ (int)(ownerBody.radius * baseMeatballCount / 3f);

                    //float scaledDamage = ownerBody.damage * damageCoefficient;
                    float scaledDamage = (baseDamage + Mathf.Max(0f, ownerBody.level - 1f) * baseDamage * 0.2f);
                    if (ownerBody.isChampion)
                    {
                        scaledDamage *= EliteReworksPlugin.eliteBossDamageMult;
                    }

                    if (AffixBlue.enablePassiveLightning) this.FireMeatballs(ownerBody.gameObject, ownerBody.isChampion, scaledDamage, ownerBody.RollCrit(),
                                    Vector3.up, ownerBody.corePosition + Vector3.up, ownerBody.transform.forward,
                                    meatballCount, 20f, 400f, ownerBody.isChampion? 25f : 20f, false);
                }
            }
            else
            {
                stunDisableTimer -= Time.fixedDeltaTime;
            }
           
        }

        //Copypasted from Magma Worm
        public void FireMeatballs(GameObject attacker, bool isChampion, float damage, bool crit,
            Vector3 impactNormal, Vector3 impactPosition, Vector3 forward,
            int meatballCount, float meatballAngle, float meatballForce, float velocity, bool randomize = false)
        {
            EffectManager.SimpleSoundEffect((isChampion ? AffixBlue.triggerBossSound : AffixBlue.triggerSound).index, impactPosition, true);

            float num = 360f / (float)meatballCount;
            float randomOffset = UnityEngine.Random.Range(0f, 360f);
            Vector3 normalized = Vector3.ProjectOnPlane(forward, impactNormal).normalized;
            Vector3 point = Vector3.zero;
            if (!randomize)
            {
                point = Vector3.RotateTowards(impactNormal, normalized, meatballAngle * 0.0174532924f, float.PositiveInfinity);
            }
            bool spawnedClose = false;
            for (int i = 0; i < meatballCount; i++)
            {
                if (randomize)
                {
                    float angle = UnityEngine.Random.Range(!spawnedClose ? 0f : meatballAngle, meatballAngle * 1.2f);
                    if (angle < meatballAngle)
                    {
                        spawnedClose = true;
                    }
                    else
                    {
                        spawnedClose = false;
                    }
                    point = Vector3.RotateTowards(impactNormal, normalized, angle * 0.0174532924f, float.PositiveInfinity);
                }
                Vector3 forward2 = Quaternion.AngleAxis(randomOffset + num * (float)i, impactNormal) * point;
                ProjectileManager.instance.FireProjectile((isChampion ? AffixBlue.lightningBossProjectilePrefab : AffixBlue.lightningProjectilePrefab), impactPosition, RoR2.Util.QuaternionSafeLookRotation(forward2),
                    attacker, damage, meatballForce, crit, DamageColorIndex.Default, null, velocity);
            }
        }
    }
}
