using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;
using RoR2.Audio;
using RoR2.Orbs;

namespace EliteReworks.Tweaks.DLC2
{
    public static class AffixGilded
    {
        public static float baseDamage = 24f;

        public static bool directSiphon = true;

        public static void Setup()
        {
            IL.RoR2.AffixAurelioniteBehavior.FireAurelioniteAttack += AffixAurelioniteBehavior_FireAurelioniteAttack;

            //Doing it this way because Gilded Logic runs in Update
            On.EntityStates.ShockState.OnEnter += ShockState_OnEnter;
            On.EntityStates.FrozenState.OnEnter += FrozenState_OnEnter;
            On.EntityStates.StunState.OnEnter += StunState_OnEnter;
            On.EntityStates.StunState.ExtendStun += StunState_ExtendStun;

            if (directSiphon)
            {
                On.RoR2.AffixAurelioniteBehavior.StealMoneyWithFX += AffixAurelioniteBehavior_StealMoneyWithFX;
            }
        }

        private static void AffixAurelioniteBehavior_StealMoneyWithFX(On.RoR2.AffixAurelioniteBehavior.orig_StealMoneyWithFX orig, AffixAurelioniteBehavior self, Transform victimTransform, int numberOfNuggetsToSpawn, CharacterBody body)
        {
            /*for (int i = 0; i < numberOfNuggetsToSpawn; i++)
            {
                PowerUpDropletController.FirePowerUpDropletRandomly(AffixAurelioniteBehavior.moneyPackPrefab, TeamIndex.Player, body.corePosition + Vector3.up * 5f, AffixAurelioniteBehavior.minimumRandomVelocity, AffixAurelioniteBehavior.maximumRandomVelocity, null);
            }*/

            //Calculate gold to steal based on numberOfNuggestToSpawn
            //Create gold orb based on that amount
            if (self.body.mainHurtBox)
            {
                GoldOrb goldOrb = new GoldOrb();
                goldOrb.origin = victimTransform.position;
                goldOrb.target = self.body.mainHurtBox;
                int moneyToGrant = numberOfNuggetsToSpawn * 8;
                goldOrb.goldAmount = (uint)Run.instance.GetDifficultyScaledCost(moneyToGrant);
                OrbManager.instance.AddOrb(goldOrb);
            }

            EntitySoundManager.EmitSoundServer(AffixAurelioniteBehavior.stealGoldSFX.index, victimTransform.gameObject);
            EffectData effectData = new EffectData();
            effectData.origin = victimTransform.position;
            EffectManager.SpawnEffect(AffixAurelioniteBehavior.staticCoinEffect, effectData, true);
        }

        private static void ShockState_OnEnter(On.EntityStates.ShockState.orig_OnEnter orig, EntityStates.ShockState self)
        {
            orig(self);
            if (self.characterBody)
            {
                var component = self.characterBody.GetComponent<AffixAurelioniteBehavior>();
                if (component && self.characterBody.isElite)
                {
                    component.QuickCooldown(AffixAurelioniteBehavior.cooldownDuration + self.shockDuration);
                }
            }
        }

        private static void StunState_OnEnter(On.EntityStates.StunState.orig_OnEnter orig, EntityStates.StunState self)
        {
            orig(self);
            if (self.characterBody && self.characterBody.isElite)
            {
                var component = self.characterBody.GetComponent<AffixAurelioniteBehavior>();
                if (component)
                {
                    component.QuickCooldown(AffixAurelioniteBehavior.cooldownDuration + self.timeRemaining);
                }
            }
        }

        private static void StunState_ExtendStun(On.EntityStates.StunState.orig_ExtendStun orig, EntityStates.StunState self, float durationDelta)
        {
            orig(self, durationDelta);
            if (self.characterBody && self.characterBody.isElite)
            {
                var component = self.characterBody.GetComponent<AffixAurelioniteBehavior>();
                if (component)
                {
                    component.QuickCooldown(AffixAurelioniteBehavior.cooldownDuration + self.timeRemaining);
                }
            }
        }

        private static void FrozenState_OnEnter(On.EntityStates.FrozenState.orig_OnEnter orig, EntityStates.FrozenState self)
        {
            orig(self);
            if (self.characterBody && self.characterBody.isElite)
            {
                var component = self.characterBody.GetComponent<AffixAurelioniteBehavior>();
                if (component)
                {
                    component.QuickCooldown(AffixAurelioniteBehavior.cooldownDuration + self.duration);
                }
            }
        }

        private static void AffixAurelioniteBehavior_FireAurelioniteAttack(ILContext il)
        {
            bool error = true;
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After, x => x.MatchCallvirt<CharacterBody>("get_damage")))
            {
                c.Emit(OpCodes.Ldarg_2);    //Body
                c.EmitDelegate<Func<float, CharacterBody, float>>((damage, body) =>
                {
                    float levelFactor = 1f + 0.2f * (body.level - 1f);
                    if (body.isChampion) levelFactor *= EliteReworks.EliteReworksPlugin.eliteBossDamageMult;
                    return baseDamage * levelFactor;
                });

                if (c.TryGotoNext(MoveType.After, x => x.MatchCallvirt<CharacterBody>("get_damage")))
                {
                    c.Emit(OpCodes.Ldarg_2);    //Body
                    c.EmitDelegate<Func<float, CharacterBody, float>>((damage, body) =>
                    {
                        float levelFactor = 1f + 0.2f * (body.level - 1f);
                        if (body.isChampion) levelFactor *= EliteReworks.EliteReworksPlugin.eliteBossDamageMult;
                        return baseDamage * levelFactor;
                    });
                    error = false;
                }
            }

            if (error)
            {
                Debug.LogError("EliteReworks: AffixGilded IL hook failed.");
            }
        }
    }
}
