using System;
using System.Collections.Generic;
using System.Text;
using EliteReworks.Tweaks.T1.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T1
{
    public static class AffixRed
    {
		public static float fireTrailBaseDamage = 21f * 0.2f;   //18f is same as lemurian

		public static void Setup()
        {
			FixBurnDamage();
			//Make Stun/Shock/Freeze disable the fire trail.
            On.RoR2.CharacterBody.UpdateFireTrail += (On.RoR2.CharacterBody.orig_UpdateFireTrail orig, CharacterBody self) =>
			{
				bool blazing = self.HasBuff(RoR2Content.Buffs.AffixRed);
				bool disableFireTrail = false;
				AffixRedStunTracker ars = null;

				if (blazing)
				{
					ars = self.gameObject.GetComponent<AffixRedStunTracker>();
					if (!ars)
                    {
						self.gameObject.AddComponent<AffixRedStunTracker>();
                    }

					if (self.healthComponent && self.healthComponent.isInFrozenState)
					{
						disableFireTrail = true;
						ars.SetStun();
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
								ars.SetStun();
							}
						}
					}
				}

				orig(self);


				if (blazing && self.fireTrail)
				{
					if (disableFireTrail || !(ars && ars.PassiveActive()))
					{
						UnityEngine.Object.Destroy(self.fireTrail.gameObject);
						self.fireTrail = null;
					}
				}
			};
		}

		public static void FixBurnDamage()
        {
			IL.RoR2.GlobalEventManager.OnHitEnemy += (il) =>
			{
				ILCursor c = new ILCursor(il);
				c.GotoNext(
						 x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AffixRed")
						);
				c.GotoNext(MoveType.After,
						 x => x.MatchStfld<InflictDotInfo>("maxStacksFromAttacker"),
						 x => x.MatchLdloc(30)
						);
				//dotinfo on stack
				c.Emit(OpCodes.Ldarg_1);//damageinfo
				c.Emit(OpCodes.Ldloc_2);//VictimBody
				c.EmitDelegate<Func<InflictDotInfo, DamageInfo, CharacterBody, InflictDotInfo>>((dotInfo, damageInfo, victimBody) =>
				{
					if (victimBody.healthComponent && victimBody.teamComponent && victimBody.teamComponent.teamIndex == TeamIndex.Player && dotInfo.totalDamage != null)
                    {
						float burnPercent = damageInfo.procCoefficient * 0.2f;	//This assumes old burn was 5% hp damage per second. Might need to doublecheck
						float totalBurnDamage = burnPercent * victimBody.healthComponent.fullCombinedHealth;
						dotInfo.totalDamage = totalBurnDamage;//Mathf.Min((float)dotInfo.totalDamage, totalBurnDamage);
                    }
					return dotInfo;
				});
			};
        }
    }
}
