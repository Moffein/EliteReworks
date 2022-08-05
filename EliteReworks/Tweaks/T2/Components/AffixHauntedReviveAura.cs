using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T2.Components
{
    [DisallowMultipleComponent]
    public class AffixHauntedReviveAura : MonoBehaviour
    {
        public static float detachRadius = 45f;
        public static float wardRadius = 30f;
        public static float refreshTime = 0.4f;

        public static float ghostPingTime = 2.5f;
        private float ghostPingStopwatch;

        public static int maxAttachedGhosts = 4;
        public static int championBonusAttachedGhosts = 2;
        public List<HauntTargetInfo> attachedGhosts;
        public List<HauntTargetInfo> attachedAliveMonsters;

        private float stopwatch;
        private CharacterBody ownerBody;
        private TeamComponent teamComponent;

        public bool wardActive = false;

        private float stunDisableTimer = 0f;

        public int GetMaxGhosts()
        {
            return maxAttachedGhosts + ((ownerBody && ownerBody.isChampion) ? championBonusAttachedGhosts : 0);
        }

        public void Awake()
        {
            wardActive = false;
            stopwatch = 0f;
            ghostPingStopwatch = 0f;

            attachedGhosts = new List<HauntTargetInfo>();
            attachedAliveMonsters = new List<HauntTargetInfo>();

            ownerBody = base.gameObject.GetComponent<CharacterBody>();
            if (!ownerBody)
            {
                Destroy(this);
            }
            teamComponent = ownerBody.teamComponent;
        }

        public void FixedUpdate()
        {

            wardActive = stunDisableTimer <= 0f;
            bool eliteActive = ownerBody && ownerBody.HasBuff(RoR2Content.Buffs.AffixHaunted) && ownerBody.healthComponent && ownerBody.healthComponent.alive;
            if (!eliteActive)
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
                if (eliteActive)
                {
                    if (ownerBody.healthComponent && ownerBody.healthComponent.isInFrozenState)
                    {
                        wardActive = false;
                        stunDisableTimer = EliteReworksPlugin.eliteStunDisableDuration;
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
                                stunDisableTimer = EliteReworksPlugin.eliteStunDisableDuration;
                            }
                        }
                    }
                }

                if (wardActive && stunDisableTimer <= 0f)
                {
                    stopwatch += Time.fixedDeltaTime;
                    if (stopwatch > refreshTime)
                    {
                        stopwatch -= refreshTime;
                        if (attachedGhosts.Count + attachedAliveMonsters.Count < GetMaxGhosts())
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
                if (stunDisableTimer > 0f)
                {
                    stunDisableTimer -= Time.fixedDeltaTime;
                }

                if (attachedGhosts.Count > 0)
                {
                    if (!ownerBody.HasBuff(AffixHaunted.ghostsActiveBuff))
                    {
                        ownerBody.AddBuff(AffixHaunted.ghostsActiveBuff);
                    }
                }
                else
                {
                    if (ownerBody.HasBuff(AffixHaunted.ghostsActiveBuff))
                    {
                        ownerBody.RemoveBuff(AffixHaunted.ghostsActiveBuff);
                    }
                }

                ghostPingStopwatch += Time.fixedDeltaTime;
                if (ghostPingStopwatch > AffixHauntedReviveAura.ghostPingTime)
                {
                    ghostPingStopwatch -= AffixHauntedReviveAura.ghostPingTime;
                    PingGhosts();
                }
            }
        }

        private void UpdateGhosts()
        {
            if (attachedGhosts.Count > 0)
            {
                List<HauntTargetInfo> toRemove = new List<HauntTargetInfo>();
                foreach (HauntTargetInfo ht in attachedGhosts)
                {
                    if (!(ht.body && ht.body.healthComponent && ht.body.healthComponent.alive))
                    {
                        toRemove.Add(ht);
                    }
                }

                if (toRemove.Count > 0)
                {
                    foreach (HauntTargetInfo ht in toRemove)
                    {
                        attachedGhosts.Remove(ht);
                    }
                }
            }
        }

        private void UpdateAliveMonsters()
        {
            int maxAlive = GetMaxGhosts() - attachedGhosts.Count;
            int aliveCount = 1;

            if (attachedAliveMonsters.Count > 0)
            {
                float radiusSquare = detachRadius * detachRadius;
                float squareDist;

                List<HauntTargetInfo> toRemove = new List<HauntTargetInfo>();
                foreach (HauntTargetInfo ht in attachedAliveMonsters)
                {
                    bool remove = true;
                    if (ht.body)
                    {
                        remove = false;
                        if (!(ht.body.healthComponent && ht.body.healthComponent.alive) || aliveCount > maxAlive)
                        {
                            remove = true;
                        }
                        else
                        {
                            squareDist = (ownerBody.corePosition - ht.body.corePosition).sqrMagnitude;
                            if (squareDist > radiusSquare)
                            {
                                remove = true;
                            }
                        }
                    }

                    if (remove)
                    {
                        toRemove.Add(ht);
                    }
                    else
                    {
                        aliveCount++;
                    }
                }

                if (toRemove.Count > 0)
                {
                    foreach (HauntTargetInfo ht in toRemove)
                    {
                        if (ht.body)
                        {
                            if (ht.body.HasBuff(AffixHaunted.reviveBuff))
                            {
                                ht.body.RemoveBuff(AffixHaunted.reviveBuff);
                            }
                        }
                        attachedAliveMonsters.Remove(ht);
                    }
                }
            }
        }

        private void FindAliveMonsters()
        {
            if (teamComponent)
            {
                Vector3 position = ownerBody.corePosition;
                int slotsRemaining = GetMaxGhosts() - (attachedGhosts.Count + attachedAliveMonsters.Count);
                if (slotsRemaining > 0)
                {
                    float radiusSquare = wardRadius * wardRadius;
                    float squareDist;
                    TeamIndex ti = teamComponent.teamIndex;

                    System.Collections.ObjectModel.ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(ti);
                    foreach (TeamComponent tc in teamMembers)
                    {
                        if (tc.body
                            && (tc.body.bodyFlags & CharacterBody.BodyFlags.Masterless) != CharacterBody.BodyFlags.Masterless
                            && !tc.body.disablingHurtBoxes
                            && tc.body.healthComponent && tc.body.healthComponent.alive)
                        {
                            if (!tc.body.HasBuff(RoR2Content.Buffs.AffixHaunted) && !tc.body.HasBuff(AffixHaunted.reviveBuff))
                            {
                                squareDist = (position - tc.body.corePosition).sqrMagnitude;
                                if (squareDist <= radiusSquare)
                                {
                                    tc.body.AddBuff(AffixHaunted.reviveBuff);

                                    HauntTargetInfo hti = new HauntTargetInfo
                                    {
                                        body = tc.body,
                                    };

                                    attachedAliveMonsters.Add(hti);

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
            foreach (HauntTargetInfo ht in attachedAliveMonsters)
            {
                if (ht.body && ht.body.HasBuff(AffixHaunted.reviveBuff) && ht.body.healthComponent && ht.body.healthComponent.alive)
                {
                    ht.body.RemoveBuff(AffixHaunted.reviveBuff);
                }
            }
            attachedAliveMonsters.Clear();
        }

        private void DestroyGhosts()
        {
            foreach (HauntTargetInfo ht in attachedGhosts)
            {
                if (ht.body && ht.body.healthComponent)
                {
                    ht.body.healthComponent.health = -100f;
                    /*if (ht.body.master)
                    {
                        MasterSuicideOnTimer msot = ht.body.master.gameObject.AddComponent<MasterSuicideOnTimer>();
                        msot.lifeTimer = 0.1f;
                    }*/
                }
            }
            attachedGhosts.Clear();
        }

        private void OnDestroy()
        {
            DestroyGhosts();
            ClearAliveMonsters();
        }

        //Hacky way to show ghost trails since Tethers dont work well with ghosts due to hurtboxes being disabled.
        //Have to go from victim to owner since orbs require hurtboxes to be active to target properly, as well as HauntOrbs having a built-in thing where they give targets the Celestine buff.
        private void PingGhosts()
        {
            if (ownerBody && ownerBody.mainHurtBox)
            {
                int indexInGroup = 0;
                foreach (HauntTargetInfo ht in attachedGhosts)
                {
                    if (ht.body)
                    {
                        HauntOrb squidOrb = new HauntOrb();
                        /*squidOrb.forceScalar = 0f;
                        squidOrb.damageValue = 0f;
                        squidOrb.isCrit = false;
                        squidOrb.teamIndex = teamComponent ? teamComponent.teamIndex : TeamIndex.None;
                        squidOrb.attacker = base.gameObject;
                        squidOrb.procCoefficient = 0f;
                        squidOrb.damageType = DamageType.NonLethal | DamageType.Silent;*/
                        squidOrb.origin = ht.body.corePosition;
                        squidOrb.target = ownerBody.mainHurtBox;
                        squidOrb.timeToArrive = 1f + indexInGroup * 0.1f;
                        squidOrb.scale = 1f;
                        OrbManager.instance.AddOrb(squidOrb);
                    }
                    indexInGroup++;
                }

                foreach (HauntTargetInfo ht in attachedAliveMonsters)
                {
                    if (ht.body && ht.body.mainHurtBox)
                    {
                        HauntOrb squidOrb = new HauntOrb();
                        /*squidOrb.forceScalar = 0f;
                        squidOrb.damageValue = 0f;
                        squidOrb.isCrit = false;
                        squidOrb.teamIndex = teamComponent ? teamComponent.teamIndex : TeamIndex.None;
                        squidOrb.attacker = base.gameObject;
                        squidOrb.procCoefficient = 0f;
                        squidOrb.damageType = DamageType.NonLethal | DamageType.Silent;*/
                        squidOrb.origin = ht.body.corePosition;
                        squidOrb.target = ownerBody.mainHurtBox;
                        squidOrb.timeToArrive = 1f + indexInGroup * 0.1f;
                        squidOrb.scale = 1f;
                        OrbManager.instance.AddOrb(squidOrb);
                    }
                    indexInGroup++;
                }
            }
        }
    }


    public class HauntTargetInfo
    {
        public CharacterBody body = null;
        //Was going to have Tether info here but had problems with getting it to work.
    }
}