using UnityEngine;
using RoR2;
using EliteReworks.Tweaks.T1.Components;
using EliteReworks.Tweaks.T2.Components;
using UnityEngine.Networking;

namespace EliteReworks.SharedHooks
{
    public static class AddBuff
    {
		public static void AddEliteComponents(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffIndex)
		{
			orig(self, buffIndex);
			if (NetworkServer.active && self.gameObject)
			{
				if (EliteReworksPlugin.affixRedEnabled && buffIndex == RoR2Content.Buffs.AffixRed.buffIndex)
				{
					if (self.HasBuff(RoR2Content.Buffs.AffixRed.buffIndex) && !self.gameObject.GetComponent<AffixRedStunTracker>())
					{
						self.gameObject.AddComponent<AffixRedStunTracker>();
					}
				}
				if (EliteReworksPlugin.affixBlueEnabled && buffIndex == RoR2Content.Buffs.AffixBlue.buffIndex)
				{
					if (self.HasBuff(RoR2Content.Buffs.AffixBlue.buffIndex) && !self.gameObject.GetComponent<AffixBluePassiveLightning>())
					{
						self.gameObject.AddComponent<AffixBluePassiveLightning>();
					}
				}
				if (EliteReworksPlugin.affixPoisonEnabled && buffIndex == RoR2Content.Buffs.AffixPoison.buffIndex)
				{
					if (self.HasBuff(RoR2Content.Buffs.AffixPoison.buffIndex) && !self.gameObject.GetComponent<AffixPoisonDebuffAura>())
					{
						self.gameObject.AddComponent<AffixPoisonDebuffAura>();
					}
				}
				if (self.HasBuff(RoR2Content.Buffs.AffixHaunted.buffIndex) && buffIndex == RoR2Content.Buffs.AffixHaunted.buffIndex)
				{
					if (EliteReworksPlugin.affixHauntedEnabled)
					{
						if (!self.gameObject.GetComponent<AffixHauntedReviveAura>())
						{
							self.gameObject.AddComponent<AffixHauntedReviveAura>();
						}
					}
				}
			}
		}
	}
}
