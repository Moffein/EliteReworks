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
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(DLC1Content.Buffs), "EliteVoid")
                    );
                c.Remove();
                c.Emit<EliteReworksPlugin>(OpCodes.Ldsfld, nameof(EliteReworksPlugin.EmptyBuff));
            };

            //Effect handled in TakeDamage

            //Remove damage penalty
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender.HasBuff(DLC1Content.Buffs.EliteVoid))
                {
                    args.damageMultAdd += 0.3f + (EliteVoid.damageBonus - 1f);
                }
            };
            
            if (tweakNullify)
            {
                On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += (orig, self, buffDef, duration) =>
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
                };
            }
        }
    }
}
