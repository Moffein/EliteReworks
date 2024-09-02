using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.T2
{
    public static class AffixBead
    {
        public static void Setup()
        {
            On.RoR2.AffixBeadBehavior.OnEnable += AffixBeadBehavior_OnEnable;
            On.RoR2.AffixBeadBehavior.Update += AffixBeadBehavior_Update;
        }

        private static void AffixBeadBehavior_Update(On.RoR2.AffixBeadBehavior.orig_Update orig, RoR2.AffixBeadBehavior self)
        {
            bool isStunned = false;
            if (NetworkServer.active)
            {
                EliteStunTracker stunTracker = self.GetComponent<EliteStunTracker>();
                if (stunTracker && stunTracker.PassiveActive() && self.body)
                {
                    if (self.body.healthComponent && self.body.healthComponent.isInFrozenState)
                    {
                        isStunned = true;
                        stunTracker.SetStun();
                    }
                    else
                    {
                        SetStateOnHurt ssoh = self.gameObject.GetComponent<SetStateOnHurt>();
                        if (ssoh)
                        {
                            Type state = ssoh.targetStateMachine.state.GetType();
                            if (state == typeof(EntityStates.StunState) || state == typeof(EntityStates.ShockState))
                            {
                                isStunned = true;
                                stunTracker.SetStun();
                            }
                        }
                    }
                }
                isStunned = stunTracker && !stunTracker.PassiveActive();
                if (isStunned && self.affixBeadWard)
                {
                    UnityEngine.Object.Destroy(self.affixBeadWard);
                }
            }
            if (!isStunned) orig(self);
        }

        private static void AffixBeadBehavior_OnEnable(On.RoR2.AffixBeadBehavior.orig_OnEnable orig, RoR2.AffixBeadBehavior self)
        {
            orig(self);
            //todo: just replace affixBeadWard

            EliteStunTracker est = self.GetComponent<EliteStunTracker>();
            if (!est) est = self.gameObject.AddComponent<EliteStunTracker>();
        }
    }
}
