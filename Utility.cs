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
        internal static HashSet<PediaEntry> pediasToPatch = new HashSet<PediaEntry>();

        public static void RegisterPediaEntry(PediaEntry pediaEntry)
        {
            if (!pediasToPatch.Contains(pediaEntry))
                pediasToPatch.Add(pediaEntry);
        }

        public static IdentifiablePediaEntry CreateIdentifiableEntry(IdentifiableType identifiableType, PediaHighlightSet highlightSet,
            LocalizedString intro, PediaEntryDetail[] entryDetails, bool isUnlockedInitially = false)
        {
            if (Get<IdentifiablePediaEntry>(identifiableType?.name))
                return null;

            IdentifiablePediaEntry identifiablePediaEntry = ScriptableObject.CreateInstance<IdentifiablePediaEntry>();
            identifiablePediaEntry.hideFlags |= HideFlags.HideAndDontSave;
            identifiablePediaEntry.name = identifiableType.name;

            identifiablePediaEntry._title = identifiableType.localizedName;
            identifiablePediaEntry._description = intro;
            identifiablePediaEntry._identifiableType = identifiableType;

            identifiablePediaEntry._details = entryDetails;
            identifiablePediaEntry._highlightSet = highlightSet;
            // identifiablePediaEntry._unlockInfoProvider = SceneContext.Instance.PediaDirector.Cast<IUnlockInfoProvider>();
            identifiablePediaEntry._isUnlockedInitially = isUnlockedInitially;

            RegisterPediaEntry(identifiablePediaEntry);
            return identifiablePediaEntry;
        }

        public static void AddPediaToCategory(PediaEntry pediaEntry, PediaCategory pediaCategory)
        {
            if (!pediaCategory)
                return;

            if (!pediaCategory._items.Contains(pediaEntry))
                pediaCategory._items = pediaCategory._items.AddItem(pediaEntry).ToArray();

            LookupDirector director = SRSingleton<GameContext>.Instance.LookupDirector;
            if (!director._categories[director._categories.IndexOf(pediaCategory.GetRuntimeCategory())].Contains(pediaEntry))
                director.AddPediaEntryToCategory(pediaEntry, pediaCategory);
        }
    }
}
