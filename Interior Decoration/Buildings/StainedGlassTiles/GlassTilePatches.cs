﻿using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InteriorDecoration.Buildings.StainedGlassTiles
{
    class GlassTilePatches
    {
        /// <summary>
        /// Adding tag for materials i want stained glass to use for building materials
        /// </summary>
        [HarmonyPatch(typeof(ElementLoader), "Load")]
        private static class Patch_ElementLoader_Load
        {

            private static void Postfix()
            {
                Tag stainedGlassMaterial = TagManager.Create("StainedGlassMaterial", "Glass dye");
                List<SimHashes> elementsToTag = new List<SimHashes>
                {
                    //SimHashes.Bitumen,
                    SimHashes.Ceramic,
                    SimHashes.Copper,
                    //SimHashes.Diamond,
                    //SimHashes.Fossil,
                    SimHashes.Gold,
                    SimHashes.Granite,
                    //SimHashes.Ice,
                    SimHashes.Iron,
                    SimHashes.Lead,
                    //SimHashes.Obsidian,
                    SimHashes.Regolith,
                    SimHashes.Salt,
                    SimHashes.SedimentaryRock,
                    SimHashes.SlimeMold,
                    SimHashes.SandStone,
                    SimHashes.Steel,
                    //SimHashes.Sulfur,
                    SimHashes.SuperInsulator
                    //SimHashes.TempConductorSolid
                };

                // Adds tag on top of existing tags, does not touch others
                foreach (SimHashes elementhash in elementsToTag)
                {
                    var element = ElementLoader.FindElementByHash(elementhash);
                    Array.Resize(ref element.oreTags, element.oreTags.Length + 1);
                    element.oreTags[element.oreTags.GetUpperBound(0)] = stainedGlassMaterial;
                }
            }
        }


        // replaces the building tool to build variants of stained glass tiles
        [HarmonyPatch(typeof(PlanScreen))]
        [HarmonyPatch("OnRecipeElementsFullySelected")]
        public static class PlanScreen_OnRecipeElementsFullySelected_Patch
        {
            public static bool Prefix(PlanScreen __instance, bool __state)
            {
                BuildingDef def = null;
                KToggle currentlySelectedToggle = Traverse.Create(__instance).Field("currentlySelectedToggle").GetValue<KToggle>();
                foreach (KeyValuePair<BuildingDef, KToggle> kvp in __instance.ActiveToggles)
                {
                    if (kvp.Value == currentlySelectedToggle)
                    {
                        def = kvp.Key;
                        break;
                    }
                }
                if (def.name.Contains("StainedGlassTile"))
                {
                    ProductInfoScreen productInfoScreen = Traverse.Create(__instance).Field("productInfoScreen").GetValue<ProductInfoScreen>();
                    IList<Tag> elements = productInfoScreen.materialSelectionPanel.GetSelectedElementAsList;
                    BuildingDef newDef = Assets.GetBuildingDef(elements[0].ToString() + "StainedGlassTile");
                    if (newDef == null) newDef = Assets.GetBuildingDef("DefaultStainedGlassTile");

                    Log.Debuglog("trying to switch  out " + def.name + " to " + elements[0].ToString() + "StainedGlassTile");


                    InterfaceTool tool = PlayerController.Instance.ActiveTool;

                    if (tool != null)
                    {
                        Type tool_type = tool.GetType();
                        if (tool_type == typeof(BuildTool) || typeof(BaseUtilityBuildTool).IsAssignableFrom(tool_type))
                            tool.DeactivateTool(null);
                    }

                    BuildTool.Instance.Activate(newDef, productInfoScreen.materialSelectionPanel.GetSelectedElementAsList, null);

                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Makes the Copy Building button target default stained glass in the build menu.
        /// </summary>
        [HarmonyPatch(typeof(PlanScreen), "OnClickCopyBuilding")]
        public static class PlanScreen_OnClickCopyBuilding_Patch
        {
            public static void Prefix(PlanScreen __instance)
            {
                KSelectable selectable = SelectTool.Instance.selected;
                if (!(selectable == null))
                {
                    Building building = SelectTool.Instance.selected.GetComponent<Building>();

                    // if it's a stained glass tile but not the default one
                    if (building != null && building.Def.name.Contains("StainedGlassTile") && building.Def.name != "DefaultStainedGlassTile")
                    {
                        var buildingDefault = UnityEngine.Object.Instantiate(building);
                        buildingDefault.Def = Assets.GetBuildingDef(StainedGlassTileConfig.ID);
                        if (buildingDefault != null)
                        {
                            PlanScreen.Instance.CopyBuildingOrder(buildingDefault);

                            buildingDefault.gameObject.SetActive(false);
                            UnityEngine.Object.Destroy(buildingDefault);

                            GameObject copyBuildingButton = Traverse.Create(PlanScreen.Instance).Field("copyBuildingButton").GetValue<GameObject>();
                            copyBuildingButton.SetActive(false);
                        }
                    }
                }
            }
        }


    }
}