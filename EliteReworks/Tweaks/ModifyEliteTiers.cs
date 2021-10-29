using RoR2;
using UnityEngine;

namespace EliteReworks.Tweaks
{
    public static class ModifyEliteTiers
    {
        public static void Setup()
        {
            On.RoR2.CombatDirector.Init += (orig) =>
            {
                orig();
                foreach (CombatDirector.EliteTierDef edf in CombatDirector.eliteTiers)
                {
                    /*string eliteTier = "\nElites:";
                    foreach(EliteDef ed in edf.eliteTypes)
                    {
                        eliteTier += " " + ed.eliteEquipmentDef.nameToken;
                    }
                    eliteTier += "\nCost: " + edf.costMultiplier + "\nHP: " + edf.healthBoostCoefficient + "\nDamage: " + edf.damageBoostCoefficient;
                    Debug.Log(eliteTier);*/
                    if (edf.damageBoostCoefficient == 2f && edf.healthBoostCoefficient == 4f && edf.costMultiplier == 6f)
                    {
                        //Debug.Log("Modifying t1 elites");
                        edf.costMultiplier = 4.5f;
                        edf.healthBoostCoefficient = 3f;
                        edf.damageBoostCoefficient = 1.5f;
                    }
                    else if (edf.damageBoostCoefficient == 6f && edf.healthBoostCoefficient == 18f && edf.costMultiplier == 36f)
                    {
                        //Debug.Log("Modifying t2 elites");
                        edf.costMultiplier = 15f;
                        edf.healthBoostCoefficient = 10f;
                        edf.damageBoostCoefficient = 4f;
                    }
                }
            };
        }
    }
}
