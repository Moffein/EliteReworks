using RoR2;
using UnityEngine;
using R2API;

namespace EliteReworks.Tweaks
{
    public static class ModifyEliteTiers
    {
        public static float t1Cost = 4.5f;
        public static float t1Health = 3f;
        public static float t1Damage = 1.5f;

        public static float t2Cost = 36f;
        public static float t2Health = 14f;
        public static float t2Damage = 3.5f;

        public static void Setup()
        {
            On.RoR2.CombatDirector.Init += (orig) =>
            {
                orig();
            };
        }
    }
}
