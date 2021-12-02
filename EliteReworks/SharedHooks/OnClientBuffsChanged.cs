using UnityEngine;
using RoR2;
using EliteReworks.Tweaks.T1.Components;
using EliteReworks.Tweaks.T2.Components;
using UnityEngine.Networking;

namespace EliteReworks.SharedHooks
{
    public static class OnClientBuffsChanged
    {
		public static void AddEliteComponents(On.RoR2.CharacterBody.orig_OnClientBuffsChanged orig, CharacterBody self)
		{
			orig(self);
			if (NetworkServer.active && self.gameObject)
			{
				if (EliteReworksPlugin.affixBlueEnabled)
				{
					if (self.HasBuff(RoR2Content.Buffs.AffixBlue.buffIndex) && !self.gameObject.GetComponent<AffixBluePassiveLightning>())
					{
						self.gameObject.AddComponent<AffixBluePassiveLightning>();
					}
				}
				if (EliteReworksPlugin.affixPoisonEnabled)
				{
					if (self.HasBuff(RoR2Content.Buffs.AffixPoison.buffIndex) && !self.gameObject.GetComponent<AffixPoisonDebuffAura>())
					{
						self.gameObject.AddComponent<AffixPoisonDebuffAura>();
					}
				}
				if (self.HasBuff(RoR2Content.Buffs.AffixHaunted.buffIndex))
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
