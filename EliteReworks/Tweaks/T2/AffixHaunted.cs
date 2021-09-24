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
        public static void Setup()
        {
            AffixHauntedAura.indicatorPrefab = BuildIndicator();
            HideBubble();
        }

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
    }
}
