using UnityEngine;
using RoR2;
using EliteReworks.Tweaks.T1.Components;
using EliteReworks.Tweaks.T2.Components;

namespace EliteReworks.Tweaks.SharedHooks
{
    public static class OnClientBuffsChanged
    {
		public static void AddEliteComponents(On.RoR2.CharacterBody.orig_OnClientBuffsChanged orig, CharacterBody self)
		{
			orig(self);
			if (self.gameObject)
			{
				if (EliteReworksPlugin.affixBlueEnabled && self.HasBuff(RoR2Content.Buffs.AffixBlue.buffIndex) && !self.gameObject.GetComponent<AffixBluePassiveLightning>())
				{
					self.gameObject.AddComponent<AffixBluePassiveLightning>();
				}
				if (EliteReworksPlugin.affixPoisonEnabled && self.HasBuff(RoR2Content.Buffs.AffixPoison.buffIndex) && !self.gameObject.GetComponent<AffixPoisonDebuffAura>())
				{
					self.gameObject.AddComponent<AffixPoisonDebuffAura>();
				}
				/*if (EliteReworksPlugin.affixHauntedEnabled && self.HasBuff(RoR2Content.Buffs.AffixHaunted.buffIndex) && !self.gameObject.GetComponent<AffixHauntedReviver>())
				{
					self.gameObject.AddComponent<AffixHauntedReviver>();
				}*/
			}
		}
	}
}
