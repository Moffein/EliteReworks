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
            if (sender.HasBuff(AffixWhite.slow80alt))
            {
                args.moveSpeedReductionMultAdd += 0.8f;
            }
            if (sender.HasBuff(AffixHaunted.reviveBuff) || sender.HasBuff(AffixHaunted.ghostsActiveBuff))
            {
                args.moveSpeedMultAdd += 0.3f;
                args.attackSpeedMultAdd += 0.3f;
            }
        }
    }
}
