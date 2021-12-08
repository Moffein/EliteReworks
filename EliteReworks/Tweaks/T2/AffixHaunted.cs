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

        public static void Setup()
        {
            reviveBuff = CreateReviveBuff();
            ghostsActiveBuff = CreateGhostsActiveBuff();
            armorReductionBuff = CreateArmorReductionBuff();

            if (EliteReworksPlugin.affixHauntedEnabled)
            {
                DisableBubble();
                On.RoR2.GlobalEventManager.OnCharacterDeath += ReviveAsGhost;
                StealBuffVFX();
                ChangeOnHitEffect();
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

        public static BuffDef CreateGhostsActiveBuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.buffColor = new Color(210f / 255f, 50f / 255f, 22f / 255f);
            buff.canStack = false;
            buff.isDebuff = false;
            buff.name = "EliteReworksHauntedGhostsActive";
            buff.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffDeathMarkIcon");
            BuffAPI.Add(new CustomBuff(buff));
            return buff;
        }

        public static BuffDef CreateArmorReductionBuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.buffColor = new Color(132f / 255f, 214f / 255f, 236f / 255f);
            buff.canStack = false;
            buff.isDebuff = true;
            buff.name = "EliteReworksHauntedArmorReduction";
            buff.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffCrippleIcon");
            BuffAPI.Add(new CustomBuff(buff));
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
                if (teamMembers != null)
                {
                    foreach (TeamComponent tc in teamMembers)
                    {
                        if (tc.body
                            && (tc.body.bodyFlags & CharacterBody.BodyFlags.Masterless) != CharacterBody.BodyFlags.Masterless
                            && tc.body.healthComponent && tc.body.healthComponent.alive)
                        {
                            if (tc.body.HasBuff(RoR2Content.Buffs.AffixHaunted))
                            {
                                squareDist = (position - tc.body.corePosition).sqrMagnitude;
                                if (squareDist < radiusSquare)
                                {
                                    AffixHauntedReviveAura ahr = tc.body.GetComponent<AffixHauntedReviveAura>();
                                    if (ahr && ahr.wardActive && ahr.attachedGhosts.Count < AffixHauntedReviveAura.maxAttachedGhosts)
                                    {
                                        CharacterBody ghostBody = Util.TryToCreateGhost(sourceBody, tc.body, 30);
                                        if (ghostBody)
                                        {
                                            if (ghostBody.master && ghostBody.master.inventory)
                                            {
                                                ghostBody.master.inventory.RemoveItem(RoR2Content.Items.BoostDamage, ghostBody.master.inventory.GetItemCount(RoR2Content.Items.BoostDamage));
                                                ghostBody.master.inventory.SetEquipmentIndex(EquipmentIndex.None);
                                            }
                                            ghostBody.AddBuff(reviveBuff);
                                            ahr.attachedGhosts.Add(ghostBody);
                                        }
                                        break;
                                    }
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

            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
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
