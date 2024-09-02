using EliteReworks.Tweaks.DLC2.Components;
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
        public static GameObject wardReworkPrefab;
        public static void Setup()
        {
            On.RoR2.AffixBeadBehavior.OnEnable += AffixBeadBehavior_OnEnable;
            On.RoR2.AffixBeadBehavior.Update += AffixBeadBehavior_Update;
            CreateWard();
        }

        private static void CreateWard()
        {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Elites/EliteBead/AffixBeadWard.prefab").WaitForCompletion().InstantiateClone("MoffeinEliteReworksTwistedWard", true);
            BuffWard buffWard = prefab.GetComponent<BuffWard>();
            if (buffWard) buffWard.buffDef = null;
            prefab.AddComponent<AffixBeadCleanseComponent>();

            wardReworkPrefab = prefab;
        }

        private static void AffixBeadBehavior_Update(On.RoR2.AffixBeadBehavior.orig_Update orig, AffixBeadBehavior self)
        {
            bool isStunned = false;
            if (NetworkServer.active)
            {
                EliteStunTracker stunTracker = self.GetComponent<EliteStunTracker>();
                if (stunTracker && stunTracker.PassiveActive() && self.body)
                {
                    if (self.body.healthComponent && self.body.healthComponent.isInFrozenState)
                    {
                        isStunned = true;
                        stunTracker.SetStun();
                    }
                    else
                    {
                        SetStateOnHurt ssoh = self.gameObject.GetComponent<SetStateOnHurt>();
                        if (ssoh)
                        {
                            Type state = ssoh.targetStateMachine.state.GetType();
                            if (state == typeof(EntityStates.StunState) || state == typeof(EntityStates.ShockState))
                            {
                                isStunned = true;
                                stunTracker.SetStun();
                            }
                        }
                    }
                }
                isStunned = stunTracker && !stunTracker.PassiveActive()
                    || self.body && self.body.healthComponent && !self.body.healthComponent.alive;
                if (isStunned && self.affixBeadWard)
                {
                    UnityEngine.Object.Destroy(self.affixBeadWard);
                }
            }
            if (!isStunned) orig(self);
        }

        private static void AffixBeadBehavior_OnEnable(On.RoR2.AffixBeadBehavior.orig_OnEnable orig, AffixBeadBehavior self)
        {
            orig(self);
            if (wardReworkPrefab) self.affixBeadWardReference = wardReworkPrefab;
            EliteStunTracker est = self.GetComponent<EliteStunTracker>();
            if (!est) est = self.gameObject.AddComponent<EliteStunTracker>();
        }
    }
}
