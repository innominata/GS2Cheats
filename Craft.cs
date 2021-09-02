using GalacticScale;
using HarmonyLib;

namespace GalacticScaleCheats
{
    public partial class GS2Cheats
    {
        //    craftOptions.Add(GSUI.Checkbox("Always Craft".Translate(), false, "alwaysCraft", null, "Never use items to handcraft".Translate()));
        //    craftOptions.Add(GSUI.Checkbox("Instant Craft".Translate(), false, "instantCraft", null, "Handcraft instantly".Translate()));
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechaForge), nameof(MechaForge.TryAddTask))]
        public static void TryAddTask_Postfix(ref bool __result)
        {
            if (!active || !Preferences.GetBool("alwaysCraft")) return;
            __result = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MechaForge), nameof(MechaForge.AddTaskIterate))]
        public static bool AddTaskIterate_Prefix(MechaForge __instance, ref ForgeTask __result, int recipeId, int count)
        {
            if (!active || !Preferences.GetBool("alwaysCraft")) return true;
            ForgeTask recipe = new ForgeTask(recipeId, count);
            for (int i = 0; i < recipe.productIds.Length; i++)
            {
                GameMain.mainPlayer.package.AddItemStacked(recipe.productIds[i], count * recipe.productCounts[i]);
            }
            __result = null;
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MechaForge), "AddTaskIterate")]
        public static void ForgeTaskCreatePostfix(ref MechaForge __instance)
        {
            if (!active || !Preferences.GetBool("instantCraft")) return;
            foreach (var task in __instance.tasks) task.tickSpend = 1;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MechaForge), "PredictTaskCount")]
        public static void PredictTaskCount(ref int __result)
        {
            __result = 999;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(MechaForge), "AddTask")]
        public static void PredictTaskCount2(ref ForgeTask __result)
        {
            if (__result == null) __result = new ForgeTask();
        }
    }
}