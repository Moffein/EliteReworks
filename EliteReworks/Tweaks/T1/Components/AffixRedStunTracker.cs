using UnityEngine;
using RoR2;

namespace EliteReworks.Tweaks.T1.Components
{
    public class AffixRedStunTracker : MonoBehaviour
    {
        private float stunDisableTimer = 0f;

        public void FixedUpdate()
        {
            if (stunDisableTimer > 0f)
            {
                stunDisableTimer -= Time.fixedDeltaTime;
            }
        }

        public bool PassiveActive()
        {
            return stunDisableTimer <= 0f;
        }

        public void SetStun()
        {
            stunDisableTimer = EliteReworksPlugin.eliteStunDisableDuration;
        }
    }
}
