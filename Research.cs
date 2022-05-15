using System;
using System.Linq;
using HarmonyLib;
using static GalacticScale.GS2;

namespace GalacticScaleCheats
{
    public partial class GS2Cheats
    {
        // All credit to Windows10CE
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameHistoryData), "EnqueueTech")]
        public static void EnqueueTech_Postfix(GameHistoryData __instance, int techId)
        {
            if (!active || !Preferences.GetBool("freeResearch")) return;
            __instance.UnlockTech(techId);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITechNode), "DoStartTech")]
        public static void StartTechPostfix(ref UITechNode __instance)
        {
            if (!active || !Preferences.GetBool("freeResearch")) return;
            if ((__instance.techProto?.ID ?? 1) == 1) return;
            UnlockTechRecursive(__instance.techProto.ID, GameMain.history);
            GameMain.history.DequeueTech();
        }

        public static void UnlockTech()
        {
            Log("Unlocking Tech");
            foreach (var tech in LDB.techs.dataArray.Where(x => x.Published))
                if (!GameMain.history.TechUnlocked(tech.ID))
                    UnlockTechRecursive(tech.ID, GameMain.history);
            ResearchUnlocked = true;
        }

        private static void UnlockTechRecursive(int techId, GameHistoryData history)
        {
            //GS2.Warn($"UnlockTechRecursive {techId} {history != null}");
            var state = history.TechState(techId);
            var proto = LDB.techs.Select(techId);
            try
            {
                foreach (var techReq in proto.PreTechs)
                    if (!history.TechState(techReq).unlocked)
                        UnlockTechRecursive(techReq, history);
                foreach (var techReq in proto.PreTechsImplicit)
                    if (!history.TechState(techReq).unlocked)
                        UnlockTechRecursive(techReq, history);
                foreach (var itemReq in proto.itemArray)
                    if (itemReq.preTech != null && !history.TechState(itemReq.preTech.ID).unlocked)
                        UnlockTechRecursive(itemReq.preTech.ID, history);

                var current = state.curLevel;
                for (; current < state.maxLevel; current++)
                for (var j = 0; j < proto.UnlockFunctions.Length; j++)
                    history.UnlockTechFunction(proto.UnlockFunctions[j], proto.UnlockValues[j], current);

                history.UnlockTech(techId);
            }
            catch (Exception e)
            {
                Log("Techunlock exception caught: " + e.Message);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameHistoryData), "PreTechUnlocked")]
        public static bool PreTechUnlockedPrefix(ref bool __result)
        {
            if (!active || !Preferences.GetBool("freeResearch")) return true;
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameHistoryData), "ImplicitPreTechRequired")]
        public static bool ImplicitPretechReqPrefix(ref int __result)
        {
            if (!active || !Preferences.GetBool("freeResearch")) return true;
            __result = 0;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameHistoryData), "CanEnqueueTech")]
        public static bool CanEnqueueTechPrefix(ref bool __result)
        {
            if (!active || !Preferences.GetBool("freeResearch")) return true;
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameHistoryData), "CanEnqueueTechIgnoreFull")]
        public static bool CanEnqueueTechIgnoreFullPrefix(ref bool __result)
        {
            if (!active || !Preferences.GetBool("freeResearch")) return true;
            __result = true;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData), "InitLandingPlace")]
        public static void InitLandingPlace()
        {
            if (!active || !Preferences.GetBool("unlockAll")) return;
            Warn("Unlocking Tech");
            UnlockTech();
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(LabComponent), "InternalUpdateResearch")]
        public static bool InternalUpdateResearch(float power, ref float speed, int[] consumeRegister, ref TechState ts, ref int techHashedThisFrame, ref long uMatrixPoint, ref long hashRegister)
        {
            if (!active || Preferences.GetFloat("labSpeed", 1f) == 1f) return true;
            // Warn("Editing " + speed);
            speed = Preferences.GetFloat("labSpeed", 1f);
            // Warn("Edited " + speed + " " + Preferences.GetFloat("labSpeed", 1f));
            return true;
        }
    }
}