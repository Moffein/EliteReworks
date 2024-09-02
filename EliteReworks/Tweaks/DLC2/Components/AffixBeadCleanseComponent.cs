using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EliteReworks.Tweaks.DLC2.Components
{
    public class AffixBeadCleanseComponent : MonoBehaviour
    {
        public static GameObject cleanseEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Cleanse/CleanseEffect.prefab").WaitForCompletion();
        public static float cleansePulseTimer = 2f;

        private float stopwatch;
        private float radius;
        private BuffWard buffWard;

        public bool wardActive = false;

        public void Awake()
        {
            stopwatch = 0f;
            buffWard = GetComponent<BuffWard>();
        }

        public void FixedUpdate()
        {
            if (!NetworkServer.active) return;
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= cleansePulseTimer)
            {
                stopwatch -= cleansePulseTimer;
                CleansePulse();
            }
        }

        //Copied from BuffWard
        public void CleansePulse()
        {
            if (!buffWard || !buffWard.teamFilter) return;

            float radiusSqr = buffWard.radius * buffWard.radius;
            Vector3 currentPosition = transform.position;
            IEnumerable<TeamComponent> recipients = TeamComponent.GetTeamMembers(buffWard.teamFilter.teamIndex);

            foreach (TeamComponent teamComponent in recipients)
            {
                Vector3 vector = teamComponent.transform.position - currentPosition;
                if (buffWard.shape == BuffWard.BuffWardShape.VerticalTube)
                {
                    vector.y = 0f;
                }
                if (vector.sqrMagnitude <= radiusSqr)
                {
                    CharacterBody body = teamComponent.GetComponent<CharacterBody>();
                    if (body && !body.HasBuff(DLC2Content.Buffs.EliteBead) && (!buffWard.requireGrounded || !body.characterMotor || body.characterMotor.isGrounded))
                    {
                        bool removedDebuff = false;

                        int dotCount = 0;
                        int activeBuffs = body.activeBuffsListCount;
                        DotController dotController = DotController.FindDotController(body.gameObject);
                        if (dotController)
                        {
                            dotCount = dotController.dotStackList.Count;
                        }

                        Util.CleanseBody(body, true, false, false, true, false, false);

                        if (activeBuffs > body.activeBuffsListCount || dotCount > 0)
                        {
                            if (cleanseEffectPrefab)
                            {
                                EffectManager.SpawnEffect(cleanseEffectPrefab, new EffectData
                                {
                                    origin = body.corePosition,
                                    rotation = Quaternion.identity,
                                    scale = 2f
                                }, true);
                            }
                        }
                    }
                }
            }
        }
    }
}
