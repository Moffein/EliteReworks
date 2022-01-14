﻿using BepInEx;
using RoR2;
using R2API.Utils;
using EliteReworks.Tweaks.T1;
using R2API;
using UnityEngine;
using EliteReworks.SharedHooks;
using BepInEx.Configuration;
using EliteReworks.Tweaks;
using EliteReworks.Tweaks.T2;
using EliteReworks.Tweaks.T1.Components;

namespace EliteReworks
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("com.TPDespair.ZetAspects", BepInDependency.DependencyFlags.SoftDependency)]
    [R2API.Utils.R2APISubmoduleDependency(nameof(PrefabAPI), nameof(EffectAPI), nameof(ProjectileAPI), nameof(BuffAPI), nameof(RecalculateStatsAPI), nameof(DamageAPI), nameof(EliteAPI))]
    [BepInPlugin("com.Moffein.EliteReworks", "Elite Reworks", "1.4.3")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class EliteReworksPlugin : BaseUnityPlugin
    {
        public static BuffDef EmptyBuff = null; //This is a buff that is never used. This is used to stop vanilla elite affix checks from working.
        public static bool modifyStats = true;
        public static bool affixBlueEnabled = true;
        public static bool affixBlueRemoveShield = true;
        public static bool affixWhiteEnabled = true;
        public static bool affixRedEnabled = true;
        public static bool affixHauntedSimpleIndicatorEnabled = true;
        public static bool affixHauntedEnabled = true;
        public static bool affixPoisonEnabled = true;

        public static float eliteStunDisableDuration = 1.2f;
        public static float eliteBossDamageMult = 1.6f;

        public static bool zetAspectsLoaded = false;

        private void CheckDependencies()
        {
            zetAspectsLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TPDespair.ZetAspects");
        }


        public void Awake()
        {
            CheckDependencies();
            ReadConfig();
            if (affixWhiteEnabled)
            {
                AffixWhite.Setup();
            }
            if (affixBlueEnabled)
            {
                AffixBlue.Setup();
            }
            if (affixBlueRemoveShield)
            {
                AffixBlue.RemoveShields();
            }
            if (affixRedEnabled)
            {
                AffixRed.Setup();
            }
            if (affixHauntedSimpleIndicatorEnabled || affixHauntedEnabled)
            {
                AffixHaunted.Setup();
            }
            if (affixPoisonEnabled)
            {
                AffixPoison.Setup();
            }

            if (modifyStats)
            {
                ModifyEliteTiers.Setup();
            }

            //These all have things to check if config features are enabled
            On.RoR2.CharacterBody.AddBuff_BuffIndex += AddBuff.AddEliteComponents;
            On.RoR2.GlobalEventManager.OnHitAll += OnHitAll.TriggerOnHitAllEffects;
            On.RoR2.HealthComponent.TakeDamage += TakeDamage.HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.RecalculateStats += RecalculateStats.CharacterBody_RecalculateStats;
            RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients.Hook;
        }

        

        public void ReadConfig()
        {
            //resizeElites = base.Config.Bind<bool>(new ConfigDefinition("General", "Resize Elites"), true, new ConfigDescription("Elites are bigger.")).Value; //Buggy.
            modifyStats = base.Config.Bind<bool>(new ConfigDefinition("General", "Modify Scaling"), true, new ConfigDescription("Modify Elite Cost/HP/Damage multipliers.")).Value;

            ModifyEliteTiers.t1Cost = base.Config.Bind<float>(new ConfigDefinition("General", "T1 Cost"), 4.5f, new ConfigDescription("T1 Director Cost multiplier. (Vanilla is 6)")).Value;
            ModifyEliteTiers.t1Health = base.Config.Bind<float>(new ConfigDefinition("General", "T1 HP"), 3f, new ConfigDescription("T1 Health multiplier. (Vanilla is 4)")).Value;
            ModifyEliteTiers.t1Damage = base.Config.Bind<float>(new ConfigDefinition("General", "T1 Damage"), 1.5f, new ConfigDescription("T1 Damage multipliers. (Vanilla is 2)")).Value;

            ModifyEliteTiers.t2Cost = base.Config.Bind<float>(new ConfigDefinition("General", "T2 Cost"), 36f, new ConfigDescription("T2 Director Cost multiplier. (Vanilla is 36)")).Value;
            ModifyEliteTiers.t2Health = base.Config.Bind<float>(new ConfigDefinition("General", "T2 HP"), 14f, new ConfigDescription("T2 Health multiplier. (Vanilla is 18)")).Value;
            ModifyEliteTiers.t2Damage = base.Config.Bind<float>(new ConfigDefinition("General", "T2 Damage"), 3.5f, new ConfigDescription("T2 Damage multipliers. (Vanilla is 6)")).Value;

            affixBlueRemoveShield = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Remove Shield"), true, new ConfigDescription("Remove shields from Overloading Enemies.")).Value;
            affixBlueEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;
            AffixBluePassiveLightning.uncapOnHitLightning = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Uncap On-Hit Lightning"), false, new ConfigDescription("Remove the fire rate cap on on-hit lightning bombs (laggy).")).Value;

            affixWhiteEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Glacial", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;

            affixRedEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Blazing", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;
            
            affixPoisonEnabled = base.Config.Bind<bool>(new ConfigDefinition("T2 - Malachite", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;

            affixHauntedEnabled = base.Config.Bind<bool>(new ConfigDefinition("T2 - Celestine", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;
        }
    }
}
