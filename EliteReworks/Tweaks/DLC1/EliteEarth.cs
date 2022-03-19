using RoR2;
using System;

namespace EliteReworks.Tweaks.DLC1
{
    public class EliteEarth
    {
        public static void Setup()
        {
            On.RoR2.AffixEarthBehavior.FixedUpdate += (orig, self) =>
            {
                bool disableAffix = false;
                EliteStunTracker stunTracker = null;
                stunTracker = self.gameObject.GetComponent<EliteStunTracker>();
                if (!stunTracker)
                {
                    stunTracker = self.gameObject.AddComponent<EliteStunTracker>();
                }

                if (self.body)
                {
                    if (self.body.healthComponent && self.body.healthComponent.isInFrozenState)
                    {
                        disableAffix = true;
                        stunTracker.SetStun();
                    }
                    else
                    {
                        SetStateOnHurt ssoh = self.body.gameObject.GetComponent<SetStateOnHurt>();
                        if (ssoh)
                        {
                            Type state = ssoh.targetStateMachine.state.GetType();
                            if (state == typeof(EntityStates.StunState) || state == typeof(EntityStates.ShockState))
                            {
                                disableAffix = true;
                                stunTracker.SetStun();
                            }
                        }
                    }
                }

                orig(self);

                if (self.affixEarthAttachment) 
                {
                    if (disableAffix || (stunTracker && !stunTracker.PassiveActive()))
                    {
                        UnityEngine.Object.Destroy(self.affixEarthAttachment);
                        self.affixEarthAttachment = null;
                    }
                }
            };
        }
    }
}
