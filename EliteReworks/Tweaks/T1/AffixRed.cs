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
		public static float fireTrailDamageCap = 0.05f;	//Max HP% damage that fire trails can deal

		public static void Setup()
        {
			//Make Stun/Shock/Freeze disable the fire trail.
            On.RoR2.CharacterBody.UpdateFireTrail += (On.RoR2.CharacterBody.orig_UpdateFireTrail orig, CharacterBody self) =>
            {
				bool disableFireTrail = false;
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
				orig(self);
				if (disableFireTrail && self.fireTrail)
				{
					UnityEngine.Object.Destroy(self.fireTrail.gameObject);
					self.fireTrail = null;
				}
			};
		}
    }
}
