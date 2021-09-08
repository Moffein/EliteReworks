﻿using EliteReworks.Tweaks.T2;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.SharedHooks
{
    public static class TakeDamage
    {
        public static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool isPoison = false;
            if (EliteReworksPlugin.affixPoisonEnabled && damageInfo.HasModdedDamageType(AffixPoison.malachiteDamage))
            {
                isPoison = true;
                damageInfo.crit = false;
                damageInfo.damage = self.combinedHealth * AffixPoison.spikeHPPercent;
                damageInfo.procCoefficient = 0f;
                damageInfo.damageType = DamageType.NonLethal;
            }
            orig(self, damageInfo);
            if (!damageInfo.rejected)
            {
                if (isPoison)
                {
                    if (self.body)
                    {
                        self.body.AddTimedBuff(RoR2Content.Buffs.HealingDisabled.buffIndex, 8f);
                        self.body.AddTimedBuff(RoR2Content.Buffs.Weak.buffIndex, 8f);
                    }
                }
            }
        }
    }
}