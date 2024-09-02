using UnityEngine;
using RoR2;
using EliteReworks.Tweaks.T1.Components;
using EliteReworks.Tweaks.T2.Components;
using UnityEngine.Networking;
using EliteReworks.Tweaks;
using System;

namespace EliteReworks.SharedHooks
{
	//TODO: REFACTOR
    public static class AddBuff
    {
		public static void AddEliteComponents(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffIndex)
		{
			orig(self, buffIndex);
			if (NetworkServer.active && self.gameObject)
			{
				bool hasStunTracker = false;
				if (EliteReworksPlugin.affixRedEnabled && buffIndex == RoR2Content.Buffs.AffixRed.buffIndex)
				{
					if (self.HasBuff(RoR2Content.Buffs.AffixRed.buffIndex))
					{
						if (!self.gameObject.GetComponent<EliteStunTracker>())
						{
							self.gameObject.AddComponent<EliteStunTracker>();
						}
						hasStunTracker = true;
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
				if (EliteReworksPlugin.affixHauntedEnabled)
				{
					if (self.HasBuff(RoR2Content.Buffs.AffixHaunted.buffIndex) && buffIndex == RoR2Content.Buffs.AffixHaunted.buffIndex)
					{
						if (!self.gameObject.GetComponent<AffixHauntedReviveAura>())
						{
							self.gameObject.AddComponent<AffixHauntedReviveAura>();
						}
					}
				}
				if (EliteReworksPlugin.eliteEarthEnabled && !hasStunTracker)
				{
					if (!self.gameObject.GetComponent<EliteStunTracker>())
					{
						self.gameObject.AddComponent<EliteStunTracker>();
					}
					hasStunTracker = true;
				}
			}
        }
    }
}
