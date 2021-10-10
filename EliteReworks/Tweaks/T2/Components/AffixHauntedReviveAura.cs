﻿using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T2.Components
{
    [DisallowMultipleComponent]
    public class AffixHauntedReviveAura : MonoBehaviour
    {
        public static float wardRadius = 30f;
        public static float buffDuration = 1f;
        public static float refreshTime = 0.2f;

        public static int maxAttachedGhosts = 7;
        public List<CharacterBody> attachedGhosts;

        private float stopwatch;
        private CharacterBody ownerBody;

        public bool wardActive = false;

        public void Awake()
        {
            wardActive = false;
            stopwatch = 0f;

            attachedGhosts = new List<CharacterBody>();

            ownerBody = base.gameObject.GetComponent<CharacterBody>();
            if (!ownerBody)
            {
                Destroy(this);
            }
        }

        public void FixedUpdate()
        {

            wardActive = true;
            if (!(ownerBody && ownerBody.HasBuff(RoR2Content.Buffs.AffixHaunted) && ownerBody.healthComponent && ownerBody.healthComponent.alive))
            {
                wardActive = false;
                if (NetworkServer.active && ownerBody.healthComponent && !ownerBody.healthComponent.alive)
                {
                    DestroyGhosts();
                }
            }

            if (NetworkServer.active)
            {
                if (ownerBody && ownerBody.HasBuff(RoR2Content.Buffs.AffixHaunted) && ownerBody.healthComponent && ownerBody.healthComponent.alive)
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

                if (wardActive)
                {
                    stopwatch += Time.fixedDeltaTime;
                    if (stopwatch > refreshTime)
                    {
                        stopwatch -= refreshTime;
                        UpdateGhosts();
                        if (ownerBody.teamComponent)
                        {
                            EliteReworksUtils.BuffSphere(ownerBody, AffixHaunted.reviveBuff.buffIndex, ownerBody.teamComponent.teamIndex, ownerBody.corePosition,
                                AffixHauntedReviveAura.wardRadius, AffixHauntedReviveAura.buffDuration, null, true);
                        }
                    }
                }
            }
        }

        private void UpdateGhosts()
        {
            if (attachedGhosts.Count > 0)
            {
                List<CharacterBody> toRemove = new List<CharacterBody>();
                foreach (CharacterBody cb in attachedGhosts)
                {
                    if (!(cb.healthComponent && cb.healthComponent.alive))
                    {
                        toRemove.Add(cb);
                    }
                }

                if (toRemove.Count > 0)
                {
                    foreach (CharacterBody cb in toRemove)
                    {
                        attachedGhosts.Remove(cb);
                    }
                }
            }
        }

        private void DestroyGhosts()
        {
            if (attachedGhosts.Count > 0)
            {
                foreach (CharacterBody cb in attachedGhosts)
                {
                    if (cb.healthComponent)
                    {
                        cb.healthComponent.health = -100f;
                    }
                }
                attachedGhosts.Clear();
            }
        }

        private void OnDestroy()
        {
            DestroyGhosts();
        }
    }
}
