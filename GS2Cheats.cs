using System.Collections.Generic;
using HarmonyLib;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using GalacticScale;

namespace GalacticScaleCheats
{   
    [BepInPlugin("dsp.galactic-scale.2.cheats", "Galactic Scale 2 Cheats", "1.0.0.0")]
    [BepInDependency("dsp.galactic-scale.2")]
    public class GS2Cheats : BaseUnityPlugin, iConfigurablePlugin
    {
        
        public string Name => "GS2Cheats";
        public string Author => "innominata";
        public string Description => "Some Cheats for DSP";
        public string Version => "1.0.0.0";
        public string GUID => "space.customizing.generators.cheats";
        public new GSGeneratorConfig Config => config; // Return our own generator config when asked, instead of using default config
        public GSOptions Options => options; // Likewise for options

        public void Init()
        {
            GS2.Log("Initializing GS2 Cheats"); // Use Galactic Scales Log system for debugging purposes.

            options.Add(GSUI.Checkbox("Sauce?", true, "sauce", null, "Make the planet look stupid :D"));

        }

       
        public void Import(GSGenPreferences prefs) //This is called on game start, with the same object that was exported last time
        {
            preferences = prefs;
        }
        public GSGenPreferences Export() // Send our custom preferences object to be saved to disk
        {
            return preferences;
        }

        //////////////////////////////////////////////////////////////////////
        ///// All code below here is generator specific
        //////////////////////////////////////////////////////////////////////
        public GSOptions options = new GSOptions();
        private GSGenPreferences preferences = new GSGenPreferences();
        private GSGeneratorConfig config = new GSGeneratorConfig();

        //////////////////////////////
        /// Finally, lets do something
        //////////////////////////////
      

        public new static ManualLogSource Logger;
        private void Awake()
        {
            Logger = new ManualLogSource("GS2Cheats");
            BepInEx.Logging.Logger.Sources.Add(Logger);
            Logger.Log(LogLevel.Message, "Loaded");
            // GS2.Plugins.Add(this);
            // GS2.LoadPreferences();
            GS2.Warn("Test");
            // Harmony.CreateAndPatchAll(typeof(GS2Cheats));
        }


        // [HarmonyPostfix, HarmonyPatch(typeof(UIBuildMenu), "OnVeinBuriedClick")]
        // public static void OnVeinBuriedClick(ref UIBuildMenu __instance)
        // {
        //     var Gen = GS2.GetGeneratorByID("space.customizing.generators.spaghetti") as Spaghetti;
        //     
        //     
        //     if (GSSettings.Stars[0].Name == "Aglio" && GSSettings.Instance.generatorGUID == null) GSSettings.Instance.generatorGUID = "space.customizing.generators.spaghetti";
        //     if (GSSettings.Instance.generatorGUID != "space.customizing.generators.spaghetti") return;
        //     __instance.reformTool.buryVeins = false;
        //     GS2.Log("Click");
        //     if (!Gen.warned)
        //     {
        //         UIMessageBox.Show("Spaghetti".Translate(), "It would be too easy if you could just bury veins!".Translate(), "I agree!".Translate(), 0);
        //         Gen.warned = true;
        //         GS2.SavePreferences();
        //     }
        //     
        //     GameMain.mainPlayer.sandCount += 99999;
        // }
        
    }
}