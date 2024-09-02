using UnityEngine;
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
        public static BuffDef reviveBuff;   //Indicates if an enemy is attached to a Celestine.
        public static BuffDef ghostsActiveBuff; //Indicates that a Celestine has active ghosts.'
        public static BuffDef armorReductionBuff;   //Applied on-hit.

        public static bool replaceOnHitEffect = true;

        public static void Setup()
        {
            reviveBuff = CreateReviveBuff();
            ghostsActiveBuff = CreateGhostsActiveBuff();
            armorReductionBuff = CreateArmorReductionBuff();

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender.HasBuff(AffixHaunted.reviveBuff))
                {
                    args.moveSpeedMultAdd += 0.7f;
                    args.attackSpeedMultAdd += 0.5f;
                }
                if (sender.HasBuff(AffixHaunted.armorReductionBuff))
                {
                    args.armorAdd -= 20f;
                }
            };

            On.EntityStates.Gup.BaseSplitDeath.OnEnter += FixGhostGupSplit;

            if (EliteReworksPlugin.affixHauntedEnabled)
            {
                DisableBubble();
                On.RoR2.GlobalEventManager.OnCharacterDeath += ReviveAsGhost;
                StealBuffVFX();
                ChangeOnHitEffect();
            }
        }

        private static void FixGhostGupSplit(On.EntityStates.Gup.BaseSplitDeath.orig_OnEnter orig, EntityStates.Gup.BaseSplitDeath self)
        {
            orig(self);
            if (NetworkServer.active && self.characterBody && self.characterBody.HasBuff(AffixHaunted.reviveBuff))
            {
                self.hasDied = true;
                self.DestroyBodyAsapServer();
            }
        }

        #region revive rework
        public static BuffDef CreateReviveBuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.buffColor = new Color(157f / 255f, 221f / 255f, 216f / 255f);
            buff.canStack = false;
            buff.isDebuff = false;
            buff.name = "EliteReworksHauntedRevive";
            buff.iconSprite = LegacyResourcesAPI.Load<BuffDef>("BuffDefs/DeathMark").iconSprite;
            R2API.ContentAddition.AddBuffDef((buff));
            return buff;
        }

        public static BuffDef CreateGhostsActiveBuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.buffColor = new Color(210f / 255f, 50f / 255f, 22f / 255f);
            buff.canStack = false;
            buff.isDebuff = false;
            buff.name = "EliteReworksHauntedGhostsActive";
            buff.iconSprite = LegacyResourcesAPI.Load<BuffDef>("BuffDefs/DeathMark").iconSprite;
            R2API.ContentAddition.AddBuffDef((buff));
            return buff;
        }

        public static BuffDef CreateArmorReductionBuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.buffColor = new Color(132f / 255f, 214f / 255f, 236f / 255f);
            buff.canStack = false;
            buff.isDebuff = true;
            buff.name = "EliteReworksHauntedArmorReduction";
            buff.iconSprite = LegacyResourcesAPI.Load<BuffDef>("BuffDefs/Cripple").iconSprite;
            R2API.ContentAddition.AddBuffDef((buff));
            return buff;
        }

        public static void AttemptSpawnGhost(CharacterBody sourceBody, Vector3 position, float radius)
        {
            if (!sourceBody || (sourceBody.bodyFlags & CharacterBody.BodyFlags.Masterless) == CharacterBody.BodyFlags.Masterless) return;

            float radiusSquare = radius * radius;
            float squareDist;

            if (sourceBody.teamComponent)
            {
                TeamIndex ti = sourceBody.teamComponent.teamIndex;
                System.Collections.ObjectModel.ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(ti);
                if (teamMembers == null) return;
                foreach (TeamComponent tc in teamMembers)
                {
                    if (tc.body && !tc.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && tc.body.HasBuff(RoR2Content.Buffs.AffixHaunted) && tc.body.healthComponent && tc.body.healthComponent.alive)
                    {
                        squareDist = (position - tc.body.corePosition).sqrMagnitude;
                        if (squareDist <= radiusSquare)
                        {
                            AffixHauntedReviveAura ahr = tc.body.GetComponent<AffixHauntedReviveAura>();
                            if (ahr && ahr.wardActive && ahr.attachedGhosts.Count < ahr.GetMaxGhosts())
                            {
                                CharacterBody ghostBody = Util.TryToCreateGhost(sourceBody, tc.body, 60 + (tc.body.isChampion ? 30 : 0));
                                if (ghostBody)
                                {
                                    if (ghostBody.master && ghostBody.master.inventory)
                                    {
                                        int boostDamageCount = ghostBody.master.inventory.GetItemCount(RoR2Content.Items.BoostDamage);
                                        if (boostDamageCount > 0)
                                        {
                                            ghostBody.master.inventory.RemoveItem(RoR2Content.Items.BoostDamage, ghostBody.master.inventory.GetItemCount(RoR2Content.Items.BoostDamage));
                                        }
                                        ghostBody.master.inventory.GiveItem(RoR2Content.Items.BoostDamage, 5);
                                        ghostBody.master.inventory.SetEquipmentIndex(EquipmentIndex.None);
                                    }
                                    ghostBody.AddBuff(reviveBuff);

                                    HauntTargetInfo hti = new HauntTargetInfo
                                    {
                                        body = ghostBody
                                    };
                                    ahr.attachedGhosts.Add(hti);
                                }
                                break;
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
            if (damageReport.victimBody
                && damageReport.victimBody.HasBuff(reviveBuff.buffIndex)
                && !damageReport.victimBody.disablingHurtBoxes
                && !damageReport.victimBody.HasBuff(RoR2Content.Buffs.AffixHaunted))
            {
                AttemptSpawnGhost(damageReport.victimBody, damageReport.damageInfo.position, AffixHauntedReviveAura.detachRadius * 1.1f);
            }
        }
        #endregion
    
        private static void StealBuffVFX()
        {
            IL.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                      x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "Cripple")
                     );
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, CharacterBody, bool>>((hasBuff, self) =>
                {
                    return hasBuff || self.HasBuff(armorReductionBuff);
                });

                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "Warbanner")
                    );
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, CharacterBody, bool>>((hasBuff, self) =>
                {
                    return hasBuff || self.HasBuff(ghostsActiveBuff);
                });
            };

            IL.RoR2.CharacterModel.UpdateOverlays += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "FullCrit")
                    );
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, CharacterModel, bool>>((hasBuff, self) =>
                {
                    return hasBuff || (self.body.HasBuff(reviveBuff));
                });
            };
        }

        private static void ChangeOnHitEffect()
        {
            if (!replaceOnHitEffect) return;

            //Remove vanilla effect
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AffixHaunted")
                    );
                c.Remove();
                c.Emit<EliteReworksPlugin>(OpCodes.Ldsfld, nameof(EliteReworksPlugin.EmptyBuff));
            };

            On.RoR2.GlobalEventManager.ProcessHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);
                if (NetworkServer.active && !damageInfo.rejected && damageInfo.procCoefficient > 0f && damageInfo.attacker && victim)
                {
                    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    if (attackerBody)
                    {
                        CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                        if (victimBody)
                        {
                            if (attackerBody.HasBuff(RoR2Content.Buffs.AffixHaunted))
                            {
                                victimBody.AddTimedBuff(AffixHaunted.armorReductionBuff, 3f);
                            }
                        }
                    }
                }
            };
        }
    }
}
