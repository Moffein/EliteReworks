using EliteReworks.Tweaks.T2;
using RoR2;

namespace EliteReworks.SharedHooks
{
    public class RecalculateStats
    {
        public static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(AffixHaunted.reviveBuff))
            {
                if (self.skillLocator)
                {
                    if (self.skillLocator.primary)
                    {
                        self.skillLocator.primary.cooldownScale *= 0.75f;
                    }
                    if (self.skillLocator.secondary)
                    {
                        self.skillLocator.secondary.cooldownScale *= 0.75f;
                    }
                    if (self.skillLocator.utility)
                    {
                        self.skillLocator.utility.cooldownScale *= 0.75f;
                    }
                    if (self.skillLocator.special)
                    {
                        self.skillLocator.special.cooldownScale *= 0.75f;
                    }
                }
            }
        }
    }
}
