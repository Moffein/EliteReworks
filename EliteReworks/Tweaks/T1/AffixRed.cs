using System;
using System.Collections.Generic;
using System.Text;
using EliteReworks.Tweaks.T1.Components;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T1
{
    public static class AffixRed
    {
		public static float fireTrailBaseDamage = 21f * 0.2f;	//18f is same as lemurian

		public static void Setup()
        {
			//Make Stun/Shock/Freeze disable the fire trail.
            On.RoR2.CharacterBody.UpdateFireTrail += (On.RoR2.CharacterBody.orig_UpdateFireTrail orig, CharacterBody self) =>
			{
				bool blazing = self.HasBuff(RoR2Content.Buffs.AffixRed);
				bool disableFireTrail = false;

				if (blazing)
				{
					if (self.healthComponent && self.healthComponent.isInFrozenState)
					{
						disableFireTrail = true;
					}
					else
					{
						SetStateOnHurt ssoh = self.gameObject.GetComponent<SetStateOnHurt>();
						if (ssoh)
						{
							Type state = ssoh.targetStateMachine.state.GetType();
							if (state == typeof(EntityStates.StunState) || state == typeof(EntityStates.ShockState))
							{
								disableFireTrail = true;
							}
						}
					}
				}

				orig(self);


				if (blazing && disableFireTrail && self.fireTrail)
				{
					UnityEngine.Object.Destroy(self.fireTrail.gameObject);
					self.fireTrail = null;
				}
			};
		}
    }
}
