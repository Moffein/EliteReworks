using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T1.Components
{
    public class AffixBluePassiveLightning : MonoBehaviour
    {
        public static SetStateOnHurt ssoh;
        public static GameObject lightningProjectilePrefab;
        public static float baseLightningTimer = 6f;
        public static CharacterBody ownerBody;
        public static int baseMeatballCount = 5;
        public static float baseDamage = 28f;
        public static float onHitCooldown = 0.33f;

        //Min time til next on-hit can be triggered
        public float onHitTimer;
        public bool onHitReady => onHitTimer <= 0f;

        private float lightningStopwatch;

        public void TriggerOnHit()
        {
            onHitTimer = onHitCooldown;
        }

        public void Awake()
        {
            ownerBody = base.GetComponent<CharacterBody>();
            onHitTimer = 0f;
            lightningStopwatch = 0f;
            ssoh = base.GetComponent<SetStateOnHurt>();
        }

        public void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (onHitTimer > 0f)
            {
                onHitTimer -= Time.fixedDeltaTime;
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
            if (lightningStopwatch > baseLightningTimer)
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
