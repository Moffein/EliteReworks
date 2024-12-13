using RoR2;
using UnityEngine;
using R2API;
using static RoR2.CombatDirector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using System;

namespace EliteReworks.Tweaks
{
    public static class ModifyEliteTiers
    {
        public static float t1Cost = 4.5f;
        public static float t1Health = 3f;
        public static float t1HealthEarth = 3f;
        public static float t1Damage = 1.5f;

        public static float t1HonorCost = 3.5f;
        public static float t1HonorHealth = 2.5f;
        public static float t1HonorHealthEarth = 1.5f;
        public static float t1HonorDamage = 1.5f;

        public static float tGildedHealth = 4f;
        public static float tGildedDamage = 2f;

        public static float tGildedHonorHealth = 3.5f;
        public static float tGildedHonorDamage = 2f;

        public static float t2Cost = 36f;
        public static float t2Health = 12f;
        public static float t2Damage = 3.5f;
        public static int t2MinStages = 5;

        public static void Setup()
        {
            On.RoR2.CombatDirector.Init += CombatDirector_Init;
        }

        private static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();

            //Dump Elite info
            /*Debug.Log("\n\n\n\n\nEliteReworks: Dumping Elite Tiers");
            for (int i = 0; i < EliteAPI.VanillaEliteTiers.Length; i++)
            {
                EliteTierDef etd = EliteAPI.VanillaEliteTiers[i];
                if (etd != null)
                {
                    Debug.Log("[" + i + "] Tier Cost: " + etd.costMultiplier);
                    foreach (EliteDef ed in etd.eliteTypes)
                    {
                        if (ed != null)
                        {
                            Debug.Log(ed.eliteEquipmentDef.name + " - " + ed.healthBoostCoefficient + "x health, " + ed.damageBoostCoefficient + "x damage");
                        }
                    }
                    Debug.Log("\n");
                }
            }*/

            EliteTierDef t1Tier = EliteAPI.VanillaEliteTiers[1];
            t1Tier.costMultiplier = t1Cost;
            foreach (EliteDef ed in t1Tier.eliteTypes)
            {
                ApplyT1Scaling(ed);
            }

            EliteTierDef t1HonorTier = EliteAPI.VanillaEliteTiers[2];
            t1HonorTier.costMultiplier = t1HonorCost;
            foreach (EliteDef ed in t1HonorTier.eliteTypes)
            {
                ApplyT1HonorScaling(ed);
            }

            EliteTierDef t1GildedHonorTier = EliteAPI.VanillaEliteTiers[3];
            t1HonorTier.costMultiplier = t1HonorCost;
            foreach (EliteDef ed in t1HonorTier.eliteTypes)
            {
                ApplyT1HonorScaling(ed);
            }

            EliteTierDef t1GildedTier = EliteAPI.VanillaEliteTiers[4];
            t1GildedTier.costMultiplier = t1Cost;
            foreach (EliteDef ed in t1GildedTier.eliteTypes)
            {
                ApplyT1Scaling(ed);
            }

            EliteTierDef t2Tier = EliteAPI.VanillaEliteTiers[5];
            if (t2MinStages != 5) t2Tier.isAvailable = (eliteRules) =>
            {
                return Run.instance && Run.instance.stageClearCount >= t2MinStages;
            };
            t2Tier.costMultiplier = t2Cost;
            foreach (EliteDef ed in t2Tier.eliteTypes)
            {
                if (ed != null) ApplyT2Scaling(ed);
            }
        }

        public static void ApplyT1Scaling(EliteDef ed)
        {
            ed.damageBoostCoefficient = t1Damage;
            ed.healthBoostCoefficient = t1Health;
            if (ed == DLC1Content.Elites.Earth)
            {
                ed.healthBoostCoefficient = t1HealthEarth;
            }
            else if (ed == DLC2Content.Elites.Aurelionite)
            {
                ed.healthBoostCoefficient = tGildedHealth;
                ed.damageBoostCoefficient = tGildedDamage;
            }
        }

        public static void ApplyT1HonorScaling(EliteDef ed)
        {
            ed.damageBoostCoefficient = t1HonorDamage;
            ed.healthBoostCoefficient = t1HonorHealth;
            if (ed == DLC1Content.Elites.EarthHonor)
            {
                ed.healthBoostCoefficient = t1HonorHealthEarth;
            }
            else if (ed == DLC2Content.Elites.AurelioniteHonor)
            {
                ed.healthBoostCoefficient = tGildedHonorHealth;
                ed.damageBoostCoefficient = tGildedHonorDamage;
            }
        }

        public static void ApplyT2Scaling(EliteDef ed)
        {
            ed.damageBoostCoefficient = t2Damage;
            ed.healthBoostCoefficient = t2Health;
        }
    }
}
