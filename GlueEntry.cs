using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.UI;
using MelonLoader;
using UnityEngine;

namespace GlueSlimes
{
    internal class GlueEntry : MelonMod
    {
        public static SlimeDefinition glueDefinition;

        public static IdentifiableType gluePlortType;

        public static List<MarketUI.PlortEntry> plortsToPatch = new List<MarketUI.PlortEntry>();

        public static List<EconomyDirector.ValueMap> valueMapsToPatch = new List<EconomyDirector.ValueMap>();

        public override void OnInitializeMelon()
        {
            glueDefinition = ScriptableObject.CreateInstance<SlimeDefinition>();
            glueDefinition.hideFlags |= HideFlags.HideAndDontSave;
            glueDefinition.name = "Glue";

            glueDefinition.color = Color.white;
            glueDefinition._pediaPersistenceSuffix = "glue_slime";

            gluePlortType = ScriptableObject.CreateInstance<IdentifiableType>();
            gluePlortType.hideFlags |= HideFlags.HideAndDontSave;
            gluePlortType.name = "GluePlort";

            gluePlortType.IsPlort = true;
            gluePlortType.color = Color.white;
            gluePlortType._pediaPersistenceSuffix = "glue_plort";
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            switch (sceneName)
            {
                case "SystemCore":
                    {
                        break;
                    }
                case "GameCore":
                    {
                        #region GLUE_PLORT
                        gluePlortType.prefab = Utility.PrefabUtils.CopyPrefab(Utility.Get<IdentifiableType>("PinkPlort").prefab);
                        gluePlortType.prefab.name = "GluePlort";
                        gluePlortType.IsPlort = true;
                        gluePlortType.prefab.GetComponent<Identifiable>().identType = gluePlortType;
                        gluePlortType.icon = Utility.CreateSprite(Utility.LoadImage("Assets.glue_plort_ico"));

                        Material material = UnityEngine.Object.Instantiate(Utility.Get<GameObject>("plortPuddle").GetComponent<MeshRenderer>().sharedMaterial);
                        material.SetColor("_TopColor", Color.white);
                        material.SetColor("_MiddleColor", Color.grey);
                        material.SetColor("_BottomColor", Color.white);
                        gluePlortType.prefab.GetComponent<MeshRenderer>().sharedMaterial = material;

                        plortsToPatch.Add(new MarketUI.PlortEntry
                        {
                            identType = gluePlortType
                        });
                        valueMapsToPatch.Add(new EconomyDirector.ValueMap
                        {
                            Accept = gluePlortType.prefab.GetComponent<Identifiable>(),
                            FullSaturation = 7f,
                            Value = 23f
                        });
                        #endregion

                        #region GLUE_SLIME
                        glueDefinition.prefab = Utility.PrefabUtils.CopyPrefab(Utility.Get<GameObject>("slimePuddle"));
                        glueDefinition.prefab.name = "GlueSlime";

                        glueDefinition.prefab.GetComponent<Identifiable>().identType = glueDefinition;
                        glueDefinition.prefab.GetComponent<SlimeEat>().SlimeDefinition = glueDefinition;

                        UnityEngine.Object.Destroy(glueDefinition.prefab.GetComponent<GotoWater>());
                        UnityEngine.Object.Destroy(glueDefinition.prefab.GetComponent<SlimeEatWater>());
                        UnityEngine.Object.Destroy(glueDefinition.prefab.GetComponent<DestroyOnTouching>());

                        glueDefinition.Diet = UnityEngine.Object.Instantiate(Utility.Get<SlimeDefinition>("Puddle")).Diet;
                        glueDefinition.Diet.MajorFoodIdentifiableTypeGroups = new IdentifiableTypeGroup[] { Utility.Get<IdentifiableTypeGroup>("VeggieGroup") };
                        glueDefinition.Diet.MajorFoodGroups = new SlimeEat.FoodGroup[] { SlimeEat.FoodGroup.VEGGIES };
                        glueDefinition.Diet.ProduceIdents = new IdentifiableType[] { gluePlortType };
                        glueDefinition.Diet.AdditionalFoodIdents = new IdentifiableType[] { Utility.Get<IdentifiableType>("PinkPlort"), Utility.Get<IdentifiableType>("Pink") };
                        glueDefinition.Diet.FavoriteIdents = new IdentifiableType[] { Utility.Get<IdentifiableType>("BeetVeggie") };
                        glueDefinition.Diet.RefreshEatMap(SRSingleton<GameContext>.Instance.SlimeDefinitions, glueDefinition);

                        glueDefinition.icon = Utility.CreateSprite(Utility.LoadImage("Assets.glue_slime_ico"));
                        glueDefinition.properties = UnityEngine.Object.Instantiate(Utility.Get<SlimeDefinition>("Pink").properties);
                        glueDefinition.defaultPropertyValues = UnityEngine.Object.Instantiate(Utility.Get<SlimeDefinition>("Pink")).defaultPropertyValues;

                        SlimeAppearance slimeAppearance = UnityEngine.Object.Instantiate(Utility.Get<SlimeAppearance>("PuddleDefault"));
                        SlimeAppearanceApplicator slimeAppearanceApplicator = glueDefinition.prefab.GetComponent<SlimeAppearanceApplicator>();
                        slimeAppearance.name = "GlueDefault";
                        slimeAppearanceApplicator.Appearance = slimeAppearance;
                        slimeAppearanceApplicator.SlimeDefinition = glueDefinition;

                        Material material2 = UnityEngine.Object.Instantiate(slimeAppearance.Structures[0].DefaultMaterials[0]);
                        material2.hideFlags |= HideFlags.HideAndDontSave;
                        material2.SetColor("_TopColor", Color.white);
                        material2.SetColor("_MiddleColor", Color.grey);
                        material2.SetColor("_BottomColor", Color.white);
                        material2.SetColor("_SpecColor", Color.grey);
                        slimeAppearance.Structures[0].DefaultMaterials[0] = material2;

                        slimeAppearance._face = UnityEngine.Object.Instantiate(Utility.Get<SlimeAppearance>("PuddleDefault").Face);
                        slimeAppearance.Face.name = "GlueFace";

                        SlimeExpressionFace[] expressionFaces = new SlimeExpressionFace[0];
                        foreach (SlimeExpressionFace slimeExpressionFace in slimeAppearance.Face.ExpressionFaces)
                        {
                            Material slimeEyes = UnityEngine.Object.Instantiate(slimeExpressionFace.Eyes);
                            if (slimeEyes)
                            {
                                slimeEyes.SetColor("_EyeRed", Color.black);
                                slimeEyes.SetColor("_EyeGreen", Color.black);
                                slimeEyes.SetColor("_EyeBlue", Color.black);
                            }
                            slimeExpressionFace.Eyes = slimeEyes;
                            expressionFaces = expressionFaces.AddToArray(slimeExpressionFace);
                        }
                        slimeAppearance.Face.ExpressionFaces = expressionFaces;
                        slimeAppearance.Face.OnEnable();

                        slimeAppearance._icon = Utility.CreateSprite(Utility.LoadImage("Assets.glue_slime_ico"));
                        slimeAppearance._splatColor = Color.white;
                        slimeAppearance._colorPalette = new SlimeAppearance.Palette
                        {
                            Ammo = Color.white,
                            Top = Color.white,
                            Middle = Color.grey,
                            Bottom = Color.white
                        };
                        glueDefinition.AppearancesDefault = new SlimeAppearance[] { slimeAppearance };
                        slimeAppearance.hideFlags |= HideFlags.HideAndDontSave;
                        #endregion
                        break;
                    }
                case "zoneCore":
                    {
                        SRSingleton<SceneContext>.Instance.SlimeAppearanceDirector.RegisterDependentAppearances(Utility.Get<SlimeDefinition>("Glue"), Utility.Get<SlimeDefinition>("Glue").AppearancesDefault[0]);
                        SRSingleton<SceneContext>.Instance.SlimeAppearanceDirector.UpdateChosenSlimeAppearance(Utility.Get<SlimeDefinition>("Glue"), Utility.Get<SlimeDefinition>("Glue").AppearancesDefault[0]);
                        SRSingleton<GameContext>.Instance.SlimeDefinitions.Slimes = SRSingleton<GameContext>.Instance.SlimeDefinitions.Slimes.AddItem(glueDefinition).ToArray();
                        SRSingleton<GameContext>.Instance.SlimeDefinitions._slimeDefinitionsByIdentifiable.TryAdd(glueDefinition, glueDefinition);
                        break;
                    }
            }

            OnSceneAddSpawners(sceneName);
        }

        public static void OnSceneAddSpawners(string sceneName)
        {
            switch (sceneName.Contains("zoneFields"))
            {
                case true:
                    {
                        IEnumerable<DirectedSlimeSpawner> source = UnityEngine.Object.FindObjectsOfType<DirectedSlimeSpawner>();
                        foreach (DirectedSlimeSpawner directedSlimeSpawner in source)
                        {
                            foreach (DirectedActorSpawner.SpawnConstraint spawnConstraint in directedSlimeSpawner.Constraints)
                            {
                                spawnConstraint.Slimeset.Members = spawnConstraint.Slimeset.Members.AddItem(new SlimeSet.Member
                                {
                                    _prefab = glueDefinition.prefab,
                                    IdentType = glueDefinition,
                                    Weight = 0.3f
                                }).ToArray();
                            }
                        }
                        break;
                    }
            }
        }
    }
}
