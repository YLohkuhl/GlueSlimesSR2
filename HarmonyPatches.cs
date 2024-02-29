using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.Script.Util;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Localization;
using MelonLoader;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Il2CppMonomiPark.SlimeRancher.Pedia;

namespace GlueSlimes
{
    internal class HarmonyPatches
    {
        [HarmonyPatch(typeof(MarketUI), "Start")]
        public static class PatchMarketUIStart
        {
            public static void Prefix(MarketUI __instance)
            {
                __instance.plorts = (from x in __instance.plorts
                                     where !GlueEntry.plortsToPatch.Exists((MarketUI.PlortEntry y) => y == x)
                                     select x).ToArray();
                __instance.plorts = __instance.plorts.ToArray().AddRangeToArray(GlueEntry.plortsToPatch.ToArray());
            }
        }

        [HarmonyPatch(typeof(EconomyDirector), "InitModel")]
        public static class PatchEconomyDirectorInitModel
        {
            public static void Prefix(EconomyDirector __instance)
            {
                __instance.BaseValueMap = __instance.BaseValueMap.ToArray().AddRangeToArray(GlueEntry.valueMapsToPatch.ToArray());
            }
        }

        [HarmonyPatch(typeof(AutoSaveDirector), "Awake")]
        public static class PatchAutoSaveDirectorAwake
        {
            public static void Prefix(AutoSaveDirector __instance)
            {
                Utility.Get<IdentifiableTypeGroup>("PlortGroup").memberTypes.Add(GlueEntry.gluePlortType);
                Utility.Get<IdentifiableTypeGroup>("BaseSlimeGroup").memberTypes.Add(GlueEntry.glueDefinition);
                Utility.Get<IdentifiableTypeGroup>("VaccableBaseSlimeGroup").memberTypes.Add(GlueEntry.glueDefinition);
                Utility.Get<IdentifiableTypeGroup>("SlimesGroup").memberTypes.Add(GlueEntry.glueDefinition);
                __instance.identifiableTypes.memberTypes.Add(GlueEntry.gluePlortType);
                __instance.identifiableTypes.memberTypes.Add(GlueEntry.glueDefinition);
            }
        }

        [HarmonyPatch(typeof(SavedGame))]
        internal static class SavedGamePushPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(SavedGame.Push), typeof(GameModel))]
            public static void PushGameModel(SavedGame __instance)
            {
                foreach (var pediaEntry in Utility.Pedia.addedPedias)
                {
                    if (!__instance.pediaEntryLookup.ContainsKey(pediaEntry.PersistenceId))
                        __instance.pediaEntryLookup.Add(pediaEntry.PersistenceId, pediaEntry);
                }
            }
        }

        [HarmonyPatch(typeof(PediaDirector), "Awake")]
        internal static class PatchPediaDirectorAwake
        {
            public static void Prefix(PediaDirector __instance)
            {
                #region PEDIAS
                Utility.Pedia.AddSlimepedia(Utility.Get<IdentifiableType>("Glue"), "Glue",
                    "Gooey, Hungry, Vegetarian Slime?",
                    "Glue Slimes are your gooey little friends! They're made out of glue entirely, along with some slimey substance. They do get hungry to the point they may or may not eat something they shouldn't. <s>Tarrs also dislike their gluey taste and will not eat them.<s>\n\n\n" +
                    "<i>They may or may not have a relation to other <b>liquid formed slimes</b>.</i>",
                    "There are no dangerous risk! Glue Slimes are usually friendly, but.. if they have no other food source, they may result to eating Pink Slimes. They're common so its easy for them to gobble on with no veggies around, so keep them away from your pink slimes if you must!",
                    "Their plorts are made out of glue as well, great for gluing things together.. that's for sure!"
                );

                // Utility.Pedia.AddSlimepediaPage("Glue", 2, "They may or may not have a relation to other <b>liquid formed slimes<b>.");
                #endregion

                foreach (var pediaEntry in Utility.Pedia.addedPedias)
                {
                    var identPediaEntry = pediaEntry.TryCast<IdentifiablePediaEntry>();
                    if (identPediaEntry && !__instance._identDict.ContainsKey(identPediaEntry.IdentifiableType))
                        __instance._identDict.Add(identPediaEntry.IdentifiableType, pediaEntry);
                }
            }
        }

        [HarmonyPatch(typeof(LocalizationDirector), "LoadTables")]
        internal static class LocalizationDirectorLoadTablePatch
        {
            public static void Postfix(LocalizationDirector __instance)
            {
                MelonCoroutines.Start(LoadTable(__instance));
            }

            private static IEnumerator LoadTable(LocalizationDirector director)
            {
                WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(0.01f);
                yield return waitForSecondsRealtime;
                foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, StringTable> keyValuePair in director.Tables)
                {
                    if (addedTranslations.TryGetValue(keyValuePair.Key, out var dictionary))
                    {
                        foreach (System.Collections.Generic.KeyValuePair<string, string> keyValuePair2 in dictionary)
                        {
                            keyValuePair.Value.AddEntry(keyValuePair2.Key, keyValuePair2.Value);
                        }
                    }
                }
                yield break;
            }

            public static LocalizedString AddTranslation(string table, string key, string localized)
            {
                System.Collections.Generic.Dictionary<string, string> dictionary;
                if (!addedTranslations.TryGetValue(table, out dictionary))
                {
                    dictionary = new System.Collections.Generic.Dictionary<string, string>(); ;
                    addedTranslations.Add(table, dictionary);
                }
                dictionary.TryAdd(key, localized);
                StringTable table2 = LocalizationUtil.GetTable(table);
                StringTableEntry stringTableEntry = table2.AddEntry(key, localized);
                return new LocalizedString(table2.SharedData.TableCollectionName, stringTableEntry.SharedEntry.Id);
            }

            public static System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>> addedTranslations = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>();
        }
    }
}
