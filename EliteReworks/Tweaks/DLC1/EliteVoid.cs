using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace EliteReworks.Tweaks.DLC1
{
    public class EliteVoid
    {
        public static float damageBonus = 1.5f;
        public static void Setup()
        {
            //Remove vanilla on-hit effect
            IL.RoR2.GlobalEventManager.OnHitEnemy += (il) =>
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
        }
    }
}
