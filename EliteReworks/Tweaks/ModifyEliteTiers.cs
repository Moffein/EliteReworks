using RoR2;
using UnityEngine;
using R2API;
using static RoR2.CombatDirector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;

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

        public static float tGildedCost = 6.75f;
        public static float tGildedHealth = 4.5f;
        public static float tGildedDamage = 2.3f;

        public static float t2Cost = 36f;
        public static float t2Health = 12f;
        public static float t2Damage = 3.5f;
        public static float t2HealthTwisted = 12f;
        public static float t2DamageTwisted = 3.5f;
        public static int t2MinStages = 5;

        public static void Setup()
        {
            On.RoR2.CombatDirector.Init += (orig) =>
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

                if (EliteReworksPlugin.affixGildedFixTier)
                {
                    EliteTierDef tierToFix = EliteAPI.VanillaEliteTiers[3];

                    List<EliteDef> toRemove = new List<EliteDef>()
                    {
                        RoR2Content.Elites.Fire,
                        RoR2Content.Elites.Lightning,
                        RoR2Content.Elites.Ice,
                        DLC1Content.Elites.Earth
                    };

                    //Remove what shouldn't be there, in case other mods add to this tier.
                    List<EliteDef> eliteList = tierToFix.eliteTypes.ToList();
                    foreach (EliteDef elite in toRemove)
                    {
                        eliteList.Remove(elite);
                    }
                    tierToFix.eliteTypes = eliteList.ToArray();
                }

                EliteTierDef t1Tier = EliteAPI.VanillaEliteTiers[1];
                t1Tier.costMultiplier = t1Cost;
                foreach (EliteDef ed in t1Tier.eliteTypes)
                {
                    if (ed != null) ApplyT1Scaling(ed);
                }

                EliteTierDef t1HonorTier = EliteAPI.VanillaEliteTiers[2];
                t1HonorTier.costMultiplier = t1HonorCost;
                foreach (EliteDef ed in t1HonorTier.eliteTypes)
                {
                    if (ed != null) ApplyT1HonorScaling(ed);
                }

                EliteTierDef t2Tier = EliteAPI.VanillaEliteTiers[4];
                if (t2MinStages != 5) t2Tier.isAvailable = (eliteRules) =>
                {
                    return Run.instance && Run.instance.stageClearCount > t2MinStages;
                };
                t2Tier.costMultiplier = t2Cost;
                foreach (EliteDef ed in t2Tier.eliteTypes)
                {
                    if (ed != null) ApplyT2Scaling(ed);
                }

                EliteTierDef gildedTier = EliteAPI.VanillaEliteTiers[3];
                gildedTier.costMultiplier = tGildedCost;
                DLC2Content.Elites.Aurelionite.damageBoostCoefficient = tGildedDamage;
                DLC2Content.Elites.Aurelionite.healthBoostCoefficient = tGildedHealth;
            };
        }

        public static void ApplyT1Scaling(EliteDef ed)
        {
            ed.damageBoostCoefficient = t1Damage;
            ed.healthBoostCoefficient = t1Health;
            if (ed == DLC1Content.Elites.Earth)
            {
                ed.healthBoostCoefficient = t1HealthEarth;
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
        }

        public static void ApplyT2Scaling(EliteDef ed)
        {
            ed.damageBoostCoefficient = t2Damage;
            ed.healthBoostCoefficient = t2Health;
            if (ed == DLC2Content.Elites.Bead)
            {
                ed.healthBoostCoefficient = t2HealthTwisted;
                ed.damageBoostCoefficient = t2DamageTwisted;
            }
        }
    }
}
