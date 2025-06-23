using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.DLC1
{
    public class EliteVoid
    {
        public static float damageBonus = 1.5f;
        public static bool tweakNullify = true;
        public static void Setup()
        {
            //Remove vanilla on-hit effect
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += RemoveVanillaVoidEliteCollapse;

            //Effect handled in TakeDamage

            //Remove damage penalty
            RecalculateStatsAPI.GetStatCoefficients += AdjustVanillaVoidEliteStats;
            
            if (tweakNullify)
            {
                On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += TweakNullify;
            }
        }

        private static void TweakNullify(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            orig(self, buffDef, duration);
            if (NetworkServer.active)
            {
                if (buffDef == RoR2Content.Buffs.NullifyStack && !self.HasBuff(RoR2Content.Buffs.Nullified))
                {
                    int nullifyCount = self.GetBuffCount(buffDef);
                    if (nullifyCount >= 2)
                    {
                        self.ClearTimedBuffs(buffDef);
                        self.AddTimedBuff(RoR2Content.Buffs.Nullified, 3f);
                    }
                }
            }
        }

        private static void AdjustVanillaVoidEliteStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(DLC1Content.Buffs.EliteVoid))
            {
                args.damageMultAdd += 0.3f + (EliteVoid.damageBonus - 1f);
            }
        }

        private static void RemoveVanillaVoidEliteCollapse(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                 x => x.MatchLdsfld(typeof(DLC1Content.Buffs), "EliteVoid")
                ))
            {
                c.EmitDelegate<Func<BuffDef, BuffDef>>(orig => null);
            }
            else
            {
                Debug.LogError("EliteReworks: RemoveVanillaPoisonOnHit IL hook failed.");
            }
        }
    }
}
