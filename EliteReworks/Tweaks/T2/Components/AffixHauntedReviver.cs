using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T2.Components
{
    public class AffixHauntedReviver : MonoBehaviour
    {
        public static float wardRadius = 22f;
        public static float buffDuration = 0.5f;
        public static float refreshTime = 0.4f;
        public static GameObject indicatorPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator");

        private float stopwatch;
        private CharacterBody ownerBody;
        private GameObject indicator;

        private List<CharacterBody> ghostList;
        private List<CharacterBody> toRemove;

        public void Awake()
        {
            ownerBody = base.gameObject.GetComponent<CharacterBody>();
            if (!ownerBody)
            {
                Destroy(this);
            }
            stopwatch = 0f;
            ghostList = new List<CharacterBody>();
            toRemove = new List<CharacterBody>();
        }

        public void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            bool wardActive = true;
            if (ownerBody && ownerBody.HasBuff(RoR2Content.Buffs.AffixHaunted) && ownerBody.healthComponent.alive)
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
                if (!ownerBody.healthComponent.alive)
                {
                    FreeGhosts();
                }
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
                        EliteReworksUtil.BuffSphere(AffixHaunted.reviveBuff.buffIndex, ownerBody.teamComponent.teamIndex,
                            ownerBody.corePosition, AffixHauntedReviver.wardRadius, AffixHauntedReviver.buffDuration, null);
                    }
                }
            }

            UpdateGhosts();
            UpdateIndicator(wardActive);
        }

        private void UpdateGhosts()
        {
            foreach(CharacterBody cb in ghostList)
            {
                if (!(cb && cb.healthComponent && cb.healthComponent.alive))
                {
                    toRemove.Add(cb);
                }
            }

            foreach(CharacterBody cb in toRemove)
            {
                ghostList.Remove(cb);
            }
            toRemove.Clear();
        }

        public void AddGhost(CharacterBody cb)
        {
            ghostList.Add(cb);
        }

        [ClientRpc]
        public void RpcDestroySelf()
        {
            Destroy(this);
        }

        public void FreeGhosts()
        {
            if (NetworkServer.active)
            {
                foreach (CharacterBody cb in ghostList)
                {
                    if (cb && cb.healthComponent && cb.healthComponent.alive)
                    {
                        cb.healthComponent.health = -1f;
                    }
                }
            }
        }

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
                    this.indicator.transform.localScale *= AffixHauntedReviver.wardRadius / 12.8f;  //12.8 is what the size is listed as in the prefab
                    return;
                }
                UnityEngine.Object.Destroy(this.indicator);
                this.indicator = null;
            }
        }
    }
}
