using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using R2API;
using EliteReworks.Tweaks.T2.Components;
using System;

namespace EliteReworks.Tweaks.T2
{
    public static class AffixHaunted
    {
        public static int ghostLifetime = 15;
        public static BuffDef reviveBuff;
        public static void Setup()
        {
            //Pause invis on stun
            On.RoR2.CharacterBody.AffixHauntedBehavior.FixedUpdate += (orig, self) =>
            {
                if (self.body && self.body.healthComponent && self.body.healthComponent.isInFrozenState)
                {
                    return;
                }
                else
                {
                    SetStateOnHurt ssoh = self.body.gameObject.GetComponent<SetStateOnHurt>();
                    if (ssoh)
                    {
                        Type state = ssoh.targetStateMachine.state.GetType();
                        if (state == typeof(EntityStates.StunState) || state == typeof(EntityStates.ShockState))
                        {
                            return;
                        }
                    }
                }
                orig(self);
            };

            #region disabled reviving
            //reviveBuff = CreateReviveBuff();
            /*
            //Marked enemies become ghosts on death
            On.RoR2.CharacterBody.OnDeathStart += (orig, self) =>
            {
                if (NetworkServer.active)
                {
                    if (self.master && self.master.inventory
                    && self.master.inventory.GetItemCount(RoR2Content.Items.Ghost) <= 0 && self.master.inventory.GetItemCount(RoR2Content.Items.ExtraLife) <= 0)
                    {
                        if (self.HasBuff(reviveBuff))
                        {
                            CharacterBody cb = EliteReworksUtil.FindElite(RoR2Content.Buffs.AffixHaunted.buffIndex, self.teamComponent.teamIndex,
                                self.corePosition, AffixHauntedReviver.wardRadius);
                            if (cb)
                            {
                                //Ghosts seem to be intangible, so that's a feature now.
                                CharacterBody ghostBody = Util.TryToCreateGhost(self, cb, ghostLifetime);
                                if (ghostBody)
                                {
                                    ghostBody.master.inventory.RemoveItem(RoR2Content.Items.BoostDamage, 150);

                                    AffixHauntedReviver arv = cb.gameObject.GetComponent<AffixHauntedReviver>();
                                    if (arv)
                                    {
                                        arv.AddGhost(ghostBody);
                                    }
                                }

                            }
                        }
                    }
                }
                orig(self);
            };

            //Celestines can't revive each other
            On.RoR2.CharacterBody.AddBuff_BuffIndex += (On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType) =>
            {
                if (buffType == reviveBuff.buffIndex && self.HasBuff(RoR2Content.Buffs.AffixHaunted.buffIndex))
                {
                    return;
                }
                orig(self, buffType);
            };

            */
            #endregion
        }

        private static BuffDef CreateReviveBuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.canStack = false;
            buff.name = "MoffeinEliteReworkReviveBuff";
            buff.isDebuff = false;
            buff.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffCrippleIcon");
            buff.buffColor = new Color(148f / 255f, 215f / 255f, 214f/255f);

            BuffAPI.Add(new CustomBuff(buff));
            return buff;
        }
    }
}
