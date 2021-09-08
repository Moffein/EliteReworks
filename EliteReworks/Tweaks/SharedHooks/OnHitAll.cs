﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;
using EliteReworks.Tweaks.T1;

namespace EliteReworks.Tweaks.SharedHooks
{
    public static class OnHitAll
    {
        //Combine everything in to 1 class to reduce GetComponent calls
        public static void TriggerOnHitAllEffects(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig(self, damageInfo, hitObject);
            if (damageInfo.procCoefficient == 0f || damageInfo.rejected || !NetworkServer.active)
            {
                return;
            }
            if (damageInfo.attacker)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    if (EliteReworksPlugin.affixWhiteEnabled && attackerBody.teamComponent && attackerBody.HasBuff(RoR2Content.Buffs.AffixWhite))
                    {
                        float radius = Mathf.Max(1f, AffixWhite.baseRadius * damageInfo.procCoefficient);
                        float slow = Mathf.Max(0.375f, AffixWhite.slowDuration * damageInfo.procCoefficient);
                        EliteReworksUtil.DebuffSphere(RoR2Content.Buffs.Slow80.buffIndex, attackerBody.teamComponent.teamIndex, damageInfo.position, radius, slow, AffixWhite.explosionEffect, false);
                    }
                    if (EliteReworksPlugin.affixBlueEnabled && attackerBody.HasBuff(RoR2Content.Buffs.AffixBlue))
                    {
                        if (damageInfo.procCoefficient >= 1f || Util.CheckRoll(100f * damageInfo.procCoefficient, attackerBody.master))
                        {
                            AffixBlue.FireMeatballs(damageInfo.attacker, damageInfo.damage * AffixBlue.lightningDamageCoefficient, damageInfo.crit,
                                Vector3.up, damageInfo.position + Vector3.up, attackerBody.transform.forward,
                                Mathf.CeilToInt(AffixBlue.baseMeatballCount * Mathf.Max(1f, damageInfo.procCoefficient)), 17f, 300f, 18f);
                        }
                    }
                }
            }
        }
    }
}
