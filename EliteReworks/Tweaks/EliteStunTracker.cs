using UnityEngine;

namespace EliteReworks.Tweaks
{
    public class EliteStunTracker : MonoBehaviour
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
