using RoR2;
using UnityEngine;
using R2API;
using EliteReworks.Tweaks.T1;
using EliteReworks.Tweaks.T2;

namespace EliteReworks.SharedHooks
{
    public class GetStatCoefficients
    {
        public static void Hook(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(AffixWhite.slow80alt) && !sender.HasBuff(RoR2Content.Buffs.Slow80))
            {
                args.moveSpeedReductionMultAdd += 0.8f;
            }
            if (sender.HasBuff(AffixHaunted.reviveBuff))
            {
                args.moveSpeedMultAdd += 0.7f;
                args.attackSpeedMultAdd += 0.5f;
            }
            if (sender.HasBuff(AffixHaunted.armorReductionBuff))
            {
                args.armorAdd -= 20f;
            }
        }
    }
}
