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
                /*EliteAPI.VanillaEliteTiers[1].costMultiplier = t1Cost;
                EliteAPI.VanillaEliteTiers[1].healthBoostCoefficient = t1Health;
                EliteAPI.VanillaEliteTiers[1].damageBoostCoefficient = t1Damage;*/

                foreach (EliteDef e in EliteAPI.VanillaEliteTiers[1].eliteTypes)
                {
                    Debug.Log(e.name + " - HP " + e.healthBoostCoefficient + " - Damage " + e.damageBoostCoefficient +"\n");
                }

                //T2
                /*EliteAPI.VanillaEliteTiers[3].costMultiplier = t2Cost;
                EliteAPI.VanillaEliteTiers[3].healthBoostCoefficient = t2Health;
                EliteAPI.VanillaEliteTiers[3].damageBoostCoefficient = t2Damage;*/
            };
        }
    }
}
