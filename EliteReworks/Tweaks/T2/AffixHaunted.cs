using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using R2API;
using EliteReworks.Tweaks.T2.Components;

namespace EliteReworks.Tweaks.T2
{
    public static class AffixHaunted
    {
        public static BuffDef reviveBuff;

        public static void Setup()
        {
            reviveBuff = CreateReviveBuff();
            if (EliteReworksPlugin.affixHauntedBetaEnabled)
            {
                DisableBubble();
                On.RoR2.GlobalEventManager.OnCharacterDeath += ReviveAsGhost;
            }
            else if (EliteReworksPlugin.affixHauntedSimpleIndicatorEnabled)
            {
                AffixHauntedAura.indicatorPrefab = BuildIndicator();
                HideBubble();
            }
        }

        #region minimal bubble
        private static GameObject BuildIndicator()
        {
            GameObject indicator = Resources.Load<GameObject>("prefabs/networkedobjects/shrines/ShrineHealingWard").InstantiateClone("MoffeinEliteReworksHauntedIndicator", true);
            indicator.transform.localScale *= 30f;

            UnityEngine.Object.Destroy(indicator.GetComponent<HealingWard>());
            ParticleSystemRenderer[] pr = indicator.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (ParticleSystemRenderer p in pr)
            {
                if (p.name == "HealingSymbols")
                {
                    UnityEngine.Object.Destroy(p);
                }
            }

            NetworkedBodyAttachment nba = indicator.AddComponent<NetworkedBodyAttachment>();
            nba.shouldParentToAttachedBody = true;
            nba.forceHostAuthority = false;

            PrefabAPI.RegisterNetworkPrefab(indicator);
            return indicator;
        }

        private static void HideBubble()
        {
            GameObject bubble = Resources.Load<GameObject>("prefabs/networkedobjects/AffixHauntedWard");
            MeshRenderer[] mr = bubble.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in mr)
            {
                if (m.name == "IndicatorSphere")
                {
                    UnityEngine.Object.Destroy(m);
                }
            }
        }
        #endregion

        #region revive rework
        public static BuffDef CreateReviveBuff()
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.buffColor = new Color(157f / 255f, 221f / 255f, 216f / 255f);
            buff.canStack = false;
            buff.isDebuff = false;
            buff.name = "EliteReworksHauntedRevive";
            buff.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffDeathMarkIcon");
            BuffAPI.Add(new CustomBuff(buff));
            return buff;
        }

        public static void AttemptSpawnGhost(CharacterBody sourceBody, Vector3 position, float radius)
        {
            float radiusSquare = radius * radius;
            float squareDist;

            if (sourceBody.teamComponent)
            {
                TeamIndex ti = sourceBody.teamComponent.teamIndex;
                System.Collections.ObjectModel.ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(ti);
                foreach (TeamComponent tc in teamMembers)
                {
                    if (tc.body && tc.body.healthComponent && tc.body.healthComponent.alive)
                    {
                        if (tc.body.HasBuff(RoR2Content.Buffs.AffixHaunted))
                        {
                            squareDist = (position - tc.body.corePosition).sqrMagnitude;
                            if (squareDist < radiusSquare)
                            {
                                AffixHauntedReviveAura ahr = tc.body.GetComponent<AffixHauntedReviveAura>();
                                if (ahr && ahr.wardActive && ahr.attachedGhosts.Count < AffixHauntedReviveAura.maxAttachedGhosts)
                                {
                                    CharacterBody ghostBody = Util.TryToCreateGhost(sourceBody, tc.body, 30);
                                    if (ghostBody.master && ghostBody.master.inventory)
                                    {
                                        ghostBody.master.inventory.RemoveItem(RoR2Content.Items.BoostDamage, 150);
                                    }
                                    ahr.attachedGhosts.Add(ghostBody);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void DisableBubble()
        {
            On.RoR2.CharacterBody.AffixHauntedBehavior.FixedUpdate += (orig, self) =>
            {
                return;
            };
        }

        private static void ReviveAsGhost(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (damageReport.victimBody && damageReport.victimBody.HasBuff(reviveBuff.buffIndex) && !damageReport.victimBody.HasBuff(RoR2Content.Buffs.AffixHaunted))
            {
                AttemptSpawnGhost(damageReport.victimBody, damageReport.damageInfo.position, AffixHauntedReviveAura.wardRadius);
            }
        }
        #endregion
    }
}
