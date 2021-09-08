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
        public static float damageCoefficient = 3f;

        public float lightningStopwatch;

        public void Awake()
        {
            ownerBody = base.GetComponent<CharacterBody>();
            if (!ownerBody)
            {
                Destroy(this);
                return;
            }
            lightningStopwatch = 0f;
            ssoh = base.GetComponent<SetStateOnHurt>();
        }

        public void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (!ownerBody || !ownerBody.HasBuff(RoR2Content.Buffs.AffixBlue.buffIndex))
            {
                Destroy(this);
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
                AffixBlue.FireMeatballs(ownerBody.gameObject, ownerBody.damage * damageCoefficient, ownerBody.RollCrit(),
                                Vector3.up, ownerBody.corePosition + Vector3.up, ownerBody.transform.forward,
                                meatballCount, 17f, 300f, 18f);
            }
        }
    }
}
