using System;
using GalacticScale;
using HarmonyLib;
using UnityEngine;

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
            var recipe = new ForgeTask(recipeId, count);
            for (var i = 0; i < recipe.productIds.Length; i++) GameMain.mainPlayer.package.AddItemStacked(recipe.productIds[i], count * recipe.productCounts[i]);
            __result = null;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechaForge), "AddTaskIterate")]
        public static void ForgeTaskCreatePostfix(ref MechaForge __instance)
        {
            if (!active || !Preferences.GetBool("instantCraft")) return;
            foreach (var task in __instance.tasks) task.tickSpend = 1;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechaForge), "PredictTaskCount")]
        public static void PredictTaskCount(ref int __result)
        {
            if (!active || !Preferences.GetBool("instantCraft")) return;
            __result = 999;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechaForge), "AddTask")]
        public static void PredictTaskCount2(ref ForgeTask __result)
        {
            if (!active || !Preferences.GetBool("instantCraft")) return;
            if (__result == null) __result = new ForgeTask();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIReplicatorWindow), "RefreshRecipeIcons")]
        public static bool RefreshRecipeIcons(ref UIReplicatorWindow __instance)
        {
            if (!active || !Preferences.GetBool("alwaysCraft")) return true;
            GS2.WarnJson("RefreshingRecipeIcons");
            Array.Clear(__instance.recipeIndexArray, 0, __instance.recipeIndexArray.Length);
            Array.Clear(__instance.recipeStateArray, 0, __instance.recipeStateArray.Length);
            Array.Clear(__instance.recipeProtoArray, 0, __instance.recipeProtoArray.Length);
            var history = GameMain.history;
            var dataArray = LDB.recipes.dataArray;
            var iconSet = GameMain.iconSet;
            for (var i = 0; i < dataArray.Length; i++)
                if (dataArray[i].GridIndex >= 1101 && true)
                {
                    var num = dataArray[i].GridIndex / 1000;
                    var num2 = (dataArray[i].GridIndex - num * 1000) / 100 - 1;
                    var num3 = dataArray[i].GridIndex % 100 - 1;
                    //bool handcraft = dataArray[i].Handcraft;
                    var handcraft = true;
                    if (num2 >= 0 && num3 >= 0 && num2 < 7 && num3 < 12)
                    {
                        var num4 = num2 * 12 + num3;
                        if (num4 >= 0 && num4 < __instance.recipeIndexArray.Length && num == __instance.currentType)
                        {
                            __instance.recipeIndexArray[num4] = iconSet.recipeIconIndex[dataArray[i].ID];
                            __instance.recipeStateArray[num4] = handcraft ? 0U : 8U;
                            __instance.recipeProtoArray[num4] = dataArray[i];
                        }
                    }
                }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIReplicatorWindow), "_OnUpdate")]
        // UIReplicatorWindow
// Token: 0x06001B77 RID: 7031 RVA: 0x001388B4 File Offset: 0x00136AB4
        public static bool _OnUpdate(ref UIReplicatorWindow __instance)
        {
            if (!active || !Preferences.GetBool("alwaysCraft")) return true;
            __instance.TestMouseQueueIndex();
            __instance.TestMouseRecipeIndex();
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus)) __instance.OnPlusButtonClick(0);
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) __instance.OnMinusButtonClick(0);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) __instance.OnOkButtonClick(0, true);
            __instance.RefreshQueueIcons();
            for (var i = 0; i < __instance.queueNumTexts.Length; i++)
                if (__instance.taskQueue.Count > i)
                    __instance.ActiveQueueText(i);
                else
                    __instance.DeactiveQueueText(i);
            if (__instance.taskQueue.Count == 0)
                __instance.queueTotalTimeText.text = __instance._tmp_text0;
            else
                __instance.queueTotalTimeText.text = string.Format("{0}  {1:0.0} s", __instance._tmp_text0, __instance.mechaForge.totalTime);
            __instance.queueCountText.text = __instance.taskQueue.Count.ToString();
            if (__instance.taskQueue.Count > 0)
                __instance.currProgressImage.fillAmount = (float)(__instance.taskQueue[0].tick / (double)__instance.taskQueue[0].tickSpend);
            else
                __instance.currProgressImage.fillAmount = 0f;
            __instance.SetBufferData();
            __instance.treeGroup.interactable = __instance.selectedRecipe != null && __instance.treeGroup.alpha > 0.999f;
            __instance.treeGroup.blocksRaycasts = __instance.selectedRecipe != null && __instance.treeGroup.alpha > 0.999f;
            __instance.treeGroup.gameObject.SetActive(__instance.treeGroup.alpha > 0.001f || __instance.selectedRecipe != null);
            var num = 0;
            var num2 = 999;
            if (__instance.selectedRecipe != null) //&& __instance.selectedRecipe.Handcraft)
                num = __instance.mechaForge.PredictTaskCount(__instance.selectedRecipe.ID, num2);
            if (num == 0)
            {
                __instance.okButton.button.interactable = false;
                __instance.currPredictCountText.text = "";
                __instance.currPredictGroup.SetActive(false);
                if (__instance.selectedRecipe != null)
                {
                    var player = __instance.mechaForge.player;
                    if (__instance._test_package == null) __instance._test_package = new StorageComponent(player.package.size);
                    __instance._test_package.SetSize(player.package.size);
                    Array.Copy(player.package.grids, __instance._test_package.grids, player.package.grids.Length);
                    for (var j = 0; j < __instance.treeDownList.Count; j++)
                    {
                        var uibutton = __instance.treeDownList[j];
                        var transition = __instance.treeDownList[j].transitions[3];
                        if (j < __instance.selectedRecipe.Items.Length && __instance.selectedRecipe.Items[j] == uibutton.tips.itemId)
                        {
                            var num3 = __instance.selectedRecipe.Items[j];
                            var num4 = __instance.selectedRecipe.ItemCounts[j];
                            var flag = false;
                            var num5 = num4 - __instance._test_package.GetItemCount(num3);
                            if (num5 > 0)
                            {
                                var itemProto = LDB.items.Select(num3);
                                if (itemProto != null && itemProto.handcraft != null)
                                {
                                    var num6 = 0;
                                    if (itemProto.handcraft.Results[0] == num3)
                                        num6 = 0;
                                    else if (itemProto.handcraft.Results[1] == num3)
                                        num6 = 1;
                                    else if (itemProto.handcraft.Results[2] == num3)
                                        num6 = 2;
                                    else if (itemProto.handcraft.Results[3] == num3)
                                        num6 = 3;
                                    else
                                        Assert.CannotBeReached();
                                    var count = Mathf.CeilToInt(num5 / (float)itemProto.handcraft.ResultCounts[num6]);
                                    if (__instance.mechaForge.TryTaskWithTestPackage(itemProto.handcraft.ID, count, __instance._test_package)) flag = true;
                                }
                            }
                            else
                            {
                                flag = true;
                            }

                            if (!flag) transition.alphaOnly = true;
                        }
                    }
                }
            }
            else
            {
                __instance.okButton.button.interactable = true;
                if (num < num2)
                    __instance.currPredictCountText.text = num.ToString();
                else
                    __instance.currPredictCountText.text = ">" + num2;
                __instance.currPredictGroup.SetActive(true);
            }

            for (var k = 0; k < __instance.treeDownList.Count; k++)
                if (__instance.treeDownList[k] != null)
                {
                    var transition2 = __instance.treeDownList[k].transitions[3];
                    transition2.target.enabled = transition2.alphaOnly;
                    transition2.alphaOnly = false;
                }

            if (__instance.showTips)
            {
                var num7 = -1;
                var num8 = -1;
                var num9 = 0;
                if (__instance.mouseRecipeIndex >= 0)
                {
                    num9 = __instance.recipeProtoArray[__instance.mouseRecipeIndex] == null ? 0 : __instance.recipeProtoArray[__instance.mouseRecipeIndex].ID;
                    num7 = __instance.mouseRecipeIndex % 12;
                    num8 = __instance.mouseRecipeIndex / 12;
                }

                var recipeProto = num9 == 0 ? null : LDB.recipes.Select(num9);
                if (recipeProto != null)
                {
                    num9 = recipeProto.Explicit ? -num9 : recipeProto.Results[0];
                    __instance.mouseInTime += Time.deltaTime;
                    if (__instance.mouseInTime > __instance.showTipsDelay)
                    {
                        if (__instance.screenTip == null) __instance.screenTip = UIItemTip.Create(num9, __instance.tipAnchor, new Vector2(num7 * 46 + 15, -(float)num8 * 46 - 50), __instance.recipeBg.transform, !recipeProto.Handcraft);
                        if (!__instance.screenTip.gameObject.activeSelf)
                        {
                            __instance.screenTip.gameObject.SetActive(true);
                            __instance.screenTip.SetTip(num9, __instance.tipAnchor, new Vector2(num7 * 46 + 15, -(float)num8 * 46 - 50), __instance.recipeBg.transform, !recipeProto.Handcraft);
                            return false;
                        }

                        if (__instance.screenTip.showingItemId != num9)
                        {
                            __instance.screenTip.SetTip(num9, __instance.tipAnchor, new Vector2(num7 * 46 + 15, -(float)num8 * 46 - 50), __instance.recipeBg.transform, !recipeProto.Handcraft);
                            return false;
                        }
                    }
                }
                else
                {
                    if (__instance.mouseInTime > 0f) __instance.mouseInTime = 0f;
                    if (__instance.screenTip != null)
                    {
                        __instance.screenTip.showingItemId = 0;
                        __instance.screenTip.gameObject.SetActive(false);
                    }
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(UIReplicatorWindow), "OnOkButtonClick")]
        [HarmonyPrefix]
        public static bool OnOkButtonClick(ref UIReplicatorWindow __instance, int whatever, bool button_enable)
        {
            
            if (!active || !Preferences.GetBool("alwaysCraft")) return true;
            GS2.Log("Test2");
            if (__instance.selectedRecipe != null)
            {
                // if (!__instance.selectedRecipe.Handcraft)
                // {
                //     UIRealtimeTip.Popup("该配方".Translate() + __instance.selectedRecipe.madeFromString + "生产".Translate());
                //     return false;
                // }

                var id = __instance.selectedRecipe.ID;
                if (!GameMain.history.RecipeUnlocked(id))
                {
                    UIRealtimeTip.Popup("配方未解锁".Translate());
                    return false;
                }

                var num = 1;
                if (__instance.multipliers.ContainsKey(id)) num = __instance.multipliers[id];

                if (num < 1)
                    num = 1;
                else if (num > 999) num = 1000;

                var num2 = __instance.mechaForge.PredictTaskCount(__instance.selectedRecipe.ID, 999);
                // GS2.Log($"{num} - {num2}");
                if (num > num2) num = num2;

                if (num == 0)
                {
                    UIRealtimeTip.Popup("材料不足".Translate());
                    return false;
                }

                if (__instance.mechaForge.AddTask(id, num) == null)
                {
                    UIRealtimeTip.Popup("材料不足".Translate());
                    return false;
                }

                GameMain.history.RegFeatureKey(1000104);
            }

            return false;
        }
    }
}