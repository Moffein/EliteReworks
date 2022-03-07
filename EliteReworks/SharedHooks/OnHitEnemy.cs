using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.SharedHooks
{
    public class OnHitEnemy
    {
        public static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, UnityEngine.GameObject victim)
        {
            CharacterBody attackerBody = null;
            CharacterBody victimBody = null;
            bool validDamage = NetworkServer.active && damageInfo.procCoefficient > 0f && !damageInfo.rejected;

            orig(self, damageInfo, victim);

            if (validDamage)
            {
                uint? maxStacksFromAttacker = null;
                if ((damageInfo != null) ? damageInfo.inflictor : null)
                {
                    ProjectileDamage component = damageInfo.inflictor.GetComponent<ProjectileDamage>();
                    if (component && component.useDotMaxStacksFromAttacker)
                    {
                        maxStacksFromAttacker = new uint?(component.dotMaxStacksFromAttacker);
                    }
                }

                if (damageInfo.attacker)
                {
                    attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    victimBody = victim ? victim.GetComponent<CharacterBody>() : null;

                    if (attackerBody)
                    {
                        if (EliteReworksPlugin.eliteVoidEnabled)
                        {
                            if (attackerBody.HasBuff(DLC1Content.Buffs.EliteVoid) && !damageInfo.procChainMask.HasProc(ProcType.FractureOnHit))
                            {
                                if (Util.CheckRoll(damageInfo.procCoefficient * 100f, attackerBody.master)) //This accounts for wisps/templars
                                {
                                    damageInfo.procChainMask.AddProc(ProcType.FractureOnHit);
                                    DotController.DotDef dotDef = DotController.GetDotDef(DotController.DotIndex.Fracture);

                                    InflictDotInfo inflictDotInfo = new InflictDotInfo
                                    {
                                        attackerObject = damageInfo.attacker,
                                        victimObject = victim,
                                        totalDamage = new float?(damageInfo.damage * 0.5f),
                                        damageMultiplier = default,
                                        dotIndex = DotController.DotIndex.Fracture,
                                        maxStacksFromAttacker = maxStacksFromAttacker
                                    };

                                    DotController.InflictDot(ref inflictDotInfo);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
