#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Xml.Serialization;
using System.IO;

namespace FantasticGlass
{

    /// <summary>
    /// Glass editor.
    /// NOTE: Do not place this file in an 'Editor' folder as it will break.
    /// </summary>
    [CustomEditor(typeof(Glass))]
    [CanEditMultipleObjects]
    public class Glass_Editor : Editor
    {
        #region Member Variables

        EditorTools tools;

        Glass_GlassManager_Settings managerSettings = null;

        GlassRenderOrderManager renderOrderManager = null;

        Glass glass;
        List<Glass> glassList = new List<Glass>();

        bool showSection_Presets;
        bool showSection_Materials;
        bool showSection_SharedGlass;
        bool showSection_Shader;
        bool showSection_Depth;
        bool showSection_ImportantNotes;
        bool showSection_AboutPresetName;
        //
        bool showSection_Albedo;
        bool showSection_Distortion;
        bool showSection_Bump;
        bool showSection_Extinction;
        bool showSection_Aberration;
        bool showSection_Fog;
        bool showSection_Surface;
        bool showSection_Textures;
        bool showSection_Physics;
        bool showAttribute_Depth;
        //
        bool showSection_AboutSharedGlass;
        bool showSection_Physics_AboutZFighting;
        //
        GlassPreset attributePreset;
        bool showAttribute_Albedo;
        bool showAttribute_Distortion;
        bool showAttribute_Bump;
        bool showAttribute_Extinction;
        bool showAttribute_Aberration;
        bool showAttribute_Fog;
        bool showAttribute_Surface;
        bool showAttribute_Textures;
        bool showAttribute_Physics;

        bool loadAllAttributeValues = false;

        Dictionary<string, bool> attributeValueNames = new Dictionary<string, bool>();

        bool showList_SharedGlass;
        List<Glass> glassOthers = null;

        Material tempMatFront;
        Material tempMatBack;
        //bool materialChangeDetected;

        string[] textureAALabels = new string[] { "None", "2 Samples", "4 Samples", "8 Samples" };
        int[] textureAAItems = new int[] { 1, 2, 4, 8 };
        int textureAAIndex = 0;

        Vector2 scrollSharedGlass = new Vector2();

        string presetName;
        GlassPreset currentPreset;
        string selectedPresetName;
        int selectedPresetIndex = -1;
        List<string> presetList;
        bool showPresetList = true;

        string packagePath = "Fantastic Glass";
        string xmlPath = "XML";
        string presetListPath = "glass_preset_list";
        string materialsPath = "Materials";

        bool editorWasDisabled = false;

        #endregion

        #region Callbacks (Enable, Disable)

        void OnEnable()
        {
            if (glass == null)
                glass = (Glass)target;

            if (tools == null)
                tools = new EditorTools("Glass_" + glass.presetName + glass.name + glass.gameObject.GetInstanceID().ToString());

            if (glass == null)
                return;

            if (glass.manager == null)
                glass.FindGlassManager();

            if (!glass.manager.LayersExist())
                GlassManager_Editor.Show_LayersWarning();

            if (renderOrderManager == null)
                renderOrderManager = GlassRenderOrderManager.Instance;

            if (!packagePath.Contains(Application.dataPath))
            {
                packagePath = Application.dataPath + "/" + packagePath + "/";
                xmlPath = packagePath + xmlPath + "/";
                presetListPath = xmlPath + presetListPath + ".xml";
                materialsPath = packagePath + materialsPath + "/";
                materialsPath = FileUtil.GetProjectRelativePath(materialsPath);
            }

            if (presetList == null)
                LoadPresetList();
            presetName = glass.presetName;
            selectedPresetIndex = presetList.IndexOf(presetName);

            glass.UpdateGlassManager();

            UpdateOtherGlassList();

            LoadGlassManagerSettings();
        }

        void LoadGlassManagerSettings()
        {
            managerSettings = Glass_GlassManager_Settings.Load(xmlPath, glass);
            if (glass.manager != null)
            {
                glass.manager.LoadManager();
            }
            UpdateTextureAAIndex();
        }

        void OnDisable()
        {
            editorWasDisabled = true;

            showAttribute_Albedo = false;
            showAttribute_Distortion = false;
            showAttribute_Bump = false;
            showAttribute_Extinction = false;
            showAttribute_Aberration = false;
            showAttribute_Fog = false;
            showAttribute_Surface = false;
            showAttribute_Textures = false;
            showAttribute_Physics = false;
        }

        #endregion

        #region GUI

        public override void OnInspectorGUI()
        {
            UpdateTarget();

            UpdateTargetList();

            UpdateEditorTools();

            FindGlassManager();

            if (editorWasDisabled)
            {
                LoadPresetList();
                editorWasDisabled = false;
            }

            Section_GlassName();
            Section_Presets();
            Section_SharedGlass();
            Section_ShaderOptions();
            Section_Textures();
            Section_PhysicsOptions();
            Section_Materials();
            //Section_RenderOrder();
            Section_ManagerSettings();
            Section_ImportantNotes();
        }

        void UpdateTarget()
        {
            glass = (Glass)target;
        }

        void UpdateTargetList()
        {
            glassList.Clear();
            if (targets != null)
            {
                foreach (UnityEngine.Object targetIter in targets)
                {
                    Glass glassIter = targetIter as Glass;
                    if (glassIter != null)
                    {
                        if (!glassList.Contains(glassIter))
                            glassList.Add(glassIter);
                    }
                }
            }
        }

        void UpdateEditorTools()
        {
            if (tools == null)
                tools = new EditorTools("Glass_" + glass.name);
        }

        void FindGlassManager()
        {
            if (glass.manager == null)
                glass.FindGlassManager();
        }

        #endregion

        #region Section GlassName

        void Section_GlassName()
        {
            string glassName = glass.presetName;
            if (GlassNamesDiffer())
            {
                if (glassList.Count > 1)
                {
                    glassName = "";
                }
            }

            tools.StartEdit(glass, "Changed Glass Name");
            tools.StartChangeCheck();

            tools.LabelOption("Glass Name", ref glassName);

            if (tools.EndChangeCheck())
            {
                glass.presetName = glassName;
                glass.manager.FindGlass();
                foreach (Glass glassIter in glassList)
                {
                    glassIter.presetName = glassName;
                }
                UpdateOtherGlassList();
                tools.SetDirty(glass);
            }

            tools.FinishEdit(glass);

            if (tools.ShowSection("(About Glass Name)", ref showSection_AboutPresetName))
            {
                tools.Label("The settings of Glass objects with matching Glass Names can be synchronised.\nSynchronisation settings are found in the GlassManager and Glass creator.", true);
                tools.EndSection();
            }
        }

        #endregion

        #region Section Presets

        void Section_Presets()
        {
            if (presetList == null)
            {
                LoadPresetList();
            }

            tools.StartChangeCheck();

            if (tools.ShowSection("Presets", ref glass.showSection_Presets))
            {
                if (tools.EndChangeCheck())
                {
                    presetName = glass.presetName;

                    selectedPresetIndex = presetList.IndexOf(presetName);
                }

                tools.BoldLabel("Save");

                tools.StringOption("Name", ref presetName, true);

                if (tools.Button("Save"))
                {
                    if (Application.isPlaying)
                    {
                        SavePreset();
                        Debug.Log("Saved settings as '" + presetName + ". Confirmation dialogue skipped as not displayable during Play. Load this preset after Play to retain these settings.");
                    }
                    else if (PresetExists(presetName))
                    {
                        if (tools.Message("Preset Exists", "A preset named '" + glass.presetName + "' aready exists.\n\nWould you like to replace it?", "Yes", "Cancel"))
                        {
                            SavePreset();
                            tools.Message("Preset Settings Saved", "Saved settings to preset '" + presetName + ".");
                        }
                    }
                    else {
                        SavePreset();
                        tools.Message("Preset Settings Saved", "Saved settings to preset '" + presetName + ".");
                    }
                }

                Section_Save_CopyList();

                tools.BoldLabel("Load");

                tools.StartEdit(glass, "Loaded Glass Preset");

                switch (tools.PresetList("Loadable Presets", presetListPath, ref selectedPresetName, ref selectedPresetIndex, ref presetList, ref showPresetList, true, false, false, false))
                {
                    case EditorToolsPreset_Option.LoadItem:
                        if (Application.isPlaying)
                        {
                            LoadSelectedPreset();
                            Debug.Log("Loaded settings from preset '" + selectedPresetName + ". Confirmation dialogue skipped as not displayable during Play. Load again after Play to retain these settings.");
                        }
                        else
                        {
                            LoadSelectedPreset();
                            tools.Message("Preset Settings Loaded", "Loaded settings from preset '" + selectedPresetName + ".");
                            presetName = glass.presetName;
                        }
                        break;
                    case EditorToolsPreset_Option.Hidden:
                        break;
                    default:
                        Section_Load_CopyList();
                        break;
                }

                tools.FinishEdit(glass);

                tools.EndSection();
            }
        }

        void Section_Save_CopyList()
        {
            if (!tools.BoolOption("Save" + " Everything", ref glass.saveCopyList.everything))
            {
                tools.StartSection();
                tools.BoolOption("Colour", ref glass.saveCopyList.albedo);
                tools.BoolOption("Distortion", ref glass.saveCopyList.distortion);
                tools.BoolOption("Bump", ref glass.saveCopyList.bump);
                tools.BoolOption("Extinction", ref glass.saveCopyList.extinction);
                tools.BoolOption("Aberration", ref glass.saveCopyList.aberration);
                tools.BoolOption("Fog", ref glass.saveCopyList.fog);
                tools.BoolOption("Surface", ref glass.saveCopyList.surface);
                tools.BoolOption("Mesh", ref glass.saveCopyList.model);
                tools.BoolOption("Depth", ref glass.saveCopyList.depth);
                tools.BoolOption("Z Fight Radius", ref glass.saveCopyList.zFightRadius);
                tools.EndSection();
            }
        }

        void Section_Load_CopyList()
        {
            if (!tools.BoolOption("Load" + " Everything", ref glass.loadCopyList.everything))
            {
                tools.StartSection();
                tools.BoolOption("Colour", ref glass.loadCopyList.albedo);
                tools.BoolOption("Distortion", ref glass.loadCopyList.distortion);
                tools.BoolOption("Bump", ref glass.loadCopyList.bump);
                tools.BoolOption("Extinction", ref glass.loadCopyList.extinction);
                tools.BoolOption("Aberration", ref glass.loadCopyList.aberration);
                tools.BoolOption("Fog", ref glass.loadCopyList.fog);
                tools.BoolOption("Surface", ref glass.loadCopyList.surface);
                //tools.BoolOption("Mesh", ref glass.loadCopyList.model);
                tools.BoolOption("Depth", ref glass.loadCopyList.depth);
                tools.BoolOption("Z Fight Radius", ref glass.loadCopyList.zFightRadius);
                tools.EndSection();
            }
        }

        bool PresetExists(string presetName)
        {
            return presetList.Contains(presetName);
        }

        string CurrentPresetFilename()
        {
            return xmlPath + glass.presetName + ".xml";
        }

        string SelectedPresetFilename()
        {
            return xmlPath + selectedPresetName + ".xml";
        }

        string NewPresetFilename()
        {
            return xmlPath + presetName + ".xml";
        }

        void SavePreset(GlassSettingsCopyList featuresToSave)
        {
            currentPreset = glass.GeneratePreset(featuresToSave, LoadPresetCopy());
            currentPreset.name = presetName;
            currentPreset.Save(NewPresetFilename());
            if (!presetList.Contains(presetName))
            {
                presetList.Add(presetName);
                SavePresetList();
                LoadPresetList();
            }
            selectedPresetIndex = presetList.IndexOf(presetName);
            selectedPresetName = presetName;
            LoadCurrentPreset();
            //LoadSelectedPreset();
        }

        void SavePreset()
        {
            SavePreset(glass.saveCopyList);
        }

        /// <summary>
        /// Loads the current Glass preset (without any current changes).
        /// </summary>
        /// <returns></returns>
        GlassPreset LoadPresetCopy()
        {
            return GlassPreset.LoadFromXML(CurrentPresetFilename()) as GlassPreset;
        }

        void LoadCurrentPreset()
        {
            tools.StartEdit(glass, "Loaded Active Glass Preset");

            currentPreset = GlassPreset.LoadFromXML(CurrentPresetFilename()) as GlassPreset;

            if (currentPreset != null)
            {
                glass.LoadFromPreset(currentPreset, glass.loadCopyList);
                glass.manager.GlassModified(glass);
            }
            else {
                Debug.LogError("Preset does not exist: " + presetName);
            }

            tools.FinishEdit(glass);
        }

        void LoadSelectedPreset()
        {
            tools.StartEdit(glass, "Loaded Selected Glass Preset");

            currentPreset = GlassPreset.LoadFromXML(SelectedPresetFilename()) as GlassPreset;

            if (currentPreset != null)
            {
                glass.LoadFromPreset(currentPreset, glass.loadCopyList);
                glass.manager.GlassModified(glass);
            }
            else {
                Debug.LogError("Preset does not exist: " + presetName);
            }

            tools.FinishEdit(glass);
        }

        bool LoadAttributePreset(string presetName)
        {
            attributePreset = GlassPreset.LoadFromXML(SelectedPresetFilename()) as GlassPreset;

            if (attributePreset == null)
            {
                Debug.LogError("Preset does not exist: " + presetName);
                return false;
            }

            ResetAttributeLoadSelectors();

            //	TODO: depricate in future
            //UpdateAttributeLoadSelectors();

            return true;
        }

        void ResetAttributeLoadSelectors()
        {
            List<string> attributeSelectorNames = new List<string>(attributeValueNames.Keys);
            foreach (string attributeSelectorName in attributeSelectorNames)
            {
                attributeValueNames[attributeSelectorName] = false;
            }
        }

        //	TODO: depricate in future
        /*
        void UpdateAttributeLoadSelectors()
        {
        }
        */

        void SavePresetList()
        {
            SaveList(presetList, presetListPath);
        }

        public void SaveList(List<string> _list, string _path)
        {
            XmlSerializer xmlserialiser = new XmlSerializer(typeof(List<string>));
            FileStream fileStream = new FileStream(_path, FileMode.Create);
            xmlserialiser.Serialize(fileStream, _list);
            fileStream.Close();
        }

        void LoadPresetList()
        {
            presetList = LoadList(presetListPath);
        }

        public static string GetPackagePath()
        {
            return Application.dataPath + "/" + "Fantastic Glass" + "/";
        }

        public static string GetXMLPath()
        {
            return GetPackagePath() + "XML" + "/";
        }

        public static string GetPresetListPath()
        {
            return GetXMLPath() + "glass_preset_list" + ".xml";
        }

        public static List<string> GetPresetList()
        {
            return LoadList(GetPresetListPath());
        }

        public static List<string> LoadList(string _path)
        {
            if (!File.Exists(_path))
            {
                Debug.Log("(Preset List) File does not exists:" + _path);
                return null;
            }
            XmlSerializer xmlserialiser = new XmlSerializer(typeof(List<string>));
            FileStream fileStream = new FileStream(_path, FileMode.Open);
            List<string> loadedList = xmlserialiser.Deserialize(fileStream) as List<string>;
            fileStream.Close();
            return loadedList;
        }

        #endregion

        #region Section SharedGlass

        private void Section_SharedGlass()
        {
            //if (glass.manager.GlassIsShared(glass))
            if (glassOthers == null)
                UpdateOtherGlassList();
            if (glassOthers.Count > 1)
            {
                tools.StartChangeCheck();
                if (tools.ShowSection("Shared Glass (" + glassOthers.Count + ")", ref glass.showSection_SharedGlass))
                {
                    if (tools.ShowSection("About Shared Glass", ref showSection_AboutSharedGlass))
                    {
                        tools.Label("The settings for Glass objects that match are synchronised.\nThe way in which Glass is matched is defined in the GlassManager and the 'GameObject/Create Other/Glass...' function.", true);
                        tools.EndSection();
                    }
                    tools.GUI_List("Objects sharing these Glass settings (" + glassOthers.Count + ")", ref glassOthers, ref showList_SharedGlass, ref scrollSharedGlass);
                    tools.EndSection();
                }
                if (tools.EndChangeCheck())
                {
                    UpdateOtherGlassList();
                }
            }
        }

        #endregion

        #region Section Other Glass List

        void UpdateOtherGlassList()
        {
            glassOthers = glass.manager.SharedGlassOthers(glass);
        }

        #endregion

        #region Settings

        void Section_ShaderOptions()
        {
            if (tools.ShowSection("Settings", ref glass.showSection_Shader))
            {
                Section_Albedo();
                Section_Distortion();
                Section_Bump();
                Section_Extinction();
                Section_Aberration();
                Section_Fog();
                Section_Depth();
                Section_Surface();
                tools.EndSection();
            }
        }

        #endregion

        #region Settings - Albedo

        void Section_Albedo()
        {
            if (tools.ShowSection("Albedo", ref glass.showSection_Albedo))
            {
                tools.StartEdit(glass, "Changed Glass Albedo");
                tools.StartChangeCheck();

                if (!Section_PresetAttribute(GlassAttributeType.albedo, ref showAttribute_Albedo))
                {
                    tools.ColourOption("Base Colour", ref glass.colour_albedo);
                    Texture previousAlbedoTexture = glass.texture_albedo;
                    tools.TextureOption("Texture", ref glass.texture_albedo);
                    if (glass.texture_albedo != null)
                    {
                        if (previousAlbedoTexture == null)  //  Setting texture anew, make it visible
                        {
                            if (glass.colour_albedoTexture.a == 0f)
                            {
                                glass.colour_albedoTexture.a = 1f;
                            }
                            if (glass.opacity == 0f)
                            {
                                glass.opacity = 1.0f;
                            }
                        }
                        tools.ColourOption("Texture Colour", ref glass.colour_albedoTexture);
                    }

                    tools.FloatOption("Opacity", ref glass.opacity);
                }

                if (tools.EndChangeCheck())
                {
                    glass.UpdateTexturesAndColours();
                }

                tools.FinishEdit(glass);

                tools.EndSection();
            }
        }

        #endregion

        #region Settings - Distortion

        void Section_Distortion()
        {
            if (tools.ShowSection("Distortion", ref glass.showSection_Distortion))
            {
                tools.StartChangeCheck();
                tools.StartEdit(glass, "Changed Glass Distortion");

                if (!Section_PresetAttribute(GlassAttributeType.distortion, ref showAttribute_Distortion))
                {
                    tools.TextureOption("Texture", ref glass.texture_distortion);

                    if (tools.BoolOption("Front", ref glass.enableDistortion_front, true))
                    {
                        tools.StartSection();
                        if (glass.texture_distortion != null)
                        {
                            tools.FloatOption("Bump", ref glass.distortion_front.x);
                            /*
                            if (glass.distortion_front.x != 0f)
                            {
                                tools.BoolOption("Double Depth Test", ref glass.enableDoubleDepth_front);
                            }
                            */
                        }
                        tools.BoolOption("Detailed Depth", ref glass.enableDoubleDepth_front);
                        tools.FloatOption("Mesh", ref glass.distortion_front.y);
                        tools.FloatOption("Overall", ref glass.distortion_front.z);
                        tools.FloatOption("Max", ref glass.distortion_front.w);
                        tools.FloatOption("Edge Bend", ref glass.distortionEdgeBend_front);
                        tools.FloatOption("Depth Fade", ref glass.distortionDepthFade_front);
                        tools.EndSection();
                    }

                    if (tools.BoolOption("Back", ref glass.enableDistortion_back, true))
                    {
                        tools.StartSection();
                        if (glass.texture_distortion != null)
                        {
                            tools.FloatOption("Bump", ref glass.distortion_back.x);
                            /*
                            if (glass.distortion_back.x != 0f)
                            {
                                tools.BoolOption("Double Depth Test", ref glass.enableDoubleDepth_back);
                            }
                            */
                        }
                        tools.BoolOption("Detailed Depth", ref glass.enableDoubleDepth_back);
                        tools.FloatOption("Mesh", ref glass.distortion_back.y);
                        tools.FloatOption("Overall", ref glass.distortion_back.z);
                        tools.FloatOption("Max", ref glass.distortion_back.w);
                        tools.FloatOption("Edge Bend", ref glass.distortionEdgeBend_back);
                        tools.FloatOption("Depth Fade", ref glass.distortionDepthFade_back);
                        tools.EndSection();
                    }

                }

                EditorGUILayout.Space();

                /*
                if (glass.texture_distortion != null)
                {
                    if ((glass.distortion_back.x != 0f) || (glass.distortion_front.x != 0f))
                    {
                        if (tools.ShowSection("Detailed Depth?", ref showSection_AboutDoubleDepth))
                        {
                            tools.Label("In short, when enabled, the effects will look more accurate. The second depth test allows the use of the distorted volume for extinction, aberration, and fog. However, when combined with high bump distortion, it can lead to edges bleeding/ghosting; try disabling when this occurs.", true);
                            tools.EndSection();
                        }
                    }
                }
                */
                if (tools.ShowSection("Detailed Depth?", ref glass.showSection_AboutDoubleDepth))
                {
                    tools.Label("In short, when enabled, the effects will look more accurate. The second depth test allows the use of the distorted volume for extinction, aberration, and fog. However, when combined with high bump distortion, it can lead to edges bleeding/ghosting; try disabling when this occurs.", true);
                    tools.EndSection();
                }

                if (tools.EndChangeCheck())
                {
                    /*
                    if (glass.texture_distortion == null)
                    {
                        glass.distortion_back.x = glass.distortion_front.x = 0f;
                    }
                    */
                    glass.UpdateDistortion();
                }

                tools.FinishEdit(glass);

                tools.EndSection();
            }
        }

        #endregion

        #region Settings - Bump

        void Section_Bump()
        {
            if (tools.ShowSection("Bump", ref showSection_Bump))
            {
                tools.StartChangeCheck();
                tools.StartEdit(glass, "Changed Glass Bump");

                if (!Section_PresetAttribute(GlassAttributeType.bump, ref showAttribute_Bump))
                {

                    tools.TextureOption("Texture", ref glass.texture_distortion);
                    if (glass.texture_distortion != null)
                    {
                        tools.FloatOption("Front", ref glass.bumpFront);
                        tools.FloatOption("Back", ref glass.bumpBack);
                    }

                }

                if (tools.EndChangeCheck())
                {
                    glass.UpdateBump();
                }

                tools.FinishEdit(glass);

                tools.EndSection();
            }
        }

        #endregion

        #region settings - physics

        void Section_PhysicsOptions()
        {
            if (tools.ShowSection("Physics Options", ref glass.showSection_Physics))
            {
                tools.StartEdit(glass, "Changed Glass Physics");

                if (!Section_PresetAttribute(GlassAttributeType.physics, ref showAttribute_Physics))
                {
                    tools.StartChangeCheck();

                    tools.FloatOption("Z Fighting Fix Magnitude", ref glass.zFightRadius);

                    if (tools.Button("Revert to Default"))
                    {
                        glass.zFightRadius = Glass.default_zFightRadius;
                    }

                    if (tools.EndChangeCheck())
                    {
                        if (tools.Message("Physics Options Changed", "Would you like to apply these changes to all matching Glass objects?", "Yes", "No"))
                        {
                            glass.UpdatePhysics();
                        }
                    }

                }

                tools.FinishEdit(glass);

                if (tools.ShowSection("About Z Fighting Fix", ref showSection_Physics_AboutZFighting))
                {
                    tools.Label("Z-fighting may occur if physical objects intersect.\nThis fix involves expanding any collider attached to the Glass object by the small set magnitude.\nThe default value should be small enough to fix z-fighting without being noticable.");
                    tools.EndSection();
                }

                tools.EndSection();
            }
        }

        #endregion

        #region settings - Extinction

        void Section_Extinction()
        {
            if (tools.ShowSection("Extinction", ref glass.showSection_Extinction))
            {
                tools.StartChangeCheck();
                tools.StartEdit(glass, "Changed Glass Extinction");

                if (!Section_PresetAttribute(GlassAttributeType.extinction, ref showAttribute_Extinction))
                {

                    glass.extinctionAppearance = (GlassExtinctionAppearance)tools.EnumOption("Extinction Appearance", glass.extinctionAppearance);

                    Section_Extinction_Front();
                    Section_Extinction_Back();
                    Section_Extinction_Both();

                }

                tools.EndSection();

                if (tools.EndChangeCheck())
                {
                    glass.UpdateExtinction();
                }

                tools.FinishEdit(glass);
            }
        }

        void Section_Extinction_Front()
        {
            if (glass.enableExtinction_both)
                return;

            tools.StartChangeCheck();

            if (tools.BoolOption("Front (Default)", ref glass.enableExtinction_front, true))
            {
                tools.StartSection();

                Section_Extinction_Front_Options();

                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Extinction = GlassFace.front;
            }
        }

        void Section_Extinction_Back()
        {
            if (glass.enableExtinction_both)
                return;

            tools.StartChangeCheck();

            if (tools.BoolOption("Back", ref glass.enableExtinction_back, true))
            {
                tools.StartSection();

                Section_Extinction_Back_Options();

                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Extinction = GlassFace.back;
            }
        }

        void Section_Extinction_Both()
        {
            tools.StartChangeCheck();

            if (tools.BoolOption("Both (matching)", ref glass.enableExtinction_both, true))
            {
                tools.StartSection();

                switch (glass.lastFaceEdited_Extinction)
                {
                    case GlassFace.front:
                        Section_Extinction_Front_Options();
                        break;
                    case GlassFace.back:
                        Section_Extinction_Back_Options();
                        break;
                }

                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                if (glass.enableExtinction_both)
                    SynchroniseExtinctionFaces();
            }
        }

        void Section_Extinction_Front_Options()
        {
            switch (glass.extinctionAppearance)
            {
                case GlassExtinctionAppearance.AsItAppears:
                    tools.ColourOption("Colour (as it appears)", ref glass.extinctionFlipped_front);
                    Glass.FlipColour(glass.extinctionFlipped_front, ref glass.extinction_front);//GetExtinctionColour(GlassFace.front);
                    break;
                case GlassExtinctionAppearance.AsApplied:
                    tools.ColourOption("Colour (extinction)", ref glass.extinction_front);
                    Glass.FlipColour(glass.extinction_front, ref glass.extinctionFlipped_front);//GetExtinctionColour_Flipped(GlassFace.front);
                    break;
            }

            tools.TextureOption("Texture", ref glass.texture_extinction_front);

            tools.FloatOption("Intensity", ref glass.extinctionMagnitude_front.x, -100f, 100f);
            tools.FloatOption("Minimum", ref glass.extinctionMagnitude_front.y, -100f, 100f);
            tools.FloatOption("Maximum", ref glass.extinctionMagnitude_front.z, -100f, 100f);

            tools.BoolOption("Cap (min,max)", ref glass.capExtinction_front);
        }

        void Section_Extinction_Back_Options()
        {
            switch (glass.extinctionAppearance)
            {
                case GlassExtinctionAppearance.AsItAppears:
                    tools.ColourOption("Colour (as it appears)", ref glass.extinctionFlipped_back);
                    Glass.FlipColour(glass.extinctionFlipped_back, ref glass.extinction_back);//GetExtinctionColour(GlassFace.back);
                    break;
                case GlassExtinctionAppearance.AsApplied:
                    tools.ColourOption("Colour (extinction)", ref glass.extinction_back);
                    Glass.FlipColour(glass.extinction_back, ref glass.extinctionFlipped_back);//GetExtinctionColour_Flipped(GlassFace.back);
                    break;
            }

            tools.TextureOption("Texture", ref glass.texture_extinction_back);

            tools.FloatOption("Intensity", ref glass.extinctionMagnitude_back.x, -10f, 10f);
            tools.FloatOption("Minimum", ref glass.extinctionMagnitude_back.y, -10f, 10f);
            tools.FloatOption("Maximum", ref glass.extinctionMagnitude_back.z, -10f, 10f);

            tools.BoolOption("Cap (min,max)", ref glass.capExtinction_back);
        }

        /// <summary>
        /// Copies values from the last edited face to its opposite. Called only when both faces' values are linked.
        /// </summary>
        void SynchroniseExtinctionFaces()
        {
            switch (glass.lastFaceEdited_Extinction)
            {
                case GlassFace.front:
                    glass.extinction_back = glass.extinction_front;
                    glass.extinctionMagnitude_back = glass.extinctionMagnitude_front;
                    glass.extinctionFlipped_back = glass.extinctionFlipped_front;
                    glass.texture_extinction_back = glass.texture_extinction_front;
                    glass.capExtinction_back = glass.capExtinction_front;
                    break;
                case GlassFace.back:
                    glass.extinction_front = glass.extinction_back;
                    glass.extinctionMagnitude_front = glass.extinctionMagnitude_back;
                    glass.extinctionFlipped_front = glass.extinctionFlipped_back;
                    glass.texture_extinction_front = glass.texture_extinction_back;
                    glass.capExtinction_front = glass.capExtinction_back;
                    break;
            }
        }

        #endregion

        #region Settings - Aberration

        void Section_Aberration()
        {
            if (tools.ShowSection("Aberration", ref glass.showSection_Aberration))
            {
                tools.StartChangeCheck();
                tools.StartEdit(glass, "Changed Glass Aberration");

                if (!Section_PresetAttribute(GlassAttributeType.aberration, ref showAttribute_Aberration))
                {

                    Section_Aberration_Front();
                    Section_Aberration_Back();
                    Section_Aberration_Both();

                }

                tools.EndSection();

                if (tools.EndChangeCheck())
                {
                    glass.UpdateAberration();
                }

                tools.FinishEdit(glass);
            }
        }

        void Section_Aberration_Front()
        {
            if (glass.enableAberration_both)
                return;

            if (tools.BoolOption("Front (Default)", ref glass.enableAberration_front, true))
            {
                tools.StartSection();

                Section_Aberration_Front_Options();

                tools.EndSection();
            }
        }

        void Section_Aberration_Back()
        {
            if (glass.enableAberration_both)
                return;

            if (tools.BoolOption("Back", ref glass.enableAberration_back, !true))
            {
                tools.StartSection();

                Section_Aberration_Back_Options();

                tools.EndSection();
            }
        }

        void Section_Aberration_Both()
        {
            tools.StartChangeCheck();

            if (tools.BoolOption("Both (matching)", ref glass.enableAberration_both, true))
            {
                tools.StartSection();

                switch (glass.lastFaceEdited_Aberration)
                {
                    case GlassFace.front:
                        Section_Aberration_Front_Options();
                        break;
                    case GlassFace.back:
                        Section_Aberration_Back_Options();
                        break;
                }

                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                SynchroniseAberrationFaces();
            }
        }

        void Section_Aberration_Front_Options()
        {
            tools.StartChangeCheck();

            tools.ColourOption("Colour (spread)", ref glass.aberration_front);
            tools.TextureOption("Texture", ref glass.texture_aberration_front);
            tools.FloatOption("Intensity", ref glass.aberrationMagnitude_front.x, -10f, 10f);
            tools.FloatOption("Minimum", ref glass.aberrationMagnitude_front.y, -10f, 10f);
            tools.FloatOption("Maximum", ref glass.aberrationMagnitude_front.z, -10f, 10f);
            tools.BoolOption("Cap (min,max)", ref glass.capAberration_front);

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Aberration = GlassFace.front;
            }
        }

        void Section_Aberration_Back_Options()
        {
            tools.StartChangeCheck();

            tools.ColourOption("Colour (spread)", ref glass.aberration_back);
            tools.TextureOption("Texture", ref glass.texture_aberration_back);
            tools.FloatOption("Intensity", ref glass.aberrationMagnitude_back.x, -10f, 10f);
            tools.FloatOption("Minimum", ref glass.aberrationMagnitude_back.y, -10f, 10f);
            tools.FloatOption("Maximum", ref glass.aberrationMagnitude_back.z, -10f, 10f);
            tools.BoolOption("Cap (min,max)", ref glass.capAberration_back);

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Aberration = GlassFace.back;
            }
        }

        /// <summary>
        /// Copies values from the last edited face to its opposite. Called only when both faces' values are linked.
        /// </summary>
        void SynchroniseAberrationFaces()
        {
            switch (glass.lastFaceEdited_Aberration)
            {
                case GlassFace.front:
                    glass.aberration_back = glass.aberration_front;
                    glass.aberrationMagnitude_back = glass.aberrationMagnitude_front;
                    glass.texture_aberration_back = glass.texture_aberration_front;
                    glass.capAberration_back = glass.capAberration_front;
                    break;
                case GlassFace.back:
                    glass.aberration_front = glass.aberration_back;
                    glass.aberrationMagnitude_front = glass.aberrationMagnitude_back;
                    glass.texture_aberration_front = glass.texture_aberration_back;
                    glass.capAberration_front = glass.capAberration_back;
                    break;
            }
        }

        #endregion

        #region Settings - Fog

        void Section_Fog()
        {
            if (tools.ShowSection("Fog", ref glass.showSection_Fog))
            {
                tools.StartChangeCheck();
                tools.StartEdit(glass, "Changed Glass Fog");

                if (!Section_PresetAttribute(GlassAttributeType.fog, ref showAttribute_Fog))
                {

                    Section_Fog_Front();
                    Section_Fog_Back();
                    Section_Fog_Both();

                }

                tools.EndSection();

                if (tools.EndChangeCheck())
                {
                    glass.UpdateFog();
                }

                tools.FinishEdit(glass);
            }
        }

        void Section_Fog_Front()
        {
            if (glass.enableFog_both)
                return;

            if (tools.BoolOption("Front", ref glass.enableFog_front, true))
            {
                Section_Fog_Front_Options();
            }
        }

        void Section_Fog_Back()
        {
            if (glass.enableFog_both)
                return;

            if (tools.BoolOption("Back", ref glass.enableFog_back, true))
            {
                Section_Fog_Back_Options();
            }
        }

        void Section_Fog_Both()
        {
            tools.StartChangeCheck();

            if (tools.BoolOption("Both (matching)", ref glass.enableFog_both, true))
            {
                tools.StartSection();

                switch (glass.lastFaceEdited_Fog)
                {
                    case GlassFace.front:
                        Section_Fog_Front_Options();
                        break;
                    case GlassFace.back:
                        Section_Fog_Back_Options();
                        break;
                }

                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                SynchroniseFogFaces();
            }
        }

        void Section_Fog_Front_Options()
        {
            tools.StartChangeCheck();

            tools.StartSection();
            tools.ColourOption("Near", ref glass.fogNear_front);
            tools.ColourOption("Far", ref glass.fogFar_front);
            tools.FloatOption("Magnitude", ref glass.fogMagnitude_front);
            tools.EndSection();

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Fog = GlassFace.front;
            }
        }

        void Section_Fog_Back_Options()
        {
            tools.StartChangeCheck();

            tools.StartSection();
            tools.ColourOption("Near", ref glass.fogNear_back);
            tools.ColourOption("Far", ref glass.fogFar_back);
            tools.FloatOption("Magnitude", ref glass.fogMagnitude_back);
            tools.EndSection();

            if (tools.EndChangeCheck())
            {
                glass.lastFaceEdited_Fog = GlassFace.front;
            }
        }

        /// <summary>
        /// Copies values from the last edited face to its opposite. Called only when both faces' values are linked.
        /// </summary>
        void SynchroniseFogFaces()
        {
            switch (glass.lastFaceEdited_Fog)
            {
                case GlassFace.front:
                    glass.fogNear_back = glass.fogNear_front;
                    glass.fogFar_back = glass.fogFar_front;
                    glass.fogMagnitude_back = glass.fogMagnitude_front;
                    break;
                case GlassFace.back:
                    glass.fogNear_front = glass.fogNear_back;
                    glass.fogFar_front = glass.fogFar_back;
                    glass.fogMagnitude_front = glass.fogMagnitude_back;
                    break;
            }
        }

        #endregion

        #region Settings - Depth

        private void Section_Depth()
        {
            if (tools.ShowSection("Depth", ref glass.showSection_Depth))
            {
                tools.StartChangeCheck();
                tools.StartEdit(glass, "Changed Object Depth Quality");
                tools.StartEdit(glass, "Changed Manager Depth Quality");

                if (!Section_PresetAttribute(GlassAttributeType.depth, ref showAttribute_Depth))
                {
                    tools.FloatOption("Depth Multiplier", ref glass.depthMultiplier);
                    tools.FloatOption("Depth Offset", ref glass.depthOffset);
                    tools.BoolOption("Flip UVs (Depth & Grab)", ref glass.flipDepth);

                    tools.BoldLabel("Quality (This Object)");

                    tools.StartSection();
                    Section_Depth_Quality_Layer("Front", ref glass.depthQuality_front, GlassDepthLayer.front);
                    Section_Depth_Quality_Layer("Back", ref glass.depthQuality_back, GlassDepthLayer.back);
                    Section_Depth_Quality_Layer("Other", ref glass.depthQuality_other, GlassDepthLayer.other);
                    if (glass.depthQuality_other == DepthQuality_GlassObject.Complex)
                    {
                        tools.StartSection();
                        tools.GUI_List("Ignored (Other)", ref glass.ignoredDepth, ref glass.showList_IgnoredDepth_Depth, ref glass.scroll_IgnoredDepth);
                        tools.EndSection();
                    }
                    tools.EndSection();

                    tools.BoldLabel("Quality (Glass Manager)");

                    tools.StartSection();
                    Section_Depth_Quality_Layer("Front", ref glass.manager.depthQuality_Front, GlassDepthLayer.front);
                    Section_Depth_Quality_Layer("Back", ref glass.manager.depthQuality_Back, GlassDepthLayer.back);
                    Section_Depth_Quality_Layer("Other", ref glass.manager.depthQuality_Other, GlassDepthLayer.other);
                    tools.EndSection();
                }

                if (tools.EndChangeCheck())
                {
                    glass.UpdateDepth();
                    SavePreset(GlassSettingsCopyList.DepthList());
                    GlassManagerChanged();
                }

                tools.FinishEdit(glass);
                tools.FinishEdit(glass.manager);

                tools.EndSection();
            }
        }

        void Section_Depth_Quality_Layer(string layerName, ref DepthQuality_GlassManager layerQuality, GlassDepthLayer depthLayer)
        {
            tools.StartEdit(glass.manager, "Changed Depth Quality: " + layerName);
            tools.StartChangeCheck();
            layerQuality = (DepthQuality_GlassManager)tools.EnumOption(layerName, layerQuality);
            if (tools.EndChangeCheck())
            {
                switch (depthLayer)
                {
                    case GlassDepthLayer.front:
                        glass.manager.DepthQualityChanged_Front();
                        break;
                    case GlassDepthLayer.back:
                        glass.manager.DepthQualityChanged_Back();
                        break;
                    case GlassDepthLayer.other:
                        glass.manager.DepthQualityChanged_Other();
                        break;
                }
            }
            tools.FinishEdit(glass.manager);
        }

        void Section_Depth_Quality_Layer(string layerName, ref DepthQuality_GlassObject layerQuality, GlassDepthLayer depthLayer)
        {
            tools.StartEdit(glass, "Changed Depth Quality: " + layerName);
            tools.StartChangeCheck();
            layerQuality = (DepthQuality_GlassObject)tools.EnumOption(layerName, layerQuality);
            if (tools.EndChangeCheck())
            {
                switch (depthLayer)
                {
                    case GlassDepthLayer.front:
                        glass.DepthQualityChanged_Front();
                        glass.manager.DepthQualityChanged_Front();
                        break;
                    case GlassDepthLayer.back:
                        glass.DepthQualityChanged_Back();
                        glass.manager.DepthQualityChanged_Back();
                        break;
                    case GlassDepthLayer.other:
                        glass.DepthQualityChanged_Other();
                        glass.manager.DepthQualityChanged_Other();
                        break;
                }
            }
            tools.FinishEdit(glass);
        }

        #endregion

        #region settings - surface

        private void Section_Surface()
        {
            if (tools.ShowSection("Surface", ref glass.showSection_Surface))
            {
                tools.StartChangeCheck();
                tools.StartEdit(glass, "Changed Glass Surface");

                if (!Section_PresetAttribute(GlassAttributeType.surface, ref showAttribute_Surface))
                {

                    tools.BoldLabel("Front");
                    EditorGUILayout.Space();
                    tools.StartSection();
                    AmountTexture("Glossiness", ref glass.ShowSection_Gloss_Front, ref glass.glossiness_front, ref glass.texture_gloss_front, 0f, 1f);
                    AmountTexture("Metallic", ref glass.ShowSection_Metal_Front, ref glass.metallic_front, ref glass.texture_metal_front, 0f, 1f);
                    AmountTexture("Glow", ref glass.ShowSection_Glow_Front, ref glass.glow_front, ref glass.texture_glow_front, -100f, 100f);
                    tools.EndSection();

                    EditorGUILayout.Space();

                    tools.BoldLabel("Back");
                    EditorGUILayout.Space();
                    tools.StartSection();
                    AmountTexture("Glossiness", ref glass.ShowSection_Gloss_Back, ref glass.glossiness_back, ref glass.texture_gloss_back, 0f, 1f);
                    AmountTexture("Metallic", ref glass.ShowSection_Metal_Back, ref glass.metallic_back, ref glass.texture_metal_back, 0f, 1f);
                    AmountTexture("Glow", ref glass.ShowSection_Glow_Back, ref glass.glow_back, ref glass.texture_glow_back, -100f, 100f);
                    tools.EndSection();

                }

                if (tools.EndChangeCheck())
                {
                    glass.UpdateSurface();
                }

                tools.EndEdit(glass);

                tools.EndSection();
            }
        }

        void AmountTexture(string label, ref bool show, ref float amount, ref Texture texture, float min, float max)
        {
            if (tools.ShowSection(label, ref show))
            {
                tools.FloatOption("Amount", ref amount, min, max);
                tools.TextureOption("Texture", ref texture);
                tools.EndSection();
            }
        }

        #endregion

        #region Textures

        void Section_Textures()
        {
            if (tools.ShowSection("Textures", ref glass.showSection_Textures))
            {
                if (!Section_PresetAttribute(GlassAttributeType.textures, ref showAttribute_Textures))
                {
                    tools.StartEdit(glass, "Changing Textures");
                    Texture previousAlbedoTexture = glass.texture_albedo;
                    if (ShowTextures("Albedo", ref glass.showTextures_Albedo, ref glass.texture_albedo))
                    {
                        if (glass.texture_albedo != null)
                        {
                            if (previousAlbedoTexture == null)  //  Setting texture anew, make it visible
                            {
                                if (glass.colour_albedoTexture.a == 0f)
                                {
                                    glass.colour_albedoTexture.a = 1f;
                                }
                                if (glass.opacity == 0f)
                                {
                                    glass.opacity = 1.0f;
                                }
                            }
                        }
                        glass.UpdateTexturesAndColours();
                    }
                    if (ShowTextures("Distortion/Bump", ref glass.showTextures_Distortion, ref glass.texture_distortion))
                        glass.UpdateDistortion();
                    if (ShowTextures("Extinction", ref glass.showTextures_Extinction, ref glass.linkTextures_Extinction, ref glass.changedTexture_Extinction, ref glass.texture_extinction_front, ref glass.texture_extinction_back))
                        glass.UpdateExtinction();
                    if (ShowTextures("Aberration", ref glass.showTextures_Aberration, ref glass.linkTextures_Aberration, ref glass.changedTexture_Aberration, ref glass.texture_aberration_front, ref glass.texture_aberration_back))
                        glass.UpdateAberration();
                    if (ShowTextures("Glossiness", ref glass.showTextures_Glossiness, ref glass.linkTextures_Glossiness, ref glass.changedTexture_Glossiness, ref glass.texture_gloss_front, ref glass.texture_gloss_back))
                        glass.UpdateSurface();
                    if (ShowTextures("Metallic", ref glass.showTextures_Metallic, ref glass.linkTextures_Metallic, ref glass.changedTexture_Metallic, ref glass.texture_metal_front, ref glass.texture_metal_back))
                        glass.UpdateSurface();
                    if (ShowTextures("Glow", ref glass.showTextures_Glow, ref glass.linkTextures_Glow, ref glass.changedTexture_Glow, ref glass.texture_glow_front, ref glass.texture_glow_back))
                        glass.UpdateSurface();

                    tools.HorizontalLine();

                    Section_Textures_Dimensions();
                }
                tools.FinishEdit(glass);

                tools.EndSection();
            }

        }

        bool ShowTextures(string label, ref bool show, ref Texture texture1)
        {
            bool changeOccurred = false;
            if (tools.ShowSection(label, ref show))
            {
                tools.StartChangeCheck();
                tools.TextureOption("Front & Back", ref texture1);
                if (tools.EndChangeCheck())
                {
                    changeOccurred = true;
                }
                tools.EndSection();
            }
            return changeOccurred;
        }

        private bool ShowTextures(string label, ref bool show, ref bool matchingTextures, ref int lastChanged, ref Texture frontTexture, ref Texture backTexture)
        {
            bool changeOccurred = false;
            if (tools.ShowSection(label, ref show))
            {
                if (matchingTextures)
                {
                    if (lastChanged == 0)
                    {
                        tools.StartChangeCheck();
                        tools.TextureOption("Front & Back", ref frontTexture);
                        if (tools.EndChangeCheck())
                        {
                            backTexture = frontTexture;
                            changeOccurred = true;
                        }
                    }
                    else
                    {
                        tools.StartChangeCheck();
                        tools.TextureOption("Front & Back", ref backTexture);
                        if (tools.EndChangeCheck())
                        {
                            frontTexture = backTexture;
                            changeOccurred = true;
                        }
                    }
                }
                else
                {
                    tools.StartChangeCheck();
                    tools.TextureOption("Front", ref frontTexture);
                    if (tools.EndChangeCheck())
                    {
                        lastChanged = 0;
                        changeOccurred = true;
                    }

                    tools.StartChangeCheck();
                    tools.TextureOption("Back", ref backTexture);
                    if (tools.EndChangeCheck())
                    {
                        lastChanged = 1;
                        changeOccurred = true;
                    }
                }

                tools.BoolOption("Matching Textures", ref matchingTextures);

                tools.EndSection();
            }

            return changeOccurred;
        }

        void UpdateTextureDimensions()
        {
            glass.UpdateTextureDimensions();
        }

        void Section_Textures_Dimensions()
        {
            if (tools.ShowSection("Texture Dimensions", ref glass.showSection_Texture_Dimensions))
            {
                tools.Label("It is highly recommended that all textures have the same dimensions.", true);
                tools.Label("Use this section to find out the dimensions you are currently using.", true);

                if (glass.textureDimensions.Count == 0)
                {
                    tools.HorizontalLine();
                    tools.Label("No Textures Found (try updating)");
                }

                foreach (Vector2 textureDimension in glass.textureDimensions)
                {
                    tools.Label(textureDimension.ToString());
                    tools.StartSection();
                    foreach (string textureName in glass.allTextures.Keys)
                    {
                        Texture texture = glass.allTextures[textureName];
                        if (texture != null)
                        {
                            Vector2 textureSize = new Vector2(texture.width, texture.height);
                            if (textureSize.Equals(textureDimension))
                            {
                                tools.TextureOption(textureName, ref texture);
                            }
                        }
                    }
                    tools.EndSection();
                }

                if (tools.Button("Update Texture Dimensions"))
                {
                    glass.UpdateTextureDimensions_Full();
                }

                tools.EndSection();
            }
        }

        #endregion

        #region settings - materials

        void Section_Materials()
        {
            if (tools.ShowSection("Materials", ref glass.showSection_Materials))
            {
                tempMatBack = glass.material_back;
                tempMatFront = glass.material_front;

                tools.StartChangeCheck();
                tools.StartEdit(glass, "Changed Glass Materials");

                tools.MaterialOption("Backface", ref tempMatBack);

                if (tools.EndChangeCheck())
                {
                    //glass.material_back = tempMatBack;
                    glass.SetMaterials(tempMatBack, glass.material_front);
                    glass.Material_BackfaceChanged();
                    //materialChangeDetected = false;
                    UpdateOtherGlassList();
                }

                tools.StartChangeCheck();

                tools.MaterialOption("Frontface", ref tempMatFront);

                if (tools.EndChangeCheck())
                {
                    //glass.material_front = tempMatFront;
                    glass.SetMaterials(glass.material_back, tempMatFront);
                    glass.Material_FrontfaceChanged();
                    //materialChangeDetected = false;
                    UpdateOtherGlassList();
                }

                if (tools.Button("Create '" + glass.presetName + "' Materials"))
                {
                    if (ReplaceExistingMaterials(glass.presetName))
                    {
                        CreateNewMaterials(glass.presetName);

                        glass.UpdateRendererMaterials();

                        glass.Material_Changed();
                    }
                }

                tools.FinishEdit(glass);

                tools.EndSection();
            }
        }

        void DeselectAll()
        {
            Selection.objects = new UnityEngine.Object[0];
            Selection.activeGameObject = null;
        }

        /// <summary>
        /// Check for Material changes.
        /// This is seperate from creating new materials so as to avoid a conflict & potential crash.
        /// </summary>
        void CheckForMaterialChanges()
        {
            if (tempMatBack != glass.material_back)
            {
                //materialChangeDetected = true;
            }

            if (tempMatBack != glass.material_back)
            {
                //materialChangeDetected = true;
            }
        }

        void CreateNewMaterials(string name)
        {
            CreateMaterial(name, GlassFace.front);
            CreateMaterial(name, GlassFace.back);
        }

        void CreateMaterial(string name, GlassFace face)
        {
            string path = MaterialPath(name, face);

            Material material = null;

            if (File.Exists(path))
            {
                material = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
            }
            else
            {
                material = glass.GenerateMaterial(face);
                SaveMaterial(material, path);
            }

            switch (face)
            {
                case GlassFace.front:
                    if (material != glass.material_front)
                    {
                        if (glass.material_front != null)
                        {
                            material.CopyPropertiesFromMaterial(glass.material_front);
                            //  Workaround for Unity bug (821208)
                            material.shader = material.shader;
                        }
                    }
                    glass.SetMaterials(glass.material_back, material);
                    break;
                case GlassFace.back:
                    if (material != glass.material_back)
                    {
                        if (glass.material_back != null)
                        {
                            material.CopyPropertiesFromMaterial(glass.material_back);
                            //  Workaround for Unity bug (821208)
                            material.shader = material.shader;
                        }
                    }
                    glass.SetMaterials(material, glass.material_front);
                    break;
            }

            tools.SetDirty(material);
        }

        bool SaveMaterial(Material material, string path)
        {
            try
            {
                AssetDatabase.CreateAsset(material, path);
                return true;
            }
            catch (Exception e)
            {
                tools.Message("ERROR Creating Material", "Please check the directory exists for Glass Materials '" + path + "'.\n\n\nError: " + e.Message);
                return false;
            }
        }

        string MaterialPath(string name, GlassFace face)
        {
            switch (face)
            {
                case GlassFace.front:
                    return materialsPath + "Glass_" + name + "_front" + ".mat";
                case GlassFace.back:
                    return materialsPath + "Glass_" + name + "_back" + ".mat";
                default:
                    Debug.LogError("Unknown Glass Face in Flass Creator!");
                    return "";
            }
        }

        bool ReplaceExistingMaterials(string newMaterialName)
        {
            if (MaterialExists(newMaterialName, GlassFace.front))
            {
                return ReplaceMaterialsMessage();
            }

            if (MaterialExists(newMaterialName, GlassFace.back))
            {
                return ReplaceMaterialsMessage();
            }

            return true;
        }

        bool MaterialExists(string name, GlassFace face)
        {
            return File.Exists(MaterialPath(name, GlassFace.front));
        }

        bool ReplaceMaterialsMessage()
        {
            return tools.Message("Replace Existing Material?", "One or more materials already exist for this material.\n\nWould you like to replace them anyway?", "Yes", "Cancel");
        }

        #endregion

        #region Section Notes

        private void Section_ImportantNotes()
        {
            if (tools.ShowSection("IMPORTANT Notes", ref showSection_ImportantNotes))
            {
                tools.Label("NOTE 1: Changing options in materials will not be reflected here.", true);
                tools.Label("NOTE 2: Changes made during Play mode are not automatically saved.", true);
                tools.Label("     2.1: Workaround 1: Save to a Preset in Play mode then Load from that Preset in the Editor.", true);
                tools.Label("     2.2: Workaround 2: Copy and paste component values, using options cog on the Glass object.", true);
                //tools.Label("\t*feature in development for future release", true);
                tools.EndSection();
            }
        }

        #endregion

        #region Glass Manager Settings

        void Section_ManagerSettings()
        {
            tools.StartEdit(glass, "Changed Glass Manager Settings");

            tools.StartChangeCheck();

            if (tools.ShowSection("Manager Settings", ref glass.showSection_ManagerSettings))
            {
                if (tools.EndChangeCheck())
                {
                    LoadGlassManagerSettings();
                }

                if (managerSettings == null)
                {
                    managerSettings = new Glass_GlassManager_Settings();
                    managerSettings.glassPresetName = glass.presetName;
                    if (glass.manager != null)
                    {
                        glass.manager.LoadManager();
                    }
                }

                Section_ManagerSettings_DepthQuality();

                Section_ManagerSettings_DepthTextures();

                Section_ManagerSettings_DepthCamera();

                Section_ManagerSettings_DepthNormalTechniques();

                tools.EndSection();
            }

            tools.FinishEdit(glass);
        }

        void GlassManagerChanged()
        {
            if (managerSettings == null)
                LoadGlassManagerSettings();
            managerSettings.Edited();
            managerSettings.Save(xmlPath);
            if (glass.manager == null)
            {
                glass.FindGlassManager();
            }
            glass.manager.LoadManager();
        }

        void Section_ManagerSettings_DepthQuality()
        {
            if (tools.ShowSection("Depth Quality", ref glass.showSection_ManagerSettings_depthQuality))
            {
                tools.StartChangeCheck();

                tools.StartEdit(glass, "Changed Object Depth Quality");
                tools.StartEdit(glass.manager, "Changed Manager Depth Quality");

                tools.BoldLabel("Quality (This Object)");

                tools.StartSection();
                Section_Depth_Quality_Layer("Front", ref glass.depthQuality_front, GlassDepthLayer.front);
                Section_Depth_Quality_Layer("Back", ref glass.depthQuality_back, GlassDepthLayer.back);
                Section_Depth_Quality_Layer("Other", ref glass.depthQuality_other, GlassDepthLayer.other);
                if (glass.depthQuality_other == DepthQuality_GlassObject.Complex)
                {
                    tools.StartSection();
                    tools.GUI_List("Ignored (Other)", ref glass.ignoredDepth, ref glass.showList_IgnoredDepth_Manager, ref glass.scroll_IgnoredDepth);
                    tools.EndSection();
                }
                tools.EndSection();

                tools.BoldLabel("Quality (Glass Manager)");

                tools.StartSection();
                Section_Depth_Quality_Layer("Front", ref glass.manager.depthQuality_Front, GlassDepthLayer.front);
                Section_Depth_Quality_Layer("Back", ref glass.manager.depthQuality_Back, GlassDepthLayer.back);
                Section_Depth_Quality_Layer("Other", ref glass.manager.depthQuality_Other, GlassDepthLayer.other);
                tools.EndSection();

                if (tools.Button("Set Defaults"))
                {
                    glass.depthQuality_front = glass.depthQuality_back = glass.depthQuality_other = Glass.default_depthQuality;
                    glass.DepthQualityChanged_Front();
                    glass.DepthQualityChanged_Back();
                    glass.DepthQualityChanged_Other();

                    glass.manager.depthQuality_Front = glass.manager.depthQuality_Back = glass.manager.depthQuality_Other = GlassManager.default_depthQuality;
                    glass.manager.DepthQualityChanged_Front();
                    glass.manager.DepthQualityChanged_Back();
                    glass.manager.DepthQualityChanged_Other();
                }

                if (tools.EndChangeCheck())
                {
                    glass.UpdateDepth();
                    SavePreset(GlassSettingsCopyList.DepthList());
                    GlassManagerChanged();
                }

                tools.FinishEdit(glass);
                tools.FinishEdit(glass.manager);

                tools.EndSection();
            }
        }

        void Section_ManagerSettings_DepthTextures()
        {
            if (tools.ShowSection("Depth Textures", ref glass.showSection_ManagerSettings_depthTextures))
            {
                tools.StartChangeCheck();

                tools.Popup("Anti-Aliasing*", textureAALabels, textureAAItems, ref textureAAIndex, ref managerSettings.depthTextureAA);
                tools.IntOption("Aniso Level*", ref managerSettings.depthTextureAniso, 0, 16);
                managerSettings.depthTextureFilterMode = (FilterMode)tools.EnumOption("Filter Mode*", managerSettings.depthTextureFilterMode);

                tools.Label("*Does not change during Play.");
                tools.Label("NB: Changes made during Play are not saved.");

                if (tools.Button("Set Defaults"))
                {
                    managerSettings.depthTextureAA = GlassManager.default_depthTextureAA;
                    UpdateTextureAAIndex();
                    managerSettings.depthTextureAniso = GlassManager.default_depthTextureAniso;
                    managerSettings.depthTextureFilterMode = GlassManager.default_depthTextureFilterMode;
                }

                if (tools.EndChangeCheck())
                {
                    GlassManagerChanged();
                }

                tools.EndSection();
            }
        }

        void Section_ManagerSettings_DepthCamera()
        {
            if (tools.ShowSection("Depth Camera", ref glass.showSection_ManagerSettings_depthCamera))
            {
                tools.StartChangeCheck();

                managerSettings.depthTextureClearMode = (CameraClearFlags)tools.EnumOption("Clear Mode", managerSettings.depthTextureClearMode);

                if (tools.Button("Set Defaults"))
                {
                    managerSettings.depthTextureClearMode = GlassManager.default_depthTextureClearMode;
                }

                if (tools.EndChangeCheck())
                {
                    GlassManagerChanged();
                }

                tools.EndSection();
            }
        }

        void Section_ManagerSettings_DepthNormalTechniques()
        {
            if (tools.ShowSection("Depth & Normal Techniques", ref glass.showSection_Manager_depthNormalTech))
            {
                Section_ManagerSettings_DepthNormalTechniques_Glass();

                Section_ManagerSettings_DepthNormalTechniques_Manager();

                tools.EndSection();
            }
        }

        void Section_ManagerSettings_DepthNormalTechniques_Glass()
        {
            tools.BoldLabel("Preference (" + glass.presetName + ")");

            tools.StartChangeCheck();

            tools.StartChangeCheck();
            managerSettings.depthTechnique = (GlassDepthTechnique)tools.EnumOption("Depth", managerSettings.depthTechnique);
            if (tools.EndChangeCheck())
            {
                glass.manager.managerSettingsType = GlassManagerSettingsType.LoadFromGlass;
                managerSettings.UpdateDepthTechnique();
            }

            tools.StartChangeCheck();
            managerSettings.normalTechnique = (GlassNormalTechnique)tools.EnumOption("Normal", managerSettings.normalTechnique);
            if (tools.EndChangeCheck())
            {
                glass.manager.managerSettingsType = GlassManagerSettingsType.LoadFromGlass;
                managerSettings.UpdateNormalTechnique();
            }

            tools.StartChangeCheck();
            managerSettings.frontDepthTechnique = (GlassFrontDepthTechnique)tools.EnumOption("Front Depth", managerSettings.frontDepthTechnique);
            if (tools.EndChangeCheck())
            {
                glass.manager.managerSettingsType = GlassManagerSettingsType.LoadFromGlass;
                managerSettings.UpdateFrontDepthTechnique();
            }

#if UNITY_5_4_OR_NEWER
                    tools.StartChangeCheck();
                    tools.BoolOption("Unity 5.4 Fix (old cards)", ref managerSettings.enable54Workaround);
                    if (tools.EndChangeCheck())
                    {
                        glass.manager.managerSettingsType = GlassManagerSettingsType.LoadFromGlass;
                        managerSettings.UpdateFrontDepthTechnique();
                    }
#endif

            if (tools.EndChangeCheck())
            {
                GlassManagerChanged();
            }
        }

        void Section_ManagerSettings_DepthNormalTechniques_Manager()
        {
            tools.BoldLabel("Scene Settings (GlassManager)");

            tools.StartEdit(GlassManager.Instance, "Changed Depth Tech (Manager)");

            tools.StartChangeCheck();
            GlassManager.Instance.depthTechnique = (GlassDepthTechnique)tools.EnumOption("Depth", GlassManager.Instance.depthTechnique);
            if (tools.EndChangeCheck())
            {
                glass.manager.managerSettingsType = GlassManagerSettingsType.LoadFromManager;
                GlassManager.Instance.UpdateDepthTechnique();
            }

            tools.StartChangeCheck();
            GlassManager.Instance.normalTechnique = (GlassNormalTechnique)tools.EnumOption("Normal", GlassManager.Instance.normalTechnique);
            if (tools.EndChangeCheck())
            {
                glass.manager.managerSettingsType = GlassManagerSettingsType.LoadFromManager;
                GlassManager.Instance.UpdateNormalTechnique();
            }

            tools.StartChangeCheck();
            GlassManager.Instance.frontDepthTechnique = (GlassFrontDepthTechnique)tools.EnumOption("Front Depth", GlassManager.Instance.frontDepthTechnique);
            if (tools.EndChangeCheck())
            {
                glass.manager.managerSettingsType = GlassManagerSettingsType.LoadFromManager;
                GlassManager.Instance.UpdateFrontDepthTechnique();
            }

#if UNITY_5_4_OR_NEWER
                    tools.StartChangeCheck();
                    tools.BoolOption("Unity 5.4 Fix (old cards)", ref GlassManager.Instance.enable54Workaround);
                    if (tools.EndChangeCheck())
                    {
                        glass.manager.managerSettingsType = GlassManagerSettingsType.LoadFromManager;
                        GlassManager.Instance.UpdateFrontDepthTechnique();
                    }
#endif

            tools.FinishEdit(GlassManager.Instance);
        }

        void UpdateTextureAAIndex()
        {
            if (managerSettings == null)
                return;
            for (int i = 0; i < textureAAItems.Length; i++)
            {
                if (textureAAItems[i] == managerSettings.depthTextureAA)
                {
                    textureAAIndex = i;
                    break;
                }
            }
        }

        #endregion

        #region Extinction

        Color GetExtinctionColour(GlassFace face)
        {
            return GetExtinctionColour(face, glass.extinctionAppearance);
        }

        Color GetExtinctionColour_Flipped(GlassFace face)
        {
            return GetExtinctionColour_Flipped(face, glass.extinctionAppearance);
        }

        Color GetExtinctionColour(GlassFace face, GlassExtinctionAppearance appearance)
        {
            switch (appearance)
            {
                case GlassExtinctionAppearance.AsApplied:
                    {
                        switch (face)
                        {
                            case GlassFace.front:
                                return glass.extinction_front;
                            case GlassFace.back:
                                return glass.extinction_back;
                        }
                        break;
                    }
                case GlassExtinctionAppearance.AsItAppears:
                    {
                        switch (face)
                        {
                            case GlassFace.front:
                                return FlippedColour(glass.extinctionFlipped_front);
                            case GlassFace.back:
                                return FlippedColour(glass.extinctionFlipped_back);
                        }
                        break;
                    }
            }
            return Color.clear;
        }

        Color GetExtinctionColour_Flipped(GlassFace face, GlassExtinctionAppearance appearance)
        {
            switch (appearance)
            {
                case GlassExtinctionAppearance.AsApplied:
                    {
                        switch (face)
                        {
                            case GlassFace.front:
                                return glass.extinctionFlipped_front;
                            case GlassFace.back:
                                return glass.extinctionFlipped_back;
                        }
                        break;
                    }
                case GlassExtinctionAppearance.AsItAppears:
                    {
                        switch (face)
                        {
                            case GlassFace.front:
                                return FlippedColour(glass.extinction_front);
                            case GlassFace.back:
                                return FlippedColour(glass.extinction_back);
                        }
                        break;
                    }
            }
            return Color.clear;
        }

        Color FlippedColour(Color colour, bool flipAlpha = false)
        {
            return new Color(1f - colour.r,
                1f - colour.g,
                1f - colour.b,
                flipAlpha ? 1f - colour.a : colour.a);
        }

        #endregion

        #region Helper Functions

        private bool GlassNamesDiffer()
        {
            string glassName = glass.presetName;
            foreach (Glass glassIter in glassList)
                if (glassIter.presetName != glassName)
                    return true;
            return false;
        }

        #endregion

        #region Preset Option Loaders

        bool ShowSection_PresetAttribute(string sectionLabel, ref bool sectionBool)
        {
            if (tools.Button(sectionBool ? "Hide " + sectionLabel + " Presets" : sectionLabel + " Presets"))
            {
                sectionBool = !sectionBool;
            }
            return sectionBool;
        }

        string AttributeName(GlassAttributeType type)
        {
            switch (type)
            {
                case GlassAttributeType.albedo:
                    return "Albedo";
                case GlassAttributeType.distortion:
                    return "Distortion";
                case GlassAttributeType.bump:
                    return "Bump";
                case GlassAttributeType.extinction:
                    return "Extinction";
                case GlassAttributeType.aberration:
                    return "Aberration";
                case GlassAttributeType.fog:
                    return "Fog";
                case GlassAttributeType.surface:
                    return "Surface";
                case GlassAttributeType.physics:
                    return "Physics";
                case GlassAttributeType.depth:
                    return "Depth";
                case GlassAttributeType.textures:
                    return "Textures";
                default:
                    return "Attribute";
            }
        }

        bool Section_PresetAttribute(GlassAttributeType attributetype, ref bool showAttribute)
        {
            tools.StartChangeCheck();

            if (!ShowSection_PresetAttribute(AttributeName(attributetype), ref showAttribute))
            {
                tools.EndChangeCheck();
                //return Application.isPlaying ? false : false;	//	show the original section instead
                return false;
            }

            Section_PresetAttribute_Options(attributetype, ref showAttribute, tools.EndChangeCheck());

            return true;

            /*
			if(Application.isPlaying)//	PLAYER VERSION:
			{
				Section_PresetAttribute_Options(attributetype, ref showAttribute);
			}
			else//	EDITOR VERSION:
			{
			//	Create an instance of the Preset Attribute Loader window
			//	pass it the Glass object being edited
			//	Albedo is automatically selected from the first drop-down 'Attributes'
			//	Below that, show a list of presets to select from.
			//	show values in rows with two columns headed 'value', 'load'
			//	at the bottom have a button that says 'Load Selected Values'
			//	when the button is pressed, load the values into that glass object and close the window
			//	add a key press check for Escape so the user can quickly cancel this window
			}

			return Application.isPlaying ? true : false;	//	do not show the original section
			*/
        }

        void Section_PresetAttribute_Options(GlassAttributeType attributeType, ref bool showAttribute, bool forceLoadPreset)
        {
            tools.StartEdit(glass, "Changed Glass using Preset Attributes");

            //	preset drop-down
            tools.StartChangeCheck();
            tools.PresetList_Basic("Preset", presetListPath, ref selectedPresetName, ref selectedPresetIndex, ref presetList);
            if (tools.EndChangeCheck() || forceLoadPreset)
            {
                LoadAttributePreset(selectedPresetName);
                loadAllAttributeValues = false;
            }

            if (attributePreset == null)
            {
                if (presetList == null)
                {
                    LoadPresetList();
                }
                selectedPresetIndex = 0;
                loadAllAttributeValues = false;
                return;
            }


            if (tools.Button("Load All Values"))
            {
                loadAllAttributeValues = true;
            }

            //	show labels 'value' - 'selected'
            EditorGUILayout.BeginHorizontal();
            tools.BoldLabel("Preset Value");
            tools.BoldLabel("Loaded / Matching");
            EditorGUILayout.EndHorizontal();

            //	show column of values and 'selected' bools
            switch (attributeType)
            {
                case GlassAttributeType.albedo:
                    tools.StartChangeCheck();
                    ValueSelector("Albedo", "Colour", attributePreset.colour_albedo, ref glass.colour_albedo);
                    ValueSelector("Albedo", "Texture", Glass.GetTexture(attributePreset.texturePath_albedo), ref glass.texture_albedo);
                    ValueSelector("Albedo", "Texture Colour", attributePreset.colour_albedoTexture, ref glass.colour_albedoTexture);
                    if (tools.EndChangeCheck())
                    {
                        glass.UpdateTexturesAndColours();
                    }
                    break;
                case GlassAttributeType.distortion:
                    tools.StartChangeCheck();
                    ValueSelector("Distortion", "Texture", Glass.GetTexture(attributePreset.texturePath_distortion), ref glass.texture_distortion);
                    ValueSelector_bool("Distortion", "Enable Front", attributePreset.enableDistortion_front, ref glass.enableDistortion_front);
                    if (attributePreset.enableDistortion_front)
                    {
                        ValueSelector("Distortion Front", "Bump", attributePreset.distortion_front_bump, ref glass.distortion_front.x);
                        ValueSelector_bool("Distortion Front", "Detailed Depth", attributePreset.enableDoubleDepth_front, ref glass.enableDoubleDepth_front);
                        ValueSelector("Distortion Front", "Mesh", attributePreset.distortion_front_mesh, ref glass.distortion_front.y);
                        ValueSelector("Distortion Front", "Multiplier", attributePreset.distortion_front_multiplier, ref glass.distortion_front.z);
                        ValueSelector("Distortion Front", "Max", attributePreset.distortion_front_max, ref glass.distortion_front.w);
                        ValueSelector("Distortion Front", "Edge Bend", attributePreset.distortion_edge_bend_front, ref glass.distortionEdgeBend_front);
                        ValueSelector("Distortion Front", "Depth Fade", attributePreset.distortion_depth_fade_front, ref glass.distortionDepthFade_front);
                    }
                    ValueSelector_bool("Distortion", "Enable Back", attributePreset.enableDistortion_back, ref glass.enableDistortion_back);
                    if (attributePreset.enableDistortion_back)
                    {
                        ValueSelector("Distortion Back", "Bump", attributePreset.distortion_back_bump, ref glass.distortion_back.x);
                        ValueSelector_bool("Distortion Back", "Detailed Depth", attributePreset.enableDoubleDepth_back, ref glass.enableDoubleDepth_back);
                        ValueSelector("Distortion Back", "Mesh", attributePreset.distortion_back_mesh, ref glass.distortion_back.y);
                        ValueSelector("Distortion Back", "Multiplier", attributePreset.distortion_back_multiplier, ref glass.distortion_back.z);
                        ValueSelector("Distortion Back", "Max", attributePreset.distortion_back_max, ref glass.distortion_back.w);
                        ValueSelector("Distortion Back", "Edge Bend", attributePreset.distortion_edge_bend_back, ref glass.distortionEdgeBend_back);
                        ValueSelector("Distortion Back", "Depth Fade", attributePreset.distortion_depth_fade_back, ref glass.distortionDepthFade_back);
                    }
                    if (tools.EndChangeCheck())
                    {
                        glass.UpdateDistortion();
                    }
                    break;
                case GlassAttributeType.bump:
                    tools.StartChangeCheck();
                    ValueSelector("Bump", "Texture", Glass.GetTexture(attributePreset.texturePath_distortion), ref glass.texture_distortion);
                    ValueSelector("Bump", "Front", attributePreset.bump_front, ref glass.bumpFront);
                    ValueSelector("Bump", "Back", attributePreset.bump_back, ref glass.bumpFront);
                    if (tools.EndChangeCheck())
                    {
                        glass.UpdateBump();
                    }
                    break;
                case GlassAttributeType.extinction:
                    tools.StartChangeCheck();
                    ValueSelector_bool("Extinction", "Enable Both", attributePreset.enableExtinction_both, ref glass.enableExtinction_both);

                    ValueSelector_bool("Extinction", "Enable Front", attributePreset.enableExtinction_front, ref glass.enableExtinction_front); if (attributePreset.enableExtinction_front || attributePreset.enableExtinction_both)
                    {
                        tools.StartSection();
                        ValueSelector("Extinction Front", "Texture", Glass.GetTexture(attributePreset.texturePath_extinction_front), ref glass.texture_extinction_front);
                        ValueSelector("Extinction Front", "Colour (As Appears)", attributePreset.colour_extinction_flipped_front, ref glass.extinctionFlipped_front);
                        ValueSelector("Extinction Front", "Colour (As Applied)", attributePreset.colour_extinction_front, ref glass.extinction_front);
                        ValueSelector("Extinction Front", "Intensity", attributePreset.extinctionIntensity_front, ref glass.extinctionMagnitude_front.x);
                        ValueSelector("Extinction Front", "Minimum", attributePreset.extinctionMin_front, ref glass.extinctionMagnitude_front.y);
                        ValueSelector("Extinction Front", "Maximum", attributePreset.extinctionMax_front, ref glass.extinctionMagnitude_front.z);
                        ValueSelector_bool("Extinction Front", "Cap(min,max)", attributePreset.capExtinction_front, ref glass.capExtinction_front);
                        tools.EndSection();
                    }
                    //
                    ValueSelector_bool("Extinction", "Enable Back", attributePreset.enableExtinction_back, ref glass.enableExtinction_back);
                    if (attributePreset.enableExtinction_back || attributePreset.enableExtinction_both)
                    {
                        tools.StartSection();
                        ValueSelector("Extinction Back", "Texture", Glass.GetTexture(attributePreset.texturePath_extinction_back), ref glass.texture_extinction_back);
                        ValueSelector("Extinction Back", "Colour (As Appears)", attributePreset.colour_extinction_flipped_back, ref glass.extinctionFlipped_back);
                        ValueSelector("Extinction Back", "Colour (As Applied)", attributePreset.colour_extinction_back, ref glass.extinction_back);
                        ValueSelector("Extinction Back", "Intensity", attributePreset.extinctionIntensity_back, ref glass.extinctionMagnitude_back.x);
                        ValueSelector("Extinction Back", "Minimum", attributePreset.extinctionMin_back, ref glass.extinctionMagnitude_back.y);
                        ValueSelector("Extinction Back", "Maximum", attributePreset.extinctionMax_back, ref glass.extinctionMagnitude_back.z);
                        ValueSelector_bool("Extinction Back", "Cap(min,max)", attributePreset.capExtinction_back, ref glass.capExtinction_back);
                        tools.EndSection();
                    }
                    if (tools.EndChangeCheck())
                    {
                        glass.UpdateExtinction();
                    }
                    break;
                case GlassAttributeType.aberration:
                    tools.StartChangeCheck();
                    ValueSelector_bool("Aberration", "Enable Both", attributePreset.enableAberration_both, ref glass.enableAberration_both);

                    ValueSelector_bool("Aberration", "Enable Front", attributePreset.enableAberration_front, ref glass.enableAberration_front);
                    if (attributePreset.enableAberration_front || attributePreset.enableAberration_both)
                    {
                        tools.StartSection();
                        ValueSelector("Aberration Front", "Texture", Glass.GetTexture(attributePreset.texturePath_aberration_front), ref glass.texture_aberration_front);
                        ValueSelector("Aberration Front", "Colour", attributePreset.colour_aberration_front, ref glass.aberration_front);
                        ValueSelector("Aberration Front", "Intensity", attributePreset.aberrationIntensity_front, ref glass.aberrationMagnitude_front.x);
                        ValueSelector("Aberration Front", "Minimum", attributePreset.aberrationMin_front, ref glass.aberrationMagnitude_front.y);
                        ValueSelector("Aberration Front", "Maximum", attributePreset.aberrationMax_front, ref glass.aberrationMagnitude_front.z);
                        ValueSelector_bool("Aberration Front", "Cap(min,max)", attributePreset.capAberration_front, ref glass.capAberration_front);
                        tools.EndSection();
                    }
                    //
                    ValueSelector_bool("Aberration", "Enable Back", attributePreset.enableAberration_back, ref glass.enableAberration_back);
                    if (attributePreset.enableAberration_back || attributePreset.enableAberration_both)
                    {
                        tools.StartSection();
                        ValueSelector("Aberration Back", "Texture", Glass.GetTexture(attributePreset.texturePath_aberration_back), ref glass.texture_aberration_back);
                        ValueSelector("Aberration Front", "Colour", attributePreset.colour_aberration_back, ref glass.aberration_back);
                        ValueSelector("Aberration Back", "Intensity", attributePreset.aberrationIntensity_back, ref glass.aberrationMagnitude_back.x);
                        ValueSelector("Aberration Back", "Minimum", attributePreset.aberrationMin_back, ref glass.aberrationMagnitude_back.y);
                        ValueSelector("Aberration Back", "Maximum", attributePreset.aberrationMax_back, ref glass.aberrationMagnitude_back.z);
                        ValueSelector_bool("Aberration Back", "Cap(min,max)", attributePreset.capAberration_back, ref glass.capAberration_back);
                        tools.EndSection();
                    }
                    if (tools.EndChangeCheck())
                    {
                        glass.UpdateAberration();
                    }
                    break;
                case GlassAttributeType.fog:
                    tools.StartChangeCheck();
                    ValueSelector_bool("Fog", "Enable Both", attributePreset.option_fog_both, ref glass.enableFog_both);
                    ValueSelector_bool("Fog", "Enable Front", attributePreset.option_fog_front, ref glass.enableFog_front);
                    if (attributePreset.option_fog_front || attributePreset.option_fog_both)
                    {
                        tools.StartSection();
                        ValueSelector("Fog Front", "Near", attributePreset.colour_fog_near_front, ref glass.fogNear_front);
                        ValueSelector("Fog Front", "Far", attributePreset.colour_fog_far_front, ref glass.fogFar_front);
                        ValueSelector("Fog Front", "Magnitude", attributePreset.fog_magnitude_front, ref glass.fogMagnitude_front);
                        tools.EndSection();
                    }
                    //
                    ValueSelector_bool("Fog", "Enable Back", attributePreset.option_fog_back, ref glass.enableFog_back);
                    if (attributePreset.option_fog_back || attributePreset.option_fog_both)
                    {
                        tools.StartSection();
                        ValueSelector("Fog Back", "Near", attributePreset.colour_fog_near_back, ref glass.fogNear_back);
                        ValueSelector("Fog Back", "Far", attributePreset.colour_fog_far_back, ref glass.fogFar_back);
                        ValueSelector("Fog Back", "Magnitude", attributePreset.fog_magnitude_back, ref glass.fogMagnitude_back);
                        tools.EndSection();
                    }
                    if (tools.EndChangeCheck())
                    {
                        glass.UpdateFog();
                    }
                    break;
                case GlassAttributeType.surface:
                    tools.StartChangeCheck();
                    tools.BoldLabel("Front");
                    tools.StartSection();
                    ValueSelector("Surface Front", "Gloss Texture", Glass.GetTexture(attributePreset.texturePath_gloss_front), ref glass.texture_gloss_front);
                    ValueSelector("Surface Front", "Gloss Value", attributePreset.glossiness_front, ref glass.glossiness_front);
                    ValueSelector("Surface Front", "Metal Texture", Glass.GetTexture(attributePreset.texturePath_metal_front), ref glass.texture_metal_front);
                    ValueSelector("Surface Front", "Metal Value", attributePreset.metallic_front, ref glass.metallic_front);
                    ValueSelector("Surface Front", "Glow Texture", Glass.GetTexture(attributePreset.texturePath_glow_front), ref glass.texture_glow_front);
                    ValueSelector("Surface Front", "Glow Value", attributePreset.glow_front, ref glass.glow_front);
                    tools.EndSection();
                    tools.BoldLabel("Back");
                    tools.StartSection();
                    ValueSelector("Surface Back", "Gloss Texture", Glass.GetTexture(attributePreset.texturePath_gloss_back), ref glass.texture_gloss_back);
                    ValueSelector("Surface Back", "Gloss Value", attributePreset.glossiness_back, ref glass.glossiness_back);
                    ValueSelector("Surface Back", "Metal Texture", Glass.GetTexture(attributePreset.texturePath_metal_back), ref glass.texture_metal_back);
                    ValueSelector("Surface Back", "Metal Value", attributePreset.metallic_back, ref glass.metallic_back);
                    ValueSelector("Surface Back", "Glow Texture", Glass.GetTexture(attributePreset.texturePath_glow_back), ref glass.texture_glow_back);
                    ValueSelector("Surface Back", "Glow Value", attributePreset.glow_back, ref glass.glow_back);
                    tools.EndSection();
                    if (tools.EndChangeCheck())
                    {
                        glass.UpdateSurface();
                    }
                    break;
                case GlassAttributeType.physics:
                    tools.Label("No Values Available");
                    break;
                case GlassAttributeType.depth:
                    tools.Label("No Values Available");
                    break;
                case GlassAttributeType.textures:
                    tools.Label("No Values Available");
                    break;
                default:
                    tools.Label("No Values Available");
                    break;
            }

            if (loadAllAttributeValues)
            {
                //showAttribute = false;
                loadAllAttributeValues = false;
            }

            /*
			//	'load selected' button
			if(tools.Button("Load Selected Values"))
			{
				//	-copy values

				//	-return to not showing section
				showAttribute = false;
			}
			*/
            tools.EndEdit(glass);
        }

        bool ValueSelector(string attributeName, string valueName, float[] value, ref Color destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (ValuesMatch(value, destination))
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.ColourOption(valueName, new Color(value[0], value[1], value[2], value[3]));
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValuesMatch(float[] v1, Color v2)
        {
            if (v1[0] != v2.r)
                return false;
            if (v1[1] != v2.g)
                return false;
            if (v1[2] != v2.b)
                return false;
            if (v1[3] != v2.a)
                return false;
            return true;
        }

        bool ValueSelector(string attributeName, string valueName, Color value, ref Color destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.ColourOption(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValueSelector(string attributeName, string valueName, int value, ref int destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.IntOption(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValueSelector(string attributeName, string valueName, float value, ref float destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.FloatOption(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValueSelector(string attributeName, string valueName, Vector2 value, ref Vector2 destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.VectorOption(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValueSelector(string attributeName, string valueName, Vector3 value, ref Vector3 destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.VectorOption(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValueSelector(string attributeName, string valueName, Vector4 value, ref Vector4 destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
                attributeValueNames[attributeName + valueName] = true;
            tools.VectorOption(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValueSelector(string attributeName, string valueName, Texture value, ref Texture destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.TextureOption(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValueSelector(string attributeName, string valueName, GameObject value, ref GameObject destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.GameObjectOption(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValueSelector(string attributeName, string valueName, Glass value, ref Glass destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.GlassOption(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValueSelector(string attributeName, string valueName, Enum value, ref Enum destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.EnumOption(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValueSelector_bool(string attributeName, string valueName, bool value, ref bool destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.BoolOption(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        bool ValueSelector_string(string attributeName, string valueName, string value, ref string destination, string selectorLabel = "")
        {
            BeginValueSelector(attributeName, valueName);
            if (value == destination)
            {
                attributeValueNames[attributeName + valueName] = true;
            }
            tools.Label(valueName, value);
            if (EndValueSelector(attributeName, valueName, selectorLabel) || loadAllAttributeValues)
            {
                CopyValue(value, ref destination);
                return true;
            }
            return false;
        }

        void BeginValueSelector(string attributeName, string valueName)
        {
            if (!attributeValueNames.ContainsKey(attributeName + valueName))
            {
                attributeValueNames.Add(attributeName + valueName, true);
            }

            EditorGUILayout.BeginHorizontal();
        }

        bool EndValueSelector(string attributeName, string valueName, string selectorLabel = "")
        {
            bool buttonPressed = false;
            if (attributeValueNames[attributeName + valueName])
            {
                tools.Label("✓");
            }
            else
            {
                buttonPressed = tools.Button("Load Value");
            }
            //attributeValueNames[attributeName+valueName] = tools.BoolOption(selectorLabel, attributeValueNames[attributeName+valueName]);
            EditorGUILayout.EndHorizontal();
            //return attributeValueNames[attributeName+valueName];
            return buttonPressed;
        }

        void CopyValue(int value, ref int destination)
        {
            destination = value;
        }

        void CopyValue(float value, ref float destination)
        {
            destination = value;
        }

        void CopyValue(string value, ref string destination)
        {
            destination = value;
        }

        void CopyValue(bool value, ref bool destination)
        {
            destination = value;
        }

        void CopyValue(GameObject value, ref GameObject destination)
        {
            destination = value;
        }

        void CopyValue(Glass value, ref Glass destination)
        {
            destination = value;
        }

        void CopyValue(float[] value, ref Color destination)
        {
            destination = new Color(value[0], value[1], value[2], value[3]);
        }

        void CopyValue(Color value, ref Color destination)
        {
            destination = value;
        }

        void CopyValue(Enum value, ref Enum destination)
        {
            destination = value;
        }

        void CopyValue(Vector2 value, ref Vector2 destination)
        {
            destination = value;
        }

        void CopyValue(Vector3 value, ref Vector3 destination)
        {
            destination = value;
        }

        void CopyValue(Vector4 value, ref Vector4 destination)
        {
            destination = value;
        }

        void CopyValue(Texture value, ref Texture destination)
        {
            destination = value;
        }

        /*
		bool ValueSelector(string attributeName, string valueName, string selectorLabel = "")
		{
			if(!attributeValueNames.ContainsKey(attributeName+valueName))
			{
				attributeValueNames.Add(valueName, true);
			}
			
			EditorGUILayout.BeginHorizontal();
			tools.Label(valueName);
			tools.BoolOption(selectorLabel, ref attributeValueNames[attributeName+valueName]);
			EditorGUILayout.EndHorizontal();
		}
		*/

        bool ValueSelected(string valueName)
        {
            if (!attributeValueNames.ContainsKey(valueName))
            {
                attributeValueNames.Add(valueName, true);
            }
            return attributeValueNames[valueName];
        }

        #endregion
    }

}

#endif
