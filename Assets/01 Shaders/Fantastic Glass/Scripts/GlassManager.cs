using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using System.Xml.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FantasticGlass
{
    [Serializable]
    /// <summary>
    /// Glass manager.
    /// Maintains Glass objects, Depth Cameras, layer, and other settings of Fantastic Glass.
    /// </summary>
    public class GlassManager : MonoBehaviour
    {
        #region Member Variables

        public GlassSystemSettings settings;
        public GlassManagerSettingsType managerSettingsType = default_managerSettingsType;
        public static GlassManagerSettingsType default_managerSettingsType = GlassManagerSettingsType.LoadFromGlass;
        public Glass chosenGlassSettingsSource = null;
        public bool showSection_SettingsType = false;
        public List<Glass> glass = new List<Glass>();
        public List<Material> activeMaterials = new List<Material>();
        public Dictionary<Material, List<Glass>> activeMaterialsAndObjects = new Dictionary<Material, List<Glass>>();
        //
        public GlassDepthCamera camFront = null;
        public GlassDepthCamera camBack = null;
        public GlassDepthCamera camOther = null;
        //
        public string frontLayerName = default_frontLayerName;
        public string backLayerName = default_backLayerName;
        public static string default_frontLayerName = "GlassFront";
        public static string default_backLayerName = "GlassBack";
        public int backLayerMask = -1;
        public int frontLayerMask = -1;
        public int otherLayerMask = -1;
        public int frontLayerSingle = -1;
        public int backLayerSingle = -1;
        //
        public List<string> frontLayerNames = new List<string>();
        public List<string> backLayerNames = new List<string>();
        public List<string> otherLayerNames = new List<string>();
        //
        public bool customRenderOrder = false;
        //
        public GlassDepthTechnique depthTechnique = GlassDepthCamera.default_depthTechnique;
        public GlassNormalTechnique normalTechnique = GlassDepthCamera.default_normalTechnique;
        public GlassFrontDepthTechnique frontDepthTechnique = GlassDepthCamera.default_frontDepthTechnique;
        public bool managerChoosesDepthTechnique = false;
        //#if UNITY_5_4_OR_NEWER
        public bool enable54Workaround = false;
        //#endif
        //
        public string depthTextureName_Front = "_DepthFront";
        public string depthTextureName_Back = "_DepthBack";
        public string depthTextureName_Other = "_DepthOther";
        public Shader depthShaderBack;
        public Shader depthShaderFront;
        public Shader glassShader;
        public int depthTextureAA = default_depthTextureAA;
        public int depthTextureAniso = default_depthTextureAniso;
        public FilterMode depthTextureFilterMode = default_depthTextureFilterMode;
        public CameraClearFlags depthTextureClearMode = default_depthTextureClearMode;
        public Camera mainCamera = null;
        //
        public static int default_depthTextureAA = 1;
        public static int default_depthTextureAniso = 16;
        public static FilterMode default_depthTextureFilterMode = FilterMode.Trilinear;
        public static CameraClearFlags default_depthTextureClearMode = CameraClearFlags.Skybox;
        //
        public bool renderDepthsSeperately = default_renderDepthSeperately;
        public float depthUpdateRate = default_depthUpdateRate;
        public float frontUpdateRate = default_frontUpdateRate;
        public float backUpdateRate = default_backUpdateRate;
        public float otherUpdateRate = default_otherUpdateRate;
        public float camHighUpdateRate = default_camHighUpdateRate;
        public float camLowUpdateRate = default_camLowUpdateRate;
        public int depthWait = 0;
        int depthWaitFront = 0;
        int depthWaitBack = 0;
        int depthWaitOther = 0;
        int depthWaitDelta = 0;
        //
        public DepthQuality_GlassManager depthQuality_Front = default_depthQuality;
        public DepthQuality_GlassManager depthQuality_Back = default_depthQuality;
        public DepthQuality_GlassManager depthQuality_Other = default_depthQuality;
        public static DepthQuality_GlassManager default_depthQuality = DepthQuality_GlassManager.Simple;
        //
        public List<RenderTexture> depthTextureCache_Front = new List<RenderTexture>();
        public List<RenderTexture> depthTextureCache_Back = new List<RenderTexture>();
        public List<RenderTexture> depthTextureCache_Other = new List<RenderTexture>();
        public List<DepthQuality_GlassObject> depthTextureCache_Quality_Front = new List<DepthQuality_GlassObject>();
        public List<DepthQuality_GlassObject> depthTextureCache_Quality_Back = new List<DepthQuality_GlassObject>();
        public List<DepthQuality_GlassObject> depthTextureCache_Quality_Other = new List<DepthQuality_GlassObject>();
        public Dictionary<int, List<int>> ignoredDepthFront = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> ignoredDepthBack = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> ignoredDepthOther = new Dictionary<int, List<int>>();
        //  QUALITY
        public static bool default_renderDepthSeperately = true;
        public static float default_depthUpdateRate = 144f;
        public static float default_frontUpdateRate = default_depthUpdateRate;
        public static float default_backUpdateRate = default_depthUpdateRate;
        public static float default_otherUpdateRate = default_depthUpdateRate;
        public static float default_camHighUpdateRate = 60f;
        public static float default_camLowUpdateRate = 1f;
        public static int default_depthWait = 0;
        //  BALANCED
        public static bool default_med_renderDepthSeperately = true;
        public static float default_med_depthUpdateRate = 60f;
        public static float default_med_frontUpdateRate = default_med_depthUpdateRate;
        public static float default_med_backUpdateRate = default_med_depthUpdateRate;
        public static float default_med_otherUpdateRate = default_med_depthUpdateRate;
        public static float default_med_camHighUpdateRate = 40f;
        public static float default_med_camLowUpdateRate = 1f;
        public static int default_med_depthWait = 0;
        //  PERFORMANCE
        public static bool default_low_renderDepthSeperately = true;
        public static float default_low_depthUpdateRate = 60f;
        public static float default_low_frontUpdateRate = default_low_depthUpdateRate;
        public static float default_low_backUpdateRate = default_low_depthUpdateRate;
        public static float default_low_otherUpdateRate = default_low_depthUpdateRate;
        public static float default_low_camHighUpdateRate = 30f;
        public static float default_low_camLowUpdateRate = 1f;
        public static int default_low_depthWait = 1;
        //
        public bool initialised = false;
        public string shaderBackName = "Depth/WorldNormals_BackFace";//"Depth/BackFace";
        public string shaderFrontName = "Depth/WorldNormals_FrontFace";//"Depth/FrontFace";
        public static GlassManager _instance = null;
        public static string managerObjectPath = "Assets/Fantastic Glass/Prefabs/GlassManager";
        public bool showDebugInfo = false;
        public bool matchByMaterial = false;
        public bool matchByGlassName = true;
        public bool synchroniseGlass = true;
        public bool disableLayerWarnings = false;
        public static float min_updateRates = 1f;
        public static float max_updateRates = 1000f;
        //  SHADER FEATURES
        public bool require_distortion = true;
        public bool require_doubleDepth = true;
        public bool require_extinction = true;
        public bool require_aberration = true;
        public bool require_fog = true;
        //  Files & Paths
        public static string default_packageName = "Fantastic Glass";
        public static string default_xml_Pathname = "XML";
        public static string default_materials_Pathname = "Materials";
        public static string default_settings_Filename = "glass_system_settings";
        public static string default_presetList_Filename = "glass_preset_list";
        public string packagePath = "";
        public string xmlPath = "";
        public string materialsPath = "";
        public string settingsPath = "";
        public string presetListPath = "";
        //  GUI Variables
        public bool showCameras = false;
        public bool showMainCamera = false;
        public bool showDepth = false;
        public bool showDepthQuality = false;
        public bool showDepthTextures = false;
        public bool showDepthNormalTechniques = false;
        public bool showGlass = false;
        public bool showGlassList = false;
        public bool showLayers = false;
        public bool showLayersAdvanced = false;
        public bool showRendering = false;
        public bool showLayers_Depth = false;
        public bool showLayersAdvanced_Depth = false;
        public bool showRenderOrder = false;
        public bool showRenderOrderList = false;
        public bool showAboutRenderOrder = false;
        public bool showPerformance = false;
        public bool showQuality = false;
        public bool show_materials = false;
        public bool showShaders = false;
        public bool showGlassUsingMaterial = false;
        public bool showSynchroniseSettings = false;
        public bool showAdvancedOptions = false;
        public bool showAdvancedInfo = false;
        public bool showGUISettings = false;
        public bool show_info_namesAndPaths = false;
        public bool showSection_Debug = false;
        public bool show_list_activeMaterials = false;
        public bool show_list_frontLayers = false;
        public bool show_list_backLayers = false;
        public bool show_list_otherLayers = false;
        public Vector2 scrollGlassList = new Vector2();
        public Vector2 scrollRenderOrderList = new Vector2();
        public Vector2 scrollActiveMaterialList = new Vector2();
        public Vector2 scroll_frontLayers = new Vector2();
        public Vector2 scroll_backLayers = new Vector2();
        public Vector2 scroll_otherLayers = new Vector2();
        //  DEBUGGING DEPTH IN RELEASE MODE:
        //public bool debugDepthtexture = false;
        public int setupVersion = -1;
        public static int intendedSetupVersion = Glass.versionInt;

        #endregion

        #region SINGLETON

        public static GlassManager Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }
                _instance = FindObjectOfType<GlassManager>();
                if (_instance != null)
                {
                    return _instance;
                }
                if (_instance != null)
                {
                    return _instance;
                }
                _instance = new GameObject("Glass Manager").AddComponent<GlassManager>();
                _instance.Initialise();
                return _instance;
            }
        }

        #endregion

        #region Start

        void Start()
        {
            Initialise();
            //
            StartCoroutine(Update_DepthBack_CO());
            StartCoroutine(Update_DepthFront_CO());
            StartCoroutine(Update_DepthOther_CO());
            StartCoroutine(Update_DepthAll_CO());
            //
            //	DEPTH TECHNIQUE 1
            StartCoroutine(Update_CamerasHighPriority_CO());
            StartCoroutine(Update_CamerasLowPriority_CO());
        }

        #endregion

        #region Init

        /// <summary>
        /// Used to determine whether this Manager has been put through
        /// all setup required for this version of Fantastic Glass.
        /// e.g. new layer names
        /// </summary>
        /// <returns></returns>
        public bool SetupVersionIsLow()
        {
            return setupVersion < intendedSetupVersion;
        }

        public bool DepthCamerasReady()
        {
            if (camBack == null)
                return false;
            if (camFront == null)
                return false;
            if (camOther == null)
                return false;
            return true;
        }

        /// <summary>
        ///	Returns true only if GlassManager is setup and ready for updates and coroutines.
        /// </summary>
        public bool Initialised()
        {
            if (initialised)
            {
                if (SetupVersionIsLow())
                {
                    Debug.Log("Updating GlassManager to version " + Glass.versionStringFormatted);
                    return false;
                }
                if (!DepthCamerasReady())
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public void Initialise()
        {
            if (!Application.isPlaying)
                return;

            if (Initialised())
                return;

            InitPaths();

            //UpdateGlassMaterialRenderOrder();

            UpdateLayerNames();
            UpdateLayerMasks();

            FindGlass();
            UpdateActiveMaterials();
            UpdateShaders();
            FindMainCamera();

            //  Should be loaded before using any settings e.g. Adding depth cameras
            LoadManager();

            AddDepthCam_Back();
            AddDepthCam_Front();
            AddDepthCam_Other();

            UpdateDepthTechnique(true);
            UpdateNormalTechnique(true);
            UpdateFrontDepthTechnique(true, false);

            UpdateGlassShader();

            UpdateRequiredShaderFeatures();

            //LinkDepthTextures();

            //UpdateDepthTextureSelector();
            UpdateDepthTextureSelector_All();

            GlassRenderOrderManager.Instance.RefreshGlassTypes();

            GlassRenderOrderManager.Instance.UpdateGlassTypes();
            GlassRenderOrderManager.Instance.RenderOrderFromOrder();
            GlassRenderOrderManager.Instance.SortByRenderOrder();
            GlassRenderOrderManager.Instance.UpdateMaterials();

            if (Application.isPlaying)
                initialised = true;

            //  At this point we should be able to presume all setup required is complete.
            setupVersion = intendedSetupVersion;
        }

        void UpdateGlassShader()
        {
            if (glassShader == null)
                glassShader = Shader.Find(Glass.shaderPath);
        }

        /*
        public void LinkDepthTextures()
        {
            foreach (Glass g in glass)
            {
                camBack.LinkGlass(g);
                camFront.LinkGlass(g);
            }
        }
        */

        /// <summary>
        /// Initialises all paths required for locating assets and files.
        /// </summary>
        public void InitPaths()
        {
            if (!packagePath.Contains(Application.dataPath))
            {
                packagePath = Application.dataPath + "/" + GlassManager.default_packageName + "/";
                xmlPath = packagePath + GlassManager.default_xml_Pathname + "/";
                presetListPath = xmlPath + GlassManager.default_presetList_Filename + ".xml";
                settingsPath = xmlPath + GlassManager.default_settings_Filename + ".xml";
                materialsPath = packagePath + GlassManager.default_materials_Pathname + "/";
#if UNITY_EDITOR
                materialsPath = FileUtil.GetProjectRelativePath(materialsPath);
#endif
            }
        }

        public GlassSystemSettings LoadSystemSettings()
        {
            settings = GlassSystemSettings.LoadFromXML(settingsPath);
            return settings;
        }

        /// <summary>
        /// Finds Glass in the scene and re-orders where necessary.
        /// </summary>
        public void FindGlass()
        {
            if (showDebugInfo)
            {
                Debug.Log("Updating Glass List");
            }
            //
            List<Glass> originalGlassList = new List<Glass>(glass);
            //
            foreach (Glass foundGlass in FindObjectsOfType<Glass>())
            {
                foundGlass.Initialise();
                if (!glass.Contains(foundGlass))
                {
                    glass.Add(foundGlass);
                }
            }
            //
            while (glass.Contains(null))
            {
                glass.Remove(null);
            }
            //
            bool differenceInGlassList = false;
            foreach (Glass g in glass)
            {
                if (!originalGlassList.Contains(g))
                {
                    differenceInGlassList = true;
                    break;
                }
            }
            if (!differenceInGlassList)
            {
                foreach (Glass g in originalGlassList)
                {
                    if (!glass.Contains(g))
                    {
                        differenceInGlassList = true;
                        break;
                    }
                }
            }
            if (differenceInGlassList)
            {
                GlassObjectsInSceneChanged();
            }
            UpdateRequiredShaderFeatures();
        }

        /// <summary>
        /// Called when any Glass objects in the scene have changed.
        /// </summary>
        void GlassObjectsInSceneChanged()
        {
            UpdateDepthTextureSelector_All();
        }

        public void UpdateShaders()
        {
            if ((depthShaderBack == null) || (depthShaderFront == null))
            {
                LoadDefaultShaders();
            }
        }

        public void LoadDefaultShaders()
        {
            depthShaderBack = Shader.Find(shaderBackName);
            depthShaderFront = Shader.Find(shaderFrontName);
        }

        #endregion

        #region Save / Load Manager

        /// <summary>
        /// If required, loads manager settings from a designated Glass object or accumulates representative settings from all Glass in scene.
        /// </summary>
        public void LoadManager()
        {
            switch (managerSettingsType)
            {
                case GlassManagerSettingsType.LoadFromManager:
                    {
                        //  no need to do anything here, settings should be stored within the object instance
                        break;
                    }
                case GlassManagerSettingsType.LoadFromGlass:
                    {
                        FindGlass();

                        Glass_GlassManager_Settings chosenLoadedSettings = null;
                        chosenLoadedSettings = Glass_GlassManager_Settings.Load(xmlPath, chosenGlassSettingsSource);
                        if (chosenLoadedSettings != null)
                        {
                            ParseSettings(chosenLoadedSettings);
                            return;
                        }

                        List<Glass_GlassManager_Settings> loadedSettings = new List<Glass_GlassManager_Settings>();
                        foreach (Glass glassObject in glass)
                        {
                            //  3.  load settings from each glass object's associated glass manager setting
                            Glass_GlassManager_Settings loadedManagerSettings = Glass_GlassManager_Settings.Load(xmlPath, glassObject);

                            if (loadedManagerSettings != null)
                            {
                                if (!loadedSettings.Contains(loadedManagerSettings))
                                {
                                    loadedSettings.Add(loadedManagerSettings);
                                }
                            }
                        }

                        if (loadedSettings.Count == 0)
                        {
                            return;
                        }

                        if (loadedSettings.Count == 1)
                        {
                            ParseSettings(loadedSettings[0]);
                        }

                        ParseSettings(loadedSettings);

                        break;
                    }
            }
            UpdateDepthTextureSelector_All();
        }

        /// <summary>
        /// Loads GlassManager settings from a single source.
        /// </summary>
        /// <param name="_settings">_settings.</param>
        public void ParseSettings(Glass_GlassManager_Settings _settings)
        {
            depthTechnique = _settings.depthTechnique;
            normalTechnique = _settings.normalTechnique;
            frontDepthTechnique = _settings.frontDepthTechnique;

            depthTextureAA = _settings.depthTextureAA;
            depthTextureAniso = _settings.depthTextureAniso;
            depthTextureFilterMode = _settings.depthTextureFilterMode;
            depthTextureClearMode = _settings.depthTextureClearMode;

            enable54Workaround = _settings.enable54Workaround;

            FinishedParsingSettings();
        }

        /// <summary>
        /// Loads GlassManager settings by accumulating the best options from a list.
        /// </summary>
        /// <param name="settingsList">Settings list.</param>
        public void ParseSettings(List<Glass_GlassManager_Settings> settingsList)
        {
            Dictionary<GlassDepthTechnique, float> depthTechnique_counter = new Dictionary<GlassDepthTechnique, float>();
            Dictionary<GlassNormalTechnique, float> normalTechnique_counter = new Dictionary<GlassNormalTechnique, float>();
            Dictionary<GlassFrontDepthTechnique, float> frontDepthTechnique_counter = new Dictionary<GlassFrontDepthTechnique, float>();
            Dictionary<int, float> aa_counter = new Dictionary<int, float>();
            Dictionary<int, float> aniso_counter = new Dictionary<int, float>();
            Dictionary<FilterMode, float> filterMode_counter = new Dictionary<FilterMode, float>();
            Dictionary<CameraClearFlags, float> clearMode_counter = new Dictionary<CameraClearFlags, float>();

            int latestEditTime = 0;
            Glass_GlassManager_Settings lastEditedSetting = null;
            foreach (Glass_GlassManager_Settings _settings in settingsList)
            {
                if (_settings.lastEdited >= latestEditTime)
                {
                    lastEditedSetting = _settings;
                    latestEditTime = _settings.lastEdited;
                }
            }

            foreach (Glass_GlassManager_Settings _settings in settingsList)
            {
                float settingValue = (_settings == lastEditedSetting) ? 1.5f : 1f;  //  last edited gets a 0.5f boost so as not to infringe on genuinely more popular choices
                depthTechnique_counter[_settings.depthTechnique] = depthTechnique_counter.ContainsKey(_settings.depthTechnique) ? depthTechnique_counter[_settings.depthTechnique] + settingValue : settingValue;
                normalTechnique_counter[_settings.normalTechnique] = normalTechnique_counter.ContainsKey(_settings.normalTechnique) ? normalTechnique_counter[_settings.normalTechnique] + settingValue : settingValue;
                frontDepthTechnique_counter[_settings.frontDepthTechnique] = frontDepthTechnique_counter.ContainsKey(_settings.frontDepthTechnique) ? frontDepthTechnique_counter[_settings.frontDepthTechnique] + settingValue : settingValue;
                aa_counter[_settings.depthTextureAA] = aa_counter.ContainsKey(_settings.depthTextureAA) ? aa_counter[_settings.depthTextureAA] + settingValue : settingValue;
                aniso_counter[_settings.depthTextureAniso] = aniso_counter.ContainsKey(_settings.depthTextureAniso) ? aniso_counter[_settings.depthTextureAniso] + settingValue : settingValue;
                filterMode_counter[_settings.depthTextureFilterMode] = filterMode_counter.ContainsKey(_settings.depthTextureFilterMode) ? filterMode_counter[_settings.depthTextureFilterMode] + settingValue : settingValue;
                clearMode_counter[_settings.depthTextureClearMode] = clearMode_counter.ContainsKey(_settings.depthTextureClearMode) ? clearMode_counter[_settings.depthTextureClearMode] + settingValue : settingValue;
            }

            //  Depth
            float counterCheck = 0f;
            foreach (GlassDepthTechnique key in depthTechnique_counter.Keys)
            {
                if (depthTechnique_counter[key] > counterCheck)
                {
                    depthTechnique = key;
                    counterCheck = depthTechnique_counter[key];
                }
            }
            //  Normal
            counterCheck = 0f;
            foreach (GlassNormalTechnique key in normalTechnique_counter.Keys)
            {
                if (normalTechnique_counter[key] > counterCheck)
                {
                    normalTechnique = key;
                    counterCheck = normalTechnique_counter[key];
                }
            }
            //  Front Depth
            counterCheck = 0f;
            foreach (GlassFrontDepthTechnique key in frontDepthTechnique_counter.Keys)
            {
                if (frontDepthTechnique_counter[key] > counterCheck)
                {
                    frontDepthTechnique = key;
                    counterCheck = frontDepthTechnique_counter[key];
                }
            }
            //  AA
            counterCheck = 0f;
            foreach (int key in aa_counter.Keys)
            {
                if (aa_counter[key] > counterCheck)
                {
                    depthTextureAA = key;
                    counterCheck = aa_counter[key];
                }
            }
            //  Aniso
            counterCheck = 0f;
            foreach (int key in aniso_counter.Keys)
            {
                if (aniso_counter[key] > counterCheck)
                {
                    depthTextureAniso = key;
                    counterCheck = aniso_counter[key];
                }
            }
            //  Filter Mode
            counterCheck = 0f;
            foreach (FilterMode key in filterMode_counter.Keys)
            {
                if (filterMode_counter[key] > counterCheck)
                {
                    depthTextureFilterMode = key;
                    counterCheck = filterMode_counter[key];
                }
            }
            //  Clear Mode
            counterCheck = 0f;
            foreach (CameraClearFlags key in clearMode_counter.Keys)
            {
                if (clearMode_counter[key] > counterCheck)
                {
                    depthTextureClearMode = key;
                    counterCheck = clearMode_counter[key];
                }
            }

            FinishedParsingSettings();
        }

        /// <summary>
        /// Call this once you have finished loading GlassManager settings from Glass objects
        /// </summary>
        void FinishedParsingSettings()
        {
            UpdateDepthTechnique();
            UpdateNormalTechnique();
            UpdateFrontDepthTechnique();

            UpdateCameraSettings();
        }

        #endregion

        #region Layers

        public bool LayersExist()
        {
            if (disableLayerWarnings)
                return true;

            if (!AllLayersDefined())
            {
                if (!UpdateLayerMasks())
                    return false;
            }

            if (!AllLayersDefined())
            {
                UpdateLayerNames();
                UpdateLayerMasks();
            }

            if (!AllLayersDefined())
            {
                return false;
            }

            return true;
        }

        bool AllLayersDefined()
        {
            if (backLayerName != default_backLayerName)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning("Back layer '" + backLayerName + "' does not match default layer name '" + default_backLayerName + "'. Delete current GlassManager to fix this.");
                }
                else {
                    Debug.Log("Back layer '" + backLayerName + "' does not match default layer name '" + default_backLayerName + "'. Delete current GlassManager to fix this.");
                }
            }

            if ((frontLayerMask == -1) || (backLayerMask == -1) || (otherLayerMask == -1))
            {
                if (showDebugInfo)
                    Debug.Log("All Layers Not Defined: some masks are not yet defined.");
                return false;
            }

            if ((frontLayerMask == backLayerMask) || (frontLayerMask == otherLayerMask) || (backLayerMask == otherLayerMask))
            {
                if (showDebugInfo)
                    Debug.Log("All Layers Not Defined: some masks have the same value.");
                return false;
            }

            if (LayerMask.NameToLayer(frontLayerName) == -1)
            {
                Debug.Log("Front Layer Not Found: '" + frontLayerName + "'.");
                return false;
            }
            if (LayerMask.NameToLayer(backLayerName) == -1)
            {
                Debug.Log("Back Layer Not Found: '" + backLayerName + "'.");
                return false;
            }

            if (frontLayerName != default_frontLayerName)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning("Front layer '" + frontLayerName + "' does not match default layer name '" + default_frontLayerName + "'. Delete current GlassManager to fix this.");
                }
                else {
                    Debug.Log("Front layer '" + frontLayerName + "' does not match default layer name '" + default_frontLayerName + "'. Delete current GlassManager to fix this.");
                }
            }

            return true;
        }

        bool AllLayersMatchNames()
        {
            if (LayerMask.GetMask(frontLayerNames.ToArray()) != frontLayerMask)
                return false;
            if (LayerMask.GetMask(backLayerNames.ToArray()) != backLayerMask)
                return false;
            if (LayerMask.GetMask(otherLayerNames.ToArray()) != otherLayerMask)
                return false;
            return true;
        }

        public void UpdateLayerNames()
        {
            if (showDebugInfo)
                Debug.Log("Updating Layer Names...");

            UpdateFrontLayerNames();
            UpdateBackLayerNames();
            UpdateOtherLayerNames();

            if (showDebugInfo)
                Debug.Log("Finished Updating Layer Names.");
        }

        public bool UpdateLayerMasks()
        {
            if (showDebugInfo)
                Debug.Log("Updating Layer Masks...");
            //	DEPTH TECHNIQUE 1.1
            if (!UpdateFrontLayerMask())
            {
                if (showDebugInfo)
                    Debug.Log("Error updating Front Layer Mask. Check Glass layers exists.");
                return false;
            }
            if (!UpdateBackLayerMask())
            {
                if (showDebugInfo)
                    Debug.Log("Error updating Back Layer Mask. Check Glass layers exists.");
                return false;
            }
            if (!UpdateOtherLayerMask())
            {
                if (showDebugInfo)
                    Debug.Log("Error updating Other Layer Mask. Check Glass layers exists.");
                return false;
            }
            if (showDebugInfo)
                Debug.Log("Finished Updating Layer Masks.");
            return true;
        }

        //	Layer Names

        public void UpdateFrontLayerNames()
        {
            if (frontLayerNames.Count == 0)
                frontLayerNames.Add(frontLayerName);
        }

        public void UpdateBackLayerNames()
        {
            if (backLayerNames.Count == 0)
                backLayerNames.Add(backLayerName);
        }

        public void UpdateOtherLayerNames()
        {
            if (SetupVersionIsLow())
            {
                otherLayerNames.Clear();
            }
            if (otherLayerNames.Count == 0)
            {
                for (int i = 0; i <= 31; i++)
                {
                    string foundLayerName = LayerMask.LayerToName(i);
                    if (foundLayerName.Length > 0)
                    {
                        if (!frontLayerNames.Contains(foundLayerName))
                        {
                            //  N.B. we are ignoring back layer names to support High DepthQuality
                            //      this presumes we set glass objects to Front when we don't want them in Other
                            //if (!backLayerNames.Contains(foundLayerName))
                            //{
                            otherLayerNames.Add(foundLayerName);
                            //}
                        }
                    }
                }
            }
        }

        //	Layer Masks

        public bool UpdateFrontLayerMask()
        {
            CleanLayerNames(ref frontLayerNames);
            frontLayerMask = LayerMask.GetMask(frontLayerNames.ToArray());
            return (frontLayerMask != -1);
        }

        public bool UpdateBackLayerMask()
        {
            CleanLayerNames(ref backLayerNames);
            backLayerMask = LayerMask.GetMask(backLayerNames.ToArray());
            return (backLayerMask != -1);
        }

        public bool UpdateOtherLayerMask()
        {
            CleanLayerNames(ref otherLayerNames);
            otherLayerMask = LayerMask.GetMask(otherLayerNames.ToArray());

            //  fix for Unity 5.2 wherein the default layer isn't included in result of GetMask:
            if (otherLayerNames.Contains("Default"))
            {
                int defaultLayer = 1 << 0;
                if ((otherLayerMask & defaultLayer) != defaultLayer)
                {
                    otherLayerMask = otherLayerMask | defaultLayer;
                }
            }

            return (otherLayerMask != -1);
        }

        public void CleanLayerNames(ref List<string> namesList)
        {
            for (int i = namesList.Count - 1; i >= 0; i--)
            {
                string layerNameTest = namesList[i];
                if (LayerMask.NameToLayer(layerNameTest) == -1)
                    namesList.RemoveAt(i);
            }
        }

        #endregion

        #region Shared / Matching Glass

        /// <summary>
        ///	Returns a list of Glass objects that match the one provided.
        /// </summary>
        /// <returns>The glass others.</returns>
        /// <param name="_glass">_glass.</param>
        public List<Glass> SharedGlassOthers(Glass _glass)
        {
            List<Glass> others = new List<Glass>();
            foreach (Glass otherGlass in glass)
            {
                if (GlassMatch(_glass, otherGlass))
                {
                    if (!others.Contains(otherGlass))
                        others.Add(otherGlass);
                }
            }
            return others;
        }

        /// <summary>
        /// Returns true if any other Glass object matches (not exactly) the one provided.
        /// </summary>
        /// <returns><c>true</c>, if the Glass has any (non-exact) matches, <c>false</c> otherwise.</returns>
        /// <param name="_glass">_glass.</param>
        public bool GlassIsShared(Glass _glass)
        {
            foreach (Glass otherGlass in glass)
            {
                if (otherGlass == _glass)
                    continue;

                if (GlassMatch(_glass, otherGlass))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if glass1 matches glass2. Empty and null names wil never match. Exact matches are ignored by default.
        /// </summary>
        /// <param name="otherGlass">Other glass.</param>
        public bool GlassMatch(Glass glass1, Glass glass2, bool ignoreExactObjectMatch = true)
        {
            return glass1.Matches(glass2, ignoreExactObjectMatch);
        }

        #endregion

        #region Material Values

        /// <summary>
        /// Tells the Glass Manager that a Glass object has changed.
        /// The Glass Manager will then likely synchronise the changed Glass's settings with others that match.
        /// </summary>
        /// <param name="glassModified"></param>
        public void GlassModified(Glass glassModified)
        {
            if (!synchroniseGlass)
                return;
            //
            if (glassModified == null)
                return;
            //
            FindGlass();
            //
            foreach (Glass otherGlass in glass)
            {
                if (otherGlass == null)
                    continue;
                if (otherGlass == glassModified)
                    continue;
                otherGlass.MaterialValuesChanged(glassModified);
            }

            try
            {
                GlassRenderOrderManager.Instance.UpdateMaterials();
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred trying to updare material values in RenderOrderManager from Glass Instance. Please try again later. Exception:" + e.Message);
            }

            UpdateRequiredShaderFeatures();
        }

        /// <summary>
        /// Enables/disables segments of Glass shader code when features required/not required.
        /// </summary>
        void UpdateRequiredShaderFeatures()
        {
            if (showDebugInfo)
            {
                Debug.Log("GlassManager: Updating Required Shader Feeatures...");
            }

            //  Aberration
            require_aberration = false;
            foreach (Glass g in glass)
            {
                if (g.enableAberration_back)
                {
                    require_aberration = true;
                    break;
                }
                if (g.enableAberration_front)
                {
                    require_aberration = true;
                    break;
                }
                if (g.enableAberration_both)
                {
                    require_aberration = true;
                    break;
                }
            }
            EnableKeyword(require_aberration, "ABERRATION_REQUIRED", "ABERRATION_NOT_REQUIRED", "Aberration");

            //  Extinction
            require_extinction = false;
            foreach (Glass g in glass)
            {
                if (g.enableExtinction_back)
                {
                    require_extinction = true;
                    break;
                }
                if (g.enableExtinction_front)
                {
                    require_extinction = true;
                    break;
                }
                if (g.enableExtinction_both)
                {
                    require_extinction = true;
                    break;
                }
            }
            EnableKeyword(require_extinction, "EXTINCTION_REQUIRED", "EXTINCTION_NOT_REQUIRED", "Extinction");

            //  Distortion
            require_distortion = false;
            foreach (Glass g in glass)
            {
                if (g.enableDistortion_back)
                {
                    require_distortion = true;
                    break;
                }
                if (g.enableDistortion_front)
                {
                    require_distortion = true;
                    break;
                }
            }
            EnableKeyword(require_distortion, "DISTORTION_REQUIRED", "DISTORTION_NOT_REQUIRED", "Distortion");

            //  Double Depth
            require_doubleDepth = false;
            foreach (Glass g in glass)
            {
                if (g.enableDoubleDepth_back)
                {
                    require_doubleDepth = true;
                    break;
                }
                if (g.enableDoubleDepth_front)
                {
                    require_doubleDepth = true;
                    break;
                }
            }
            EnableKeyword(require_doubleDepth, "DOUBLE_DEPTH_REQUIRED", "DOUBLE_DEPTH_NOT_REQUIRED", "Double Depth");

            //  Fog
            require_fog = false;
            foreach (Glass g in glass)
            {
                if (g.enableFog_back)
                {
                    require_fog = true;
                    break;
                }
                if (g.enableFog_front)
                {
                    require_fog = true;
                    break;
                }
                if (g.enableFog_both)
                {
                    require_fog = true;
                    break;
                }
            }
            EnableKeyword(require_fog, "FOG_REQUIRED", "FOG_NOT_REQUIRED", "Fog");

            if (showDebugInfo)
            {
                Debug.Log("GlassManager: Updating Required Shader Feeatures...DONE");
            }
        }

        /// <summary>
        /// Enables/disables a shader keyword, while disabling/enabling its 'OFF' counterpart.
        /// </summary>
        /// <param name="enableFeature">If set to <c>true</c> enable feature.</param>
        /// <param name="string_enabled">String_enabled.</param>
        /// <param name="string_disabled">String_disabled.</param>
        /// <param name="featureName">Feature name.</param>
        private void EnableKeyword(bool enableFeature, string string_enabled, string string_disabled, string featureName)
        {
            Shader.EnableKeyword(enableFeature ? string_enabled : string_disabled);
            Shader.DisableKeyword(enableFeature ? string_disabled : string_enabled);

            if (showDebugInfo)
            {
                Debug.Log("GlassManager: " + (enableFeature ? "Enabled" : "Disabled") + featureName);
            }
        }

        #endregion

        #region Physics Values

        public void PhysicsValueChanged(Glass glassChanged)
        {
            if (!synchroniseGlass)
                return;
            //
            if (glassChanged == null)
                return;
            //
            foreach (Glass otherGlass in glass)
            {
                if (otherGlass == null)
                    continue;
                if (otherGlass == glassChanged)
                    continue;
                otherGlass.PhysicsValuesChanged(glassChanged);
            }
        }

        #endregion

        #region Materials

        /// <summary>
        /// Updates the list of active Glass materials in the scene.
        /// </summary>
        public void UpdateActiveMaterials()
        {
            if (showDebugInfo)
                Debug.Log("Updating Active Materials.");
            lock (activeMaterials)
            {
                activeMaterials.Clear();
                activeMaterialsAndObjects.Clear();
                foreach (Glass activeGlass in glass)
                {
                    if (activeGlass == null)
                        continue;
                    if (!activeMaterials.Contains(activeGlass.material_back))
                        activeMaterials.Add(activeGlass.material_back);
                    if (!activeMaterials.Contains(activeGlass.material_front))
                        activeMaterials.Add(activeGlass.material_front);
                }
                //
                foreach (Material activeMaterial in activeMaterials)
                {
                    if (activeMaterial == null)
                        continue;
                    if (activeMaterialsAndObjects.ContainsKey(activeMaterial))
                        continue;
                    activeMaterialsAndObjects.Add(activeMaterial, new List<Glass>());
                    foreach (Glass activeGlass in glass)
                    {
                        if (activeGlass == null)
                            continue;
                        if (activeGlass.material_back == activeMaterial)
                            if (!activeMaterialsAndObjects[activeMaterial].Contains(activeGlass))
                                activeMaterialsAndObjects[activeMaterial].Add(activeGlass);
                        if (activeGlass.material_front == activeMaterial)
                            if (!activeMaterialsAndObjects[activeMaterial].Contains(activeGlass))
                                activeMaterialsAndObjects[activeMaterial].Add(activeGlass);
                    }
                }
            }
        }

        #endregion

        #region Main Camera

        public Camera FindMainCamera()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera != null)
            {
                if (settings != null)
                {
                    if (settings.enableAlwaysSetOptimumCamera)
                    {
                        mainCamera.renderingPath = settings.optimumCamera_renderingPath;
                        mainCamera.allowHDR = settings.optimumCamera_enableHDR;
                    }
                }
            }

            if (settings != null)
            {
                if (settings.enableAlwaysSetOptimumCamera)
                {
                    mainCamera.renderingPath = settings.optimumCamera_renderingPath;
                    mainCamera.allowHDR = settings.optimumCamera_enableHDR;
                }
            }

            if (mainCamera == null)
            {
                if (Camera.allCamerasCount > 0)
                {
                    mainCamera = Camera.allCameras[0];
                    if (settings.enableAlwaysSetOptimumCamera)
                    {
                        mainCamera.renderingPath = settings.optimumCamera_renderingPath;
                        mainCamera.allowHDR = settings.optimumCamera_enableHDR;
                    }
                }
                else {
                    GameObject mainCameraObject = new GameObject("MainCamera");
                    mainCamera = mainCameraObject.AddComponent<Camera>();
                    if (settings.enableAlwaysSetOptimumCamera)
                    {
                        mainCamera.renderingPath = settings.optimumCamera_renderingPath;
                        mainCamera.allowHDR = settings.optimumCamera_enableHDR;
                    }
                    Debug.LogWarning("GlassManager: No main camera was found so one was created.");
                }
            }

            return mainCamera;
        }

        #endregion

        #region Depth Cameras

        public void DepthQualityChanged_Front()
        {
            UpdateDepthTextureSelector_Front();
        }

        public void DepthQualityChanged_Back()
        {
            UpdateDepthTextureSelector_Back();
        }

        public void DepthQualityChanged_Other()
        {
            UpdateDepthTextureSelector_Other();
        }

        public void UpdateDepthTechnique(bool forceUpdate = false, bool updateTextures = true)
        {
            if (camFront != null)
                camFront.SetDepthTechnique(depthTechnique, forceUpdate);
            if (camBack != null)
                camBack.SetDepthTechnique(depthTechnique, forceUpdate);
            if (camOther != null)
                camOther.SetDepthTechnique(depthTechnique, forceUpdate);

            if (updateTextures)
            {
                UpdateDepthTextureSelector_All();
            }

            normalTechnique = GlassDepthCamera.NormalTechFromDepth(normalTechnique, depthTechnique);
            frontDepthTechnique = GlassDepthCamera.FrontDepthTechFromDepth(frontDepthTechnique, depthTechnique, enable54Workaround);
        }

        public void UpdateNormalTechnique(bool forceUpdate = false, bool updateTextures = true)
        {
            if (camFront != null)
                camFront.SetNormalTechnique(normalTechnique, forceUpdate);
            if (camBack != null)
                camBack.SetNormalTechnique(normalTechnique, forceUpdate);
            if (camOther != null)
                camOther.SetNormalTechnique(normalTechnique, forceUpdate);

            if (updateTextures)
            {
                UpdateDepthTextureSelector_All();
            }

            depthTechnique = GlassDepthCamera.DepthTechFromNormal(depthTechnique, normalTechnique);
            frontDepthTechnique = GlassDepthCamera.FrontDepthTechFromNormal(frontDepthTechnique, normalTechnique, enable54Workaround);
        }

        public void UpdateFrontDepthTechnique(bool forceUpdate = false, bool updateTextures = true)
        {
            if (camFront != null)
                camFront.SetFrontDepthTechnique(frontDepthTechnique, forceUpdate);
            if (camBack != null)
                camBack.SetFrontDepthTechnique(frontDepthTechnique, forceUpdate);
            if (camOther != null)
                camOther.SetFrontDepthTechnique(frontDepthTechnique, forceUpdate);

            if (updateTextures)
            {
                UpdateDepthTextureSelector_All();
            }

            GlassDepthCamera.DepthNormalTechFromFrontDepth(frontDepthTechnique, ref depthTechnique, ref normalTechnique, enable54Workaround);
        }

        //	DEPTH TECHNIQUE 1.1
        public void AddDepthCam_Front()
        {
            AddDepthCam(ref camFront, "glass_depthCam_Front", frontLayerMask, LayerMask.NameToLayer(frontLayerName), depthTextureName_Front, depthShaderFront);
            //  DEBUGGING DEPTH IN RELEASE BUILDS
            //camFront.debugDepthObject.transform.localPosition = camFront.debugDepthObject.transform.localPosition + new Vector3(-0.5f, 0f, 0f);
        }

        public void AddDepthCam_Back()
        {
            AddDepthCam(ref camBack, "glass_depthCam_Back", backLayerMask, LayerMask.NameToLayer(backLayerName), depthTextureName_Back, depthShaderBack);
            //  DEBUGGING DEPTH IN RELEASE BUILDS
            //camBack.debugDepthObject.transform.localPosition = camBack.debugDepthObject.transform.localPosition + new Vector3(0f, 0f, 0f);
        }

        public void AddDepthCam_Other()
        {
            AddDepthCam(ref camOther, "glass_depthCam_Other", otherLayerMask, LayerMask.NameToLayer(frontLayerName), depthTextureName_Other, depthShaderFront);
            //  DEBUGGING DEPTH IN RELEASE BUILDS
            //camOther.debugDepthObject.transform.localPosition = camOther.debugDepthObject.transform.localPosition + new Vector3(0.5f, 0f, 0f);
        }

        public void AddDepthCam(ref GlassDepthCamera cam, string _name, LayerMask _cameralayer, int _glasslayer, string _DepthTexture, Shader _depthShader)
        {
            if (cam == null)
            {
                FindMainCamera();

                GameObject camObj = GameObject.Find(_name);
                if (camObj == null)
                {
                    camObj = new GameObject(_name);
                    camObj.transform.parent = mainCamera.transform;
                    camObj.transform.localPosition = Vector3.zero;
                    camObj.transform.localRotation = Quaternion.identity;
                }

                cam = camObj.GetComponentInChildren<GlassDepthCamera>();
                if (cam == null)
                {
                    cam = camObj.AddComponent<GlassDepthCamera>();
                }
            }
            cam.Initialise(_cameralayer, _glasslayer, _DepthTexture, _depthShader, this);
        }


        IEnumerator Update_DepthAll_CO()
        {
            while (true)
            {
                if (initialised && (!renderDepthsSeperately))
                {
                    depthWaitDelta++;
                    if (depthWaitDelta >= depthWait)
                    {
                        if (frontDepthTechnique == GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF)
                        {
                            RenderDepth(camFront, backLayerSingle, frontLayerSingle, depthTextureCache_Front, depthTextureCache_Quality_Front, depthQuality_Front, true, null);
                        }
                        RenderDepth(camBack, frontLayerSingle, backLayerSingle, depthTextureCache_Back, depthTextureCache_Quality_Back, depthQuality_Back, true, null);
                        RenderDepth(camOther, backLayerSingle, frontLayerSingle, depthTextureCache_Other, depthTextureCache_Quality_Other, depthQuality_Other, true, ignoredDepthOther);

                        depthWaitDelta = -1;
                    }
                }
                //
                yield return new WaitForSeconds(1f / depthUpdateRate);
            }
        }

        //	DEPTH TECHNIQUE 1.1
        private IEnumerator Update_DepthFront_CO()
        {
            while (true)
            {
                if (!initialised)
                {
                    //  Not ready
                }
                else if (!renderDepthsSeperately)
                {
                    //  Depths rendered in single function
                }
                else if (frontDepthTechnique != GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF)
                {
                    //  Do not need to render front depth
                }
                else
                {
                    depthWaitFront++;
                    if (depthWaitFront >= depthWait)
                    {
                        try
                        {
                            RenderDepth(camFront, backLayerSingle, frontLayerSingle, depthTextureCache_Front, depthTextureCache_Quality_Front, depthQuality_Front, true, null);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("GlassManager: Failed to render front depth. ERROR: " + e.Message);
                        }
                        depthWaitFront = -1;
                    }
                }
                //
                yield return new WaitForSeconds(1f / frontUpdateRate);
            }
        }

        private IEnumerator Update_DepthBack_CO()
        {
            while (true)
            {
                if (initialised && renderDepthsSeperately)
                {
                    depthWaitBack++;
                    if (depthWaitBack >= depthWait)
                    {
                        try
                        {
                            RenderDepth(camBack, frontLayerSingle, backLayerSingle, depthTextureCache_Back, depthTextureCache_Quality_Back, depthQuality_Back, true, null);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("GlassManager: Failed to render front depth. ERROR: " + e.Message);
                        }
                        //RenderDepth_Back();
                        depthWaitBack = -1;
                    }
                }
                //
                yield return new WaitForSeconds(1f / backUpdateRate);
            }
        }

        private IEnumerator Update_DepthOther_CO()
        {
            while (true)
            {
                if (initialised && renderDepthsSeperately)
                {
                    depthWaitOther++;
                    if (depthWaitOther >= depthWait)
                    {
                        RenderDepth(camOther, backLayerSingle, frontLayerSingle, depthTextureCache_Other, depthTextureCache_Quality_Other, depthQuality_Other, true, ignoredDepthOther);
                        //RenderDepth_Other();
                        depthWaitOther = -1;
                    }
                }
                yield return new WaitForSeconds(1f / otherUpdateRate);
            }
        }

        //

        void RenderDepth_Front()
        {
            camFront.RenderDepth();
        }

        void RenderDepth_Back()
        {
            camBack.RenderDepth();
        }


        public void UpdateDepthTextureSelector_All()
        {
            UpdateDepthTextureSelector_Front();
            UpdateDepthTextureSelector_Back();
            UpdateDepthTextureSelector_Other();
        }

        public void UpdateDepthTextureSelector_Front()
        {
            UpdateDepthTextureSelector(depthTextureCache_Front, depthTextureCache_Quality_Front, camFront, depthTextureName_Front, depthQuality_Front, GlassDepthLayer.front, null);
        }

        public void UpdateDepthTextureSelector_Back()
        {
            UpdateDepthTextureSelector(depthTextureCache_Back, depthTextureCache_Quality_Back, camBack, depthTextureName_Back, depthQuality_Back, GlassDepthLayer.back, null);
        }

        public void UpdateDepthTextureSelector_Other()
        {
            UpdateDepthTextureSelector(depthTextureCache_Other, depthTextureCache_Quality_Other, camOther, depthTextureName_Other, depthQuality_Other, GlassDepthLayer.other, ignoredDepthOther);
        }

        /// <summary>
        /// Updates the texture and quality cache for the defined depth layer.
        /// </summary>
        /// <param name="textureCache"></param>
        /// <param name="qualityCache"></param>
        /// <param name="depthCam"></param>
        /// <param name="depthTextureName"></param>
        /// <param name="depthQuality"></param>
        /// <param name="depthLayer"></param>
        void UpdateDepthTextureSelector(List<RenderTexture> textureCache, List<DepthQuality_GlassObject> qualityCache, GlassDepthCamera depthCam, string depthTextureName, DepthQuality_GlassManager depthQuality, GlassDepthLayer depthLayer, Dictionary<int, List<int>> ignoredDepth)
        {
            if (showDebugInfo)
            {
                Debug.Log("GlassManager: Updating Texture Selector '" + depthLayer.ToString() + "'.");
            }

            frontLayerSingle = LayerMask.NameToLayer(frontLayerName);
            backLayerSingle = LayerMask.NameToLayer(backLayerName);

            while (glass.Contains(null))
            {
                glass.Remove(null);
            }

            lock (textureCache)
            {

                foreach (RenderTexture rt in textureCache)
                {
#if UNITY_EDITOR
                    RenderTexture.DestroyImmediate(rt);
#else
                RenderTexture.Destroy(rt);
#endif
                }
                textureCache.Clear();
                qualityCache.Clear();

                switch (depthQuality)
                {
                    case DepthQuality_GlassManager.Simple:

                        if (depthCam != null)
                        {
                            depthCam.SetDefaultTargetTexture();
                            foreach (Glass glassObject in glass)
                            {
                                depthCam.LinkGlass(glassObject);
                            }
                        }
                        break;

                    case DepthQuality_GlassManager.Complex:

                        List<Glass> glassList = new List<Glass>();
                        glassList.AddRange(glass);

                        while (glassList.Count > 0)
                        {
                            RenderTexture rt_quality = GlassDepthCamera.NewRenderTexture(this);
                            int tID_quality = textureCache.Count;
                            textureCache.Add(rt_quality);
                            qualityCache.Add(DepthQuality_GlassObject.Complex);
                            Glass g0 = glassList[0];
                            glassList.Remove(g0);
                            g0.depthTextureID = tID_quality;
                            g0.SetDepthTexture(rt_quality, depthTextureName);
                            for (int i = glassList.Count - 1; i >= 0; i--)
                            {
                                Glass g = glassList[i];
                                if (g.Matches(g0))
                                {
                                    g.depthTextureID = tID_quality;
                                    g.SetDepthTexture(rt_quality, depthTextureName);
                                    glassList.Remove(g);
                                }
                            }
                        }
                        break;

                    case DepthQuality_GlassManager.ObjectDefined:

                        List<Glass> glassList2 = new List<Glass>();
                        glassList2.AddRange(glass);

                        while (glassList2.Count > 0)
                        {
                            Glass g0 = glassList2[0];
                            switch (g0.GetDepthQuality(depthLayer))
                            {
                                case DepthQuality_GlassObject.Simple:

                                    RenderTexture rt_obj_performance = null;
                                    int tID_obj_performance = textureCache.Count;
                                    textureCache.Add(rt_obj_performance);
                                    qualityCache.Add(DepthQuality_GlassObject.Simple);
                                    glassList2.Remove(g0);
                                    g0.depthTextureID = tID_obj_performance;

                                    if (depthCam != null)
                                    {
                                        depthCam.LinkGlass(g0);
                                    }
                                    for (int i = glassList2.Count - 1; i >= 0; i--)
                                    {
                                        Glass g = glassList2[i];
                                        if (g.Matches(g0))
                                        {
                                            g.depthTextureID = tID_obj_performance;
                                            if (depthCam != null)
                                            {
                                                depthCam.LinkGlass(g);
                                            }
                                            glassList2.Remove(g);
                                        }
                                    }
                                    break;
                                case DepthQuality_GlassObject.Complex:

                                    RenderTexture rt_obj_quality = GlassDepthCamera.NewRenderTexture(this);
                                    int tID_obj_Quality = textureCache.Count;
                                    textureCache.Add(rt_obj_quality);
                                    qualityCache.Add(DepthQuality_GlassObject.Complex);
                                    glassList2.Remove(g0);
                                    g0.depthTextureID = tID_obj_Quality;
                                    g0.SetDepthTexture(rt_obj_quality, depthTextureName);

                                    for (int i = glassList2.Count - 1; i >= 0; i--)
                                    {
                                        Glass g = glassList2[i];
                                        if (g.Matches(g0))
                                        {
                                            g.depthTextureID = tID_obj_Quality;
                                            g.SetDepthTexture(rt_obj_quality, depthTextureName);
                                            glassList2.Remove(g);
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }

            }

            //  Update ignored depth dictionary
            if (ignoredDepth != null)
            {
                ignoredDepth.Clear();
                foreach (Glass g in glass)
                {
                    if (g == null)
                        continue;
                    if (!ignoredDepth.ContainsKey(g.depthTextureID))
                    {
                        ignoredDepth.Add(g.depthTextureID, new List<int>());
                        foreach (Glass ignoredG in g.ignoredDepth)
                        {
                            if (ignoredG == null)
                                continue;
                            ignoredDepth[g.depthTextureID].Add(ignoredG.depthTextureID);
                        }
                    }
                }
            }

            if (showDebugInfo)
            {
                Debug.Log("GlassManager: FINISHED Updating Texture Selector '" + depthLayer.ToString() + "'.");
            }
        }

        /// <summary>
        /// Renders all depth textures for the specified layer.
        /// </summary>
        /// <param name="depthCam"></param>
        /// <param name="inactiveGlassLayer"></param>
        /// <param name="activeGlassLayer"></param>
        /// <param name="textureCache"></param>
        /// <param name="qualityCache"></param>
        /// <param name="depthQuality"></param>
        /// <param name="activeOnSimple"></param>
        void RenderDepth(GlassDepthCamera depthCam, int inactiveGlassLayer, int activeGlassLayer, List<RenderTexture> textureCache, List<DepthQuality_GlassObject> qualityCache, DepthQuality_GlassManager depthQuality, bool activeOnSimple, Dictionary<int, List<int>> ignoredDepth)
        {
            switch (depthQuality)
            {
                case DepthQuality_GlassManager.Simple:
                    {
                        if (activeOnSimple)
                        {
                            foreach (Glass g in glass)
                            {
                                g.gameObject.layer = activeGlassLayer;
                            }
                        }
                        /*
                        else
                        {
                            foreach (Glass g in glass)
                            {
                                g.gameObject.layer = inactiveGlassLayer;
                            }
                        }
                        */
                        depthCam.SetDefaultTargetTexture();
                        depthCam.RenderDepth();
                        break;
                    }
                case DepthQuality_GlassManager.Complex:
                    {
                        lock (textureCache)
                        {
                            for (int i = 0; i < textureCache.Count; i++)
                            {
                                if (ignoredDepth == null)
                                {
                                    foreach (Glass g in glass)
                                    {
                                        g.gameObject.layer = (g.depthTextureID == i) ? activeGlassLayer : inactiveGlassLayer;
                                    }
                                }
                                else if (ignoredDepth[i].Count == 0)
                                {
                                    foreach (Glass g in glass)
                                    {
                                        g.gameObject.layer = (g.depthTextureID == i) ? activeGlassLayer : inactiveGlassLayer;
                                    }
                                }
                                else
                                {
                                    foreach (Glass g in glass)
                                    {
                                        if (g.depthTextureID != i)
                                        {
                                            if (ignoredDepth[i].Contains(g.depthTextureID))
                                            {
                                                g.gameObject.layer = activeGlassLayer;
                                            }
                                            else
                                            {
                                                g.gameObject.layer = inactiveGlassLayer;
                                            }
                                        }
                                        else
                                        {
                                            g.gameObject.layer = activeGlassLayer;
                                        }
                                    }
                                }
                                depthCam.RenderDepth(textureCache[i]);
                            }
                        }
                        break;
                    }
                case DepthQuality_GlassManager.ObjectDefined:
                    {
                        lock (textureCache)
                        {
                            bool renderStandard = false;
                            for (int i = 0; i < textureCache.Count; i++)
                            {
                                switch (qualityCache[i])
                                {
                                    case DepthQuality_GlassObject.Complex:
                                        if (ignoredDepth == null)
                                        {
                                            foreach (Glass g in glass)
                                            {
                                                g.gameObject.layer = (g.depthTextureID == i) ? activeGlassLayer : inactiveGlassLayer;
                                            }
                                        }
                                        else if (ignoredDepth[i].Count == 0)
                                        {
                                            foreach (Glass g in glass)
                                            {
                                                g.gameObject.layer = (g.depthTextureID == i) ? activeGlassLayer : inactiveGlassLayer;
                                            }
                                        }
                                        else
                                        {
                                            foreach (Glass g in glass)
                                            {
                                                if (g.depthTextureID != i)
                                                {
                                                    if (ignoredDepth[i].Contains(g.depthTextureID))
                                                    {
                                                        g.gameObject.layer = activeGlassLayer;
                                                    }
                                                    else
                                                    {
                                                        g.gameObject.layer = inactiveGlassLayer;
                                                    }
                                                }
                                                else
                                                {
                                                    g.gameObject.layer = activeGlassLayer;
                                                }
                                            }
                                        }
                                        depthCam.RenderDepth(textureCache[i]);
                                        break;
                                    case DepthQuality_GlassObject.Simple:
                                        renderStandard = true;
                                        break;
                                }
                            }
                            if (renderStandard)
                            {
                                if (activeOnSimple)
                                {
                                    foreach (Glass g in glass)
                                    {
                                        g.gameObject.layer = activeGlassLayer;
                                    }
                                }
                                depthCam.SetDefaultTargetTexture();
                                depthCam.RenderDepth();
                            }
                        }
                        break;
                    }
            }
        }

        //

        private IEnumerator Update_CamerasHighPriority_CO()
        {
            while (true)
            {
                if (initialised)
                {
                    if (normalTechnique == GlassNormalTechnique.NORMAL_WORLD_CAM_SHADER)
                    {
                        SetNormalFromCamera();
                    }
                }
                yield return new WaitForSeconds(1f / camHighUpdateRate);
            }
        }

        private IEnumerator Update_CamerasLowPriority_CO()
        {
            while (true)
            {
                if (initialised)
                {
                    UpdateCameraDepths();
                }
                yield return new WaitForSeconds(1f / camLowUpdateRate);
            }
        }

        //

        void UpdateCameraDepths()
        {
            float depthFrontLength = camFront.DepthLength();
            float depthBackLength = camBack.DepthLength();
            float depthOtherLength = camOther.DepthLength();

            switch (frontDepthTechnique)
            {
                case GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_ON:
                    {
                        foreach (Glass aGlass in glass)
                        {
                            aGlass.SetDepthBack(depthBackLength);
                            aGlass.SetDepthOther(depthOtherLength);
                        }
                        break;
                    }
                default:
                    foreach (Glass aGlass in glass)
                    {
                        aGlass.SetDepthFront(depthFrontLength);
                        aGlass.SetDepthBack(depthBackLength);
                        aGlass.SetDepthOther(depthOtherLength);
                    }
                    break;
            }
        }

        public void UpdateCameraSettings()
        {
            if (camFront != null)
                camFront.SetDepthTextureClearMode(depthTextureClearMode);
            if (camBack != null)
                camBack.SetDepthTextureClearMode(depthTextureClearMode);
            if (camOther != null)
                camOther.SetDepthTextureClearMode(depthTextureClearMode);
        }

        //	TODO: deprecate in future
        /*
        private void SetFrontFace()
        {
            foreach (Glass aGlass in glass)
            {
                aGlass.SetFrontFace();
            }
        }
        */

        //	TODO: deprecate in future
        /*
        private void SetBackFace()
        {
            foreach (Glass aGlass in glass)
            {
                aGlass.SetBackFace();
            }
        }
        */

        //	TODO: deprecate in future
        /*
        private void SetAllFace()
        {
            foreach (Glass aGlass in glass)
            {
                aGlass.SetAllFace();
            }
        }
        */

        /// <summary>
        /// Sets the normalFrom Camera value in all Glass Materials.
        /// </summary>
        void SetNormalFromCamera()
        {
            foreach (Glass aGlass in glass)
            {
                if (aGlass == null)
                    continue;
                aGlass.SetCameraNormal(mainCamera.transform.forward);
            }
        }

        /// <summary>
        /// Sets the depth texture for the provided texture name in all Glass materials.
        /// </summary>
        /// <param name="renderTexture">Render texture.</param>
        /// <param name="textureName">Texture name.</param>
        public void SetDepthTexture(RenderTexture renderTexture, string textureName)
        {
            foreach (Glass aGlass in glass)
            {
                if (aGlass == null)
                    continue;
                aGlass.SetDepthTexture(renderTexture, textureName);
            }
        }

        //	TODO: deprecate in future
        /*
        public void SetLayer(int _layer)
        {
            if (_layer < 0 || _layer > 31)
            {
                Debug.LogError("An error has occured trying to set a Glass object's layer. Make sure 'GlassFront' and 'GlassBack' exist in Tags and Layers.");
                frontLayerMask = -1;
                backLayerMask = -1;
                otherLayerMask = -1;
                return;
            }
            foreach (Glass aGlass in glass)
            {
                aGlass.gameObject.layer = _layer;
            }
        }
        */

        #endregion
    }

}
