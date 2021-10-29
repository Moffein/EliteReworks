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
        public static CharacterBody ownerBody;
        public static int baseMeatballCount = 5;
        public static float baseDamage = 32f;

        public static bool uncapOnHitLightning = false;
        public static int baseBodyMeatballStock = 20;
        public static float totalBodyMeatballRechargeInterval = 1.6f;
        public int bodyMeatballStock { get; private set; }
        private float bodyMeatballRechargeInterval;
        private float bodyMeatballRechargeStopwatch;

        private float lightningStopwatch;

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
            if (!NetworkServer.active)
            {
                return;
            }

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
            }
            else if (ssoh)
            {
                Type state = ssoh.targetStateMachine.state.GetType();
                if (state == typeof(EntityStates.StunState) || state == typeof(EntityStates.ShockState))
                {
                    lightningStopwatch = 0f;
                }
            }

            lightningStopwatch += Time.fixedDeltaTime;
            if (lightningStopwatch >= baseLightningTimer)
            {
                lightningStopwatch -= baseLightningTimer;
                int meatballCount = baseMeatballCount + (int)(ownerBody.radius * baseMeatballCount/3f);

                //float scaledDamage = ownerBody.damage * damageCoefficient;
                float scaledDamage = (baseDamage + Mathf.Max(0f, ownerBody.level - 1f) * baseDamage * 0.2f);

                AffixBlue.FireMeatballs(ownerBody.gameObject, ownerBody.isChampion, scaledDamage, ownerBody.RollCrit(),
                                Vector3.up, ownerBody.corePosition + Vector3.up, ownerBody.transform.forward,
                                meatballCount, 20f, 400f, 20f);
            }
        }
    }
}
