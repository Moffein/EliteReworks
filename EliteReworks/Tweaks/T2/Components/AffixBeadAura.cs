using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace EliteReworks.Tweaks.T2.Components
{
    [DisallowMultipleComponent]
    public class AffixBeadAura : MonoBehaviour
    {
        public static float wardRadius = 20f;
        public static float refreshTime = 3f;

        private float stopwatch;
        private CharacterBody ownerBody;
        private TeamComponent teamComponent;

        public bool wardActive = false;

        private float stunDisableTimer = 0f;

        public void Awake()
        {
            wardActive = false;

            ownerBody = base.gameObject.GetComponent<CharacterBody>();
            if (!ownerBody)
            {
                Destroy(this);
            }
            teamComponent = ownerBody.teamComponent;
        }

        public void FixedUpdate()
        {
            wardActive = stunDisableTimer <= 0f;
            bool eliteActive = ownerBody && ownerBody.HasBuff(DLC2Content.Buffs.EliteBead) && ownerBody.healthComponent && ownerBody.healthComponent.alive;
        
            if (!eliteActive)
            {

            }
        }
    }
}
