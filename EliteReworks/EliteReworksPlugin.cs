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

namespace EliteReworks
{
    [BepInDependency("com.bepis.r2api")]
    [R2API.Utils.R2APISubmoduleDependency(nameof(PrefabAPI), nameof(EffectAPI), nameof(ProjectileAPI), nameof(BuffAPI), nameof(DamageAPI))]
    [BepInPlugin("com.Moffein.EliteReworks", "Elite Reworks", "0.0.4")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class EliteReworksPlugin : BaseUnityPlugin
    {
        public static BuffDef EmptyBuff; //This is a buff that is never used. This is used to stop vanilla elite affix checks from working.
        public static bool modifyStats = true;
        public static bool affixBlueEnabled = true;
        public static bool affixBlueRemoveShield = true;
        public static bool affixWhiteEnabled = true;
        public static bool affixRedEnabled = true;
        public static bool affixHauntedEnabled = true;
        public static bool affixPoisonEnabled = true;


        public void Awake()
        {
            ReadConfig();
            BuildEmptyBuff();
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
            if (affixHauntedEnabled)
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
        }

        public void ReadConfig()
        {
            //resizeElites = base.Config.Bind<bool>(new ConfigDefinition("General", "Resize Elites"), true, new ConfigDescription("Elites are bigger.")).Value; //Buggy.
            modifyStats = base.Config.Bind<bool>(new ConfigDefinition("General", "Modify Scaling"), true, new ConfigDescription("Modify Elite Cost/HP/Damage multipliers.")).Value;

            affixBlueRemoveShield = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Remove Shield"), true, new ConfigDescription("Remove shields from Overloading Enemies.")).Value;
            affixBlueEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Overloading", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;

            affixWhiteEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Glacial", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;

            affixRedEnabled = base.Config.Bind<bool>(new ConfigDefinition("T1 - Blazing", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;
            
            affixPoisonEnabled = base.Config.Bind<bool>(new ConfigDefinition("T2 - Malachite", "Enable Changes"), true, new ConfigDescription("Enable changes to this elite type.")).Value;

            affixHauntedEnabled = base.Config.Bind<bool>(new ConfigDefinition("T2 - Celestine", "Simple Indicator"), false, new ConfigDescription("Replace the bubble with a less obtrusive indicator.")).Value;
        }

        public void BuildEmptyBuff()
        {
            EmptyBuff = ScriptableObject.CreateInstance<BuffDef>();
            EmptyBuff.name = "MoffeinEliteReworksEmptyBuff";
            BuffAPI.Add(new CustomBuff(EmptyBuff));
        }
    }
}
