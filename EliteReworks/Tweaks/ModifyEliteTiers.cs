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

                //T1
                EliteAPI.VanillaEliteTiers[1].costMultiplier = 4.5f;
                EliteAPI.VanillaEliteTiers[1].healthBoostCoefficient = 3f;
                EliteAPI.VanillaEliteTiers[1].damageBoostCoefficient = 1.5f;

                //T2
                EliteAPI.VanillaEliteTiers[3].costMultiplier = 36f;
                EliteAPI.VanillaEliteTiers[3].healthBoostCoefficient = 14f;
                EliteAPI.VanillaEliteTiers[3].damageBoostCoefficient = 3.5f;
            };
        }
    }
}
