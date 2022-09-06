using RoR2;
using UnityEngine;
using R2API;
using static RoR2.CombatDirector;
using System.Collections.Generic;
using System.Linq;

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

        public static float t2Cost = 36f;
        public static float t2Health = 12f;
        public static float t2Damage = 3.5f;
        public static int t2MinStages = 5;

        public static void Setup()
        {
            On.RoR2.CombatDirector.Init += (orig) =>
            {
                orig();

                //Seems inefficient, but all the array sizes are small so there's not too much searching hopefully
                foreach(EliteTierDef etd in CombatDirector.eliteTiers)
                {
                    if (etd != null)
                    {
                        if (etd.eliteTypes.Length > 0)
                        {
                            List<EliteDef> elites = etd.eliteTypes.ToList();
                            if (elites.Contains(RoR2Content.Elites.Lightning))
                            {
                                etd.costMultiplier = t1Cost;
                                foreach(EliteDef ed in etd.eliteTypes)
                                {
                                    if (ed != null) ApplyT1Scaling(ed);
                                }
                            }
                            else if (elites.Contains(RoR2Content.Elites.LightningHonor))
                            {
                                etd.costMultiplier = t1HonorCost;
                                foreach (EliteDef ed in etd.eliteTypes)
                                {
                                    if (ed != null) ApplyT1HonorScaling(ed);
                                }
                            }
                            else if (elites.Contains(RoR2Content.Elites.Poison))
                            {
                                etd.isAvailable = ((SpawnCard.EliteRules rules) => Run.instance.stageClearCount >= t2MinStages && rules == SpawnCard.EliteRules.Default);    //checks Run.instance.loopCount in Vanilla
                                etd.costMultiplier = t2Cost;
                                foreach (EliteDef ed in etd.eliteTypes)
                                {
                                    if (ed != null) ApplyT2Scaling(ed);
                                }
                            }
                        }
                    }
                }

                //Dump Elite info
                /*Debug.Log("\n\n\n\n\nEliteReworks: Modifying Elite Tiers");
                for (int i = 0; i < CombatDirector.eliteTiers.Length; i++)
                {
                    EliteTierDef etd = CombatDirector.eliteTiers[i];
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
        }
    }
}
