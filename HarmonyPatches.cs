using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.Script.Util;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Localization;
using MelonLoader;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Il2CppMonomiPark.SlimeRancher.Pedia;

using static Utility;
using static GlueSlimes.GlueEntry;

namespace GlueSlimes
{
    internal class HarmonyPatches
    {
        [HarmonyPatch(typeof(MarketUI), "Start")]
        internal static class PatchMarketUIStart
        {
            public static void Prefix(MarketUI __instance)
            {
                __instance.plorts = (from x in __instance.plorts
                                     where !plortsToPatch.Exists((MarketUI.PlortEntry y) => y == x)
                                     select x).ToArray();
                __instance.plorts = __instance.plorts.ToArray().AddRangeToArray(plortsToPatch.ToArray());
            }
        }

        [HarmonyPatch(typeof(EconomyDirector), "InitModel")]
        internal static class PatchEconomyDirectorInitModel
        {
            public static void Prefix(EconomyDirector __instance)
            {
                __instance.BaseValueMap = __instance.BaseValueMap.ToArray().AddRangeToArray(valueMapsToPatch.ToArray());
            }
        }

        [HarmonyPatch(typeof(LookupDirector), "Awake")]
        internal static class PatchLookupDirectorAwake
        {
            private static IdentifiableTypeGroup[][] _registryIdentifiableGroups = new IdentifiableTypeGroup[0][];

            public static void Prefix(LookupDirector __instance)
            {
                RegisterPedias();

                _registryIdentifiableGroups =
                [
                    [
                        Get<IdentifiableTypeGroup>("BaseSlimeGroup"),
                        Get<IdentifiableTypeGroup>("EdibleSlimeGroup"),
                        Get<IdentifiableTypeGroup>("SlimesSinkInShallowWaterGroup"),
                        Get<IdentifiableTypeGroup>("VaccableBaseSlimeGroup"),
                        Get<IdentifiableTypeGroup>("IdentifiableTypesGroup")
                    ],
                    [
                        Get<IdentifiableTypeGroup>("EdiblePlortFoodGroup"),
                        Get<IdentifiableTypeGroup>("PlortGroup"),
                        Get<IdentifiableTypeGroup>("IdentifiableTypesGroup")
                    ]
                ];

                RegisterIdentifiables(__instance);
            }

            private static void RegisterIdentifiables(LookupDirector director)
            {
                foreach (var identifiableTypeGroup in _registryIdentifiableGroups[0])
                    AddIdentifiableTypeToGroup(director, glueDefinition, identifiableTypeGroup);

                foreach (var identifiableTypeGroup in _registryIdentifiableGroups[1])
                    AddIdentifiableTypeToGroup(director, gluePlortType, identifiableTypeGroup);
            }

            private static void RegisterPedias()
            {
                gluePlortType.localizedName = LocalizationDirectorLoadTablePatch.AddTranslation("Actor", "l.glue_plort", "Glue Plort");
                glueDefinition.localizedName = LocalizationDirectorLoadTablePatch.AddTranslation("Actor", "l.glue_slime", "Glue Slime");

                PediaEntry glueEntry = Pedia.CreateIdentifiableEntry(glueDefinition, Get<PediaEntry>("Pink")._highlightSet,
                    LocalizationDirectorLoadTablePatch.AddTranslation("PediaPage", "m.intro.glue_slime", "Gooey, Hungry, Vegetarian Slime?"),
                    [
                        new PediaEntryDetail()
                        {
                            Section = Get<PediaDetailSection>("Slimeology"),
                            Text = LocalizationDirectorLoadTablePatch.AddTranslation("PediaPage", "m.slimeology.glue_slime",
                                "Glue Slimes are your gooey little friends! They're made out of glue entirely, along with some slimey substance. " +
                                "They do get hungry to the point they may or may not eat something they shouldn't. " +
                                "<s>Tarrs also dislike their gluey taste and will not eat them.<s>\n\n\n" +
                                "<i>They may or may not have a relation to other <b>liquid formed slimes</b>.</i>"
                            ),
                            TextGamepad = new(),
                            TextPS4 = new()
                        },
                        new PediaEntryDetail()
                        {
                            Section = Get<PediaDetailSection>("Rancher Risks"),
                            Text = LocalizationDirectorLoadTablePatch.AddTranslation("PediaPage", "m.risks.glue_slime",
                                "There are no dangerous risk! Glue Slimes are usually friendly, but.. if they have no other food source, they may result to eating Pink Slimes. " +
                                "They're common so its easy for them to gobble on with no veggies around, so keep them away from your pink slimes if you must!"
                            ),
                            TextGamepad = new(),
                            TextPS4 = new()
                        },
                        new PediaEntryDetail()
                        {
                            Section = Get<PediaDetailSection>("Plortonomics"),
                            Text = LocalizationDirectorLoadTablePatch.AddTranslation("PediaPage", "m.plortonomics.glue_slime", 
                                "Their plorts are made out of glue as well, great for gluing things together.. that's for sure!"
                            ),
                            TextGamepad = new(),
                            TextPS4 = new()
                        }
                    ]
                );

                Pedia.AddPediaToCategory(glueEntry, Get<PediaCategory>("Slimes"));
            }

            public static void AddIdentifiableTypeToGroup(LookupDirector director, IdentifiableType identifiableType, IdentifiableTypeGroup identifiableTypeGroup)
            {
                if (!identifiableTypeGroup._memberTypes.Contains(identifiableType))
                    identifiableTypeGroup._memberTypes.Add(identifiableType);
                director.AddIdentifiableTypeToGroup(identifiableType, identifiableTypeGroup);
            }
        }

        [HarmonyPatch(typeof(PediaDirector), "Awake")]
        internal static class PatchPediaDirectorAwake
        {
            public static void Prefix(PediaDirector __instance)
            {
                foreach (var pediaEntry in Pedia.pediasToPatch)
                {
                    if (!pediaEntry)
                        continue;
                    pediaEntry._unlockInfoProvider = __instance.Cast<IUnlockInfoProvider>();
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
