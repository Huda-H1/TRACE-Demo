#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml.Serialization;

namespace FantasticGlass
{
    /// <summary>
    /// Glass Maker Editor Window.
    /// </summary>
    public class Editor_CreateGlass : EditorWindow
    {
        #region Member Variables

        EditorTools tools = null;

        bool showGUISettings = false;

        //	Paths
        string packagePath = "";
        string xmlPath = "";
        string presetListPath = "";
        string materialsPath = "";
        //	Settings
        GlassSystemSettings settings = null;
        string settingsPath = "";
        //	Shader
        Shader glassShader = null;
        string glassShaderName = "Custom/Glass";
        //
        bool showBasicSettings = false;
        //	Presets
        GlassPreset currentPreset = null;
        List<string> presetList = new List<string>();
        bool showPresets = false;
        string currentPresetName = "";
        int currentPresetIndex = -1;
        //	Mesh
        Mesh mesh = null;
        string meshPath = "";
        string meshName = "";
        bool showSection_MeshScaling = false;
        bool showSection_DefaultMeshScale = false;
        bool showSection_DefaultMeshScale_CreateNew = false;
        bool showSection_AboutMeshScaling = false;
        bool showSection_AboutMeshScaling_Default = false;
        bool showSection_AboutMeshScaling_Default_CreateNew = false;
        //	Mesh Scaling
        //  N.B. Presumes FBX models, as those are required for Asset Store submission:
        GlassMeshScaleFix meshScaleFix = GlassMeshScaleFix.fbx;
        float meshScale = GlassMeshScaleFixLookup.scale_fbx;
        GlassPrimitiveType meshPrimitive = GlassPrimitiveType.none;
        //	Create
        bool showSection_CreateNew = true;
        bool showSection_CreateExisting = false;
        Editor gameObjectEditor = null;
        // Existing
        bool showList_SelectedRenderers = false;
        Vector2 scroll_selectedRenderers = new Vector2();
        GameObject existingObject = null;
        List<Renderer> selectedRenderers = new List<Renderer>();
        //  TODO:
        //bool copyExistingObject = true;
        //	Main Scroll
        Vector2 scroll_main = new Vector2();
        //	Preview
        MeshFilter previewMeshFilter;
        GameObject previewObjectContainer = null;
        GameObject previewObject = null;
        Material previewMaterial_Front = null;
        Material previewMaterial_Back = null;
        MeshRenderer previewRenderer = null;
        //	Materials
        bool showSection_ExistingMaterials = false;
        bool assignCustomMaterials = false;
        Material frontMat = null;
        Material backMat = null;
        string frontMatPath = "";
        string backMatPath = "";
        //	Physics
        bool showSection_Physics = false;
        GlassPhysicalObjectType physicalObjectType = GlassPhysicalObjectType.box;
        bool enablePhysicalObject = false;
        //	Extinction
        bool showSection_Extinction = false;
        GlassFace lastFaceEdited_Extinction = GlassFace.front;
        GlassExtinctionAppearance extinctionAppearance = GlassExtinctionAppearance.AsItAppears;
        float min_float_extinctionIntensity = -100f;
        float max_float_extinctionIntensity = 100f;
        bool enableExtinctionFront = true;
        bool enableExtinctionBack = false;
        bool enableExtinctionBoth = false;
        bool enableCapExtinctionFront = true;
        bool enableCapExtinctionBack = true;
        Texture texture_extinction_front;
        Texture texture_extinction_back;
        Color colour_extinction_front = new Color(0.1f, 0.033f, 0.00666f, 1.0f);
        Color colour_extinction_back = new Color(0.1f, 0.033f, 0.00666f, 1.0f);
        Color colour_extinction_flipped_front = new Color(0.9f, 0.977f, 0.99444f, 1.0f);
        Color colour_extinction_flipped_back = new Color(0.9f, 0.977f, 0.99444f, 1.0f);
        Vector3 vector_extinctionIntensity_front = new Vector3(1f, 0f, 1f);
        Vector3 vector_extinctionIntensity_back = new Vector3(1f, 0f, 1f);
        //	Aberration
        bool showSection_Aberration = false;
        GlassFace lastFaceEdited_Aberration = GlassFace.front;
        bool enable_Aberration_front = true;
        bool enable_Aberration_back = false;
        bool enable_Aberration_both = false;
        Texture texture_aberration_front;
        Texture texture_aberration_back;
        Color colour_aberration_front = new Color(0.1f, 0.033f, 0.00666f, 1.0f);
        Color colour_aberration_back = new Color(0.1f, 0.033f, 0.00666f, 1.0f);
        Vector3 vector_aberrationIntensity_front = new Vector3(1f, 0f, 1f);
        Vector3 vector_aberrationIntensity_back = new Vector3(1f, 0f, 1f);
        float float_aberrationMin = -10f;
        float float_aberrationMax = 10f;
        bool bool_capAberration_front = true;
        bool bool_capAberration_back = true;
        //	Fog
        bool showSection_Fog = false;
        bool enableFog_front = false;
        bool enableFog_back = false;
        bool enableFog_both = false;
        GlassFace lastFaceEdited_Fog = GlassFace.front;
        Color colour_fog_near_front = new Color(0f, 1f, 0f, 0f);
        Color colour_fog_near_back = new Color(0f, 1f, 0f, 0f);
        Color colour_fog_far_front = new Color(0f, 0f, 1f, 0.5f);
        Color colour_fog_far_back = new Color(0f, 0f, 1f, 0.5f);
        float float_fogMagnitude_front = 1f;
        float float_fogMagnitude_back = 1f;
        //	Distortion
        bool showSection_Distortion = false;
        bool enableDistortion_front = true;
        bool enableDistortion_back = true;
        Vector4 vector_distortionMagnitude_Front = new Vector4(0.0f, -0.01f, 1f, 1f);
        Vector4 vector_distortionMagnitude_Back = new Vector4(0.0f, 0.1f, 1f, 1f);
        Texture texture_distortion = null;
        string texture_distortionPath = "";
        bool ShowSection_Bump = false;
        float float_bumpBack = 0f;
        float float_bumpFront = 0f;
        float float_distortionEdgeBend_front = 0.3f;
        float float_distortionEdgeBend_back = 0.3f;
        float float_distortionDepthFade_front = 0.1f;
        float float_distortionDepthFade_back = 0.1f;
        bool enableDoubleDepth_front = true;
        bool enableDoubleDepth_back = true;
        //	Help
        bool showHelp = false;
        bool ShowHelp_Editor = false;
        bool showHelp_Editor_Rendering = false;
        bool ShowHelp_Errors = false;
        //	Albedo
        bool enableAlbedo = false;
        Color colour_albedo = new Color(1, 1, 1, 0);
        float float_opacity = 0f;
        Texture texture_albedo = null;
        string texture_albedoPath = "";
        Color colour_albedoTexture = new Color(1, 1, 1, 0);
        //	Surface
        private bool ShowSection_Gloss_Front = false;
        private bool ShowSection_Metal_Front = false;
        private bool ShowSection_Glow_Front = false;
        private bool ShowSection_Gloss_Back = false;
        private bool ShowSection_Metal_Back = false;
        private bool ShowSection_Glow_Back = false;
        bool showSection_Surface;
        float float_glossiness_front = 0.5f;
        float float_glossiness_back = 0f;
        private Texture texture_gloss_front;
        private Texture texture_metal_front;
        private Texture texture_glow_front;
        private Texture texture_gloss_back;
        private Texture texture_metal_back;
        private Texture texture_glow_back;
        //	Metallic
        float float_metallic_front = 0f;
        float float_metallic_back = 0f;
        //	Glow
        float float_glow_front = 0f;
        float float_glow_back = 0f;
        //	Advanced
        bool showSection_Advanced = false;
        bool showAdvancedObject = false;
        bool showTurnObjectMoreOptions = false;
        bool enableCustomTurnObjectPosition = false;
        bool enableCustomTurnObjectRotation = false;
        Vector3 objectPosition = new Vector3(0f, 0.501f, 0f);
        Vector3 objectRotation = new Vector3();
        bool showExistingMaterialsSettings = false;
        bool showSection_Camera = false;
        //	Z-fighting
        bool showSection_ZFightingFix;
        bool showSection_ZFightingFix_About;
        float zFightRadius = 0.01f;
        //	Settings
        bool showDebug = false;
        bool showDefaultResources = false;
        bool showSynchroniseSettings = false;
        bool anyChangesHaveBeenMade = false;
        bool showPreviewSettings = false;
        Vector3 previewRotationOffset = new Vector3(0f, -85f, 0f);

        string[] errors = { "GlassManager_Editor.cs(##,##): error: The best overloaded method match for `EditorTools.ShowList(...)' has some invalid arguments",
        "NullReferenceException: (null) UnityEditor.SerializedObject..ctor(UnityEngine.Object[] objs)(at..."
    };
        string[] errors_Solutions = {
        "Add 'FD_GLASS' to  Scripting Define Symbols in Player settings (Edit/Project Settings/Player).",
        "This is apparently a bug within the Unity Editor. It appears once for each time the Glass Creator window is opened. Restarting Unity will stop them appearing."
    };
        List<bool> errors_sections = null;

        string[] qa_rendering = { "Why does the glass look weird in the Editor / Preview?",
        "The depth / aberration / colour extinction seems to be rotated / flipped / offset."
    };
        string[] qa_rendering_answers = {
        "The glass shader requires resources that are only updated during Play, such as depth textures from cameras.",
        "Fix1: Change the main camera Rendering Path or change the Glass material's 'Flip Depth UV' option.\nFix2: Make sure you are using a viewport resolution / aspect that matches your screen."
    };
        List<bool> qa_rendering_sections = null;

        #endregion


        #region Init

        [MenuItem("GameObject/Create Other/Glass...", false, 0)]
        static void Init()
        {
            if (Application.isPlaying)
            {
                Debug.Log("Not loading Glass Maker during Play mode.");
                return;
            }
            //
            Editor_CreateGlass window = ScriptableObject.CreateInstance(typeof(Editor_CreateGlass)) as Editor_CreateGlass;
            window.ShowUtility();
        }

        #endregion


        #region Editor Window Callbacks (SelectionChange / Enable / Disable / Close / Destroy)

        public void OnSelectionChange()
        {
            if (showDebug)
                Debug.Log("Selection Changed");

            lock (selectedRenderers)
            {
                selectedRenderers.Clear();
                foreach (GameObject obj in Selection.gameObjects)
                {
                    foreach (Renderer selectedRenderer in obj.GetComponentsInChildren<Renderer>())
                    {
                        if (selectedRenderer != null)
                            selectedRenderers.Add(selectedRenderer);
                    }
                }
            }

            UpdateFirstExistingObject();
        }

        void OnEnable()
        {
#if UNITY_5_2 || UNITY_5_3_OR_NEWER
            titleContent.text = "Fantastic Glass (" + Glass.versionStringFormatted + ") Glass Maker";
#else
            title = "Fantastic Glass (" + Glass.versionStringFormatted + ") Glass Maker";
#endif
            if (tools == null)
                tools = new EditorTools("Glass Maker");

            InitPaths();

            anyChangesHaveBeenMade = false;

            FindGlassShader(true);

            LoadSettings();

            if (settings.guiSkin == null)
            {
                if (settings.guiSkinPath == "")
                {
                    settings.guiSkinPath = packagePath + "GUI/" + "Resources/" + "Glass_Window.guiskin";
                }
                settings.guiSkinPath = FileUtil.GetProjectRelativePath(settings.guiSkinPath);
                try
                {
                    settings.guiSkin = AssetDatabase.LoadAssetAtPath(settings.guiSkinPath, typeof(GUISkin)) as GUISkin;
                }
                catch (Exception e)
                {
                    Debug.Log("Failed to load gui skin at path: '" + settings.guiSkinPath + "'. Error message: " + e.Message);
                }
            }

            LoadGlassPresets();

            SetupPreviewAssets();

            LoadLastUsedPreset();

            UpdatePreviewMaterials();

            if (Selection.gameObjects.Length > 0)
            {
                OnSelectionChange();
            }
            else
            {
                UpdateNewPreviewObject();
            }
        }

        void InitPaths()
        {
            if (!packagePath.Contains(Application.dataPath))
            {
                packagePath = Application.dataPath + "/" + GlassManager.default_packageName + "/";
                xmlPath = packagePath + GlassManager.default_xml_Pathname + "/";
                presetListPath = xmlPath + GlassManager.default_presetList_Filename + ".xml";
                settingsPath = xmlPath + GlassManager.default_settings_Filename + ".xml";
                materialsPath = packagePath + GlassManager.default_materials_Pathname + "/";
                materialsPath = FileUtil.GetProjectRelativePath(materialsPath);
            }
        }

        void OnDisable()
        {
            if (settings != null)
            {
                if (settings.enableDebugLogging)
                {
                    Debug.Log("Glass Maker : OnDisable");
                }
            }
            //
            SaveSettings();
            //
            DestroySomething(previewMeshFilter);
            DestroySomething(previewMaterial_Back, true);
            DestroySomething(previewMaterial_Front, true);
            DestroySomething(gameObjectEditor);
            DestroySomething(previewObject);
            DestroySomething(previewObjectContainer);
        }

        void OnClose()
        {
            OnDisable();
            if (settings.enableDebugLogging)
            {
                Debug.Log("Glass Maker : OnClose");
            }
            if (anyChangesHaveBeenMade)
            {
                if (tools.Message("Closing Glass Creator", "Save changes made to the current preset?", "Yes", "No"))
                {
                    if (tools.SavePreset(currentPreset, currentPresetName, currentPresetIndex, ref presetList, presetListPath))
                    {
                        currentPreset.name = currentPresetName;
                        currentPreset.Save(xmlPath + currentPresetName + ".xml");
                        SaveList(presetList, presetListPath);//tools.SaveComponent (presetList, presetListPath);
                    }
                }
            }
        }

        void OnDestroy()
        {
            if (settings.enableDebugLogging)
            {
                Debug.Log("Glass Maker : OnDestroy");
            }
        }

        #endregion


        #region GUI

        void OnGUI()
        {
            if (Application.isPlaying)
            {
                if (tools.enableDebugLogging)
                    Debug.Log("Glass Creator : Closing window as App is running.");
                Close();
            }
            //
            if (tools == null)
                tools = new EditorTools("Glass Maker");
            //
            if (settings == null)
                LoadSettings();
            //
            //currentGUISkin = GUI.skin;
            //if(EditorGUIUtility.isProSkin)
            //{
            if (settings.useCustomGUI)
            {
                GUI.skin = settings.guiSkin;
            }
            //}
            //
            tools.StartChangeCheck();
            //
            FindGlassShader();
            //
            scroll_main = EditorGUILayout.BeginScrollView(scroll_main);
            //
            Section_Presets();
            //
            Section_MainOptions();
            //
            Section_Physics();
            //
            Section_Create_New();
            //
            Section_Create_Existing();
            //
            Section_Advanced();
            //
            Section_Debug();
            //
            Section_Help();
            //
            if (tools.EndChangeCheck())
            {
                anyChangesHaveBeenMade = true;
            }
            //
            EditorGUILayout.EndScrollView();
            //
            GUI.skin = null;
        }

        #endregion


        #region Section Presets

        void Section_Presets()
        {
            if (presetList == null)
                return;
            //
            switch (tools.PresetList("Glass Presets", presetListPath, ref currentPresetName, ref currentPresetIndex, ref presetList, ref showPresets, false, true, true))
            {
                case EditorToolsPreset_Option.SaveItem:
                    {
                        int presetCount = presetList.Count;
                        currentPreset = GeneratePreset();
                        if (tools.SavePreset(currentPreset, currentPresetName, currentPresetIndex, ref presetList, presetListPath))
                        {
                            currentPreset.name = currentPresetName;
                            currentPreset.Save(xmlPath + currentPresetName + ".xml");
                        }
                        SaveList(presetList, presetListPath);//tools.SaveComponent (presetList, presetListPath);
                        LoadGlassPresets();
                        if (presetCount != presetList.Count)
                        {
                            LoadLastAddedPreset();
                        }
                        break;
                    }
                case EditorToolsPreset_Option.LoadItem:
                    {
                        LoadGlassPreset(currentPresetName, currentPresetIndex, presetListPath);
                        break;
                    }
                case EditorToolsPreset_Option.DeleteItem:
                    {
                        tools.DeletePreset(currentPresetName, presetListPath, currentPresetIndex, ref presetList);
                        File.Delete(xmlPath + currentPresetName + ".xml");
                        SaveList(presetList, presetListPath);
                        LoadLastAddedPreset();
                        break;
                    }
                case EditorToolsPreset_Option.ItemChanged:
                    {
                        LoadGlassPreset(currentPresetName, currentPresetIndex, presetListPath);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion


        #region Section Main Options

        void Section_MainOptions()
        {
            tools.StartChangeCheck();
            //
            if (tools.ShowSection("Main Options", ref showBasicSettings))
            {
                Section_TexturesAndColours();
                Section_Distortion();
                Section_BumpHeight();
                Section_Extinction();
                Section_Aberration();
                Section_Fog();
                Section_Surface();
                Section_ExistingMaterials();
                tools.EndSection(true);
            }
            //
            if (tools.EndChangeCheck())
            {
                UpdatePreviewMaterials();
                SetupPreviewAssets();
            }
        }

        #endregion


        #region Section Texturs and Colours

        void Section_TexturesAndColours()
        {
            if (tools.ShowSection("Albedo", ref enableAlbedo))
            {
                tools.ColourOption("Base Colour", ref colour_albedo);
                tools.TextureOption("Texture", ref texture_albedo);
                if (texture_albedo != null)
                {
                    tools.ColourOption("Texture Colour", ref colour_albedoTexture);
                }
                tools.FloatOption("Opacity", ref float_opacity);
                tools.EndSection();
            }
        }

        #endregion

        #region Section Extinction

        void Section_Extinction()
        {
            if (tools.ShowSection("Extinction", ref showSection_Extinction))
            {
                extinctionAppearance = (GlassExtinctionAppearance)tools.EnumOption("Extinction Appearance", extinctionAppearance);
                Section_Extinction_Front();
                Section_Extinction_Back();
                Section_Extinction_Both();
                tools.EndSection();
            }
        }

        void Section_Extinction_Front()
        {
            if (enableExtinctionBoth)
                return;

            tools.StartChangeCheck();

            if (tools.BoolOption("Front (Default)", ref enableExtinctionFront))
            {

                tools.StartSection();
                Section_Extinction_Front_Options();
                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                lastFaceEdited_Extinction = GlassFace.front;
            }
        }

        void Section_Extinction_Back()
        {
            if (enableExtinctionBoth)
                return;

            tools.StartChangeCheck();

            if (tools.BoolOption("Back", ref enableExtinctionBack))
            {
                tools.StartSection();
                Section_Extinction_Back_Options();
                tools.EndSection();
            }

            if (tools.EndChangeCheck())
            {
                lastFaceEdited_Extinction = GlassFace.back;
            }
        }

        void Section_Extinction_Front_Options()
        {
            switch (extinctionAppearance)
            {
                case GlassExtinctionAppearance.AsItAppears:
                    tools.ColourOption("Colour (as it appears)", ref colour_extinction_flipped_front);
                    colour_extinction_front = GetExtinctionColour(GlassFace.front);
                    break;
                case GlassExtinctionAppearance.AsApplied:
                    tools.ColourOption("Colour (extinction)", ref colour_extinction_front);
                    colour_extinction_flipped_front = GetExtinctionColour_Flipped(GlassFace.front);
                    break;
            }

            tools.FloatOption("Intensity", ref vector_extinctionIntensity_front.x, min_float_extinctionIntensity, max_float_extinctionIntensity);
            tools.FloatOption("Minimum", ref vector_extinctionIntensity_front.y, min_float_extinctionIntensity, max_float_extinctionIntensity);
            tools.FloatOption("Maximum", ref vector_extinctionIntensity_front.z, min_float_extinctionIntensity, max_float_extinctionIntensity);

            tools.TextureOption("Texture", ref texture_extinction_front);
        }

        void Section_Extinction_Back_Options()
        {
            switch (extinctionAppearance)
            {
                case GlassExtinctionAppearance.AsItAppears:
                    tools.ColourOption("Colour (as it appears)", ref colour_extinction_flipped_back);
                    colour_extinction_back = GetExtinctionColour(GlassFace.back);
                    break;
                case GlassExtinctionAppearance.AsApplied:
                    tools.ColourOption("Colour (extinction)", ref colour_extinction_back);
                    colour_extinction_flipped_back = GetExtinctionColour_Flipped(GlassFace.back);
                    break;
            }

            tools.FloatOption("Intensity", ref vector_extinctionIntensity_back.x, min_float_extinctionIntensity, max_float_extinctionIntensity);
            tools.FloatOption("Minimum", ref vector_extinctionIntensity_back.y, min_float_extinctionIntensity, max_float_extinctionIntensity);
            tools.FloatOption("Maximum", ref vector_extinctionIntensity_back.z, min_float_extinctionIntensity, max_float_extinctionIntensity);

            tools.TextureOption("Texture", ref texture_extinction_back);
        }

        void Section_Extinction_Both()
        {
            tools.StartChangeCheck();

            if (tools.BoolOption("Both (matching)", ref enableExtinctionBoth))
            {

                tools.StartSection();

                switch (lastFaceEdited_Extinction)
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
                Sync_Faces_Extinction();
            }
        }

        void Sync_Faces_Extinction()
        {
            switch (lastFaceEdited_Extinction)
            {
                case GlassFace.front:
                    colour_extinction_back = colour_extinction_front;
                    vector_extinctionIntensity_back = vector_extinctionIntensity_front;
                    colour_extinction_flipped_back = colour_extinction_flipped_front;
                    texture_extinction_back = texture_extinction_front;
                    break;
                case GlassFace.back:
                    colour_extinction_front = colour_extinction_back;
                    vector_extinctionIntensity_front = vector_extinctionIntensity_back;
                    colour_extinction_flipped_front = colour_extinction_flipped_back;
                    texture_extinction_front = texture_extinction_back;
                    break;
            }
        }

        #endregion

        #region Section Aberration

        void Section_Aberration()
        {
            if (tools.ShowSection("Aberration", ref showSection_Aberration))
            {
                Section_Aberration_Front();
                Section_Aberration_Back();
                Section_Aberration_Both();
                tools.EndSection();
            }
        }

        void Section_Aberration_Front()
        {
            if (enable_Aberration_both)
                return;

            if (tools.BoolOption("Front (Default)", ref enable_Aberration_front))
            {
                tools.StartSection();
                Section_Aberration_Front_Options();
                tools.EndSection();
            }
        }

        void Section_Aberration_Back()
        {
            if (enable_Aberration_both)
                return;

            if (tools.BoolOption("Back", ref enable_Aberration_back))
            {
                tools.StartSection();
                Section_Aberration_Front_Options();
                tools.EndSection();
            }
        }

        void Section_Aberration_Both()
        {
            tools.StartChangeCheck();

            if (tools.BoolOption("Both (matching)", ref enable_Aberration_both))
            {

                tools.StartSection();

                switch (lastFaceEdited_Aberration)
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
                Sync_Faces_Aberration();
            }
        }

        void Section_Aberration_Front_Options()
        {
            tools.ColourOption("Colour (spread)", ref colour_aberration_front);
            tools.TextureOption("Texture", ref texture_aberration_front);
            tools.FloatOption("Intensity", ref vector_aberrationIntensity_front.x, float_aberrationMin, float_aberrationMax);
            tools.FloatOption("Minimum", ref vector_aberrationIntensity_front.y, float_aberrationMin, float_aberrationMax);
            tools.FloatOption("Maximum", ref vector_aberrationIntensity_front.z, float_aberrationMin, float_aberrationMax);
            tools.BoolOption("Cap (min,max)", ref bool_capAberration_front);
        }

        void Section_Aberration_Back_Options()
        {
            tools.ColourOption("Colour (spread)", ref colour_aberration_back);
            tools.TextureOption("Texture", ref texture_aberration_back);
            tools.FloatOption("Intensity", ref vector_aberrationIntensity_back.x, float_aberrationMin, float_aberrationMax);
            tools.FloatOption("Minimum", ref vector_aberrationIntensity_back.y, float_aberrationMin, float_aberrationMax);
            tools.FloatOption("Maximum", ref vector_aberrationIntensity_back.z, float_aberrationMin, float_aberrationMax);
            tools.BoolOption("Cap (min,max)", ref bool_capAberration_front);
        }

        /// <summary>
        /// Copies values from the last edited face to its opposite. Called only when both faces' values are linked.
        /// </summary>
        void Sync_Faces_Aberration()
        {
            switch (lastFaceEdited_Aberration)
            {
                case GlassFace.front:
                    colour_aberration_back = colour_aberration_front;
                    vector_aberrationIntensity_back = vector_aberrationIntensity_front;
                    texture_aberration_back = texture_aberration_front;
                    bool_capAberration_back = bool_capAberration_front;
                    break;
                case GlassFace.back:
                    colour_aberration_front = colour_aberration_back;
                    vector_aberrationIntensity_front = vector_aberrationIntensity_back;
                    texture_aberration_front = texture_aberration_back;
                    bool_capAberration_front = bool_capAberration_back;
                    break;
            }
        }

        #endregion

        #region Section Distortion

        void Section_Distortion()
        {
            if (tools.ShowSection("Distortion", ref showSection_Distortion))
            {
                tools.TextureOption("Texture", ref texture_distortion);

                if (tools.BoolOption("Front", ref enableDistortion_front))
                {
                    tools.StartSection();
                    if (texture_distortion != null)
                    {
                        tools.FloatOption("Bump", ref vector_distortionMagnitude_Front.x);
                        /*
                        if (vector_distortionMagnitude_Front.x != 0f)
                        {
                            tools.BoolOption("Bump Depth", ref enableDoubleDepth_front);
                        }
                        */
                    }
                    tools.BoolOption("Detailed Depth", ref enableDoubleDepth_front);
                    tools.FloatOption("Mesh", ref vector_distortionMagnitude_Front.y);
                    tools.FloatOption("Overall", ref vector_distortionMagnitude_Front.z);
                    tools.FloatOption("Max", ref vector_distortionMagnitude_Front.w);
                    tools.FloatOption("Edge Bend", ref float_distortionEdgeBend_front);
                    tools.FloatOption("Depth Fade", ref float_distortionDepthFade_front);
                    tools.EndSection();
                }

                if (tools.BoolOption("Back", ref enableDistortion_back))
                {
                    tools.StartSection();
                    if (texture_distortion != null)
                    {
                        tools.FloatOption("Bump", ref vector_distortionMagnitude_Back.x);
                        /*
                        if (vector_distortionMagnitude_Back.x != 0f)
                        {
                            tools.BoolOption("Bump Depth", ref enableDoubleDepth_back);
                        }
                        */
                    }
                    tools.BoolOption("Detailed Depth", ref enableDoubleDepth_back);
                    tools.FloatOption("Mesh", ref vector_distortionMagnitude_Back.y);
                    tools.FloatOption("Overall", ref vector_distortionMagnitude_Back.z);
                    tools.FloatOption("Max", ref vector_distortionMagnitude_Back.w);
                    tools.FloatOption("Edge Bend", ref float_distortionEdgeBend_back);
                    tools.FloatOption("Depth Fade", ref float_distortionDepthFade_back);
                    tools.EndSection();
                }

                tools.EndSection();
            }
        }

        #endregion

        #region Section Bump

        void Section_BumpHeight()
        {
            if (tools.ShowSection("Bump", ref ShowSection_Bump))
            {
                tools.TextureOption("Texture", ref texture_distortion);
                if (texture_distortion != null)
                {
                    tools.FloatOption("Front", ref float_bumpFront);
                    tools.FloatOption("Back", ref float_bumpBack);
                }
                tools.EndSection();
            }
        }

        #endregion

        #region Section Fog

        void Section_Fog()
        {
            if (tools.ShowSection("Fog", ref showSection_Fog))
            {
                Section_Fog_Front();
                Section_Fog_Back();
                Section_Fog_Both();
                tools.EndSection();
            }
        }

        void Section_Fog_Front()
        {
            if (enableFog_both)
                return;

            if (tools.BoolOption("Front", ref enableFog_front))
            {
                Section_Fog_Front_Options();
            }
        }

        void Section_Fog_Back()
        {
            if (enableFog_both)
                return;

            if (tools.BoolOption("Back", ref enableFog_back))
            {
                Section_Fog_Back_Options();
            }
        }

        void Section_Fog_Both()
        {
            tools.StartChangeCheck();

            if (tools.BoolOption("Both (matching)", ref enableFog_both))
            {

                tools.StartSection();

                switch (lastFaceEdited_Fog)
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
                Sync_Faces_Fog();
            }
        }

        void Section_Fog_Front_Options()
        {
            tools.StartSection();
            tools.ColourOption("Near", ref colour_fog_near_front);
            tools.ColourOption("Far", ref colour_fog_far_front);
            tools.FloatOption("Magnitude", ref float_fogMagnitude_front);
            tools.EndSection();
        }

        void Section_Fog_Back_Options()
        {
            tools.StartSection();
            tools.ColourOption("Near", ref colour_fog_near_back);
            tools.ColourOption("Far", ref colour_fog_far_back);
            tools.FloatOption("Magnitude", ref float_fogMagnitude_back);
            tools.EndSection();
        }

        void Sync_Faces_Fog()
        {
            switch (lastFaceEdited_Fog)
            {
                case GlassFace.front:
                    colour_fog_near_back = colour_fog_near_front;
                    colour_fog_far_back = colour_fog_far_front;
                    float_fogMagnitude_back = float_fogMagnitude_front;
                    break;
                case GlassFace.back:
                    colour_fog_near_front = colour_fog_near_back;
                    colour_fog_far_front = colour_fog_far_back;
                    float_fogMagnitude_front = float_fogMagnitude_back;
                    break;
            }
        }

        #endregion

        #region Section Surface

        void Section_Surface()
        {
            if (tools.ShowSection("Surface", ref showSection_Surface))
            {
                tools.Label("Front");
                EditorGUILayout.Space();
                tools.StartSection();
                AmountTexture("Glossiness", ref ShowSection_Gloss_Front, ref float_glossiness_front, ref texture_gloss_front, 0f, 1f);
                AmountTexture("Metallic", ref ShowSection_Metal_Front, ref float_metallic_front, ref texture_metal_front, 0f, 1f);
                AmountTexture("Glow", ref ShowSection_Glow_Front, ref float_glow_front, ref texture_glow_front, -100f, 100f);
                tools.EndSection();

                EditorGUILayout.Space();

                tools.Label("Back");
                EditorGUILayout.Space();
                tools.StartSection();
                AmountTexture("Glossiness", ref ShowSection_Gloss_Back, ref float_glossiness_back, ref texture_gloss_back, 0f, 1f);
                AmountTexture("Metallic", ref ShowSection_Metal_Back, ref float_metallic_back, ref texture_metal_back, 0f, 1f);
                AmountTexture("Glow", ref ShowSection_Glow_Back, ref float_glow_back, ref texture_glow_back, -100f, 100f);
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

        #region Section Existing Materials

        void Section_ExistingMaterials()
        {
            if (tools.ShowSection("Existing Materials", ref showSection_ExistingMaterials))
            {
                if (tools.BoolOption("Use Existing Materials", ref assignCustomMaterials))
                {
                    tools.StartSection();
                    tools.MaterialOption("Front-facing Material", ref frontMat);
                    tools.MaterialOption("Back-facing Material", ref backMat);
                    tools.Label("Note: When this option is disabled, existing matching materials can still be found and applied automatically.");
                    tools.EndSection();
                }
                tools.EndSection();
            }
        }

        #endregion

        #region Section Physics

        void Section_Physics()
        {
            if (tools.ShowSection("Physics(" + PhysicsTypeName() + ")", ref showSection_Physics))
            {
                //
                if (tools.BoolOption("Physical Object", ref enablePhysicalObject))
                {
                    tools.StartSection();
                    physicalObjectType = (GlassPhysicalObjectType)tools.EnumOption("", physicalObjectType);
                    tools.EndSection();
                }
                //
                tools.EndSection();
            }
        }

        #endregion


        #region Section Create New

        void Section_Create_New()
        {
            string newObjectLabel = (mesh != null) ? "Create New GameObject (" + mesh.name + ")" : "Create New GameObject (no mesh)";
            //
            tools.StartChangeCheck();
            //
            if (tools.ShowSection(newObjectLabel, ref showSection_CreateNew))
            {
                if (tools.EndChangeCheck())
                {
                    UpdateNewPreviewObject();
                }
                if (gameObjectEditor != null)
                {
                    tools.Label("Glass Preview");
                    tools.Label("(not all changes will be visible)");
                    gameObjectEditor.OnPreviewGUI(GUILayoutUtility.GetRect(200, 200), EditorStyles.foldout);
                }
                //
                tools.StartChangeCheck();
                tools.MeshOption("Mesh", ref mesh);
                if (tools.EndChangeCheck())
                {
                    UpdateNewPreviewObject();
                }
                //
                Section_MeshScale();
                //
                Section_MeshScale_Default(ref showSection_DefaultMeshScale_CreateNew, ref showSection_AboutMeshScaling_Default_CreateNew);
                //
                if (tools.Button("Create Glass"))
                {
                    CreateGlass();
                }
                tools.EndSection(true);
            }
            else {
                tools.EndChangeCheck();
            }
        }

        void Section_MeshScale()
        {
            if (tools.ShowSection("Mesh Scaling", ref showSection_MeshScaling))
            {
                //  Scale Fix
                tools.StartChangeCheck();
                meshScaleFix = (GlassMeshScaleFix)tools.EnumOption("Scale Fix", meshScaleFix);
                if (tools.EndChangeCheck())
                {
                    GlassMeshScaleFixLookup.GetScale(meshScaleFix, ref meshScale);
                }
                //  Scale
                tools.StartChangeCheck();
                tools.FloatOption("Scale", ref meshScale);
                if (tools.EndChangeCheck())
                {
                    GlassMeshScaleFixLookup.GetEnum(ref meshScaleFix, meshScale);
                }
                //  Save Defaults
                if (tools.Button("Save As Default"))
                {
                    settings.defaultMeshScale = meshScale;
                    settings.defaultMeshScaleFix = meshScaleFix;
                }
                //  about
                if (tools.ShowSection("About Mesh Scaling", ref showSection_AboutMeshScaling))
                {
                    tools.Label("Some meshes are exported based on a unit measurement that differs to Unity's.\nIt is therefore necessary to re-scale some imported meshes.\nThe scale you set here is different for each Glass preset but a default can also be set for convenience.", true);
                }
                //
                tools.EndSection();
            }
        }

        void Section_MeshScale_Default(ref bool showDefaultMeshScale_Bool, ref bool showDefaultMeshScale_About_Bool)
        {
            if (tools.ShowSection("Default Mesh Scale", ref showDefaultMeshScale_Bool))
            {
                //  Scale Fix
                tools.StartChangeCheck();
                settings.defaultMeshScaleFix = (GlassMeshScaleFix)tools.EnumOption("Scale Fix", settings.defaultMeshScaleFix);
                if (tools.EndChangeCheck())
                {
                    GlassMeshScaleFixLookup.GetScale(settings.defaultMeshScaleFix, ref settings.defaultMeshScale);
                }
                //  Scale
                tools.StartChangeCheck();
                tools.FloatOption("Scale", ref settings.defaultMeshScale);
                if (tools.EndChangeCheck())
                {
                    GlassMeshScaleFixLookup.GetEnum(ref settings.defaultMeshScaleFix, settings.defaultMeshScale);
                }
                //  about
                if (tools.ShowSection("About Mesh Scaling", ref showDefaultMeshScale_About_Bool))
                {
                    tools.Label("Some meshes are exported based on a unit measurement that differs to Unity's.\nIt is therefore necessary to re-scale some imported meshes.\nThe scale you set here is the default value for new Presets.", true);
                }
                tools.EndSection();
            }
        }

        #endregion

        #region Section Create Existing

        void Section_Create_Existing()
        {
            string existingObjectsLabel = (selectedRenderers.Count == 1) ? "Use Existing GameObject (1)" : "Use Existing GameObject (" + selectedRenderers.Count + ")";
            if (tools.ShowSection(existingObjectsLabel, ref showSection_CreateExisting))
            {
                EditorGUI.BeginChangeCheck();
                //tools.GameObjectOption("Game Object", ref existingObject);
                tools.GUI_List("Selected Renderers (change here / in editor)", ref selectedRenderers, ref showList_SelectedRenderers, ref scroll_selectedRenderers);
                //
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateFirstExistingObject();
                }
                if (existingObject != null)
                {
                    tools.Label("Glass Preview");
                    tools.Label("(not all changes will be visible)");
                    gameObjectEditor.OnPreviewGUI(GUILayoutUtility.GetRect(200, 200), EditorStyles.foldout);
                }
                //
                if (tools.Button("Turn to Glass"))
                {
                    TurnObjectIntoGlass();
                }
                if (tools.ShowSection("Advanced Options", ref showTurnObjectMoreOptions))
                {
                    //  TODO
                    //tools.BoolOption("Create Glass as Copy", ref copyExistingObject);

                    if (tools.BoolOption("Set New Position", ref enableCustomTurnObjectPosition))
                    {
                        tools.VectorOption("Object Position", ref objectPosition);
                        if (tools.Button("Get from GameObject"))
                            objectPosition = existingObject.transform.position;
                    }
                    if (tools.BoolOption("Set New Rotation", ref enableCustomTurnObjectRotation))
                    {
                        tools.VectorOption("Object Rotation", ref objectRotation);
                        if (tools.Button("Get from GameObject"))
                            objectRotation = existingObject.transform.rotation.eulerAngles;
                    }
                    tools.EndSection();
                }
                tools.EndSection(true);
            }
        }

        #endregion


        #region Section Advanced

        void Section_Advanced()
        {
            if (tools.ShowSection("Advanced Settings", ref showSection_Advanced))
            {
                if (tools.ShowSection("New GameObject/Transform", ref showAdvancedObject))
                {
                    tools.VectorOption("Position (scaled by object size) (0.501 to avoid z-fighting)", ref objectPosition);
                    tools.VectorOption("Rotation", ref objectRotation);
                    tools.EndSection();
                }

                Section_MeshScale_Default(ref showSection_DefaultMeshScale, ref showSection_AboutMeshScaling_Default);

                if (tools.ShowSection("Z-fighting Fix", ref showSection_ZFightingFix))
                {
                    tools.FloatOption("Z Fighting Fix Magnitude", ref zFightRadius);
                    if (tools.Button("Revert to Default"))
                    {
                        zFightRadius = Glass.default_zFightRadius;
                    }
                    if (tools.ShowSection("About", ref showSection_ZFightingFix_About))
                    {
                        tools.Label("Z-fighting may occur if physical objects intersect.\nThis fix involves expanding any collider attached to the Glass object by the small set magnitude.\nThe default value should be small enough to fix z-fighting without being noticable.");
                        tools.EndSection();
                    }
                    tools.EndSection();
                }

                if (tools.ShowSection("Camera", ref showSection_Camera))
                {
                    tools.BoolOption("Always Optimise", ref settings.enableAlwaysSetOptimumCamera);
                    if (tools.ShowSection("Optimum Settings", ref settings.showSection_optimumCameraSettings))
                    {
                        settings.optimumCamera_renderingPath = (RenderingPath)tools.EnumOption("Rendering Path", settings.optimumCamera_renderingPath);
                        tools.BoolOption("Enable HDR", ref settings.optimumCamera_enableHDR);
                        if (!settings.IsDefault_OptimumCamera())
                        {
                            if (tools.Button("Restore Default Optimum Settings"))
                            {
                                settings.SetDefault_OptimumCamera();
                            }
                        }
                        tools.EndSection();
                    }
                    tools.EndSection();
                }

                if (tools.ShowSection("Existing Materials", ref showExistingMaterialsSettings))
                {
                    tools.BoolOption("Always use existing materials", ref settings.enableAlwaysUseExistingMaterials);
                    tools.EndSection();
                }

                if (tools.ShowSection("Default Resources", ref showDefaultResources))
                {
                    tools.StringOption("Default/Primitive Resources Path", ref settings.unityDefaultResourcesPath);
                    if (tools.Button("Revert to Default"))
                    {
                        settings.unityDefaultResourcesPath = "unity default resources";
                    }
                    tools.EndSection();
                }

                if (tools.ShowSection("Synchronised Settings", ref showSynchroniseSettings))
                {
                    tools.BoolOption("Synchronise Glass", ref GlassManager.Instance.synchroniseGlass);
                    tools.Label("The following options define how Glass objects are synchronised.");
                    tools.Label("When a Glass object is modified its settings are copied to any matches.");
                    tools.BoolOption("Match by Preset Name", ref GlassManager.Instance.matchByGlassName);
                    tools.BoolOption("Match by Materials", ref GlassManager.Instance.matchByMaterial);
                    tools.EndSection();
                }

                if (tools.ShowSection("Preview Settings", ref showPreviewSettings))
                {
                    tools.StartChangeCheck();
                    tools.VectorOption("Rotation Offset", ref previewRotationOffset);
                    settings.previewRotationOffset_x = previewRotationOffset.x;
                    settings.previewRotationOffset_y = previewRotationOffset.y;
                    settings.previewRotationOffset_z = previewRotationOffset.z;
                    if (tools.EndChangeCheck())
                    {
                        UpdatePreviewObjectRotation();
                        UpdateEditorObject();
                    }
                    tools.EndSection();
                }

                if (tools.ShowSection("GUI", ref showGUISettings))
                {
                    if (tools.BoolOption("Custom GUI Skin", ref settings.useCustomGUI))
                    {
                        tools.StartSection();
                        tools.StartChangeCheck();
                        settings.guiSkin = EditorGUILayout.ObjectField("GUI Skin", settings.guiSkin, typeof(GUISkin), true) as GUISkin;
                        if (tools.EndChangeCheck())
                        {
                            settings.guiSkinPath = AssetDatabase.GetAssetPath(settings.guiSkin);
                        }
                        tools.EndSection();
                    }
                    tools.EndSection();
                }
                tools.EndSection(true);
            }
        }

        #endregion

        #region Section Debug

        void Section_Debug()
        {
            if (tools.ShowSection("Debug", ref showDebug))
            {
                tools.StartChangeCheck();
                tools.BoolOption("Enable Debug Logging", ref settings.enableDebugLogging);
                if (tools.EndChangeCheck())
                {
                    tools.enableDebugLogging = settings.enableDebugLogging;
                }
                tools.EndSection(true);
            }
        }

        #endregion

        #region Section Help

        void Section_Help()
        {
            if (tools.ShowSection("Help", ref showHelp))
            {
                if (tools.ShowSection("Editor", ref ShowHelp_Editor))
                {
                    if (tools.ShowSection("Rendering / Appearance", ref showHelp_Editor_Rendering))
                    {
                        QuestionAnswerSection(qa_rendering, qa_rendering_answers, ref qa_rendering_sections, "Question: ", "Answer: ");
                        tools.EndSection();
                    }
                    tools.EndSection();
                }
                if (tools.ShowSection("ERRORS", ref ShowHelp_Errors))
                {
                    QuestionAnswerSection(errors, errors_Solutions, ref errors_sections, "Error: ", "Solution: ");
                    tools.EndSection();
                }
                tools.EndSection(true);
            }
        }

        void QuestionAnswerSection(string[] _questions, string[] _answers, ref List<bool> _sectonEnableList, string _questionSuffix, string _answerSuffix)
        {
            if (_sectonEnableList == null)
            {
                _sectonEnableList = new List<bool>();
            }
            while (_sectonEnableList.Count < _questions.Length)
            {
                _sectonEnableList.Add(false);
            }
            for (int i = 0; i < _questions.Length; i++)
            {
                bool showCurrentQuestionSection = _sectonEnableList[i];
                _sectonEnableList[i] = tools.ShowSection(_questions[i], ref showCurrentQuestionSection);
                if (showCurrentQuestionSection)
                {
                    tools.Label(_questionSuffix + _questions[i], true);
                    tools.HorizontalLine();
                    tools.Label(_answerSuffix + _answers[i], true);
                    tools.HorizontalLine();
                    tools.EndSection();
                }
            }
        }

        #endregion

        #region Editor Settings

        void LoadSettings()
        {
            settings = GlassSystemSettings.LoadFromXML(settingsPath);
            //settings = tools.LoadComponent (component_settings) as GlassSystemSettings;
            if (settings == null)
            {
                settings = GlassSystemSettings.GenerateDefaultSettings();
                SaveSettings();
            }
        }

        void SaveSettings()
        {
            if (settings != null)
            {
                if (!Application.isPlaying)
                    settings.Save(settingsPath);
            }
            //tools.SaveComponent (settings, component_settings);
        }

        #endregion


        #region Preview / Existing Assets

        void SetupPreviewAssets()
        {
            if (previewObjectContainer == null)
            {
                previewObjectContainer = new GameObject(" ");
                previewObjectContainer.AddComponent<DestroyAfterTime>();
                previewObjectContainer.AddComponent<DestroyOnModeChange>();
            }
            if (previewObject == null)
            {
                previewObject = new GameObject("Glass Creator Preview (ignore me - i am auto-deleted");
            }
            previewObject.transform.parent = previewObjectContainer.transform;
            if (previewMeshFilter == null)
                previewMeshFilter = previewObject.AddComponent<MeshFilter>();
            if (previewRenderer == null)
                previewRenderer = previewObject.AddComponent<MeshRenderer>();
            if (previewMaterial_Front == null)
                previewMaterial_Front = new Material(glassShader);
            if (previewMaterial_Back == null)
                previewMaterial_Back = new Material(glassShader);
            previewRenderer.sharedMaterials = new Material[] { previewMaterial_Back, previewMaterial_Front };
        }

        void UpdatePreviewMaterials()
        {
            CreateMaterial(ref previewMaterial_Front, GlassFace.front, true, true);
            CreateMaterial(ref previewMaterial_Back, GlassFace.back, true, true);
        }

        void UpdateFirstExistingObject()
        {
            while (selectedRenderers.Contains(null))
                selectedRenderers.Remove(null);
            if (selectedRenderers.Count > 0)
                existingObject = selectedRenderers[0].gameObject;
            if (existingObject != null)
            {
                CopyExistingObjectToPreviewObject();
                UpdateEditorObject();
                UpdatePreviewObjectRotation();
                showList_SelectedRenderers = true;
                showSection_CreateExisting = true;
                showSection_CreateNew = false;
            }
            else {
                gameObjectEditor = null;
                showList_SelectedRenderers = false;
                showSection_CreateExisting = false;
                showSection_CreateNew = true;
            }
        }

        void UpdateEditorObject()
        {
            if (previewMeshFilter == null)
                SetupPreviewAssets();

            if (previewMeshFilter.sharedMesh != null)
                gameObjectEditor = Editor.CreateEditor(previewObjectContainer);
            else
                gameObjectEditor = null;
        }

        void UpdateNewPreviewObject()
        {
            CopyNewObjectToPreviewObject();
            UpdatePreviewObjectRotation();
            UpdateEditorObject();
        }

        void UpdatePreviewObjectRotation()
        {
            if (previewObject == null)
                return;
            if (currentPreset == null)
                return;
            previewObject.transform.localRotation = Quaternion.Euler(objectRotation + previewRotationOffset);
        }

        void CopyExistingObjectToPreviewObject()
        {
            MeshFilter existingMeshfilter = existingObject.GetComponent<MeshFilter>();
            Mesh existingMesh = existingMeshfilter.sharedMesh;
            previewMeshFilter.sharedMesh = existingMesh;
        }

        void CopyNewObjectToPreviewObject()
        {
            if (mesh == null)
                return;
            if (previewMeshFilter == null)
                return;
            previewMeshFilter.mesh = mesh;
        }

        #endregion

        #region Mesh

        void UpdateMeshPath()
        {
            meshPath = AssetDatabase.GetAssetPath(mesh);
            //meshFilename = Path.GetFileName(meshPath);
            if (mesh != null)
                meshName = mesh.name;
            else
                meshName = "";
            if (meshPath.Contains(settings.unityDefaultResourcesPath))
            {
                if (meshName == "Cube")
                    meshPrimitive = GlassPrimitiveType.cube;
                else if (meshName == "Sphere")
                    meshPrimitive = GlassPrimitiveType.sphere;
                else if (meshName == "Cylinder")
                    meshPrimitive = GlassPrimitiveType.cylinder;
                else if (meshName == "Quad")
                    meshPrimitive = GlassPrimitiveType.quad;
                else if (meshName == "Plane")
                    meshPrimitive = GlassPrimitiveType.plane;
                else if (meshName == "Capsule")
                    meshPrimitive = GlassPrimitiveType.capsule;
                else
                    meshPrimitive = GlassPrimitiveType.none;
            }
            else {
                meshPrimitive = GlassPrimitiveType.none;
            }
        }

        Mesh GetMesh(GlassPrimitiveType meshPrimitive)
        {
            GameObject primitiveTempObject = null;
            switch (meshPrimitive)
            {
                case GlassPrimitiveType.none:
                    break;
                case GlassPrimitiveType.cube:
                    primitiveTempObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case GlassPrimitiveType.sphere:
                    primitiveTempObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                case GlassPrimitiveType.capsule:
                    primitiveTempObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    break;
                case GlassPrimitiveType.cylinder:
                    primitiveTempObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    break;
                case GlassPrimitiveType.quad:
                    primitiveTempObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    break;
                case GlassPrimitiveType.plane:
                    primitiveTempObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    break;
                default:
                    break;
            }
            Mesh primitiveMesh = null;
            if (primitiveTempObject != null)
            {
                MeshFilter primitiveMeshFilter = primitiveTempObject.GetComponent<MeshFilter>();
                primitiveMesh = primitiveMeshFilter.sharedMesh;
                DestroySomething(primitiveTempObject);
            }
            return primitiveMesh;
        }

        #endregion

        #region Physics

        void AddPhysicalObject(ref GameObject glassObject)
        {
            Rigidbody glassRigidBody = glassObject.GetComponent<Rigidbody>();
            if (glassRigidBody == null)
            {
                glassRigidBody = Undo.AddComponent<Rigidbody>(glassObject);//glassObject.AddComponent<Rigidbody>();
            }
            //
            switch (physicalObjectType)
            {
                case GlassPhysicalObjectType.box:
                    {
                        if (glassObject.GetComponent<BoxCollider>() == null)
                            Undo.AddComponent<BoxCollider>(glassObject);//glassObject.AddComponent<BoxCollider>();
                        break;
                    }
                case GlassPhysicalObjectType.sphere:
                    {
                        if (glassObject.GetComponent<SphereCollider>() == null)
                            Undo.AddComponent<SphereCollider>(glassObject);//glassObject.AddComponent<SphereCollider>();
                        break;
                    }
                case GlassPhysicalObjectType.mesh:
                    {
                        MeshCollider meshCollider = glassObject.GetComponent<MeshCollider>();
                        if (meshCollider == null)
                            meshCollider = Undo.AddComponent<MeshCollider>(glassObject);//glassObject.AddComponent<MeshCollider>();
                        meshCollider.convex = false;
                        glassRigidBody.isKinematic = true;
                        break;
                    }
                case GlassPhysicalObjectType.convexMesh:
                    {
                        MeshCollider meshCollider = glassObject.GetComponent<MeshCollider>();
                        if (meshCollider == null)
                            meshCollider = Undo.AddComponent<MeshCollider>(glassObject);//glassObject.AddComponent<MeshCollider>();
                        meshCollider.convex = true;
                        break;
                    }
                default:
                    break;
            }
        }

        private string PhysicsTypeName()
        {
            if (!enablePhysicalObject)
                return "Disabled";
            switch (physicalObjectType)
            {
                case GlassPhysicalObjectType.box:
                    return "Box";
                case GlassPhysicalObjectType.sphere:
                    return "Sphere";
                case GlassPhysicalObjectType.mesh:
                    return "Non-convex Mesh (Kinematic)";
                case GlassPhysicalObjectType.convexMesh:
                    return "Convex Mesh";
                default:
                    return "Disabled";
            }
        }

        #endregion

        #region Materials

        private Material LoadMaterial(GlassFace face)
        {
            switch (face)
            {
                case GlassFace.front:
                    return AssetDatabase.LoadAssetAtPath(NewMaterialName(face), typeof(Material)) as Material;
                case GlassFace.back:
                    return AssetDatabase.LoadAssetAtPath(NewMaterialName(face), typeof(Material)) as Material;
                default:
                    break;
            }
            return null;
        }

        void CreateMaterials()
        {
            backMat = new Material(glassShader);
            frontMat = new Material(glassShader);
        }

        private Material CreateMaterial(GlassFace face)
        {
            Material material = new Material(glassShader);
            try
            {
                AssetDatabase.CreateAsset(material, NewMaterialName(face));
            }
            catch (Exception e)
            {
                tools.Message("ERROR Creating Material", "Please check the directory exists for Glass Materials '" + NewMaterialName(face) + "'.\n\n\nError: " + e.Message);
            }
            return material;
        }

        void CreateMaterial(ref Material material, GlassFace face, bool overriteExisting = false, bool isCopy = false)
        {
            if (material == null)
            {
                //material = new Material(glassShader);
                return;
            }

            Material loadedMaterial = LoadMaterial(face);
            if (isCopy)
            {
                if (material != null)
                {
                    if (loadedMaterial != null)
                    {
                        material.CopyPropertiesFromMaterial(loadedMaterial);
                        //  Workaround for Unity bug (821208)
                        material.shader = material.shader;
                    }
                }
            }
            else {
                material = loadedMaterial;
            }

            if (material == null)
            {
                material = CreateMaterial(face);
            }
            else {
                if (!overriteExisting)
                {
                    if (UseExistingCreatedMaterial(NewMaterialName(face)))
                    {
                        EditorUtility.SetDirty(material);
                        return;
                    }
                }
            }

            material.SetFloat(Glass.mat_valueName_opacity, float_opacity);
            material.SetColor(Glass.mat_valueName_albedoColour, colour_albedo);
            material.SetTexture(Glass.mat_valueName_albedoTexture, texture_albedo);

            material.SetTexture(Glass.mat_valueName_distortionTexture, texture_distortion);
            if (texture_distortion == null)
            {
                vector_distortionMagnitude_Back.x = vector_distortionMagnitude_Front.x = 0f;
            }

            switch (face)
            {
                case GlassFace.front:
                    material.SetVector(Glass.mat_valueName_distortionMagnitude, vector_distortionMagnitude_Front);
                    material.SetFloat(Glass.mat_valueName_distortionEdgeBend, float_distortionEdgeBend_front);
                    material.SetFloat(Glass.mat_valueName_distortionDepthFade, float_distortionDepthFade_front);
                    material.SetFloat(Glass.mat_valueName_BumpMagnitude, float_bumpFront);
                    //
                    material.SetFloat(Glass.mat_valueName_glossiness, float_glossiness_front);
                    material.SetFloat(Glass.mat_valueName_metallic, float_metallic_front);
                    material.SetFloat(Glass.mat_valueName_glow, float_glow_front);
                    //
                    material.SetInt(Glass.mat_valueName_enableExtinction, (enableExtinctionFront || enableExtinctionBoth) ? 1 : 0);
                    material.SetColor(Glass.mat_valueName_extinction, GetExtinctionColour(GlassFace.front));
                    material.SetTexture(Glass.mat_valueName_extinctionTexture, texture_extinction_front);
                    material.SetVector(Glass.mat_valueName_extinctionIntensity, vector_extinctionIntensity_front);
                    //
                    material.SetInt(Glass.mat_valueName_enableAberration, enable_Aberration_front ? 1 : 0);
                    material.SetTexture(Glass.mat_valueName_aberrationTexture, texture_aberration_front);
                    material.SetVector(Glass.mat_valueName_aberrationIntensity, vector_aberrationIntensity_front);
                    material.SetColor(Glass.mat_valueName_aberration, colour_aberration_front);
                    material.SetInt(Glass.mat_valueName_capAberration, bool_capAberration_front ? 1 : 0);
                    //
                    material.SetInt(Glass.mat_valueName_enableFog, (enableFog_front || enableFog_both) ? 1 : 0);
                    material.SetColor(Glass.mat_valueName_fogNear, colour_fog_near_front);
                    material.SetColor(Glass.mat_valueName_fogFar, colour_fog_far_front);
                    material.SetFloat(Glass.mat_valueName_fogMagnitude, float_fogMagnitude_front);
                    //
                    material.SetInt(Glass.mat_valueName_disableBackFace, 0);
                    material.SetInt(Glass.mat_valueName_doubleDepthPass, enableDoubleDepth_front ? 1 : 0);
                    material.SetInt(Glass.mat_valueName_zWrite, 1);
                    material.SetInt(Glass.mat_valueName_cullMode, (int)UnityEngine.Rendering.CullMode.Back);
                    break;
                case GlassFace.back:
                    material.SetVector(Glass.mat_valueName_distortionMagnitude, vector_distortionMagnitude_Back);
                    material.SetFloat(Glass.mat_valueName_BumpMagnitude, float_bumpBack);
                    material.SetFloat(Glass.mat_valueName_distortionEdgeBend, float_distortionEdgeBend_back);
                    material.SetFloat(Glass.mat_valueName_distortionDepthFade, float_distortionDepthFade_back);
                    //
                    material.SetFloat(Glass.mat_valueName_glossiness, float_glossiness_back);
                    material.SetFloat(Glass.mat_valueName_metallic, float_metallic_back);
                    material.SetFloat(Glass.mat_valueName_glow, float_glow_back);
                    //
                    material.SetInt(Glass.mat_valueName_enableExtinction, (enableExtinctionBack || enableExtinctionBoth) ? 1 : 0);
                    material.SetColor(Glass.mat_valueName_extinction, GetExtinctionColour(GlassFace.back));
                    material.SetTexture(Glass.mat_valueName_extinctionTexture, texture_extinction_back);
                    material.SetVector(Glass.mat_valueName_extinctionIntensity, vector_extinctionIntensity_back);
                    //
                    material.SetInt(Glass.mat_valueName_enableAberration, enable_Aberration_back ? 1 : 0);
                    material.SetTexture(Glass.mat_valueName_aberrationTexture, texture_aberration_back);
                    material.SetVector(Glass.mat_valueName_aberrationIntensity, vector_aberrationIntensity_back);
                    material.SetColor(Glass.mat_valueName_aberration, colour_aberration_back);
                    material.SetInt(Glass.mat_valueName_capAberration, bool_capAberration_back ? 1 : 0);
                    //
                    material.SetInt(Glass.mat_valueName_enableFog, (enableFog_back || enableFog_both) ? 1 : 0);
                    material.SetColor(Glass.mat_valueName_fogNear, colour_fog_near_back);
                    material.SetColor(Glass.mat_valueName_fogFar, colour_fog_far_back);
                    material.SetFloat(Glass.mat_valueName_fogMagnitude, float_fogMagnitude_back);
                    //
                    material.SetInt(Glass.mat_valueName_disableBackFace, 1);
                    material.SetInt(Glass.mat_valueName_doubleDepthPass, enableDoubleDepth_back ? 1 : 0);
                    material.SetInt(Glass.mat_valueName_zWrite, 1);
                    material.SetInt(Glass.mat_valueName_cullMode, (int)UnityEngine.Rendering.CullMode.Front);
                    break;
                default:
                    break;
            }
            EditorUtility.SetDirty(material);
        }

        private string NewMaterialName(GlassFace face)
        {
            switch (face)
            {
                case GlassFace.front:
                    return materialsPath + "Glass_" + currentPresetName + "_front" + ".mat";
                case GlassFace.back:
                    return materialsPath + "Glass_" + currentPresetName + "_back" + ".mat";
                default:
                    Debug.LogError("Unknown Glass Face in Flass Creator!");
                    return "";
            }
        }

        private bool UseExistingCreatedMaterial(string _existngMaterialName)
        {
            if (settings.enableAlwaysUseExistingMaterials)
                return true;
            return tools.Message("Material Already Exists!", "An existing material called '" + _existngMaterialName + "' has been found.\n\nWould you prefer to use the existing material or replace it?\n\nIf you are seeing this dialog too often, you can change when it appears in Advanced Settings", "Use existing material", "Replace existing material");
        }

        #endregion

        #region Shader

        void FindGlassShader(bool forceUpdate = false)
        {
            if (glassShader == null || forceUpdate == true)
                glassShader = Shader.Find(glassShaderName);
        }

        #endregion

        #region Extinction

        Color GetExtinctionColour(GlassFace face)
        {
            return GetExtinctionColour(face, extinctionAppearance);
        }

        Color GetExtinctionColour_Flipped(GlassFace face)
        {
            return GetExtinctionColour_Flipped(face, extinctionAppearance);
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
                                return colour_extinction_front;
                            case GlassFace.back:
                                return colour_extinction_back;
                        }
                        break;
                    }
                case GlassExtinctionAppearance.AsItAppears:
                    {
                        switch (face)
                        {
                            case GlassFace.front:
                                return FlippedColour(colour_extinction_flipped_front);
                            case GlassFace.back:
                                return FlippedColour(colour_extinction_flipped_back);
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
                                return colour_extinction_flipped_front;
                            case GlassFace.back:
                                return colour_extinction_flipped_back;
                        }
                        break;
                    }
                case GlassExtinctionAppearance.AsItAppears:
                    {
                        switch (face)
                        {
                            case GlassFace.front:
                                return FlippedColour(colour_extinction_front);
                            case GlassFace.back:
                                return FlippedColour(colour_extinction_back);
                        }
                        break;
                    }
            }
            return Color.clear;
        }

        Color FlippedColour(Color colour)
        {
            return new Color(1f - colour.r, 1f - colour.g, 1f - colour.b, 1f - colour.a);
        }

        #endregion

        #region Create

        void CreateGlass()
        {
            if (!CheckSettingsBeforeCreate())
            {
                return;
            }

            UpdateMeshPath();

            string newGlassObjectName = "Glass_" + currentPresetName + "_" + meshName;
            GameObject glassObject = new GameObject(newGlassObjectName);
            Undo.RegisterCreatedObjectUndo(glassObject, "Created " + newGlassObjectName);
            //
            glassObject.transform.position = objectPosition.Scaled(ObjectSize(glassObject));
            glassObject.transform.rotation = Quaternion.Euler(objectRotation);
            //
            ScaleNewGlassObject(ref glassObject);
            //
            Glass glass = glassObject.AddComponent<Glass>();
            //
            MeshFilter meshFilter = glassObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            //
            //MeshRenderer renderer = glassObject.AddComponent<MeshRenderer>();
            glassObject.AddComponent<MeshRenderer>();
            //
            if (backMat == null)
                backMat = new Material(glassShader);
            if (frontMat == null)
                frontMat = new Material(glassShader);
            //
            if (!assignCustomMaterials)
            {
                CreateMaterial(ref backMat, GlassFace.back);
                CreateMaterial(ref frontMat, GlassFace.front);
                SaveSettingsToGlass(ref glass);
            }
            //
            glass.presetName = currentPresetName;
            //
            //renderer.sharedMaterials = new Material[] { backMat, frontMat };
            glass.SetMaterials(backMat, frontMat);
            //
            if (enablePhysicalObject)
            {
                AddPhysicalObject(ref glassObject);
            }
            //
            if (settings.enableAlwaysSetOptimumCamera)
                SetOptimumCamera();
            else
                ShowOptimumCameraSettingsDialogue();
            //
            //	TODO:	Remove and replace with neater workaround
            //glass.LoadFromPreset(currentPreset, new GlassSettingsCopyList());
            //
            glass.FindGlassManager();
            glass.UpdateGlassManager();
            //
            if (!glass.manager.LayersExist())
            {
                GlassManager_Editor.Show_LayersWarning();
            }
        }

        void ScaleNewGlassObject(ref GameObject _newGlassObject)
        {
            switch (meshPrimitive)
            {
                case GlassPrimitiveType.none:
                    _newGlassObject.transform.localScale = new Vector3(meshScale, meshScale, meshScale);
                    break;
                case GlassPrimitiveType.cube:
                    break;
                case GlassPrimitiveType.sphere:
                    break;
                case GlassPrimitiveType.capsule:
                    break;
                case GlassPrimitiveType.cylinder:
                    break;
                case GlassPrimitiveType.quad:
                    break;
                case GlassPrimitiveType.plane:
                    break;
                default:
                    break;
            }
        }

        void TurnObjectIntoGlass(ref GameObject targetObject, ref OptimumCameraChoice optimumCameraOptionChosen)
        {
            if (targetObject == null)
            {
                tools.Message("Turn Object into Glass Failed", "You must provide a GameObject to be turned into glass.");
                return;
            }
            //
            if (!CheckSettingsBeforeCreate())
            {
                tools.Message("Turn Object into Glass Failed", "An error occured during settings check for object:" + targetObject.name);
                return;
            }
            //
            Glass glass = targetObject.GetComponent<Glass>();
            if (glass == null)
            {
                glass = Undo.AddComponent<Glass>(targetObject);//glass = targetObject.AddComponent<Glass>();
            }
            //
            MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = Undo.AddComponent<MeshFilter>(targetObject);//meshFilter = targetObject.AddComponent<MeshFilter>();
            }
            //
            Mesh targetMesh = meshFilter.sharedMesh;
            if (targetMesh == null)
            {
                Undo.RecordObject(meshFilter, "Assign glass mesh to mesh filter");
                meshFilter.sharedMesh = mesh;
            }
            //
            MeshRenderer targetRenderer = targetObject.GetComponent<MeshRenderer>();
            if (targetRenderer == null)
            {
                targetRenderer = Undo.AddComponent<MeshRenderer>(targetObject);//targetRenderer = targetObject.AddComponent<MeshRenderer>();
            }
            //
            Material targetMaterialBack = glass.material_back;
            Material targetMaterialFront = glass.material_front;
            //
            if (!assignCustomMaterials)
            {
                if (targetMaterialBack == null)
                    targetMaterialBack = new Material(glassShader);
                if (targetMaterialFront == null)
                    targetMaterialFront = new Material(glassShader);
                //
                CreateMaterial(ref targetMaterialBack, GlassFace.back);
                CreateMaterial(ref targetMaterialFront, GlassFace.front);
            }
            //
            glass.SetMaterials(targetMaterialBack, targetMaterialFront);
            //glass.material_back = targetMaterialBack;
            //glass.material_front = targetMaterialFront;
            //
            Undo.RecordObject(targetRenderer, "Assign glass materials to mesh renderer");
            targetRenderer.sharedMaterials = new Material[] { targetMaterialBack, targetMaterialFront };
            //
            if (enablePhysicalObject)
            {
                AddPhysicalObject(ref targetObject);
            }
            //
            glass.zFightRadius = zFightRadius;
            //
            if (settings.enableAlwaysSetOptimumCamera)
            {
                SetOptimumCamera();
            }
            else
            {
                switch (optimumCameraOptionChosen)
                {
                    case OptimumCameraChoice.NoChoice:
                        optimumCameraOptionChosen = ShowOptimumCameraSettingsDialogue();

                        switch (optimumCameraOptionChosen)
                        {
                            case OptimumCameraChoice.OptimiseAlways:
                            case OptimumCameraChoice.OptimiseOnce:
                                SetOptimumCamera();
                                break;
                            case OptimumCameraChoice.NoChoice:
                            case OptimumCameraChoice.DoNotOptimise:
                            default:
                                break;
                        }
                        break;
                    case OptimumCameraChoice.DoNotOptimise:
                    case OptimumCameraChoice.OptimiseOnce:
                    case OptimumCameraChoice.OptimiseAlways:
                    default:
                        break;
                }
            }
            //
            glass.presetName = currentPresetName;
            //
            glass.FindGlassManager();
            //
            if (!glass.manager.LayersExist())
            {
                if (!tools.Message("Layer Missing", "Please make sure the following Layers exist:\n\nGlass_Front, Glass_Back", "OK", "Disable This Warning"))
                {
                    glass.manager.disableLayerWarnings = true;
                }
            }
        }

        void TurnObjectIntoGlass()
        {
            OptimumCameraChoice cameraOptimisationChoice = OptimumCameraChoice.NoChoice;
            for (int i = 0; i < selectedRenderers.Count; i++)
            {
                Renderer glassTargetRenderer = selectedRenderers[i];
                GameObject glassTarget = glassTargetRenderer.gameObject;
                TurnObjectIntoGlass(ref glassTarget, ref cameraOptimisationChoice);
            }
            //
            tools.Message("Glassification Complete!", "Finished turning objects into Glass." + (settings.enableAlwaysUseExistingMaterials ? "\n\nNOTE: Currently set to always use existing materials. Any changes made to the preset will not be copied to an existing material.\n-You can change this option in 'Advanced Settings'-" : ""));
        }

        private bool CheckSettingsBeforeCreate()
        {
            if (mesh == null)
                if (!tools.Message("NO MESH", "You have not provided a mesh for the glass object.\nDo you wish to create the glass objct anyway?", "Make Anyway", "Go Back"))
                    return false;

            if (assignCustomMaterials)
                if (frontMat == null || backMat == null)
                    if (!tools.Message("MISSING MATERIAL", "You have chosen to assign your own materials but not all are set.\nDo you wish to create the glass object anyway?", "Make Anyway", "Go Back"))
                        return false;

            return true;
        }

        public void SaveSettingsToGlass(ref Glass glass)
        {
            glass.colour_albedo = colour_albedo;
            glass.opacity = float_opacity;
            glass.texture_albedo = texture_albedo;
            glass.colour_albedoTexture = colour_albedoTexture;

            glass.enableDistortion_front = enableDistortion_front;
            glass.enableDistortion_back = enableDistortion_back;
            glass.texture_distortion = texture_distortion;
            glass.distortion_back = vector_distortionMagnitude_Back;
            glass.distortion_front = vector_distortionMagnitude_Front;
            glass.distortionEdgeBend_front = float_distortionEdgeBend_front;
            glass.distortionEdgeBend_back = float_distortionEdgeBend_back;
            glass.distortionDepthFade_front = float_distortionDepthFade_front;
            glass.distortionDepthFade_back = float_distortionDepthFade_back;

            glass.enableDoubleDepth_front = enableDoubleDepth_front;
            glass.enableDoubleDepth_back = enableDoubleDepth_back;

            glass.bumpBack = float_bumpBack;
            glass.bumpFront = float_bumpFront;

            glass.enableExtinction_front = enableExtinctionFront;
            glass.enableExtinction_back = enableExtinctionBack;
            glass.enableExtinction_both = enableExtinctionBoth;
            glass.capExtinction_front = enableCapExtinctionFront;
            glass.capExtinction_back = enableCapExtinctionBack;
            glass.extinctionMagnitude_front = vector_extinctionIntensity_front;
            glass.extinctionMagnitude_back = vector_extinctionIntensity_back;
            glass.extinction_front = GetExtinctionColour(GlassFace.front);
            glass.extinction_back = GetExtinctionColour(GlassFace.back);
            glass.extinctionFlipped_front = colour_extinction_flipped_front;
            glass.extinctionFlipped_back = colour_extinction_flipped_back;
            glass.texture_extinction_front = texture_extinction_front;
            glass.texture_extinction_back = texture_extinction_back;

            glass.enableAberration_front = enable_Aberration_front;
            glass.enableAberration_back = enable_Aberration_back;
            glass.enableAberration_both = enable_Aberration_both;
            glass.aberrationMagnitude_front = vector_aberrationIntensity_front;
            glass.aberrationMagnitude_back = vector_aberrationIntensity_back;
            glass.aberration_front = colour_aberration_front;
            glass.aberration_back = colour_aberration_back;
            glass.texture_aberration_front = texture_aberration_front;
            glass.texture_aberration_back = texture_aberration_back;
            glass.capAberration_front = bool_capAberration_front;
            glass.capAberration_back = bool_capAberration_back;

            glass.enableFog_front = enableFog_front;
            glass.enableFog_back = enableFog_back;
            glass.enableFog_both = enableFog_both;
            glass.fogNear_front = colour_fog_near_front;
            glass.fogNear_back = colour_fog_near_back;
            glass.fogFar_front = colour_fog_far_front;
            glass.fogFar_back = colour_fog_far_back;
            glass.fogMagnitude_front = float_fogMagnitude_front;
            glass.fogMagnitude_back = float_fogMagnitude_back;

            glass.glossiness_front = float_glossiness_front;
            glass.glossiness_back = float_glossiness_back;

            glass.metallic_front = float_metallic_front;
            glass.metallic_back = float_metallic_back;

            glass.glow_front = float_glow_front;
            glass.glow_back = float_glow_back;

            glass.SetMaterials(backMat, frontMat);
            //glass.material_back = backMat;
            //glass.material_front = frontMat;

            glass.zFightRadius = zFightRadius;

            EditorUtility.SetDirty(glass);
        }

        #endregion

        #region Camera

        OptimumCameraChoice ShowOptimumCameraSettingsDialogue()
        {
            switch (EditorUtility.DisplayDialogComplex("Optimum Camera Settings", "Set Main Camera to optimum settings?", "Yes", "Always", "Not Now"))
            {
                case 0:
                    {
                        SetOptimumCamera();
                        return OptimumCameraChoice.OptimiseOnce;
                    }
                case 1:
                    {
                        SetOptimumCamera();
                        settings.enableAlwaysSetOptimumCamera = true;
                        return OptimumCameraChoice.OptimiseAlways;
                    }
                case 2:
                default:
                    {
                        return OptimumCameraChoice.DoNotOptimise;
                    }
            }
        }

        void SetOptimumCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
                cam = Camera.allCameras[0];
            cam.allowHDR = settings.optimumCamera_enableHDR;
            cam.renderingPath = settings.optimumCamera_renderingPath;
        }

        #endregion

        #region Presets

        private GlassPreset GenerateDefaultPreset()
        {
            GlassPreset _defaultPreset = GeneratePreset();
            //
            _defaultPreset.meshPath = settings.unityDefaultResourcesPath;
            _defaultPreset.meshPrimitive = GlassPrimitiveType.cube;
            _defaultPreset.meshScaleFix = settings.defaultMeshScaleFix;
            _defaultPreset.meshScale = settings.defaultMeshScale;
            //
            return _defaultPreset;
        }

        private GlassPreset GeneratePreset()
        {
            GlassPreset preset = new GlassPreset();
            //
            preset.name = currentPresetName;
            //
            backMatPath = AssetDatabase.GetAssetPath(backMat);
            backMatPath = AssetDatabase.GetAssetPath(frontMat);
            preset.backMatPath = backMatPath;
            preset.frontMatPath = backMatPath;
            //
            UpdateMeshPath();
            preset.meshPath = meshPath;
            preset.meshPrimitive = meshPrimitive;
            preset.meshScaleFix = meshScaleFix;
            preset.meshScale = meshScale;
            //
            preset.opacity = float_opacity;
            GlassPreset.SetColour(ref preset.colour_albedo, colour_albedo);
            preset.texturePath_albedo = AssetDatabase.GetAssetPath(texture_albedo);
            GlassPreset.SetColour(ref preset.colour_albedoTexture, colour_albedoTexture);
            //
            preset.enableDistortion_front = enableDistortion_front;
            preset.enableExtinction_back = enableDistortion_back;
            preset.texturePath_distortion = AssetDatabase.GetAssetPath(texture_distortion);
            preset.distortion_front_bump = vector_distortionMagnitude_Front.x;
            preset.distortion_back_bump = vector_distortionMagnitude_Back.x;
            preset.enableDoubleDepth_front = enableDoubleDepth_front;
            preset.enableDoubleDepth_back = enableDoubleDepth_back;
            preset.distortion_front_mesh = vector_distortionMagnitude_Front.y;
            preset.distortion_back_mesh = vector_distortionMagnitude_Back.y;
            preset.distortion_front_multiplier = vector_distortionMagnitude_Front.z;
            preset.distortion_back_multiplier = vector_distortionMagnitude_Back.z;
            preset.distortion_front_max = vector_distortionMagnitude_Front.w;
            preset.distortion_back_max = vector_distortionMagnitude_Back.w;
            preset.distortion_edge_bend_front = float_distortionEdgeBend_front;
            preset.distortion_edge_bend_back = float_distortionEdgeBend_back;
            preset.distortion_depth_fade_front = float_distortionDepthFade_front;
            preset.distortion_depth_fade_back = float_distortionDepthFade_back;
            //
            preset.bump_front = float_bumpFront;
            preset.bump_back = float_bumpBack;
            //
            preset.enableAberration_front = enable_Aberration_front;
            preset.enableAberration_back = enable_Aberration_back;
            preset.enableAberration_both = enable_Aberration_both;
            preset.texturePath_aberration_front = AssetDatabase.GetAssetPath(texture_aberration_front);
            preset.texturePath_aberration_back = AssetDatabase.GetAssetPath(texture_aberration_back);
            GlassPreset.SetColour(ref preset.colour_aberration_front, colour_aberration_front);
            GlassPreset.SetColour(ref preset.colour_aberration_back, colour_aberration_back);
            preset.aberrationIntensity_front = vector_aberrationIntensity_front.x;
            preset.aberrationIntensity_back = vector_aberrationIntensity_back.x;
            preset.aberrationMin_front = vector_aberrationIntensity_front.y;
            preset.aberrationMin_back = vector_aberrationIntensity_back.y;
            preset.aberrationMax_front = vector_aberrationIntensity_front.z;
            preset.aberrationMax_back = vector_aberrationIntensity_back.z;
            preset.capAberration_front = bool_capAberration_front;
            preset.capAberration_back = bool_capAberration_back;
            //
            preset.extinctionAppearance = (int)extinctionAppearance;
            preset.enableExtinction_front = enableExtinctionFront;
            preset.enableExtinction_back = enableExtinctionBack;
            preset.enableExtinction_both = enableExtinctionBoth;
            preset.texturePath_extinction_front = AssetDatabase.GetAssetPath(texture_extinction_front);
            preset.texturePath_extinction_back = AssetDatabase.GetAssetPath(texture_extinction_back);
            preset.extinctionIntensity_front = vector_extinctionIntensity_front.x;
            preset.extinctionIntensity_back = vector_extinctionIntensity_back.x;
            preset.extinctionMin_front = vector_extinctionIntensity_front.y;
            preset.extinctionMin_back = vector_extinctionIntensity_back.y;
            preset.extinctionMax_front = vector_extinctionIntensity_front.z;
            preset.extinctionMax_back = vector_extinctionIntensity_back.z;
            GlassPreset.SetColour(ref preset.colour_extinction_front, colour_extinction_front);
            GlassPreset.SetColour(ref preset.colour_extinction_back, colour_extinction_back);
            GlassPreset.SetColour(ref preset.colour_extinction_flipped_front, colour_extinction_flipped_front);
            GlassPreset.SetColour(ref preset.colour_extinction_flipped_back, colour_extinction_flipped_back);
            //
            preset.option_fog_front = enableFog_front;
            preset.option_fog_back = enableFog_back;
            preset.option_fog_both = enableFog_both;
            preset.fog_magnitude_front = float_fogMagnitude_front;
            preset.fog_magnitude_back = float_fogMagnitude_back;
            GlassPreset.SetColour(ref preset.colour_fog_near_front, colour_fog_near_front);
            GlassPreset.SetColour(ref preset.colour_fog_near_back, colour_fog_near_back);
            GlassPreset.SetColour(ref preset.colour_fog_far_front, colour_fog_far_front);
            GlassPreset.SetColour(ref preset.colour_fog_far_back, colour_fog_far_back);
            //
            preset.glow_front = float_glow_front;
            preset.glow_back = float_glow_back;
            preset.glossiness_front = float_glossiness_front;
            preset.glossiness_back = float_glossiness_back;
            preset.metallic_front = float_metallic_front;
            preset.metallic_back = float_metallic_back;
            //
            preset.texturePath_glow_front = AssetDatabase.GetAssetPath(texture_glow_front);
            preset.texturePath_glow_back = AssetDatabase.GetAssetPath(texture_glow_back);
            preset.texturePath_gloss_front = AssetDatabase.GetAssetPath(texture_gloss_front);
            preset.texturePath_gloss_back = AssetDatabase.GetAssetPath(texture_gloss_back);
            preset.texturePath_metal_front = AssetDatabase.GetAssetPath(texture_metal_front);
            preset.texturePath_metal_back = AssetDatabase.GetAssetPath(texture_metal_back);
            //
            preset.physicalObject = enablePhysicalObject;
            preset.physicalObjectType = physicalObjectType;
            //
            preset.zFightingRadius = Glass.default_zFightRadius;
            //
            GlassPreset.SetVector(ref preset.objectPosition, objectPosition);
            GlassPreset.SetVector(ref preset.objectRotation, objectRotation);
            //
            return preset;
        }

        private bool LoadGlassPreset(int presetIndex)
        {
            if (presetIndex < 0)
                return false;
            if (presetList.Count < 1)
                return false;
            if (presetIndex >= presetList.Count)
                return false;
            currentPresetIndex = presetIndex;
            currentPresetName = presetList[presetIndex];
            return LoadGlassPreset(currentPresetName, currentPresetIndex, presetListPath);
        }

        private bool LoadGlassPreset(string presetName, int presetIndex, string listFilename)
        {
            currentPresetIndex = presetIndex;
            currentPresetName = presetName;
            //
            //currentPreset = tools.LoadComponent (tools.PresetFilename (presetName, listFilename)) as GlassPreset;
            currentPreset = GlassPreset.LoadFromXML(xmlPath + currentPresetName + ".xml") as GlassPreset;
            //
            if (currentPreset == null)
            {
                tools.Message("Failed to load Glass Preset", "Unable to load preset: '" + presetName + "' at index: '" + presetIndex + "'. Deleting from List...");
                tools.DeletedPreset(presetName, presetIndex, ref presetList, listFilename);
                SaveList(presetList, presetListPath);
                if (presetList != null)
                    if (presetList.Count > 0)
                        LoadGlassPreset(0);
                return false;
            }
            //
            backMatPath = currentPreset.backMatPath;
            frontMatPath = currentPreset.frontMatPath;
            backMat = AssetDatabase.LoadAssetAtPath(backMatPath, typeof(Material)) as Material;
            frontMat = AssetDatabase.LoadAssetAtPath(frontMatPath, typeof(Material)) as Material;
            //
            meshPath = currentPreset.meshPath;
            meshPrimitive = currentPreset.meshPrimitive;
            mesh = GetMesh(meshPrimitive);
            if (mesh == null)
                mesh = AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh)) as Mesh;
            //
            meshScale = currentPreset.meshScale;
            meshScaleFix = currentPreset.meshScaleFix;
            //
            float_opacity = currentPreset.opacity;
            GlassPreset.SetColour(ref colour_albedo, currentPreset.colour_albedo);
            texture_albedoPath = currentPreset.texturePath_albedo;
            texture_albedo = AssetDatabase.LoadAssetAtPath(texture_albedoPath, typeof(Texture)) as Texture;
            GlassPreset.SetColour(ref colour_albedoTexture, currentPreset.colour_albedoTexture);
            //
            enableDistortion_front = currentPreset.enableDistortion_front;
            enableDistortion_back = currentPreset.enableDistortion_back;
            //
            texture_distortionPath = currentPreset.texturePath_distortion;
            texture_distortion = AssetDatabase.LoadAssetAtPath(texture_distortionPath, typeof(Texture)) as Texture;
            //
            vector_distortionMagnitude_Back.x = currentPreset.distortion_back_bump;
            vector_distortionMagnitude_Back.y = currentPreset.distortion_back_mesh;
            vector_distortionMagnitude_Back.z = currentPreset.distortion_back_multiplier;
            vector_distortionMagnitude_Back.w = currentPreset.distortion_back_max;
            //
            vector_distortionMagnitude_Front.x = currentPreset.distortion_front_bump;
            vector_distortionMagnitude_Front.y = currentPreset.distortion_front_mesh;
            vector_distortionMagnitude_Front.z = currentPreset.distortion_front_multiplier;
            vector_distortionMagnitude_Front.w = currentPreset.distortion_front_max;
            //
            float_distortionEdgeBend_front = currentPreset.distortion_edge_bend_front;
            float_distortionEdgeBend_back = currentPreset.distortion_edge_bend_back;
            float_distortionDepthFade_front = currentPreset.distortion_depth_fade_front;
            float_distortionDepthFade_back = currentPreset.distortion_depth_fade_back;
            //
            enableDoubleDepth_front = currentPreset.enableDoubleDepth_front;
            enableDoubleDepth_back = currentPreset.enableDoubleDepth_back;
            //
            float_bumpFront = currentPreset.bump_front;
            float_bumpBack = currentPreset.bump_back;
            //
            enableFog_front = currentPreset.option_fog_front;
            enableFog_back = currentPreset.option_fog_back;
            enableFog_both = currentPreset.option_fog_both;
            GlassPreset.SetColour(ref colour_fog_near_front, currentPreset.colour_fog_near_front);
            GlassPreset.SetColour(ref colour_fog_near_back, currentPreset.colour_fog_near_back);
            GlassPreset.SetColour(ref colour_fog_far_front, currentPreset.colour_fog_far_front);
            GlassPreset.SetColour(ref colour_fog_far_back, currentPreset.colour_fog_far_back);
            float_fogMagnitude_front = currentPreset.fog_magnitude_front;
            float_fogMagnitude_back = currentPreset.fog_magnitude_back;
            //
            enable_Aberration_front = currentPreset.enableAberration_front;
            enable_Aberration_back = currentPreset.enableAberration_back;
            enable_Aberration_both = currentPreset.enableAberration_both;
            texture_aberration_front = AssetDatabase.LoadAssetAtPath(currentPreset.texturePath_aberration_front, typeof(Texture)) as Texture;
            texture_aberration_back = AssetDatabase.LoadAssetAtPath(currentPreset.texturePath_aberration_back, typeof(Texture)) as Texture;
            vector_aberrationIntensity_front.x = currentPreset.aberrationIntensity_front;
            vector_aberrationIntensity_back.x = currentPreset.aberrationIntensity_back;
            vector_aberrationIntensity_front.y = currentPreset.aberrationMin_front;
            vector_aberrationIntensity_back.y = currentPreset.aberrationMin_back;
            vector_aberrationIntensity_front.z = currentPreset.aberrationMax_front;
            vector_aberrationIntensity_back.z = currentPreset.aberrationMax_back;
            GlassPreset.SetColour(ref colour_aberration_front, currentPreset.colour_aberration_front);
            GlassPreset.SetColour(ref colour_aberration_back, currentPreset.colour_aberration_back);
            bool_capAberration_front = currentPreset.capAberration_front;
            bool_capAberration_back = currentPreset.capAberration_back;
            //
            enableCapExtinctionFront = currentPreset.capExtinction_front;
            enableCapExtinctionBack = currentPreset.capExtinction_back;
            enableExtinctionFront = currentPreset.enableExtinction_front;
            enableExtinctionBack = currentPreset.enableExtinction_back;
            enableExtinctionBoth = currentPreset.enableExtinction_both;
            texture_extinction_front = AssetDatabase.LoadAssetAtPath(currentPreset.texturePath_extinction_front, typeof(Texture)) as Texture;
            texture_extinction_back = AssetDatabase.LoadAssetAtPath(currentPreset.texturePath_extinction_back, typeof(Texture)) as Texture;
            vector_extinctionIntensity_front.x = currentPreset.extinctionIntensity_front;
            vector_extinctionIntensity_back.x = currentPreset.extinctionIntensity_back;
            vector_extinctionIntensity_front.y = currentPreset.extinctionMin_front;
            vector_extinctionIntensity_back.y = currentPreset.extinctionMin_back;
            vector_extinctionIntensity_front.z = currentPreset.extinctionMax_front;
            vector_extinctionIntensity_back.z = currentPreset.extinctionMax_back;
            GlassPreset.SetColour(ref colour_extinction_front, currentPreset.colour_extinction_front);
            GlassPreset.SetColour(ref colour_extinction_back, currentPreset.colour_extinction_back);
            GlassPreset.SetColour(ref colour_extinction_flipped_front, currentPreset.colour_extinction_flipped_front);
            GlassPreset.SetColour(ref colour_extinction_flipped_back, currentPreset.colour_extinction_flipped_back);
            //
            float_glow_front = currentPreset.glow_front;
            float_glow_back = currentPreset.glow_back;
            float_glossiness_front = currentPreset.glossiness_front;
            float_glossiness_back = currentPreset.glossiness_back;
            float_metallic_front = currentPreset.metallic_front;
            float_metallic_back = currentPreset.metallic_back;
            texture_gloss_back = AssetDatabase.LoadAssetAtPath(currentPreset.texturePath_gloss_back, typeof(Texture)) as Texture;
            texture_gloss_front = AssetDatabase.LoadAssetAtPath(currentPreset.texturePath_gloss_front, typeof(Texture)) as Texture;
            texture_glow_back = AssetDatabase.LoadAssetAtPath(currentPreset.texturePath_glow_back, typeof(Texture)) as Texture;
            texture_glow_back = AssetDatabase.LoadAssetAtPath(currentPreset.texturePath_glow_back, typeof(Texture)) as Texture;
            texture_metal_back = AssetDatabase.LoadAssetAtPath(currentPreset.texturePath_metal_back, typeof(Texture)) as Texture;
            texture_metal_back = AssetDatabase.LoadAssetAtPath(currentPreset.texturePath_metal_back, typeof(Texture)) as Texture;
            //
            GlassPreset.SetVector(ref objectPosition, currentPreset.objectPosition);
            GlassPreset.SetVector(ref objectRotation, currentPreset.objectRotation);
            //
            enablePhysicalObject = currentPreset.physicalObject;
            physicalObjectType = currentPreset.physicalObjectType;
            //
            UpdatePreviewMaterials();
            if (showSection_CreateExisting)
                UpdateFirstExistingObject();
            else if (showSection_CreateNew)
                UpdateNewPreviewObject();
            //
            settings.lastUsedPreset = currentPresetIndex;
            //
            return true;
        }

        void LoadLastUsedPreset()
        {
            currentPresetIndex = settings.lastUsedPreset;
            if (currentPresetIndex == -1)
                currentPresetIndex = 0;
            if (presetList.Count <= currentPresetIndex)
                return;
            currentPresetName = presetList[currentPresetIndex];
            LoadGlassPreset(currentPresetName, currentPresetIndex, presetListPath);
        }

        public void LoadGlassPresets()
        {
            //presetList = tools.LoadComponent (presetListPath) as List<string>;
            presetList = LoadList(presetListPath);
            if (presetList == null)
            {
                Debug.Log("Could not find Glass Presets list. Either not generated or deleted. Generating...");
                presetList = new List<string>();
                SaveList(presetList, presetListPath);//tools.SaveComponent (presetList, presetListPath);
            }
            if (presetList.Count == 0)
            {
                GlassPreset defaultPreset = GenerateDefaultPreset();
                if (tools.SavePreset(defaultPreset, "Default", 0, ref presetList, presetListPath))
                {
                    defaultPreset.name = "Default";
                    defaultPreset.Save(xmlPath + "Default.xml");
                    SaveList(presetList, presetListPath);//tools.SaveComponent (presetList, presetListPath);
                }
            }
            if (currentPresetIndex == -1)
            {
                currentPresetIndex = 0;
                currentPresetName = presetList[0];
            }
            CleanUpPresetList();
        }

        void CleanUpPresetList()
        {
            for(int i = presetList.Count-1; i >=0; i--)
            {
                string testPresetName = presetList[i];
                GlassPreset testPreset = GlassPreset.LoadFromXML(xmlPath + testPresetName + ".xml") as GlassPreset;
                //
                if (testPreset == null)
                {
                    tools.DeletedPreset(testPresetName, i, ref presetList, presetListPath);
                    Debug.Log("Removed missing preset from list: '" + testPresetName + "'.");
                }
            }
        }

        public void SaveList(List<string> _list, string _path)
        {
            XmlSerializer xmlserialiser = new XmlSerializer(typeof(List<string>));
            FileStream fileStream = new FileStream(_path, FileMode.Create);
            xmlserialiser.Serialize(fileStream, _list);
            fileStream.Close();
        }

        public List<string> LoadList(string _path)
        {
            if (!File.Exists(_path))
            {
                Debug.Log("(Preset List)File does not exists:" + _path);
                return null;
            }
            XmlSerializer xmlserialiser = new XmlSerializer(typeof(List<string>));
            FileStream fileStream = new FileStream(_path, FileMode.Open);
            List<string> loadedList = xmlserialiser.Deserialize(fileStream) as List<string>;
            fileStream.Close();
            return loadedList;
        }

        public void LoadLastAddedPreset()
        {
            if (presetList == null)
                return;
            if (presetList.Count == 0)
                return;
            currentPresetIndex = presetList.Count - 1;
            currentPresetName = presetList[currentPresetIndex];
            LoadGlassPreset(currentPresetName, currentPresetIndex, presetListPath);
        }

        #endregion


        #region Helper Functions

        public void DestroySomething(UnityEngine.Object obj, bool allowDestroyingAssets)
        {
            if (obj == null)
                return;
            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else {
#if UNITY_EDITOR
                DestroyImmediate(obj, allowDestroyingAssets);
#endif
            }
        }

        public void DestroySomething(UnityEngine.Object obj)
        {
            if (obj == null)
                return;
            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else {
#if UNITY_EDITOR
                DestroyImmediate(obj);
#endif
            }
        }

        private Vector3 ObjectSize(GameObject obj)
        {
            BoxCollider tempBoxCollider = obj.AddComponent<BoxCollider>();
            Bounds tempBounds = tempBoxCollider.bounds;
            Vector3 objectSize = tempBounds.size;
            DestroySomething(tempBoxCollider);
            return objectSize;
        }

        #endregion
    }

}

#endif
