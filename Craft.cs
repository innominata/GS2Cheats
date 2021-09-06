using System;
using System.Collections.Generic;
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
            // GS2.Log("TATPost" +__result);
            if (!active || (!Preferences.GetBool("alwaysCraft")&&!Preferences.GetBool("handCraftEverything"))) return;
            __result = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MechaForge), nameof(MechaForge.AddTaskIterate))]
        public static bool AddTaskIterate_Prefix(MechaForge __instance, ref ForgeTask __result, int recipeId, int count)
        {
            if (!active) return true;
            if (Preferences.GetBool("alwaysCraft"))
            {
                var recipe = new ForgeTask(recipeId, count);
                for (var i = 0; i < recipe.productIds.Length; i++) GameMain.mainPlayer.package.AddItemStacked(recipe.productIds[i], count * recipe.productCounts[i]);
                __result = null;
                return false;
            }

            if (Preferences.GetBool("handCraftEverything"))
            {
                GS2.Log("HCE");
                __instance.iterLevel++;
                if (__instance.iterLevel > 20)
                {
                    __instance.iterLevel--;
                    __result = null;
                    GS2.Log("IterLevel >20");
                    return false;
                }

                if (recipeId == 0)
                {
                    __instance.iterLevel--;
                    __result = null;
                    GS2.Log("ID 0");
                    return false;
                }

                if (!__instance.gameHistory.RecipeUnlocked(recipeId))
                {
                    __instance.iterLevel--;
                    __result = null;
                    GS2.Log("NotUnlocked");
                    return false;
                }

                if (count <= 0)
                {
                    __instance.iterLevel--;
                    __result = null;
                    GS2.Log("Count<=0");
                    return false;
                }

                var forgeTask = new ForgeTask(recipeId, count);
                var list = new List<ForgeTask>();
                for (var i = 0; i < forgeTask.itemIds.Length; i++)
                {
                    var num = forgeTask.itemIds[i];
                    var count2 = __instance._test_pack.GetCount(num);
                    int num3;
                    var num2 = num3 = count * forgeTask.itemCounts[i];
                    var num4 = 0;
                    if (count2 > 0)
                    {
                        var num5 = count2 < num3 ? count2 : num3;
                        num3 -= num5;
                        num4 += num5;
                        __instance._test_pack.Alter(num, -num5);
                    }

                    if (num3 > 0)
                    {
                        var num6 = __instance.player.package.TakeItem(num, num3);
                        num4 += num6;
                        forgeTask.served[i] += num6;
                    }

                    var num7 = num2 - num4;
                    if (num7 > 0)
                    {
                        var itemProto = LDB.items.Select(num);
                        if (itemProto == null)
                        {
                            __instance.iterLevel--;
                            __result = null;
                            GS2.Log("ItemProtoNull");
                            return false;
                        }

                        var handcraft = itemProto.maincraft;
                        if (handcraft == null)
                        {
                            __instance.iterLevel--;
                            __result = null;
                            GS2.Log("Handcraft=Null");
                            return false;
                        }

                        if (!__instance.gameHistory.RecipeUnlocked(handcraft.ID))
                        {
                            __instance.iterLevel--;
                            __result = null;
                            GS2.Log("Recipe!Unlocked");
                            return false;
                        }

                        var num8 = Mathf.CeilToInt(num7 / (float)itemProto.maincraftProductCount);
                        var forgeTask2 = __instance.AddTaskIterate(handcraft.ID, num8);
                        if (forgeTask2 == null)
                        {
                            __instance.iterLevel--;
                            __result = null;
                            GS2.Log("AddTaskIterate Returned Null");
                            return false;
                        }

                        for (var j = 0; j < handcraft.Results.Length; j++)
                        {
                            var num9 = handcraft.Results[j];
                            var num10 = num8 * handcraft.ResultCounts[j];
                            if (num9 == num) num10 -= num7;
                            __instance._test_pack.Alter(num9, num10);
                        }

                        list.Add(forgeTask2);
                    }
                }

                var flag = list.Count == 0 && __instance.tasks.Count > 0 && __instance.tasks[__instance.tasks.Count - 1].recipeId == forgeTask.recipeId && __instance.iterLevel == 1;
                foreach (var forgeTask3 in list) forgeTask3.parentTaskIndex = __instance.tasks.Count;
                list.Clear();
                if (flag)
                {
                    var forgeTask4 = __instance.tasks[__instance.tasks.Count - 1];
                    forgeTask4.count += forgeTask.count;
                    for (var k = 0; k < forgeTask4.served.Length; k++)
                    {
                        if (k >= forgeTask.served.Length) break;
                        if (forgeTask4.itemIds[k] == forgeTask.itemIds[k]) forgeTask4.served[k] += forgeTask.served[k];
                    }
                }
                else
                {
                    __instance.tasks.Add(forgeTask);
                }

                __instance.CalculateExtra();
                __instance.iterLevel--;
                __result = forgeTask;
                GS2.Log("Success");
                return false;
            }
            return true;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechaForge), "AddTaskIterate")]
        public static void AddTaskIteratePostfix(ref MechaForge __instance)
        {
            if (!active || !Preferences.GetBool("instantCraft")) return;
            foreach (var task in __instance.tasks) task.tickSpend = 1;
            // Instant craft set time to one tick for every item in the queue
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechaForge), "PredictTaskCount")]
        public static void PredictTaskCount(ref int __result)
        {
            // GS2.Log("PTCPost" +__result);
            if (!active || !Preferences.GetBool("alwaysCraft")) return;
            __result = 999; // Always report 999 available to craft
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechaForge), "AddTask")]
        public static void AddTaskPostfix(ref ForgeTask __result)
        {
            GS2.Log("ATPost" +__result);
            if (!active || !Preferences.GetBool("alwaysCraft")) return;
            if (__result == null) __result = new ForgeTask();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MechaForge), "AddTask")]
        public static bool AddTaskPrefix(ref MechaForge __instance, ref ForgeTask __result, int recipeId, int count)
        {
            if (!active || !(Preferences.GetBool("alwaysCraft") || Preferences.GetBool("handCraftEverything"))) return true;
            if (__instance.TryAddTask(recipeId, count))
            {
                __instance.extraItems.CopyTo(__instance._test_pack);
                __instance.iterLevel = 0;
                var forgeTask = __instance.AddTaskIterate(recipeId, count);
                Assert.NotNull(forgeTask);
                __result = forgeTask;
                GS2.Log("AT Success");
                return false;
            }
            GS2.Log("AT FAIL");
            __result = null;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIReplicatorWindow), "RefreshRecipeIcons")]
        public static bool RefreshRecipeIcons(ref UIReplicatorWindow __instance)
        {
            if (!active || !(Preferences.GetBool("alwaysCraft") || Preferences.GetBool("handCraftEverything"))) return true;
            GS2.WarnJson("RefreshingRecipeIcons");
            Array.Clear(__instance.recipeIndexArray, 0, __instance.recipeIndexArray.Length);
            Array.Clear(__instance.recipeStateArray, 0, __instance.recipeStateArray.Length);
            Array.Clear(__instance.recipeProtoArray, 0, __instance.recipeProtoArray.Length);
            var history = GameMain.history;
            var dataArray = LDB.recipes.dataArray;
            var iconSet = GameMain.iconSet;
            for (var i = 0; i < dataArray.Length; i++)
                if (dataArray[i].GridIndex >= 1101 && (history.RecipeUnlocked(dataArray[i].ID) || Preferences.GetBool("alwaysCraft")))
                {
                    var num = dataArray[i].GridIndex / 1000;
                    var num2 = (dataArray[i].GridIndex - num * 1000) / 100 - 1;
                    var num3 = dataArray[i].GridIndex % 100 - 1;

                    if (num2 >= 0 && num3 >= 0 && num2 < 7 && num3 < 12)
                    {
                        var num4 = num2 * 12 + num3;
                        if (num4 >= 0 && num4 < __instance.recipeIndexArray.Length && num == __instance.currentType)
                        {
                            __instance.recipeIndexArray[num4] = iconSet.recipeIconIndex[dataArray[i].ID];
                            __instance.recipeStateArray[num4] = 0U;
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
            if (!active || !(Preferences.GetBool("alwaysCraft") || Preferences.GetBool("handCraftEverything"))) return true;
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
                        if (__instance.screenTip == null) __instance.screenTip = UIItemTip.Create(num9, __instance.tipAnchor, new Vector2(num7 * 46 + 15, -(float)num8 * 46 - 50), __instance.recipeBg.transform, false);
                        if (!__instance.screenTip.gameObject.activeSelf)
                        {
                            __instance.screenTip.gameObject.SetActive(true);
                            __instance.screenTip.SetTip(num9, __instance.tipAnchor, new Vector2(num7 * 46 + 15, -(float)num8 * 46 - 50), __instance.recipeBg.transform, false);
                            return false;
                        }

                        if (__instance.screenTip.showingItemId != num9)
                        {
                            __instance.screenTip.SetTip(num9, __instance.tipAnchor, new Vector2(num7 * 46 + 15, -(float)num8 * 46 - 50), __instance.recipeBg.transform, false);
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
            if (!active || !(Preferences.GetBool("alwaysCraft") || Preferences.GetBool("handCraftEverything")))
            {
                GS2.Log("SKIPPING");
                return true;
            }
            GS2.Log("Not Skipping");
            if (__instance.selectedRecipe != null)
            {
                // if (!__instance.selectedRecipe.Handcraft)
                // {
                //     UIRealtimeTip.Popup("该配方".Translate() + __instance.selectedRecipe.madeFromString + "生产".Translate());
                //     return false;
                // }

                var id = __instance.selectedRecipe.ID;
                if (!GameMain.history.RecipeUnlocked(id) && !Preferences.GetBool("alwaysCraft"))
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

                // if (num == 0)
                // {
                //     UIRealtimeTip.Popup("材料不足".Translate());
                //     return false;
                // }

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