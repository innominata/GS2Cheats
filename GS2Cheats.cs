using System;
using BepInEx;
using BepInEx.Logging;
using GalacticScale;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace GalacticScaleCheats
{
    [BepInPlugin("dsp.galactic-scale.2.cheats", "Galactic Scale 2 Cheats", "1.0.0.3")]
    [BepInDependency("dsp.galactic-scale.2")]
    public partial class GS2Cheats : BaseUnityPlugin, iConfigurablePlugin
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
        private bool recipesDirty = true;

        private RecipeProto[] recipesProto;
        public new GSGeneratorConfig Config { get; } = new GSGeneratorConfig();

        public static GSGenPreferences Preferences => instance.preferences;

        private static bool active => Preferences.GetBool("Enabled", true);

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

        public bool Enabled
        {
            get
            {
                if (preferences.ContainsKey("Enabled")) GS2.Warn("Contains");
                else GS2.Warn("Doesnt contain");
                return Preferences.GetBool("Enabled", true);
            }
            set
            {
                GS2.Warn($"Setting Enabled:{value}");
                preferences.Set("Enabled", value);
            }
        }

        public void Import(GSGenPreferences prefs) //This is called on game start, with the same object that was exported last time
        {
            // GS2.Warn("Import");
            // GS2.WarnJson(preferences.GetString("Enabled"));
            preferences = prefs;
            // GS2.WarnJson(preferences.GetString("Enabled"));
        }

        public GSGenPreferences Export() // Send our custom preferences object to be saved to disk
        {
            // GS2.Warn("Exporting");
            // GS2.WarnJson(preferences.GetString("Enabled"));
            preferences.Set("Enabled", active);
            return preferences;
        }

        public void Init()
        {
            GS2.Log("Initializing GS2 Cheats"); // Use Galactic Scales Log system for debugging purposes.
            options.Add(GSUI.Spacer());
            CreateRecipeOptions();
            //options.Add(GSUI.Spacer());
            
            var playerOptions = new GSOptions();
            playerOptions.Add(GSUI.Checkbox("Unlimited Energy".Translate(), false, "unlimitedEnergy", null, "Never run out of core power".Translate()));
            playerOptions.Add(GSUI.Checkbox("Unlock Sail".Translate(), false, "unlockSail", null, "Start game able to fly".Translate()));
            playerOptions.Add(GSUI.Checkbox("Unlock Warp".Translate(), false, "unlockWarp", null, "Start game able to warp".Translate()));
            playerOptions.Add(GSUI.Checkbox("Always Warp".Translate(), false, "alwaysWarp", null, "Never require warpers to warp".Translate()));
            playerOptions.Add(GSUI.Checkbox("Boost Walk Speed".Translate(), false, "boostWalkSpeed", null, "Walk faster".Translate()));
            playerOptions.Add(GSUI.Checkbox("Boost Sail Speed".Translate(), false, "boostSailSpeed", null, "Sail faster".Translate()));
            playerOptions.Add(GSUI.Checkbox("Boost Warp Speed".Translate(), false, "boostWarpSpeed", null, "Warp faster".Translate()));
            options.Add(GSUI.Group("Mecha".Translate(), playerOptions, "Settings which affect Icarus".Translate()));
            //options.Add(GSUI.Spacer());
            var researchOptions = new GSOptions();
            researchOptions.Add(GSUI.Checkbox("Unlock All".Translate(), false, "unlockAll", (o) => {if (o && GameMain.isPaused && !GS2.IsMenuDemo) UnlockTech();}, "Research everything instantly".Translate()));
            researchOptions.Add(GSUI.Checkbox("Free Research".Translate(), false, "freeResearch", null, "Unlock research when clicked".Translate()));
            researchOptions.Add(GSUI.Slider("Research Speed Multiplier".Translate(), 0.1f, 1f, 20f, 0.1f, "labSpeed", null, "Speed labs consume matrices".Translate()));
            options.Add(GSUI.Group("Research".Translate(), researchOptions, "Settings which affect research".Translate()));
            //options.Add(GSUI.Spacer());
            var craftOptions = new GSOptions();
            craftOptions.Add(GSUI.Checkbox("Always Craft".Translate(), false, "alwaysCraft", null, "Never use items to handcraft".Translate()));
            craftOptions.Add(GSUI.Checkbox("Instant Craft".Translate(), false, "instantCraft", null, "Handcraft instantly".Translate()));
            options.Add(GSUI.Group("Crafting".Translate(), craftOptions, "Settings which affectHandcrafting".Translate()));
            //options.Add(GSUI.Spacer());
            var sphereOptions = new GSOptions();
            sphereOptions.Add(GSUI.Checkbox("Unlock Dyson Spheres".Translate(), false, "unlockDyson", null, "Start with the dyson sphere system unlocked".Translate()));
            sphereOptions.Add(GSUI.Checkbox("Start with Full Sphere".Translate(), false, "fullSphere", null, "Remove tech requirement to access all latitudes".Translate()));
            options.Add(GSUI.Group("Dyson Sphere".Translate(), sphereOptions, "Settings which affect Dyson Spheres".Translate()));
            //options.Add(GSUI.Spacer());
            var factoryOptions = new GSOptions();
            factoryOptions.Add(GSUI.Checkbox("Fast Build".Translate(), false, "fastBuild", null, "Drones are lightning fast".Translate()));
            options.Add(GSUI.Group("Factory".Translate(), factoryOptions, "Settings which affect Dyson Spheres".Translate()));
            options.Add(GSUI.Spacer());
        }

        private void CreateRecipeOptions()
        {
            var recipeOptions = new GSOptions();
            var rOptions = new GSOptions();
            rOptions.Add(GSUI.Slider("Assembler Multiplier".Translate(), 0.5f, 1f, 10f, 0.5f, "assemblerMulti", o => recipesDirty = true, "Increase item count produced from assemblers".Translate()));
            rOptions.Add(GSUI.Slider("Smelter Multiplier".Translate(), 0.5f, 1f, 10f, 0.5f, "smelterMulti", o => recipesDirty = true, "Increase item count produced from smelters".Translate()));
            rOptions.Add(GSUI.Slider("Oil Refinery Multiplier".Translate(), 0.5f, 1f, 10f, 0.5f, "refineryMulti", o => recipesDirty = true, "Increase item count produced from oil refineries".Translate()));
            rOptions.Add(GSUI.Slider("Chemical Plant Multiplier".Translate(), 0.5f, 1f, 10f, 0.5f, "chemicalPlantMulti", o => recipesDirty = true, "Increase item count produced from chemical plants".Translate()));
            rOptions.Add(GSUI.Slider("Laboratory Multiplier".Translate(), 0.5f, 1f, 5f, 0.5f, "laboratoryMulti", o => recipesDirty = true, "Increase item count produced from laboratories".Translate()));
            rOptions.Add(GSUI.Slider("Particle Accelerator Multiplier".Translate(), 0.5f, 1f, 10f, 0.5f, "particleMulti", o => recipesDirty = true, "Increase item count produced from accelerators".Translate()));
            rOptions.Add(GSUI.Slider("Photon Reciever Multiplier".Translate(), 0.5f, 1f, 10f, 0.5f, "photonMulti", o => recipesDirty = true, "Increase item count produced from ray recievers".Translate()));
            rOptions.Add(GSUI.Slider("Energy Exchange Multiplier".Translate(), 0.5f, 1f, 10f, 0.5f, "exchangeMulti", o => recipesDirty = true, "Increase item count produced from energy exchangers".Translate()));
            rOptions.Add(GSUI.Slider("Fractionator Multiplier".Translate(), 0.5f, 1f, 10f, 0.5f, "fractionatorMulti", o => recipesDirty = true, "Increase item count produced from fractionators".Translate()));
            recipeOptions.Add(GSUI.Group("Item Output Multipliers", rOptions, "Alter the output from assemblers etc".Translate()));
            recipeOptions.Add(GSUI.Spacer());

            var iOptions = new GSOptions();
            iOptions.Add(GSUI.Slider("Assembler Input Multiplier".Translate(), 0.25f, 1f, 4f, 0.25f, "assemblerInputMulti", o => recipesDirty = true, "Decrease item count consumed by assemblers".Translate()));
            iOptions.Add(GSUI.Slider("Smelter Input Multiplier".Translate(), 0.25f, 1f, 4f, 0.25f, "smelterInputMulti", o => recipesDirty = true, "Decrease item count consumed by smelters".Translate()));
            iOptions.Add(GSUI.Slider("Oil Refinery Input Multiplier".Translate(), 0.25f, 1f, 4, 0.25f, "refineryInputMulti", o => recipesDirty = true, "Decrease item count consumed by oil refineries".Translate()));
            iOptions.Add(GSUI.Slider("Chemical Plant Input Multiplier".Translate(), 0.25f, 1f, 4, 0.25f, "chemicalPlantInputMulti", o => recipesDirty = true, "Decrease item count consumed by chemical plants".Translate()));
            iOptions.Add(GSUI.Slider("Laboratory Input Multiplier".Translate(), 0f, 1f, 4, 0.25f, "laboratoryInputMulti", o => recipesDirty = true, "Decrease item count consumed by laboratories".Translate()));
            iOptions.Add(GSUI.Slider("Particle Accelerator Input Multiplier".Translate(), 0.25f, 1f, 4, 0.25f, "particleInputMulti", o => recipesDirty = true, "Decrease item count consumed by accelerators".Translate()));
            iOptions.Add(GSUI.Slider("Photon Reciever Input Multiplier".Translate(), 0.25f, 1f, 4, 0.25f, "photonInputMulti", o => recipesDirty = true, "Decrease item count consumed by ray recievers".Translate()));
            iOptions.Add(GSUI.Slider("Energy Exchange Input Multiplier".Translate(), 0.25f, 1f, 4, 0.25f, "exchangeInputMulti", o => recipesDirty = true, "Decrease item count consumed by energy exchangers".Translate()));
            iOptions.Add(GSUI.Slider("Fractionator Input Multiplier".Translate(), 0.25f, 1f, 4, 0.25f, "fractionatorInputMulti", o => recipesDirty = true, "Decrease item count consumed by fractionators".Translate()));
            recipeOptions.Add(GSUI.Group("Item Input Multipliers", iOptions, "Alter the required items for assemblers etc".Translate()));
            recipeOptions.Add(GSUI.Spacer());

            var sOptions = new GSOptions();
            sOptions.Add(GSUI.Slider("Assembler Speed Multiplier".Translate(), 0.0f, 1f, 4f, 0.1f, "assemblerSpeedMulti", o => recipesDirty = true, "Increase speed of assemblers".Translate()));
            sOptions.Add(GSUI.Slider("Smelter Speed Multiplier".Translate(), 0.0f, 1f, 4f, 0.1f, "smelterSpeedMulti", o => recipesDirty = true, "Increase speed of smelters".Translate()));
            sOptions.Add(GSUI.Slider("Oil Refinery Speed Multiplier".Translate(), 0.0f, 1f, 4, 0.1f, "refinerySpeedMulti", o => recipesDirty = true, "Increase speed of oil refineries".Translate()));
            sOptions.Add(GSUI.Slider("Chemical Plant Speed Multiplier".Translate(), 0.0f, 1f, 4, 0.1f, "chemicalPlantSpeedMulti", o => recipesDirty = true, "Increase speed of chemical plants".Translate()));
            sOptions.Add(GSUI.Slider("Laboratory Speed Multiplier".Translate(), 0.0f, 1f, 4, 0.1f, "laboratorySpeedMulti", o => recipesDirty = true, "Increase speed of laboratories".Translate()));
            sOptions.Add(GSUI.Slider("Particle Accelerator Speed Multiplier".Translate(), 0.1f, 1f, 4, 0.1f, "particleSpeedMulti", o => recipesDirty = true, "Increase speed of accelerators".Translate()));
            sOptions.Add(GSUI.Slider("Photon Reciever Speed Multiplier".Translate(), 0.0f, 1f, 4, 0.1f, "photonSpeedMulti", o => recipesDirty = true, "Increase speed of ray recievers".Translate()));
            sOptions.Add(GSUI.Slider("Energy Exchange Speed Multiplier".Translate(), 0.0f, 1f, 4, 0.1f, "exchangeSpeedMulti", o => recipesDirty = true, "Increase speed of energy exchangers".Translate()));
            sOptions.Add(GSUI.Slider("Fractionator Speed Multiplier".Translate(), 0.0f, 1f, 4, 0.1f, "fractionatorSpeedMulti", o => recipesDirty = true, "Increase speed of by fractionators".Translate()));
            recipeOptions.Add(GSUI.Group("Speed Multipliers".Translate(), sOptions, "Alter the speed of assemblers etc".Translate()));
            options.Add(GSUI.Group("Recipe Settings".Translate(), recipeOptions, "Alter item recipes".Translate()));

            BackupProtos();
        }


        public void BackupProtos()
        {
            recipesProto = LDB.recipes.dataArray.Copy();
        }

        public void ProcessProtos(Val o)
        {
            if (recipesDirty)
            {
                recipesDirty = false;
                var assemblerMulti = instance.preferences.GetFloat("assemblerMulti", 1);
                SetRecipeResultCounts(assemblerMulti, ERecipeType.Assemble);

                var laboratoryMulti = instance.preferences.GetFloat("laboratoryMulti", 1);
                SetRecipeResultCounts(laboratoryMulti, ERecipeType.Research);

                var smelterMulti = instance.preferences.GetFloat("smelterMulti", 1);
                SetRecipeResultCounts(smelterMulti, ERecipeType.Smelt);

                var chemicalPlantMulti = instance.preferences.GetFloat("chemicalPlantMulti", 1);
                SetRecipeResultCounts(chemicalPlantMulti, ERecipeType.Chemical);

                var refineryMulti = instance.preferences.GetFloat("refineryMulti", 1);
                SetRecipeResultCounts(refineryMulti, ERecipeType.Refine);

                var particleMulti = instance.preferences.GetFloat("particleMulti", 1);
                SetRecipeResultCounts(particleMulti, ERecipeType.Particle);

                var photonMulti = instance.preferences.GetFloat("photonMulti", 1);
                SetRecipeResultCounts(photonMulti, ERecipeType.PhotonStore);

                var fractionatorMulti = instance.preferences.GetFloat("fractionatorMulti", 1);
                SetRecipeResultCounts(fractionatorMulti, ERecipeType.Fractionate);

                var exchangeMulti = instance.preferences.GetFloat("exchangeMulti", 1);
                SetRecipeResultCounts(exchangeMulti, ERecipeType.Exchange);


                var assemblerInputMulti = instance.preferences.GetFloat("assemblerInputMulti", 1);
                SetRecipeInputCounts(assemblerInputMulti, ERecipeType.Assemble);
                var laboratoryInputMulti = instance.preferences.GetFloat("laboratoryInputMulti", 1);
                SetRecipeInputCounts(laboratoryInputMulti, ERecipeType.Research);
                var smelterInputMulti = instance.preferences.GetFloat("smelterInputMulti", 1);
                SetRecipeInputCounts(smelterInputMulti, ERecipeType.Smelt);
                var chemicalPlantInputMulti = instance.preferences.GetFloat("chemicalPlantInputMulti", 1);
                SetRecipeInputCounts(chemicalPlantInputMulti, ERecipeType.Chemical);
                var refineryInputMulti = instance.preferences.GetFloat("refineryInputMulti", 1);
                SetRecipeInputCounts(refineryInputMulti, ERecipeType.Refine);
                var particleInputMulti = instance.preferences.GetFloat("particleInputMulti", 1);
                SetRecipeInputCounts(particleInputMulti, ERecipeType.Particle);
                var photonInputMulti = instance.preferences.GetFloat("photonInputMulti", 1);
                SetRecipeInputCounts(photonInputMulti, ERecipeType.PhotonStore);
                var fractionatorInputMulti = instance.preferences.GetFloat("fractionatorInputMulti", 1);
                SetRecipeInputCounts(fractionatorInputMulti, ERecipeType.Fractionate);
                var exchangeInputMulti = instance.preferences.GetFloat("exchangeInputMulti", 1);
                SetRecipeInputCounts(exchangeInputMulti, ERecipeType.Exchange);

                var assemblerSpeedMulti = instance.preferences.GetFloat("assemblerSpeedMulti", 1);
                SetRecipeSpeeds(assemblerSpeedMulti, ERecipeType.Assemble);
                var laboratorySpeedMulti = instance.preferences.GetFloat("laboratorySpeedMulti", 1);
                SetRecipeSpeeds(laboratorySpeedMulti, ERecipeType.Research);
                var smelterSpeedMulti = instance.preferences.GetFloat("smelterSpeedMulti", 1);
                SetRecipeSpeeds(smelterSpeedMulti, ERecipeType.Smelt);
                var chemicalPlantSpeedMulti = instance.preferences.GetFloat("chemicalPlantSpeedMulti", 1);
                SetRecipeSpeeds(chemicalPlantSpeedMulti, ERecipeType.Chemical);
                var refinerySpeedMulti = instance.preferences.GetFloat("refinerySpeedMulti", 1);
                SetRecipeSpeeds(refinerySpeedMulti, ERecipeType.Refine);
                var particleSpeedMulti = instance.preferences.GetFloat("particleSpeedMulti", 1);
                SetRecipeSpeeds(particleSpeedMulti, ERecipeType.Particle);
                var photonSpeedMulti = instance.preferences.GetFloat("photonSpeedMulti", 1);
                SetRecipeSpeeds(photonSpeedMulti, ERecipeType.PhotonStore);
                var fractionatorSpeedMulti = instance.preferences.GetFloat("fractionatorSpeedMulti", 1);
                SetRecipeSpeeds(fractionatorSpeedMulti, ERecipeType.Fractionate);
                var exchangeSpeedMulti = instance.preferences.GetFloat("exchangeSpeedMulti", 1);
                SetRecipeSpeeds(exchangeSpeedMulti, ERecipeType.Exchange);
                if (!DSPGame.IsMenuDemo)
                {
                    GS2.Warn("Game Running");
                    foreach (var factory in GameMain.data.factories)
                    {
                        GS2.Warn("Processing Factory:");
                        if (factory is null) continue;
                        if (factory.factorySystem != null)
                        {
                            if (factory.factorySystem.assemblerPool != null)
                                foreach (var assembler in factory.factorySystem.assemblerPool)
                                    assembler.SetRecipe(assembler.recipeId, factory.entitySignPool);
                            if (factory.factorySystem.labPool != null)
                                foreach (var lab in factory.factorySystem.labPool)
                                    if (lab.researchMode == false)
                                    {
                                        var id = lab.recipeId;
                                        lab.SetFunction(false, 0, 0, factory.entitySignPool);
                                        lab.SetFunction(false, id, 0, factory.entitySignPool);
                                    }
                        }
                    }
                }
            }
        }

        public void SetRecipeResultCounts(float multi, ERecipeType type)
        {
            if (!active) return;
            for (var i = 0; i < LDB.recipes.dataArray.Length; i++)
            for (var j = 0; j < LDB.recipes.dataArray[i].ResultCounts.Length; j++)
                if (LDB.recipes.dataArray[i].Type == type)
                    LDB.recipes.dataArray[i].ResultCounts[j] = Mathf.CeilToInt(recipesProto[i].ResultCounts[j] * multi);
        }

        public void SetRecipeInputCounts(float multi, ERecipeType type)
        {
            if (!active) return;

            for (var i = 0; i < LDB.recipes.dataArray.Length; i++)
            for (var j = 0; j < LDB.recipes.dataArray[i].ItemCounts.Length; j++)
            {
                var newCount = Mathf.RoundToInt(recipesProto[i].ItemCounts[j] * multi);
                if (newCount < 1) newCount = 1;
                if (LDB.recipes.dataArray[i].Type == type) LDB.recipes.dataArray[i].ItemCounts[j] = newCount;
            }
        }

        public void SetRecipeSpeeds(float multi, ERecipeType type)
        {
            for (var i = 0; i < LDB.recipes.dataArray.Length; i++)
            {
                if (LDB.recipes.dataArray[i].Type == ERecipeType.Research && type == ERecipeType.Research && LDB.recipes.dataArray[i].ID == 101) GS2.WarnJson(LDB.recipes.dataArray[i]);
                if (LDB.recipes.dataArray[i].Type == type) LDB.recipes.dataArray[i].TimeSpend = Mathf.CeilToInt(recipesProto[i].TimeSpend * multi);
                if (LDB.recipes.dataArray[i].Type == ERecipeType.Research && type == ERecipeType.Research && LDB.recipes.dataArray[i].ID == 101) GS2.WarnJson(LDB.recipes.dataArray[i]);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIOptionWindow), "OnApplyClick")]
        public static void Apply()
        {
            if (!active) return;
            instance.ProcessProtos(null);
        }

        void FixedUpdate()
        {
            if (active)
            {
                if (!GS2.IsMenuDemo && GameMain.isRunning && !GameMain.isPaused)
                {
                    if (GameMain.mainPlayer != null)
                    {
                        MechaUpdate();
                    }
                }
            }
        }
    }
}