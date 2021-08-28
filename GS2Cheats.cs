using System;
using BepInEx;
using BepInEx.Logging;
using GalacticScale;
using HarmonyLib;

namespace GalacticScaleCheats
{
    [BepInPlugin("dsp.galactic-scale.2.cheats", "Galactic Scale 2 Cheats", "1.0.0.0")]
    [BepInDependency("dsp.galactic-scale.2")]
    public class GS2Cheats : BaseUnityPlugin, iConfigurablePlugin
    {
        //////////////////////////////
        /// Finally, lets do something
        //////////////////////////////
        public static GS2Cheats instance;

        public new static ManualLogSource Logger;

        //////////////////////////////////////////////////////////////////////
        ///// All code below here is generator specific
        //////////////////////////////////////////////////////////////////////
        public GSOptions options = new GSOptions();
        public GSGenPreferences preferences = new GSGenPreferences();
        public new GSGeneratorConfig Config { get; } = new GSGeneratorConfig();

        public static GSGenPreferences Preferences => instance.preferences;

        private void Awake()
        {
            Logger = new ManualLogSource("GS2Cheats");
            BepInEx.Logging.Logger.Sources.Add(Logger);
            Logger.Log(LogLevel.Message, "Loaded");
            GS2.Plugins.Add(this);
            instance = this;
            instance.Init();
            // GS2.LoadPreferences();
            // GS2.Warn("Test");
            Harmony.CreateAndPatchAll(typeof(GS2Cheats));
        }

        public string Name => "GS2Cheats";
        public string Author => "innominata";
        public string Description => "Some Cheats for DSP";
        public string Version => "1.0.0.0";
        public string GUID => "space.customizing.generators.cheats";
        public GSOptions Options => options; // Likewise for options

        public void Init()
        {
            GS2.Log("Initializing GS2 Cheats"); // Use Galactic Scales Log system for debugging purposes.
            options.Add(GSUI.Spacer());
            options.Add(GSUI.Slider("Assembler Multiplier".Translate(), 1f, 1f, 10f, 1f, "assemblerMulti", o => GS2.Log(o), "Increase item count produced from assemblers".Translate()));
            options.Add(GSUI.Slider("Smelter Multiplier".Translate(), 1f, 1f, 10f, 1f, "smelterMulti", null, "Increase item count produced from smelters".Translate()));
            options.Add(GSUI.Slider("Oil Refinery Multiplier".Translate(), 1f, 1f, 10f, 1f, "refineryMulti", null, "Increase item count produced from oil refineries".Translate()));
            options.Add(GSUI.Slider("Chemical Plant Multiplier".Translate(), 1f, 1f, 10f, 1f, "chemicalPlantMulti", null, "Increase item count produced from chemical plants".Translate()));
            options.Add(GSUI.Slider("Laboratory Multiplier".Translate(), 1f, 1f, 10f, 1f, "laboratoryMulti", null, "Increase item count produced from laboratories".Translate()));
            // options.Add(GSUI.Slider("Miner Multiplier".Translate(), 1f, 1f, 20f, 1f, "minerMulti", null, "Increase item count produced from miners".Translate()));
            options.Add(GSUI.Spacer());
        }


        public void Import(GSGenPreferences prefs) //This is called on game start, with the same object that was exported last time
        {
            preferences = prefs;
        }

        public GSGenPreferences Export() // Send our custom preferences object to be saved to disk
        {
            return preferences;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIRecipeEntry), "SetRecipe")]
        public static void SetRecipe2(ref UIRecipeEntry __instance, RecipeProto recipe)
        {
            GS2.WarnJson(instance.preferences);
            var assemblerMulti = Preferences.GetInt("assemblerMulti", 1);
            var smelterMulti = Preferences.GetInt("smelterMulti", 1);
            var chemicalPlantMulti = Preferences.GetInt("chemicalPlantMulti", 1);
            var laboratoryMulti = Preferences.GetInt("laboratoryMulti", 1);
            var minerMulti = Preferences.GetInt("minerMulti", 1);
            var refineryMulti = Preferences.GetInt("refineryMulti", 1);
            GS2.Warn(assemblerMulti + " " + smelterMulti);
            for (var i = 0; i < recipe.ResultCounts.Length; i++)
                if (recipe.Type == ERecipeType.Assemble)
                {
                    // GS2.Warn($"Setting {recipe.ResultCounts[0]}");
                    __instance.countTexts[0].text = (recipe.ResultCounts[0] * assemblerMulti).ToString();
                }
                else if (recipe.Type == ERecipeType.Chemical)
                {
                    // GS2.Warn($"Setting {recipe.ResultCounts[0]}");
                    __instance.countTexts[0].text = (recipe.ResultCounts[0] * chemicalPlantMulti).ToString();
                }
                else if (recipe.Type == ERecipeType.Smelt)
                {
                    // GS2.Warn($"Setting {recipe.ResultCounts[0]}");
                    __instance.countTexts[0].text = (recipe.ResultCounts[0] * smelterMulti).ToString();
                }
                else if (recipe.Type == ERecipeType.Research)
                {
                    // GS2.Warn($"Setting {LDB.items.Select(recipe.Results[0]).name} {recipe.ResultCounts[0]}");
                    __instance.countTexts[0].text = (recipe.ResultCounts[0] * laboratoryMulti).ToString();
                }
                else if (recipe.Type == ERecipeType.Refine)
                {
                    // GS2.Warn($"Setting {recipe.ResultCounts[0]}");
                    __instance.countTexts[0].text = (recipe.ResultCounts[0] * refineryMulti).ToString();
                }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AssemblerComponent), "SetRecipe")]
        public static void SetRecipe(ref AssemblerComponent __instance, int recpId, SignData[] signPool)
        {
            // GS2.Warn("Set Recipe");
            var assemblerMulti = Preferences.GetInt("assemblerMulti", 1);
            var smelterMulti = Preferences.GetInt("smelterMulti", 1);
            var chemicalPlantMulti = Preferences.GetInt("chemicalPlantMulti", 1);
            var minerMulti = Preferences.GetInt("minerMulti", 1);
            var refineryMulti = Preferences.GetInt("refineryMulti", 1);
            RecipeProto recipeProto = null;
            if (recpId > 0) recipeProto = LDB.recipes.Select(recpId);
            if (recipeProto != null)
            {
                // GS2.Warn("Not Null");
                __instance.timeSpend = recipeProto.TimeSpend * 10000;
                __instance.requires = new int[recipeProto.Items.Length];
                Array.Copy(recipeProto.Items, __instance.requires, __instance.requires.Length);
                __instance.requireCounts = new int[recipeProto.ItemCounts.Length];
                Array.Copy(recipeProto.ItemCounts, __instance.requireCounts, __instance.requireCounts.Length);
                __instance.served = new int[__instance.requireCounts.Length];
                Assert.True(__instance.requires.Length == __instance.requireCounts.Length);
                __instance.needs = new int[6];
                __instance.products = new int[recipeProto.Results.Length];
                Array.Copy(recipeProto.Results, __instance.products, __instance.products.Length);
                __instance.productCounts = new int[recipeProto.ResultCounts.Length];
                Array.Copy(recipeProto.ResultCounts, __instance.productCounts, __instance.productCounts.Length);
                for (var i = 0; i < __instance.productCounts.Length; i++)
                    if (__instance.recipeType == ERecipeType.Assemble)
                    {
                        // GS2.Warn($"Increasing {recipeProto.name} {__instance.productCounts[i]}");
                        __instance.productCounts[i] *= assemblerMulti;
                    }
                    else if (__instance.recipeType == ERecipeType.Smelt)
                    {
                        __instance.productCounts[i] *= smelterMulti;
                    }
                    else if (__instance.recipeType == ERecipeType.Refine)
                    {
                        __instance.productCounts[i] *= refineryMulti;
                    }
                    else if (__instance.recipeType == ERecipeType.Chemical)
                    {
                        __instance.productCounts[i] *= chemicalPlantMulti;
                    }

                __instance.produced = new int[__instance.productCounts.Length];
                Assert.True(__instance.products.Length == __instance.productCounts.Length);
                signPool[__instance.entityId].iconId0 = (uint)__instance.recipeId;
                signPool[__instance.entityId].iconType = 2U;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AssemblerComponent), "Import")]
        public static void SetRecipe(ref AssemblerComponent __instance)
        {
            var assemblerMulti = Preferences.GetInt("assemblerMulti", 1);
            var smelterMulti = Preferences.GetInt("smelterMulti", 1);
            var chemicalPlantMulti = Preferences.GetInt("chemicalPlantMulti", 1);
            var minerMulti = Preferences.GetInt("minerMulti", 1);
            var refineryMulti = Preferences.GetInt("refineryMulti", 1);
            if (__instance.productCounts == null) return;
            // GS2.Warn("Import");
            for (var i = 0; i < __instance.productCounts.Length; i++)
                if (__instance.recipeType == ERecipeType.Assemble)
                    __instance.productCounts[i] *= assemblerMulti;
                else if (__instance.recipeType == ERecipeType.Smelt)
                    __instance.productCounts[i] *= smelterMulti;
                else if (__instance.recipeType == ERecipeType.Refine)
                    __instance.productCounts[i] *= refineryMulti;
                else if (__instance.recipeType == ERecipeType.Chemical) __instance.productCounts[i] *= chemicalPlantMulti;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LabComponent), "SetFunction")]
        public static void SetFunction(ref LabComponent __instance)
        {
            // GS2.Warn("Set Recipe");
            if (__instance.productCounts == null) return;
            var laboratoryMulti = Preferences.GetInt("laboratoryMulti", 1);

            for (var i = 0; i < __instance.productCounts.Length; i++)
                if (__instance.researchMode == false)
                    __instance.productCounts[i] *= laboratoryMulti;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LabComponent), "Import")]
        public static void LabImport(ref LabComponent __instance)
        {
            var laboratoryMulti = Preferences.GetInt("laboratoryMulti", 1);
            if (__instance.productCounts == null) return;
            // GS2.Warn("Import");
            for (var i = 0; i < __instance.productCounts.Length; i++)
                if (__instance.researchMode == false)
                    __instance.productCounts[i] *= laboratoryMulti;
        }
    }
}