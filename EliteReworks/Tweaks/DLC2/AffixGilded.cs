using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.DLC2
{
    public static class AffixGilded
    {
        public static float baseDamage = 24f;
        public static void Setup()
        {
            IL.RoR2.AffixAurelioniteBehavior.FireAurelioniteAttack += AffixAurelioniteBehavior_FireAurelioniteAttack;

            //Doing it this way because Gilded Logic runs in Update
            On.EntityStates.ShockState.OnEnter += ShockState_OnEnter;
            On.EntityStates.FrozenState.OnEnter += FrozenState_OnEnter;
            On.EntityStates.StunState.OnEnter += StunState_OnEnter;
            On.EntityStates.StunState.ExtendStun += StunState_ExtendStun;
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
