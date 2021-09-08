using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T2.Components
{
    public class AffixPoisonAura : NetworkBehaviour
    {
        public static float wardRadius = 30f;
        public static float buffDuration = 1f;
        public static float refreshTime = 0.5f;
        public static GameObject indicatorPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator");

        private float stopwatch;
        private CharacterBody ownerBody;
        private GameObject indicator;

        public void Awake()
        {
            stopwatch = 0f;
            ownerBody = base.gameObject.GetComponent<CharacterBody>();
            if (!ownerBody)
            {
                Destroy(this);
            }
        }

        public void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            bool wardActive = true;
            if (ownerBody && ownerBody.HasBuff(RoR2Content.Buffs.AffixPoison))
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
                if (this.indicator)
                {
                    UnityEngine.Object.Destroy(this.indicator);
                }
                RpcDestroySelf();
                return;
            }

            if (wardActive)
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch > refreshTime)
                {
                    stopwatch -= refreshTime;
                    if (ownerBody.teamComponent)
                    {
                        EliteReworksUtil.DebuffSphere(RoR2Content.Buffs.HealingDisabled.buffIndex, ownerBody.teamComponent.teamIndex,
                            ownerBody.corePosition, AffixPoisonAura.wardRadius, AffixPoisonAura.buffDuration, null, true);
                    }
                }
            }

            UpdateIndicator(wardActive);
        }

        [ClientRpc]
        public void RpcDestroySelf()
        {
            Destroy(this);
        }

        /*public void OnDestroy()
        {
            UnityEngine.Object.Destroy(this.indicator);
        }*/

        [Server]
        public void UpdateIndicator(bool wardActive)
        {
            if (this.indicator != wardActive)
            {
                if (wardActive)
                {
                    GameObject original = indicatorPrefab;
                    this.indicator = UnityEngine.Object.Instantiate<GameObject>(original);
                    this.indicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
                    this.indicator.transform.localScale *= AffixPoisonAura.wardRadius / 12.8f;  //12.8 is what the size is listed as in the prefab
                    return;
                }
                UnityEngine.Object.Destroy(this.indicator);
                this.indicator = null;
            }
        }
    }
}
