using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.UI.Pedia;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Localization;
using static GlueSlimes.HarmonyPatches;
using static GlueSlimes.HarmonyPatches.LocalizationDirectorLoadTablePatch;

internal class Utility
{
    public static T Get<T>(string name) where T : UnityEngine.Object
    {
        return Resources.FindObjectsOfTypeAll<T>().FirstOrDefault((T found) => found.name.Equals(name));
    }

    public static Texture2D LoadImage(string filename)
    {
        Assembly executingAssembly = Assembly.GetExecutingAssembly();
        Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + "." + filename + ".png");
        byte[] array = new byte[manifestResourceStream.Length];
        manifestResourceStream.Read(array, 0, array.Length);
        Texture2D texture2D = new Texture2D(1, 1);
        ImageConversion.LoadImage(texture2D, array);
        texture2D.filterMode = FilterMode.Bilinear;
        return texture2D;
    }

    public static Sprite CreateSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);
    }

    public static class PrefabUtils
    {
        static PrefabUtils()
        {
            DisabledParent.gameObject.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(DisabledParent.gameObject);
            DisabledParent.gameObject.hideFlags |= HideFlags.HideAndDontSave;
        }

        public static GameObject CopyPrefab(GameObject prefab)
        {
            return UnityEngine.Object.Instantiate(prefab, DisabledParent);
        }

        public static Transform DisabledParent = new GameObject("DeactivedObject").transform;
    }

    public static class Spawner
    {
        public static void ToSpawn(string name)
        {
            InstantiationHelpers.InstantiateActor(Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault((GameObject x) => x.name == name), SRSingleton<SceneContext>.Instance.RegionRegistry.CurrentSceneGroup, SRSingleton<SceneContext>.Instance.Player.transform.position, Quaternion.identity, false, SlimeAppearance.AppearanceSaveSet.NONE, SlimeAppearance.AppearanceSaveSet.NONE);
        }
    }

    public static class Pedia
    {
        internal static HashSet<PediaEntry> addedPedias = new HashSet<PediaEntry>();

        public static string CreateIdentifiableKey(string prefix, IdentifiableType identifiableType)
        { return "m." + prefix + "." + identifiableType._pediaPersistenceSuffix; }

        public static string CreateIdentifiablePageKey(string prefix, IdentifiableType identifiableType)
        { return "m." + prefix + "." + identifiableType._pediaPersistenceSuffix; }

        public static PediaEntry AddSlimepedia(IdentifiableType identifiableType, string pediaEntryName, string pediaIntro, string pediaSlimeology, string pediaRisks, string pediaPlortonomics, bool unlockedInitially = false)
        {
            if (Get<IdentifiablePediaEntry>(pediaEntryName))
                return null;

            PediaCategory basePediaEntryCategory = SRSingleton<SceneContext>.Instance.PediaDirector._pediaConfiguration.Categories.ToArray().First(x => x.name == "Slimes");
            PediaEntry pediaEntry = basePediaEntryCategory._items.First();
            IdentifiablePediaEntry identifiablePediaEntry = ScriptableObject.CreateInstance<IdentifiablePediaEntry>();

            LocalizedString intro = AddTranslation("Pedia", CreateIdentifiableKey("intro", identifiableType), pediaIntro);
            LocalizedString slimeology = AddTranslation("PediaPage", CreateIdentifiablePageKey("slimeology", identifiableType), pediaSlimeology);
            LocalizedString risks = AddTranslation("PediaPage", CreateIdentifiablePageKey("risks", identifiableType), pediaRisks);
            LocalizedString plortonomics = AddTranslation("PediaPage", CreateIdentifiablePageKey("plortonomics", identifiableType), pediaPlortonomics);

            PediaEntryDetail[] entryDetails = new PediaEntryDetail[]
            {
                new PediaEntryDetail()
                {
                    Section = Get<PediaDetailSection>("Slimeology"),
                    Text = slimeology,
                    TextGamepad = new LocalizedString(),
                    TextPS4 = new LocalizedString()
                },
                new PediaEntryDetail()
                {
                    Section = Get<PediaDetailSection>("Rancher Risks"),
                    Text = risks,
                    TextGamepad = new LocalizedString(),
                    TextPS4 = new LocalizedString()
                },
                new PediaEntryDetail()
                {
                    Section = Get<PediaDetailSection>("Plortonomics"),
                    Text = plortonomics,
                    TextGamepad = new LocalizedString(),
                    TextPS4 = new LocalizedString()
                }
            };

            identifiablePediaEntry.hideFlags |= HideFlags.HideAndDontSave;
            identifiablePediaEntry.name = pediaEntryName;
            identifiablePediaEntry._title = identifiableType.localizedName;
            identifiablePediaEntry._description = intro;
            identifiablePediaEntry._identifiableType = identifiableType;

            identifiablePediaEntry._highlightSet = pediaEntry._highlightSet;
            identifiablePediaEntry._details = entryDetails;
            identifiablePediaEntry._unlockInfoProvider = SceneContext.Instance.PediaDirector.Cast<IUnlockInfoProvider>();
            identifiablePediaEntry._isUnlockedInitially = unlockedInitially;

            if (!basePediaEntryCategory._items.Contains(identifiablePediaEntry))
                basePediaEntryCategory._items = basePediaEntryCategory._items.ToArray().AddToArray(identifiablePediaEntry);
            if (!addedPedias.Contains(identifiablePediaEntry))
                addedPedias.Add(identifiablePediaEntry);

            return identifiablePediaEntry;
        }
    }
}
