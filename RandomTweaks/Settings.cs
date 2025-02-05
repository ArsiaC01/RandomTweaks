﻿using HarmonyLib;
using SFS.Input;
using SFS.IO;
using SFS.Parsers.Json;
using SFS.Translations;
using SFS.UI;
using System;
using System.Globalization;
using UnityEngine;
using static SFS.Input.KeybindingsPC;

namespace RandomTweaks
{
    // Make everything use the same non-static classes
    public static class ClassHolder
    {
        public static CustomKeybinds custom_keybinds = new CustomKeybinds();
        public static PlayerPreferences player_preferences = new PlayerPreferences();
    }

    // Save hook for... Saving
    [HarmonyPatch(typeof(Settings<Data>), "Save")]
    public class KeybindsSaveHook
    {
        [HarmonyPostfix]
        public static void SaveHook(Settings<Data> __instance)
        {
            // This works. I don't know why. At least it sucessfully prevents infinite loops
            if (__instance is KeybindingsPC)
            {
                ClassHolder.custom_keybinds.SaveData();
            }
        }
    }

    // Awake hook to add the menu items
    [HarmonyPatch(typeof(KeybindingsPC), "Awake")]
    public class KeybindsAwakeHook
    {
        [HarmonyPrefix]
        public static void Prefix(KeybindingsPC __instance)
        {
            ClassHolder.custom_keybinds.AwakeHook(__instance);
        }
    }

    // Akin to KeybindingsPC, holds the relevant keys not already included
    public class CustomKeybinds : Settings<CustomKeybinds.DefaultData>
    {
        protected override string FileName => "RandomTweaksKeybinds";

        public static DefaultData custom_keys = new DefaultData();
        public class DefaultData
        {

            public Key[] Move_Parts = new Key[4]
            {
                KeyCode.UpArrow,
                KeyCode.DownArrow,
                KeyCode.LeftArrow,
                KeyCode.RightArrow
            };

            public Key[] Resize_Parts = new Key[4]
            {
                Key.Ctrl_(KeyCode.UpArrow),
                Key.Ctrl_(KeyCode.DownArrow),
                Key.Ctrl_(KeyCode.LeftArrow),
                Key.Ctrl_(KeyCode.RightArrow)
            };

            public Key Toggle_New_Build_System = KeyCode.N;

            public Key[] Set_Values = new Key[3]
            {
                Key.Ctrl_(KeyCode.Comma),
                Key.Ctrl_(KeyCode.Period),
                Key.Ctrl_(KeyCode.Minus)
            };

        }

        // Loads automatically(?), no need for a hook here
        protected override void OnLoad()
        {
            custom_keys = settings;
        }

        public void AwakeHook(KeybindingsPC __instance)
        {

            // Load the saved and default keys
            Load();
            DefaultData defaultData = new DefaultData();

            // Reflection needed since these are private
            Traverse createTraverse = Traverse.Create(__instance).Method("Create", new object[] { custom_keys.Move_Parts[0], defaultData.Move_Parts[0], "Move_Selected_Up" });
            Traverse createSpaceTraverse = Traverse.Create(__instance).Method("CreateSpace");
            Traverse createTextTraverse = Traverse.Create(__instance).Method("CreateText", "RandomTweaks Keybinds");

            // Finally actually call the code
            createTextTraverse.GetValue("RandomTweaks Keybinds");
            createTraverse.GetValue(new object[] { custom_keys.Move_Parts[0], defaultData.Move_Parts[0], "Move_Selected_Up" });
            createTraverse.GetValue(new object[] { custom_keys.Move_Parts[1], defaultData.Move_Parts[1], "Move_Selected_Down" });
            createTraverse.GetValue(new object[] { custom_keys.Move_Parts[2], defaultData.Move_Parts[2], "Move_Selected_Left" });
            createTraverse.GetValue(new object[] { custom_keys.Move_Parts[3], defaultData.Move_Parts[3], "Move_Selected_Right" });
            createSpaceTraverse.GetValue();
            createTraverse.GetValue(new object[] { custom_keys.Resize_Parts[0], defaultData.Resize_Parts[0], "Increase_Part_Height" });
            createTraverse.GetValue(new object[] { custom_keys.Resize_Parts[1], defaultData.Resize_Parts[1], "Decrease_Part_Height" });
            createTraverse.GetValue(new object[] { custom_keys.Resize_Parts[2], defaultData.Resize_Parts[2], "Increase_Part_Width" });
            createTraverse.GetValue(new object[] { custom_keys.Resize_Parts[3], defaultData.Resize_Parts[3], "Decrease_Part_Width" });
            createSpaceTraverse.GetValue();
            createTraverse.GetValue(new object[] { custom_keys.Set_Values[0], defaultData.Set_Values[0], "Set_Move_Small_Incremenet" });
            createTraverse.GetValue(new object[] { custom_keys.Set_Values[1], defaultData.Set_Values[1], "Set_Rotate_Small_Incremenet" });
            createTraverse.GetValue(new object[] { custom_keys.Set_Values[2], defaultData.Set_Values[2], "Set_Size_Small_Incremenet" });
            createSpaceTraverse.GetValue();
            createTraverse.GetValue(new object[] { custom_keys.Toggle_New_Build_System, defaultData.Toggle_New_Build_System, "Toggle_New_Build_Mode" });
            createSpaceTraverse.GetValue();
            createSpaceTraverse.GetValue();
            createSpaceTraverse.GetValue();
        }

        public void SaveData()
        {
            Save();
        }

    }

    public class PlayerPreferences : Settings<PlayerPreferences.DefaultData>
    {
        protected override string FileName => "RandomTweaksPreferences";

        public static DefaultData custom_values = new DefaultData();
        public class DefaultData
        {
           
            public float normal_move = 0.5f;
            public float normal_rotate = 90f;
            public float normal_resize = 0.5f;
            public float small_move = 0.1f;
            public float small_rotate = 15f;
            public float small_resize = 0.1f;

        }

        public void SetSmallMove()
        {
            Load();
            Debug.Log(JsonWrapper.ToJson(settings, pretty: true));
            Menu.textInput.Open(Loc.main.Cancel, Loc.main.Save, SetNewSmallMove, CloseMode.Current, TextInputMenu.Element("Small move step size", string.Empty));

            void SetNewSmallMove(string[] value)
            {
                try
                {
                    float new_value = float.Parse(value[0], CultureInfo.InvariantCulture);
                    custom_values.small_move = new_value;
                    Save();
                }
                catch
                {
                    MsgDrawer.main.Log("Could not parse value, the new move value will not be set");
                }
            }
        }

        public void SetSmallRotate()
        {
            Load();
            Menu.textInput.Open(Loc.main.Cancel, Loc.main.Save, SetNewSmallResize, CloseMode.Current, TextInputMenu.Element("Small rotate step size", string.Empty));

            void SetNewSmallResize(string[] value)
            {
                try
                {
                    float new_value = float.Parse(value[0], CultureInfo.InvariantCulture);
                    custom_values.small_rotate = new_value;
                    Save();
                }
                catch
                {
                    MsgDrawer.main.Log("Could not parse value, the new rotate value will not be set");
                }
            }
        }

        public void SetSmallResize()
        {
            Load();
            Menu.textInput.Open(Loc.main.Cancel, Loc.main.Save, SetNewSmallResize, CloseMode.Current, TextInputMenu.Element("Small resize step size", string.Empty));

            void SetNewSmallResize(string[] value)
            {
                try
                {
                    float new_value = float.Parse(value[0], CultureInfo.InvariantCulture);
                    custom_values.small_move = new_value;
                    Save();
                }
                catch
                {
                    MsgDrawer.main.Log("Could not parse value, the new resize value will not be set");
                }
            }
        }

        // Loads automatically(?), no need for a hook here
        protected override void OnLoad()
        {
            custom_values = settings;
        }

    }
}
