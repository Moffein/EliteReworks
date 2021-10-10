using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;
using EliteReworks.Tweaks.T1;
using EliteReworks.Tweaks.T1.Components;

namespace EliteReworks.SharedHooks
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
                        float radius = AffixWhite.baseRadius;
                        float slow = AffixWhite.slowDuration;
                        EliteReworksUtils.DebuffSphere(EliteReworksPlugin.zetAspectsLoaded ? AffixWhite.slow80alt.buffIndex : RoR2Content.Buffs.Slow80.buffIndex, attackerBody.teamComponent.teamIndex,
                            damageInfo.position, radius, slow,
                            AffixWhite.explosionEffectPrefab, AffixWhite.hitEffectPrefab, false);
                    }
                    if (EliteReworksPlugin.affixBlueEnabled && attackerBody.HasBuff(RoR2Content.Buffs.AffixBlue))
                    {
                        AffixBluePassiveLightning ab = attackerBody.GetComponent<AffixBluePassiveLightning>();
                        if (ab)
                        {
                            int meatballCount = Math.Min(Math.Max(1, Mathf.RoundToInt(AffixBlue.baseMeatballCount * damageInfo.procCoefficient)), AffixBluePassiveLightning.baseBodyMeatballStock);
                            if (ab.OnHitReady(meatballCount))
                            {
                                ab.TriggerOnHit(meatballCount);
                                AffixBlue.FireMeatballs(damageInfo.attacker, attackerBody.isChampion, damageInfo.damage * AffixBlue.lightningDamageCoefficient, damageInfo.crit,
                                        Vector3.up, damageInfo.position + Vector3.up, attackerBody.transform.forward,
                                        meatballCount, 20f, 400f, 20f);
                            }
                        }
                    }
                }
            }
        }
    }
}
