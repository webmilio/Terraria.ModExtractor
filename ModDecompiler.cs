using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace ModDecompiler;

public class ModDecompiler : Mod
{
    private static Assembly tModLoader = typeof(ModLoader).Assembly;
    
    private static Type tInterface = tModLoader.GetType("Terraria.ModLoader.UI.Interface");
    private static FieldInfo fUiMods = tInterface.GetField("modsMenu", BindingFlags.NonPublic | BindingFlags.Static);
    
    private static Type tUiMods = tModLoader.GetType("Terraria.ModLoader.UI.UIMods");
    private  static FieldInfo fUiModsList = tUiMods.GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
    
    private  static Type tModItem = tModLoader.GetType("Terraria.ModLoader.UI.UIModItem");
    private static FieldInfo fMod = tModItem.GetField("_mod", BindingFlags.NonPublic | BindingFlags.Instance);
    
    private static FieldInfo fBuildProperties = fMod.FieldType.GetField("properties", BindingFlags.Public | BindingFlags.Instance);
    private static string[] fieldNames = { "hideCode", "hideResources" };
    private static FieldInfo[] fields;

    private IList modItems;
    private int _lastModCount = 0;

    public override void Load()
    {
        On_Main.DrawMenu += On_Main_DrawMenu;

        var mFields = new List<FieldInfo>(fieldNames.Length);
        var tBuildProperties = fBuildProperties.FieldType;

        foreach (var name in fieldNames)
        {
            var field = tBuildProperties.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            mFields.Add(field);
        }

        fields = mFields.ToArray();

        var uiMods = fUiMods.GetValue(null);
        modItems = fUiModsList.GetValue(uiMods) as IList;
    }

    private void On_Main_DrawMenu(On_Main.orig_DrawMenu orig, Main self, Microsoft.Xna.Framework.GameTime gameTime)
    {
        orig(self, gameTime);

        if (modItems?.Count > 0)
        {
            if (_lastModCount == modItems.Count)
            {
                // UI building is done.
                foreach (var modItem in modItems)
                {
                    var localMod = fMod.GetValue(modItem);
                    var buildProperties = fBuildProperties.GetValue(localMod);

                    foreach (var field in fields)
                    {
                        field.SetValue(buildProperties, false);
                    }
                }

                On_Main.DrawMenu -= On_Main_DrawMenu;
            }

            _lastModCount = modItems.Count;
        }
    }
}