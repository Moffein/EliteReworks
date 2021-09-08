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

			On.RoR2.HealthComponent.TakeDamage += (On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) =>
			{
				if (NetworkServer.active && damageInfo.inflictor && damageInfo.inflictor.name == "FireTrail(Clone)")
                {
					damageInfo.crit = false;
					/*float maxDamage = self.fullCombinedHealth * fireTrailDamageCap;
					if (damageInfo.damage > self.fullCombinedHealth * fireTrailDamageCap)
                    {
						damageInfo.damage = maxDamage;
                    }*/
					damageInfo.procCoefficient = 0f;
					damageInfo.damage = self.fullCombinedHealth * fireTrailDamageCap;
				}
				orig(self, damageInfo);
			};
		}
    }
}
