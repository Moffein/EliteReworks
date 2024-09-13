using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.DLC2
{
    public static class AffixGilded
    {
        public static void Setup()
        {
            //Allow bubble to be interrupted.
            On.RoR2.AffixAurelioniteBehavior.Start += AffixAurelioniteBehavior_Start;
            On.RoR2.AffixAurelioniteBehavior.Update += AffixAurelioniteBehavior_Update;

            //Rewrite buff to scale with time costs properly.
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            //Stop minions from feeding
            //In the original code, Minions don't actually lose money, but they still cause Gilded Elites to gain money.
            IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
        }

        private static void HealthComponent_TakeDamageProcess(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After, x => x.MatchLdsfld(typeof(DLC2Content.Buffs), "AurelioniteBlessing"));
            c.Emit(OpCodes.Ldarg_0);    //self
            c.Emit(OpCodes.Ldloc_1);    //attackerBody
            c.Emit(OpCodes.Ldarg_1);    //damageInfo
            c.EmitDelegate<Func<BuffDef, HealthComponent, CharacterBody, DamageInfo, BuffDef>>((buffDef, self, attackerBody, damageInfo) =>
            {
                if (damageInfo.procCoefficient <= 0f) return null;
                bool victimIsPlayer = self.body && self.body.isPlayerControlled;
                bool attackerIsPlayer = attackerBody && attackerBody.isPlayerControlled;
                return (victimIsPlayer || attackerIsPlayer) ? buffDef : null;
            });
        }

        private static void AffixAurelioniteBehavior_Update(On.RoR2.AffixAurelioniteBehavior.orig_Update orig, AffixAurelioniteBehavior self)
        {
            bool isStunned = false;
            if (NetworkServer.active)
            {
                EliteStunTracker stunTracker = self.GetComponent<EliteStunTracker>();
                if (stunTracker && stunTracker.PassiveActive() && self.body)
                {
                    if (self.body.healthComponent && self.body.healthComponent.isInFrozenState)
                    {
                        isStunned = true;
                        stunTracker.SetStun();
                    }
                    else
                    {
                        SetStateOnHurt ssoh = self.gameObject.GetComponent<SetStateOnHurt>();
                        if (ssoh)
                        {
                            Type state = ssoh.targetStateMachine.state.GetType();
                            if (state == typeof(EntityStates.StunState) || state == typeof(EntityStates.ShockState))
                            {
                                isStunned = true;
                                stunTracker.SetStun();
                            }
                        }
                    }
                }
                isStunned = stunTracker && !stunTracker.PassiveActive()
                    || self.body && self.body.healthComponent && !self.body.healthComponent.alive;
                if (isStunned)
                {
                    //self.timer = self.duration;
                    self.body.ClearTimedBuffs(DLC2Content.Buffs.AurelioniteBlessing);
                }
            }
            if (!isStunned) orig(self);
        }

        private static void AffixAurelioniteBehavior_Start(On.RoR2.AffixAurelioniteBehavior.orig_Start orig, AffixAurelioniteBehavior self)
        {
            orig(self);
            
            EliteStunTracker est = self.GetComponent<EliteStunTracker>();
            if (!est) est = self.gameObject.AddComponent<EliteStunTracker>();
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender.HasBuff(DLC2Content.Buffs.AurelioniteBlessing)) return;
            float bonusArmor = 60f;

            if (sender.master && Run.instance)
            {
                //orig is just flat 0.1 * money
                //this one gives more reduction earlygame, but the general reduction remains consistent
                float difficultyCoefficient = Run.instance.difficultyCoefficient;
                if (Stage.instance) difficultyCoefficient = Stage.instance.entryDifficultyCoefficient;
                bonusArmor += 25f * ((float)sender.master.money / (float)Run.instance.GetDifficultyScaledCost(25, difficultyCoefficient));
            }

            args.armorAdd += bonusArmor;
        }

        private static void CharacterBody_RecalculateStats(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                 x => x.MatchLdsfld(typeof(DLC2Content.Buffs), "AurelioniteBlessing")
                );
            c.Remove();
            c.Emit<EliteReworksPlugin>(OpCodes.Ldsfld, nameof(EliteReworksPlugin.EmptyBuff));
        }
    }
}
