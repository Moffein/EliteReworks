using BepInEx;
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
    [R2API.Utils.R2APISubmoduleDependency(nameof(PrefabAPI), nameof(EffectAPI), nameof(ProjectileAPI), nameof(BuffAPI), nameof(RecalculateStatsAPI), nameof(DamageAPI))]
    [BepInPlugin("com.Moffein.EliteReworks", "Elite Reworks", "0.1.4")]
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
        public static bool affixHauntedBetaEnabled = true;
        public static bool affixPoisonEnabled = true;

        public static bool zetAspectsLoaded = false;

        private void CheckDependencies()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TPDespair.ZetAspects"))
            {
                zetAspectsLoaded = true;
            }
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
            if (affixHauntedSimpleIndicatorEnabled || affixHauntedBetaEnabled)
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
            On.RoR2.CharacterBody.OnClientBuffsChanged += OnClientBuffsChanged.AddEliteComponents;
            On.RoR2.GlobalEventManager.OnHitAll += OnHitAll.TriggerOnHitAllEffects;
            On.RoR2.HealthComponent.TakeDamage += TakeDamage.HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.RecalculateStats += RecalculateStats.CharacterBody_RecalculateStats;
            RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients.Hook;
        }

        

        public void ReadConfig()
        {
            //resizeElites = base.Config.Bind<bool>(new ConfigDefinition("General", "Resize Elites"), true, new ConfigDescription("Elites are bigger.")).Value; //Buggy.
            modifyStats = base.Config.Bind<bool>(new ConfigDefinition("General", "Modify Scaling"), true, new ConfigDescription("Modify Elite Cost/HP/Damage multipliers.")).Value;

            affixBlueRemoveShield = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Remove Shield"), true, new ConfigDescription("Remove shields from Overloading Enemies.")).Value;
            affixBlueEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;
            AffixBluePassiveLightning.uncapOnHitLightning = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Uncap On-Hit Lightning"), false, new ConfigDescription("Remove the fire rate cap on on-hit lightning bombs (laggy).")).Value;

            affixWhiteEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Glacial", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;

            affixRedEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Blazing", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;
            
            affixPoisonEnabled = base.Config.Bind<bool>(new ConfigDefinition("T2 - Malachite", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;

            affixHauntedSimpleIndicatorEnabled = base.Config.Bind<bool>(new ConfigDefinition("T2 - Celestine", "Simple Indicator"), false, new ConfigDescription("Replace the bubble with a less obtrusive indicator.")).Value;
            affixHauntedBetaEnabled = base.Config.Bind<bool>(new ConfigDefinition("T2 - Celestine", "Enable Changes (beta)"), false, new ConfigDescription("Enables beta rework to this elite type. Removes invisibility/bubbles, enemies that die near Celestines revive as ghosts.")).Value;
        }
    }
}
