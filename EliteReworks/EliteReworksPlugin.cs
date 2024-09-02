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
using System.Collections;
using System;
using EliteReworks.Tweaks.DLC1;
using System.Reflection;

namespace EliteReworks
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2API.SoundAPI.PluginGUID)]
    [BepInDependency(R2API.DamageAPI.PluginGUID)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency("com.TPDespair.ZetAspects", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("com.Moffein.EliteReworks", "Elite Reworks", "1.10.0")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class EliteReworksPlugin : BaseUnityPlugin
    {
        public static BuffDef EmptyBuff = null; //This is a buff that is never used. This is used to stop vanilla elite affix checks from working.
        public static bool modifyStats = true;
        public static bool affixBlueEnabled = true;
        public static bool affixBlueRemoveShield = true;
        public static bool affixBlueReduceShield = false;
        public static bool affixWhiteEnabled = true;
        public static bool affixRedEnabled = true;
        public static bool affixHauntedEnabled = true;
        public static bool affixPoisonEnabled = true;

        public static bool eliteEarthEnabled = true;
        public static bool eliteVoidEnabled = true;

        public static bool affixBeadEnabled = true;

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
            if (affixRedEnabled)
            {
                AffixRed.Setup();
            }
            if (affixHauntedEnabled)
            {
                AffixHaunted.Setup();
            }
            if (affixPoisonEnabled)
            {
                AffixPoison.Setup();
            }
            if (eliteEarthEnabled)
            {
                EliteEarth.Setup();
            }
            if (eliteVoidEnabled)
            {
                EliteVoid.Setup();
            }
            if (affixBeadEnabled)
            {
                AffixBead.Setup();
            }

            //These all have things to check if config features are enabled
            On.RoR2.CharacterBody.AddBuff_BuffIndex += AddBuff.AddEliteComponents;
            On.RoR2.GlobalEventManager.OnHitAllProcess += OnHitAll.TriggerOnHitAllEffects;
            On.RoR2.HealthComponent.TakeDamageProcess += TakeDamage.HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.RecalculateStats += RecalculateStats.CharacterBody_RecalculateStats;

            ModifyEliteTiers.Setup();


            using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EliteReworks.EliteReworks.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                R2API.SoundAPI.SoundBanks.Add(bytes);
            }
        }

        public void ReadConfig()
        {
            //resizeElites = base.Config.Bind<bool>(new ConfigDefinition("General", "Resize Elites"), true, new ConfigDescription("Elites are bigger.")).Value; //Buggy.
            modifyStats = base.Config.Bind<bool>(new ConfigDefinition("General", "Modify Scaling"), true, new ConfigDescription("Modify Elite Cost/HP/Damage multipliers.")).Value;

            ModifyEliteTiers.t1Cost = base.Config.Bind<float>(new ConfigDefinition("General", "T1 Cost"), 4.5f, new ConfigDescription("T1 Director Cost multiplier. (Vanilla is 6)")).Value;
            ModifyEliteTiers.t1Health = base.Config.Bind<float>(new ConfigDefinition("General", "T1 HP"), 3f, new ConfigDescription("T1 Health multiplier. (Vanilla is 4)")).Value;
            ModifyEliteTiers.t1HealthEarth = base.Config.Bind<float>(new ConfigDefinition("General", "T1 HP - Mending"), 3f, new ConfigDescription("Mending Health multiplier. (Vanilla is 3)")).Value;
            ModifyEliteTiers.t1Damage = base.Config.Bind<float>(new ConfigDefinition("General", "T1 Damage"), 1.5f, new ConfigDescription("T1 Damage multipliers. (Vanilla is 2)")).Value;

            ModifyEliteTiers.t1HonorCost = base.Config.Bind<float>(new ConfigDefinition("General", "T1 (Honor) Cost"), 3.5f, new ConfigDescription("T1 Director Cost multiplier while Honor is active. (Vanilla is 3.5)")).Value;
            ModifyEliteTiers.t1HonorHealth = base.Config.Bind<float>(new ConfigDefinition("General", "T1 (Honor) HP"), 2.5f, new ConfigDescription("T1 Health multiplier while Honor is active. (Vanilla is 2.5)")).Value;
            ModifyEliteTiers.t1HonorHealthEarth = base.Config.Bind<float>(new ConfigDefinition("General", "T1 (Honor) HP - Mending"), 1.5f, new ConfigDescription("Mending Health multiplier while Honor is active. (Vanilla is 1.5)")).Value;
            ModifyEliteTiers.t1HonorDamage = base.Config.Bind<float>(new ConfigDefinition("General", "T1 (Honor) Damage"), 1.5f, new ConfigDescription("T1 Damage multipliers while Honor is active. (Vanilla is 1.5)")).Value;

            ModifyEliteTiers.t2Cost = base.Config.Bind<float>(new ConfigDefinition("General", "T2 Cost"), 36f, new ConfigDescription("T2 Director Cost multiplier. (Vanilla is 36)")).Value;
            ModifyEliteTiers.t2Health = base.Config.Bind<float>(new ConfigDefinition("General", "T2 HP"), 12f, new ConfigDescription("T2 Health multiplier. (Vanilla is 18)")).Value;
            ModifyEliteTiers.t2Damage = base.Config.Bind<float>(new ConfigDefinition("General", "T2 Damage"), 3.5f, new ConfigDescription("T2 Damage multipliers. (Vanilla is 6)")).Value;
            ModifyEliteTiers.t2MinStages = base.Config.Bind<int>(new ConfigDefinition("General", "T2 Min Stages"), 5, new ConfigDescription("Minimum stage completions before T2 elites start spawning. (Vanilla is 5)")).Value;

            EliteVoid.damageBonus = base.Config.Bind<float>(new ConfigDefinition("General", "Voidtouched Damage"), 1.5f, new ConfigDescription("Voidtouched Damage multipliers. (Vanilla is 0.7)")).Value;

            affixBlueEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;
            affixBlueRemoveShield = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Remove Shield"), true, new ConfigDescription("Remove shields from Overloading Enemies.")).Value;
            affixBlueReduceShield = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Reduce Shield"), false, new ConfigDescription("Reduces shields of Overloading Enemies. Requires Remove Shield option to be disabled.")).Value;
            AffixBlue.enableOnHitRework = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "On-Hit Rework"), true, new ConfigDescription("Increases the blast radius of Overloading stickybombs.")).Value;
            AffixBlue.enablePassiveLightning = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Passive Lightning"), true, new ConfigDescription("Overloading elites periodically shoot out lightning nearby.")).Value;
            AffixBluePassiveLightning.scatterBombs = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "On-Hit Scatterbombs"), false, new ConfigDescription("Requires On-Hit Rework. Replaces Overloading stickybombs with a spread of projectiles.")).Value;
            AffixBluePassiveLightning.uncapOnHitLightning = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "On-Hit Scatterbombs - Uncap On-Hit Lightning"), false, new ConfigDescription("Remove the fire rate cap on on-hit scatterbombs (laggy).")).Value;

            affixWhiteEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Glacial", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;

            affixRedEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Blazing", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;
            
            affixPoisonEnabled = base.Config.Bind<bool>(new ConfigDefinition("T2 - Malachite", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;

            affixHauntedEnabled = base.Config.Bind<bool>(new ConfigDefinition("T2 - Celestine", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;
            AffixHaunted.replaceOnHitEffect = base.Config.Bind<bool>(new ConfigDefinition("T2 - Celestine", "New On-Hit Effect"), true, new ConfigDescription("Replaces the Vanilla Slow-on-Hit with an armor reduction penalty.")).Value;

            affixBeadEnabled = base.Config.Bind<bool>(new ConfigDefinition("DLC2 - T2 - Twisted", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;

            eliteEarthEnabled = base.Config.Bind<bool>(new ConfigDefinition("DLC1 - T1 - Mending", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;
            eliteVoidEnabled = base.Config.Bind<bool>(new ConfigDefinition("DLC1 - Voidtouched", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;
            EliteVoid.tweakNullify = base.Config.Bind<bool>(new ConfigDefinition("DLC1 - Voidtouched", "Tweak Nullify"), true, new ConfigDescription("Nullify only takes 2 stacks to root. Requires Voidtouched changes to be eanbled.")).Value;
        }
    }
}
