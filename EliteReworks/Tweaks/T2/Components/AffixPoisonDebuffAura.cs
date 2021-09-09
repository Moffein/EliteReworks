using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T2.Components
{
    public class AffixPoisonDebuffAura : MonoBehaviour
    {
        public static float wardRadius = 25f;
        public static float buffDuration = 1f;
        public static float refreshTime = 0.5f;
        public static GameObject indicatorPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator");

        private float stopwatch;
        private CharacterBody ownerBody;

        private GameObject indicator = null;
        public bool wardActive = false;


        public void Awake()
        {

            wardActive = false;
            stopwatch = 0f;
            ownerBody = base.gameObject.GetComponent<CharacterBody>();
            if (!ownerBody)
            {
                Destroy(this);
            }
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
                Destroy(this);
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
                                ownerBody.corePosition, AffixPoisonDebuffAura.wardRadius + ownerBody.radius, AffixPoisonDebuffAura.buffDuration, null, null, true);
                        }
                    }
                }
            }
            //UpdateIndicator(wardActive);
        }

        //Indicator disabled because UNET fails to patch for some reason, even when this class is stripped to nothing but variables with no syncvars or networking.
        public void UpdateIndicator(bool wardActive)
        {
            if (this.indicator != wardActive)
            {
                if (wardActive)
                {
                    if (NetworkServer.active)
                    {
                        this.indicator = UnityEngine.Object.Instantiate<GameObject>(indicatorPrefab);
                        this.indicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
                    }

                    //Size needs to be set on the client
                    if (indicator)
                    {
                        this.indicator.transform.localScale *= AffixPoisonDebuffAura.wardRadius / 12.8f;  //12.8 is what the size is listed as in the prefab
                    }
                    return;
                }

                if (NetworkServer.active)
                {
                    Destroy(this.indicator);
                    this.indicator = null;
                }
            }
        }
    }
}
