using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.DLC2
{
    public static class AffixBead
    {
        public static float baseDamage = 42f;

        public static void Setup()
        {
            IL.RoR2.AffixBeadAttachment.FireProjectile += AffixBeadAttachment_FireProjectile;
        }

        private static void AffixBeadAttachment_FireProjectile(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCallvirt<CharacterBody>("get_damage")))
            {
                c.Emit(OpCodes.Ldarg_0);    //Self
                c.EmitDelegate<Func<float, AffixBeadAttachment, float>>((damage, self) =>
                {
                    CharacterBody body = null;
                    if (self.networkedBodyAttachment) body = self.networkedBodyAttachment.attachedBody;

                    if (body)
                    {
                        float levelFactor = 1f + 0.2f * (body.level - 1f);
                        if (body.isChampion) levelFactor *= EliteReworks.EliteReworksPlugin.eliteBossDamageMult;
                        damage = baseDamage * levelFactor * self.damageCoefficient;
                    }
                    return damage;
                });
            }
            else
            {
                Debug.LogError("EliteReworks: AffixBead IL hook failed.");
            }
        }
    }
}
