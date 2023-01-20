#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FantasticGlass
{
    [ExecuteInEditMode]
    [InitializeOnLoad]
    public class GlassPackageSetup
    {
        public static string glassDefineSymbol = "FD_GLASS";
        public static bool debugGlassPackageSetup = false;
        public static string glassSetupFirstRunComplete = "glassSetupFirstRunComplete";
        public static string glassSetupFirstRunCompleteThisVersion = "glassSetupFirstRunComplete_version_" + Glass.versionString;

        static GlassPackageSetup()
        {
            int firstRunComplete = PlayerPrefs.GetInt(glassSetupFirstRunComplete);
            int firstRunCompleteThisVersion = PlayerPrefs.GetInt(glassSetupFirstRunCompleteThisVersion);
            //
            if (firstRunComplete == 0)
            {
                Debug.Log("Fantastic Glass :: Running First Time Setup (Ever / 1.2.0)");
                RunSetup_FirstTime_Ever();
                PlayerPrefs.SetInt(glassSetupFirstRunComplete, 1);
                Debug.Log("Fantastic Glass :: Finished First Time Setup (Ever / 1.2.0)");
            }
            //
            if (firstRunCompleteThisVersion == 0)
            {
                Debug.Log("Fantastic Glass :: Running First Time Setup (This Version)");
                RunSetup_FirstTime_ThisVersion();
                PlayerPrefs.SetInt(glassSetupFirstRunCompleteThisVersion, 1);
                Debug.Log("Fantastic Glass :: Finished First Time Setup (This Version)");
            }
        }

        public static void RunSetup_FirstTime_Ever()
        {
            AddDefineSymbols();
            SetCustomGUI_IfPro();
        }

        public static void RunSetup_FirstTime_ThisVersion()
        {
            List<GlassPreset> presets = LoadAllPresets();
			CleanUpPresetList(ref presets);
            for(int i=0; i < presets.Count; i++)
            {
                GlassPreset preset = presets[i];
                Setup_Preset_ThisVersion(ref preset);
				if(preset!=null)
				{
                	preset.Save();
				}
            }
        }
	
		public static void CleanUpPresetList(ref List<GlassPreset> presetList)
		{
			for(int i = presetList.Count-1; i >=0; i--)
			{
				GlassPreset testPreset = presetList[i];
				//
				if (testPreset == null)
				{
					presetList.RemoveAt(i);
				}
			}
		}

        public static void Setup_Preset_ThisVersion(ref GlassPreset preset)
        {
            //  TODO: handle changes that should occur for this version of Glass's presets that didn't previously
        }

        public static List<GlassPreset> LoadAllPresets()
        {
            List<string> presetNames = Glass_Editor.GetPresetList();
            List<GlassPreset> presets = new List<GlassPreset>();
            foreach (string presetName in presetNames)
            {
                GlassPreset preset = GlassPreset.LoadFromName(presetName);
                presets.Add(preset);
            }
            return presets;
        }

        /// <summary>
        /// Sets the Glass System Settings option 'useCusomGUI' if the user is using the Pro Unity Skin.
        /// </summary>
        public static void SetCustomGUI_IfPro()
        {
            string packagePath = Application.dataPath + "/" + GlassManager.default_packageName + "/";
            string xmlPath = packagePath + GlassManager.default_xml_Pathname + "/";
            string settingsPath = xmlPath + GlassManager.default_settings_Filename + ".xml";
            GlassSystemSettings settings = GlassSystemSettings.LoadFromXML(settingsPath);
            if (EditorGUIUtility.isProSkin)
            {
                Debug.Log("Fantastic Glass :: User has Pro Unity Skin, enabling custom GUI skin - setting available from Glass Manager & Glass Maker.");
                settings.useCustomGUI = EditorGUIUtility.isProSkin;
            }
        }

        public static void AddDefineSymbols()
        {
            foreach (BuildTargetGroup buildTargetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (buildTargetGroup == BuildTargetGroup.Unknown)
                {
                    if (debugGlassPackageSetup)
                        Debug.Log("Glass Package Setup: skipping unknown target group '" + buildTargetGroup.ToString() + "'");
                    continue;
                }
#if UNITY_5_4_OR_NEWER
				if ((buildTargetGroup == (BuildTargetGroup)15)
				    || (buildTargetGroup == (BuildTargetGroup)16))
				{
					if (debugGlassPackageSetup)
						Debug.Log("Glass Package Setup: skipping built target group '" + buildTargetGroup + "' as group is obsolote.");
					continue;
				}
#endif
                string buildTargetGroupDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                if (!buildTargetGroupDefineSymbols.Contains(glassDefineSymbol))
                {
                    Debug.Log("Added Define Symbols for Glass package.");
                    if (buildTargetGroupDefineSymbols.Length == 0)
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, glassDefineSymbol);
                    else
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, "; " + glassDefineSymbol);
                }
            }
        }
    }

}

#endif
