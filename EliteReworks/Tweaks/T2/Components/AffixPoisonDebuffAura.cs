using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T2.Components
{
    public class AffixPoisonDebuffAura : MonoBehaviour
    {
        public static float wardRadius = 22f;
        public static float buffDuration = 0.6f;
        public static float refreshTime = 0.2f;
        public static GameObject indicatorPrefab;

        private float stopwatch;
        private CharacterBody ownerBody;

        private GameObject indicator = null;
        public bool wardActive = false;


        public void Awake()
        {

            wardActive = false;
            stopwatch = 0f;
            ownerBody = base.gameObject.GetComponent<CharacterBody>();
        }

        public void FixedUpdate()
        {
            wardActive = true;
            if (ownerBody && ownerBody.HasBuff(RoR2Content.Buffs.AffixPoison) && ownerBody.healthComponent && ownerBody.healthComponent.alive)
            {
                if (ownerBody.healthComponent && ownerBody.healthComponent.isInFrozenState)
                {
                    wardActive = false;
                }
                else
                {
                    SetStateOnHurt ssoh = base.gameObject.GetComponent<SetStateOnHurt>();
                    if (ssoh)
                    {
                        Type state = ssoh.targetStateMachine.state.GetType();
                        if (state == typeof(EntityStates.StunState) || state == typeof(EntityStates.ShockState))
                        {
                            wardActive = false;
                        }
                    }
                }
            }
            else
            {
                if (NetworkServer.active)
                {
                    if (this.indicator)
                    {
                        Destroy(this.indicator);
                        this.indicator = null;
                    }
                }
                return;
            }

            if (NetworkServer.active)
            {
                if (wardActive)
                {
                    stopwatch += Time.fixedDeltaTime;
                    if (stopwatch > refreshTime)
                    {
                        stopwatch -= refreshTime;
                        if (ownerBody.teamComponent)
                        {
                            Util.DebuffSphere(RoR2Content.Buffs.HealingDisabled.buffIndex, ownerBody.teamComponent.teamIndex,
                                ownerBody.corePosition, AffixPoisonDebuffAura.wardRadius, AffixPoisonDebuffAura.buffDuration, null, null, true);
                        }
                    }
                }
            }
            UpdateIndicator(wardActive);
        }

        public void UpdateIndicator(bool wardActive)
        {
            if (NetworkServer.active)
            {
                if (this.indicator != wardActive)
                {
                    if (wardActive)
                    {
                        this.indicator = UnityEngine.Object.Instantiate<GameObject>(indicatorPrefab);
                        this.indicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
                    }
                    else
                    {
                        Destroy(this.indicator);
                        this.indicator = null;
                    }
                }
            }
        }
    }
}
