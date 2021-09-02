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
        public static bool AddTaskIterate_Prefix(MechaForge __instance, ForgeTask __result, int recipeId, int count)
        {
            if (!active || !Preferences.GetBool("instantCraft")) return true;
            ForgeTask recipe = new ForgeTask(recipeId, count);
            for (int i = 0; i < recipe.productIds.Length; i++)
            {
                GameMain.mainPlayer.package.AddItemStacked(recipe.productIds[i], count);
            }
            __result = null;
            return false;
        }
    }
}