using RoR2;
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
        public List<CharacterBody> attachedAliveMonsters;

        private float stopwatch;
        private CharacterBody ownerBody;
        private TeamComponent teamComponent;

        public bool wardActive = false;

        public void Awake()
        {
            wardActive = false;
            stopwatch = 0f;

            attachedGhosts = new List<CharacterBody>();
            attachedAliveMonsters = new List<CharacterBody>();

            ownerBody = base.gameObject.GetComponent<CharacterBody>();
            if (!ownerBody)
            {
                Destroy(this);
            }
            teamComponent = ownerBody.teamComponent;
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
                    ClearAliveMonsters();
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
                        if (attachedGhosts.Count + attachedAliveMonsters.Count < maxAttachedGhosts)
                        {
                            FindAliveMonsters();
                        }
                        UpdateGhosts();
                        UpdateAliveMonsters();
                    }
                }
                else
                {
                    ClearAliveMonsters();
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

        private void UpdateAliveMonsters()
        {
            if (attachedAliveMonsters.Count > 0)
            {
                float radiusSquare = wardRadius * wardRadius;
                float squareDist;

                List<CharacterBody> toRemove = new List<CharacterBody>();
                foreach (CharacterBody cb in attachedAliveMonsters)
                {
                    bool remove = true;
                    bool dead = false;
                    if (cb)
                    {
                        remove = false;
                        if (!(cb.healthComponent && cb.healthComponent.alive))
                        {
                            remove = true;
                        }
                        else
                        {
                            squareDist = (ownerBody.corePosition - cb.corePosition).sqrMagnitude;
                            if (squareDist > radiusSquare)
                            {
                                if (cb.HasBuff(AffixHaunted.reviveBuff))
                                {
                                    cb.RemoveBuff(AffixHaunted.reviveBuff);
                                }
                                remove = true;
                            }
                        }
                    }

                    if (remove)
                    {
                        toRemove.Add(cb);
                    }
                }

                if (toRemove.Count > 0)
                {
                    foreach (CharacterBody cb in toRemove)
                    {
                        attachedAliveMonsters.Remove(cb);
                    }
                }
            }
        }

        private void FindAliveMonsters()
        {
            if (teamComponent)
            {
                Vector3 position = ownerBody.corePosition;
                int slotsRemaining = maxAttachedGhosts - (attachedGhosts.Count + attachedAliveMonsters.Count);
                if (slotsRemaining > 0)
                {
                    float radiusSquare = wardRadius * wardRadius;
                    float squareDist;
                    TeamIndex ti = teamComponent.teamIndex;

                    System.Collections.ObjectModel.ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(ti);
                    foreach (TeamComponent tc in teamMembers)
                    {
                        if (tc.body && !tc.body.disablingHurtBoxes && tc.body.healthComponent && tc.body.healthComponent.alive)
                        {
                            if (!tc.body.HasBuff(RoR2Content.Buffs.AffixHaunted) && !tc.body.HasBuff(AffixHaunted.reviveBuff))
                            {
                                squareDist = (position - tc.body.corePosition).sqrMagnitude;
                                if (squareDist <= radiusSquare)
                                {
                                    tc.body.AddBuff(AffixHaunted.reviveBuff);
                                    attachedAliveMonsters.Add(tc.body);

                                    slotsRemaining--;
                                    if (slotsRemaining <= 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ClearAliveMonsters()
        {
            foreach(CharacterBody cb in attachedAliveMonsters)
            {
                if (cb && cb.HasBuff(AffixHaunted.reviveBuff) && cb.healthComponent && cb.healthComponent.alive)
                {
                    cb.RemoveBuff(AffixHaunted.reviveBuff);
                }
            }
            attachedAliveMonsters.Clear();
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
            ClearAliveMonsters();
        }
    }
}
