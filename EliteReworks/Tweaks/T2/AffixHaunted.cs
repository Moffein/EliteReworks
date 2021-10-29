﻿using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using R2API;
using EliteReworks.Tweaks.T2.Components;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;

namespace EliteReworks.Tweaks.T2
{
    public static class AffixHaunted
    {
        public static BuffDef reviveBuff;

        public static void Setup()
        {
            reviveBuff = CreateReviveBuff();
            if (EliteReworksPlugin.affixHauntedBetaEnabled)
            {
                DisableBubble();
                On.RoR2.GlobalEventManager.OnCharacterDeath += ReviveAsGhost;
                AddToWarbannerBuff();
                ChangeOnHitEffect();
            }
            else if (EliteReworksPlugin.affixHauntedSimpleIndicatorEnabled)
            {
                AffixHauntedAura.indicatorPrefab = BuildIndicator();
                HideBubble();
            }
        }

        #region minimal bubble
        private static GameObject BuildIndicator()
        {
            GameObject indicator = Resources.Load<GameObject>("prefabs/networkedobjects/shrines/ShrineHealingWard").InstantiateClone("MoffeinEliteReworksHauntedIndicator", true);
            indicator.transform.localScale *= 30f;

            UnityEngine.Object.Destroy(indicator.GetComponent<HealingWard>());
            ParticleSystemRenderer[] pr = indicator.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (ParticleSystemRenderer p in pr)
            {
                if (p.name == "HealingSymbols")
                {
                    UnityEngine.Object.Destroy(p);
                }
            }

            NetworkedBodyAttachment nba = indicator.AddComponent<NetworkedBodyAttachment>();
            nba.shouldParentToAttachedBody = true;
            nba.forceHostAuthority = false;

            PrefabAPI.RegisterNetworkPrefab(indicator);
            return indicator;
        }

        private static void HideBubble()
        {
            GameObject bubble = Resources.Load<GameObject>("prefabs/networkedobjects/AffixHauntedWard");
            MeshRenderer[] mr = bubble.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in mr)
            {
                if (m.name == "IndicatorSphere")
                {
                    UnityEngine.Object.Destroy(m);
                }
            }
        }
        #endregion

        #region revive rework
        public static BuffDef CreateReviveBuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.buffColor = new Color(157f / 255f, 221f / 255f, 216f / 255f);
            buff.canStack = false;
            buff.isDebuff = false;
            buff.name = "EliteReworksHauntedRevive";
            buff.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffDeathMarkIcon");
            BuffAPI.Add(new CustomBuff(buff));
            return buff;
        }

        public static void AttemptSpawnGhost(CharacterBody sourceBody, Vector3 position, float radius)
        {
            float radiusSquare = radius * radius;
            float squareDist;

            if (sourceBody.teamComponent)
            {
                TeamIndex ti = sourceBody.teamComponent.teamIndex;
                System.Collections.ObjectModel.ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(ti);
                foreach (TeamComponent tc in teamMembers)
                {
                    if (tc.body && tc.body.healthComponent && tc.body.healthComponent.alive)
                    {
                        if (tc.body.HasBuff(RoR2Content.Buffs.AffixHaunted))
                        {
                            squareDist = (position - tc.body.corePosition).sqrMagnitude;
                            if (squareDist < radiusSquare)
                            {
                                AffixHauntedReviveAura ahr = tc.body.GetComponent<AffixHauntedReviveAura>();
                                if (ahr && ahr.wardActive && (ahr.attachedGhosts.Count + ahr.attachedAliveMonsters.Count) < AffixHauntedReviveAura.maxAttachedGhosts)
                                {
                                    CharacterBody ghostBody = Util.TryToCreateGhost(sourceBody, tc.body, 30);
                                    if (ghostBody.master && ghostBody.master.inventory)
                                    {
                                        ghostBody.master.inventory.RemoveItem(RoR2Content.Items.BoostDamage, 150);
                                    }
                                    ghostBody.AddBuff(reviveBuff);
                                    ahr.attachedGhosts.Add(ghostBody);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void DisableBubble()
        {
            On.RoR2.CharacterBody.AffixHauntedBehavior.FixedUpdate += (orig, self) =>
            {
                return;
            };
        }

        private static void ReviveAsGhost(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (damageReport.victimBody && damageReport.victimBody.HasBuff(reviveBuff.buffIndex) && !damageReport.victimBody.disablingHurtBoxes && !damageReport.victimBody.HasBuff(RoR2Content.Buffs.AffixHaunted))
            {
                AttemptSpawnGhost(damageReport.victimBody, damageReport.damageInfo.position, AffixHauntedReviveAura.wardRadius);
            }
        }
        #endregion
    
        private static void AddToWarbannerBuff()
        {
            //Revive Buff shows the warbanner effect
            IL.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "Warbanner")
                    );
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, CharacterBody, bool>>((hasWarbanner, self) =>
                {
                    return hasWarbanner || self.HasBuff(reviveBuff);
                });
            };
        }

        private static void ChangeOnHitEffect()
        {
            //Remove vanilla effect
            IL.RoR2.GlobalEventManager.OnHitEnemy += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AffixHaunted")
                    );
                c.Remove();
                c.Emit<EliteReworksPlugin>(OpCodes.Ldsfld, nameof(EliteReworksPlugin.EmptyBuff));
            };

            //Attach to CrippleOnHit
            IL.RoR2.HealthComponent.TakeDamage += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AffixLunar")
                    );
                c.Index += 2;
                c.Emit(OpCodes.Ldloc_1);
                c.EmitDelegate<Func<bool, CharacterBody, bool>>((hasAffixLunar, body) =>
                {
                    return hasAffixLunar || body.HasBuff(RoR2Content.Buffs.AffixHaunted);
                });
            };
        }
    }
}
