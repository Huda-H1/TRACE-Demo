using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FantasticGlass
{
    //  N.B. Add 'FD_GLASS' to  Scripting Define Symbols in Player settings (Edit/Project Settings/Player)
    //  in order to have any functions enabled that are Glass-Specific
    [Serializable]
    public class Glass : MonoBehaviour
    {
        #region Member Variables

        /// <summary>
        /// The version's number. e.g. 1.x.x
        /// </summary>
        public static int version = 1;
        /// <summary>
        /// This versions major update number. e.g. x.1.x
        /// </summary>
        public static int versionMajor = 2;
        /// <summary>
        /// This versions minor update number. e.g. x.x.1
        /// </summary>
        public static int versionMinor = 4;

        public static int versionInt = VersionInt(version, versionMajor, versionMinor);

        /// <summary>
        /// The version of this instance of Fantastic Glass as a string 'x_x_x' - used for filenames.
        /// </summary>
        public static string versionString = VersionString(version, versionMajor, versionMinor, "_");
        /// <summary>
        /// The version of this instance of Fantastic Glass as a string 'x.x.x' - used for labels.
        /// </summary>
        public static string versionStringFormatted = VersionString(version, versionMajor, versionMinor, ".");
        //
        public bool showSection_ManagerSettings = false;
        public bool showSection_ManagerSettings_depthQuality = false;
        public bool showSection_ManagerSettings_depthTextures = false;
        public bool showSection_ManagerSettings_depthCamera = false;
        public bool showSection_Manager_depthNormalTech = false;
        //
        public static string unityDefaulResourcesPath = "unity default resources";
        public static string shaderPath = "Custom/Glass";
        //
        public Material material_back;
        public Material material_front;
        public bool getCurrentMaterials = false;
        public List<Material> materials = new List<Material>();
        //
        public string presetName;
        public GlassSettingsCopyList saveCopyList = new GlassSettingsCopyList();
        public GlassSettingsCopyList loadCopyList = new GlassSettingsCopyList();
        //
        public GlassManager manager;
        /// <summary>
        /// zFightRadius avoids the appearance of z-fighting by offseting world position
        /// and adjusting the collider size.
        /// </summary>
        public float zFightRadius = defaultZFightRadius;
        public static float defaultZFightRadius = 0.001f;
        //
        public static int default_customRenderOrder = 3010;
        //
        public bool enableExtinction_front = true;
        public bool enableExtinction_back = false;
        public bool enableExtinction_both = false;
        public GlassExtinctionAppearance extinctionAppearance = GlassExtinctionAppearance.AsItAppears;
        public bool capExtinction_front = true;
        public bool capExtinction_back = true;
        public GlassFace lastFaceEdited_Extinction = GlassFace.front;
        public Texture texture_extinction_front;
        public Texture texture_extinction_back;
        public Color extinction_front = new Color(0.1f, 0.033f, 0.00666f, 1.0f);
        public Color extinction_back = new Color(0.1f, 0.033f, 0.00666f, 1.0f);
        public Color extinctionFlipped_front = new Color(0.9f, 0.977f, 0.99444f, 1.0f);
        public Color extinctionFlipped_back = new Color(0.9f, 0.977f, 0.99444f, 1.0f);
        /// <summary>
        /// x = multiplier, y = min, z = max
        /// </summary>
        public Vector3 extinctionMagnitude_front = new Vector3(1f, 0f, 1f);
        /// <summary>
        /// x = multiplier, y = min, z = max
        /// </summary>
        public Vector3 extinctionMagnitude_back = new Vector3(1f, 0f, 1f);
        //
        public bool enableAberration_front = true;
        public bool enableAberration_back = false;
        public bool enableAberration_both = false;
        public bool capAberration_front = true;
        public bool capAberration_back = true;
        public GlassFace lastFaceEdited_Aberration = GlassFace.front;
        public Texture texture_aberration_front;
        public Texture texture_aberration_back;
        public Color aberration_front = new Color(0.1f, 0.033f, 0.00666f, 1.0f);
        public Color aberration_back = new Color(0.1f, 0.033f, 0.00666f, 1.0f);
        /// <summary>
        /// x = multiplier, y = min, z = max
        /// </summary>
        public Vector3 aberrationMagnitude_front = new Vector3(0.01f, 0f, 0.1f);
        /// <summary>
        /// x = multiplier, y = min, z = max
        /// </summary>
        public Vector3 aberrationMagnitude_back = new Vector3(0.01f, 0f, 0.1f);
        //
        public bool enableFog_front = false;
        public bool enableFog_back = false;
        public bool enableFog_both = false;
        public GlassFace lastFaceEdited_Fog = GlassFace.front;
        public float fogMagnitude_front = 1f;
        public float fogMagnitude_back = 1f;
        public Color fogNear_front = new Color(0f, 1f, 0f, 0f);
        public Color fogNear_back = new Color(0f, 1f, 0f, 0f);
        public Color fogFar_front = new Color(0f, 0f, 1f, 0.5f);
        public Color fogFar_back = new Color(0f, 0f, 1f, 0.5f);
        //
        public bool enableDistortion_front = true;
        public bool enableDistortion_back = true;
        public Texture texture_distortion;
        /// <summary>
        ///  x = texture, y = mesh normal, z = multiplier, w = max
        /// </summary>
        public Vector4 distortion_front = new Vector4(0.0f, -0.01f, 1f, 1f);
        /// <summary>
        ///  x = texture, y = mesh normal, z = multiplier, w = max
        /// </summary>
        public Vector4 distortion_back = new Vector4(0.0f, 0.1f, 1f, 1f);
        /// <summary>
        ///  x = texture, y = mesh normal, z = multiplier, w = max
        /// </summary>
        public Vector4 distortion_front_shaderCopy = new Vector4();
        /// <summary>
        ///  x = texture, y = mesh normal, z = multiplier, w = max
        /// </summary>
        public Vector4 distortion_back_shaderCopy = new Vector4();
        public float distortionEdgeBend_front = 0.3f;
        public float distortionEdgeBend_back = 0.3f;
        public float distortionDepthFade_front = 0.1f;
        public float distortionDepthFade_back = 0.1f;
        //
        public float bumpBack = 0f;
        public float bumpFront = 0f;
        //
        public float depthMultiplier = 1f;
        public float depthOffset = 0f;
        public bool enableDoubleDepth_front = true;
        public bool enableDoubleDepth_back = true;
        public bool flipDepth = false;
        public int depthTextureID = -1;
        //
        public List<Glass> ignoredDepth = new List<Glass>();
        //
        public float opacity = 0f;
        public Color colour_albedo = new Color(1, 1, 1, 0);
        public Texture texture_albedo;
        public Color colour_albedoTexture = new Color(1, 1, 1, 0);
        //
        public Texture texture_gloss_front;
        public Texture texture_gloss_back;
        public float glossiness_front = 0.5f;
        public float glossiness_back = 0f;
        //
        public Texture texture_metal_front;
        public Texture texture_metal_back;
        public float metallic_front = 0f;
        public float metallic_back = 0f;
        //
        public Texture texture_glow_front;
        public Texture texture_glow_back;
        public float glow_front = 0f;
        public float glow_back = 0f;
        //
        public static string mat_valueName_enableExtinction = "_EnableColourExtinction";
        public static string mat_valueName_extinction = "_ColourExtinction";
        public static string mat_valueName_extinctionIntensity = "_ColourExtinctionMagnitude";
        public static string mat_valueName_extinctionTexture = "_ColourExtinctionTexture";
        public static string mat_valueName_capExtinction = "_CapExtinctionValues";
        //
        public static string mat_valueName_enableAberration = "_EnableAberration";
        public static string mat_valueName_aberration = "_Aberration";
        public static string mat_valueName_aberrationIntensity = "_AberrationMagnitude";
        public static string mat_valueName_aberrationTexture = "_AberrationTexture";
        public static string mat_valueName_capAberration = "_CapAberrationValues";
        //
        public static string mat_valueName_enableFog = "_EnableFog";
        public static string mat_valueName_fogMagnitude = "_FogMagnitude";
        public static string mat_valueName_fogNear = "_DepthColourNear";
        public static string mat_valueName_fogFar = "_DepthColourFar";
        //
        public static string mat_valueName_enableDistortion = "_EnableDistortion";
        public static string mat_valueName_distortionMagnitude = "_DistortionIntensity";
        public static string mat_valueName_distortionTexture = "_DistortionTexture";
        public static string mat_valueName_distortionEdgeBend = "_DistortionEdgeBend";
        public static string mat_valueName_distortionDepthFade = "_DistortionDepthFade";
        //
        public static string mat_valueName_disableBackFace = "_DisableBackFace";
        public static string mat_valueName_doubleDepthPass = "_DoubleDepthPass";
        public static string mat_valueName_depthMultiplier = "_DepthMagnitude";
        public static string mat_valueName_depthOffset = "_DepthOffset";
        public static string mat_valueName_flipDepth = "_FlipUV_Depth";
        public static string mat_valueName_BumpMagnitude = "_BumpMagnitude";
        public static string mat_valueName_zWrite = "_ZWrite";
        public static string mat_valueName_cullMode = "_CullMode";
        //
        //
        public static string mat_valueName_albedoTexture = "_MainTex";
        public static string mat_valueName_albedoTextureColour = "_MainTexIntensity";
        public static string mat_valueName_albedoColour = "_Color";
        public static string mat_valueName_opacity = "_Opacity";
        //
        public static string mat_valueName_glossiness = "_Glossiness";
        public static string mat_valueName_metallic = "_Metallic";
        public static string mat_valueName_glow = "_Glow";
        public static string mat_valueName_glossinessTexture = "_GlossinessTexture";
        public static string mat_valueName_metallicTexture = "_MetallicTexture";
        public static string mat_valueName_glowTexture = "_GlowTexture";
        //
        public static float default_zFightRadius = 0.001f;
        //
        public bool showSection_AboutDoubleDepth = false;
        //
        public bool ShowSection_Gloss_Front = false;
        public bool ShowSection_Metal_Front = false;
        public bool ShowSection_Glow_Front = false;
        public bool ShowSection_Gloss_Back = false;
        public bool ShowSection_Metal_Back = false;
        public bool ShowSection_Glow_Back = false;
        //
        public bool showTextures_Albedo = false;
        public bool showTextures_Distortion = false;
        public bool showTextures_Extinction = false;
        public bool showTextures_Aberration = false;
        public bool showTextures_Glossiness = false;
        public bool showTextures_Metallic = false;
        public bool showTextures_Glow = false;
        //
        public int changedTexture_Extinction = 0;
        public int changedTexture_Glow = 0;
        public int changedTexture_Metallic = 0;
        public int changedTexture_Glossiness = 0;
        public int changedTexture_Aberration = 0;
        //
        public bool linkTextures_Extinction = true;
        public bool linkTextures_Glow = true;
        public bool linkTextures_Metallic = true;
        public bool linkTextures_Glossiness = true;
        public bool linkTextures_Aberration = true;
        //
        public bool showSection_Texture_Dimensions;
        public Dictionary<string, Texture> allTextures = new Dictionary<string, Texture>();
        public List<Vector2> textureDimensions = new List<Vector2>();
        //
        public DepthQuality_GlassObject depthQuality_front = default_depthQuality;
        public DepthQuality_GlassObject depthQuality_back = default_depthQuality;
        public DepthQuality_GlassObject depthQuality_other = default_depthQuality;
        public static DepthQuality_GlassObject default_depthQuality = DepthQuality_GlassObject.Simple;
        public int otherDepthIndex = 0;
        //
        public bool showSection_Textures = false;
        public bool showSection_Surface = false;
        public bool showSection_Depth = false;
        public bool showSection_Fog = false;
        public bool showSection_Aberration = false;
        public bool showSection_Extinction = false;
        public bool showSection_Physics = false;
        public bool showSection_Distortion = false;
        public bool showSection_Albedo = false;
        public bool showSection_Materials = false;
        public bool showSection_RenderOrder = false;
        public bool showSection_AboutRenderOrder = false;
        public bool showSection_Presets = false;
        public bool showSection_SharedGlass = false;
        public bool showSection_Shader = false;
        public bool showList_IgnoredDepth_Depth = false;
        public bool showList_IgnoredDepth_Manager = false;
        //
        public Vector2 scroll_IgnoredDepth = new Vector2();

        #endregion

        #region Start

        void Start()
        {
            /*
            if (getCurrentMaterials)
            {
                Renderer renderer = gameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (renderer.sharedMaterials.Length == 2)
                    {
                        material_back = renderer.sharedMaterials[0];
                        material_front = renderer.sharedMaterials[1];
                    }
                }
            }
            */
            //
            FindGlassManager();
            UpdateGlassManager();
            //
            ExpandColliderForZFightFix();
        }

        #endregion

        #region Init

        public void Initialise()
        {
            //InitMaterial(ref material_back, 0);
            //InitMaterial(ref material_front, 1);
        }

        public void InitMaterial(ref Material mat, int index)
        {
            if (mat == null)
                return;
            if (index < 0)
                return;
            if (index >= GetComponent<Renderer>().sharedMaterials.Length)
            {
                int newMaterialSize = index + 1;
                Material[] sm = new Material[newMaterialSize];
                for (int i = 0; i < GetComponent<Renderer>().sharedMaterials.Length; i++)
                {
                    sm[i] = GetComponent<Renderer>().sharedMaterials[i];
                }
                sm[index] = null;
                GetComponent<Renderer>().sharedMaterials = sm;
            }
            if (mat == null)
                mat = GetComponent<Renderer>().sharedMaterials[index];
            if (GetComponent<Renderer>().sharedMaterials[index] == null)
                if (mat != null)
                    GetComponent<Renderer>().sharedMaterials[index] = mat;
        }

        #endregion

		#region Paths

		public static string GetXMLPath()
		{
			return GetPackagePath() + "XML" + "/";
        }
		
		public static string GetPackagePath()
		{
			return Application.dataPath + "/" + "Fantastic Glass" + "/";
		}
        
        #endregion
        
        #region Version Formatting

        public static int VersionInt(int _version = 1, int _versionMajor = 0, int _versionMinor = 0)
        {
            return (_version * 10 ^ 9) + (_versionMajor * 10 ^ 6) + (_versionMinor * 10 ^ 3);
        }

        public static string VersionString(int _version = 1, int _versionMajor = 0, int _versionMinor = 0, string _seperator = "_")
        {
            return _version.ToString()
                + _seperator
                + _versionMajor.ToString()
                + _seperator
                + _versionMinor.ToString();
        }

        public string VersionStringFormatted = "1.1.2";

        #endregion

        #region Z-Fighting

        public void ExpandColliderForZFightFix()
        {
            if (zFightRadius == 0f)
                return;
            //
            RemoveDuplicateMeshColliders.RemoveDuplicatesFromObject(gameObject);
            //
            foreach (BoxCollider foundBoxCollider in GetComponentsInChildren<BoxCollider>())
            {
                if (foundBoxCollider != null)
                {
                    if (!foundBoxCollider.isTrigger)
                    {
                        foundBoxCollider.size = foundBoxCollider.size * (1f + zFightRadius);
                    }
                }
            }
            //
            foreach (SphereCollider foundSphereCollider in GetComponentsInChildren<SphereCollider>())
            {
                if (foundSphereCollider != null)
                {
                    if (!foundSphereCollider.isTrigger)
                    {
                        foundSphereCollider.radius = foundSphereCollider.radius * (1f + zFightRadius);
                    }
                }
            }
            //
            foreach (MeshCollider foundMeshCollider in GetComponentsInChildren<MeshCollider>())
            {
                if (foundMeshCollider != null)
                {
                    if (!foundMeshCollider.isTrigger)
                    {
                        GameObject meshColliderContainer = new GameObject("MeshColliderContainer");
                        meshColliderContainer.transform.parent = transform;
                        meshColliderContainer.transform.localPosition = Vector3.zero;
                        meshColliderContainer.transform.localRotation = Quaternion.identity;
                        MeshCollider meshColliderCopy = meshColliderContainer.AddComponent<MeshCollider>();
                        meshColliderCopy.sharedMesh = foundMeshCollider.sharedMesh;
                        meshColliderCopy.convex = foundMeshCollider.convex;
                        meshColliderContainer.transform.localScale = new Vector3(1f + zFightRadius, 1f + zFightRadius, 1f + zFightRadius);
                    }
                }
            }
        }

        #endregion

        #region Glass Manager

        public void UpdateGlassManager()
        {
            if (manager == null)
                FindGlassManager();
            //	NB: The following functions should be called by the GlassManager in its Init function.
            //manager.FindGlass();
            //manager.UpdateActiveMaterials();
            //manager.UpdateShaders();
        }

        public GlassManager FindGlassManager()
        {
            if (manager == null)
                manager = GlassManager.Instance;
            return manager;
        }

        #endregion

        #region Camera

        //	NORMAL TECHNIQUE 1
        public void SetCameraNormal(Vector3 cameraNormal)
        {
            if (!MaterialsReady())
                return;
            material_back.SetVector("_CameraNormal", cameraNormal);
            material_front.SetVector("_CameraNormal", cameraNormal);
        }

        #endregion

        #region Depth

        public void SetDepthTexture(RenderTexture renderTexture, string textureName)
        {
            if (!MaterialsReady())
                return;
            material_back.SetTexture(textureName, renderTexture);
            material_front.SetTexture(textureName, renderTexture);
            //material_back.SetTexture(textureName, renderTexture);
            //material_front.SetTexture(textureName, renderTexture);
            RemoveNullMaterials();
            foreach (Material material in materials)
            {
                material.SetTexture(textureName, renderTexture);
            }
        }

        //  Depth Quality

        public void DepthQualityChanged_Front()
        {
            GlassRenderOrderManager.Instance.DepthQualityChanged_Front(this);
            manager.depthQuality_Front = DepthQuality_GlassManager.ObjectDefined;
        }

        public void DepthQualityChanged_Back()
        {
            GlassRenderOrderManager.Instance.DepthQualityChanged_Back(this);
            manager.depthQuality_Back = DepthQuality_GlassManager.ObjectDefined;
        }

        public void DepthQualityChanged_Other()
        {
            GlassRenderOrderManager.Instance.DepthQualityChanged_Other(this);
            manager.depthQuality_Other = DepthQuality_GlassManager.ObjectDefined;
        }

        #endregion

        #region Values Changed - Material

        public void SetMaterials(Material back, Material front)
        {
            materials.Clear();
            material_back = back;
            material_front = front;
            UpdateRendererMaterials();
        }

        public void SetMaterials(List<Material> updatedMaterials)
        {
            materials.Clear();
            materials.AddRange(updatedMaterials);
            UpdateRendererMaterials();
        }

        /// <summary>
        /// Updates the materials attached to the renderer and those stored in 'materials'
        /// </summary>
        public void UpdateRendererMaterials()
        {
            while (materials.Contains(null))
                materials.Remove(null);

            if (materials.Count == 0)
            {
                materials.Add(material_back);
                materials.Add(material_front);
            }

            MeshRenderer _foundRenderer = gameObject.GetComponent<MeshRenderer>();

            if (_foundRenderer == null)
                return;

            Material[] materialsCopy = _foundRenderer.sharedMaterials;

            if (materialsCopy.Length != materials.Count)
                materialsCopy = new Material[materials.Count];

            for (int i = 0; i < materials.Count; i++)
            {
                materialsCopy[i] = materials[i];
            }

            /*
            materialsCopy[0] = material_back;
            materialsCopy[1] = material_front;
            */

            _foundRenderer.sharedMaterials = materialsCopy;
        }

        /// <summary>
        /// Returns materials attached to the renderer that are NOT Glass.
        /// </summary>
        /// <returns></returns>
        public List<Material> NonGlassMaterials()
        {
            List<Material> results = new List<Material>();
            Shader glassShader = Shader.Find(Glass.shaderPath);
            foreach(Material material in materials)
                if(material.shader != glassShader)
                    if (!results.Contains(material))
                        results.Add(material);
            return results;
        }

        /// <summary>
        /// Returns materials attached to the renderer that ARE Glass.
        /// </summary>
        /// <returns></returns>
        public List<Material> GlassMaterials()
        {
            List<Material> results = new List<Material>();
            Shader glassShader = Shader.Find(Glass.shaderPath);
            foreach (Material material in materials)
                if (material.shader == glassShader)
                    if (!results.Contains(material))
                        results.Add(material);
            return results;
        }

        /// <summary>
        /// Tells the Glass object that another Glass object has changed.
        /// Gives it the chance to copy settings from it if they match.
        /// </summary>
        /// <param name="glassChanged"></param>
        public void MaterialValuesChanged(Glass glassChanged)
        {
            if (!Matches(glassChanged))
                return;

            try
            {
                CopyMaterialValues(glassChanged);
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred trying to copy material values. Please try again later. Exception:" + e.Message);
            }

            /*
            try
            {
                UpdateRendererMaterials();
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred trying to update materials. Please try again later. Exception:" + e.Message);
            }
            */
        }

        public void CopyMaterialValues(Glass otherGlass)
        {
            SetMaterials(otherGlass.material_back, otherGlass.material_front);
            //material_back = otherGlass.material_back;
            //material_front = otherGlass.material_front;
            //
            enableExtinction_front = otherGlass.enableExtinction_front;
            enableExtinction_back = otherGlass.enableExtinction_back;
            enableExtinction_both = otherGlass.enableExtinction_both;
            capExtinction_front = otherGlass.capExtinction_front;
            capExtinction_back = otherGlass.capExtinction_back;
            texture_extinction_front = otherGlass.texture_extinction_front;
            texture_extinction_back = otherGlass.texture_extinction_back;
            extinction_front = otherGlass.extinction_front;
            extinction_back = otherGlass.extinction_back;
            extinctionFlipped_front = otherGlass.extinctionFlipped_front;
            extinctionFlipped_back = otherGlass.extinctionFlipped_back;
            extinctionMagnitude_front = otherGlass.extinctionMagnitude_front;
            extinctionMagnitude_back = otherGlass.extinctionMagnitude_back;
            //
            enableAberration_front = otherGlass.enableAberration_front;
            enableAberration_back = otherGlass.enableAberration_back;
            enableAberration_both = otherGlass.enableAberration_both;
            capAberration_front = otherGlass.capAberration_front;
            capAberration_back = otherGlass.capAberration_back;
            texture_aberration_front = otherGlass.texture_aberration_front;
            texture_aberration_back = otherGlass.texture_aberration_back;
            aberration_front = otherGlass.aberration_front;
            aberration_back = otherGlass.aberration_back;
            aberrationMagnitude_front = otherGlass.aberrationMagnitude_front;
            aberrationMagnitude_back = otherGlass.aberrationMagnitude_back;
            //
            enableFog_front = otherGlass.enableFog_front;
            enableFog_front = otherGlass.enableFog_front;
            enableFog_front = otherGlass.enableFog_front;
            fogNear_front = otherGlass.fogNear_front;
            fogNear_back = otherGlass.fogNear_back;
            fogFar_front = otherGlass.fogFar_front;
            fogFar_back = otherGlass.fogFar_back;
            fogMagnitude_front = otherGlass.fogMagnitude_front;
            fogMagnitude_back = otherGlass.fogMagnitude_back;
            //
            enableDistortion_front = otherGlass.enableDistortion_front;
            enableDistortion_back = otherGlass.enableDistortion_back;
            texture_distortion = otherGlass.texture_distortion;
            distortion_front = otherGlass.distortion_front;
            distortion_back = otherGlass.distortion_back;
            distortionEdgeBend_front = otherGlass.distortionEdgeBend_front;
            distortionEdgeBend_back = otherGlass.distortionEdgeBend_back;
            distortionDepthFade_front = otherGlass.distortionDepthFade_front;
            distortionDepthFade_back = otherGlass.distortionDepthFade_back;
            //
            bumpBack = otherGlass.bumpBack;
            bumpFront = otherGlass.bumpFront;
            //
            depthMultiplier = otherGlass.depthMultiplier;
            depthOffset = otherGlass.depthOffset;
            flipDepth = otherGlass.flipDepth;
            enableDoubleDepth_front = otherGlass.enableDoubleDepth_front;
            enableDoubleDepth_back = otherGlass.enableDoubleDepth_back;
            //
            depthQuality_front = otherGlass.depthQuality_front;
            depthQuality_back = otherGlass.depthQuality_back;
            depthQuality_other = otherGlass.depthQuality_other;
            //
            opacity = otherGlass.opacity;
            colour_albedo = otherGlass.colour_albedo;
            texture_albedo = otherGlass.texture_albedo;
            colour_albedoTexture = otherGlass.colour_albedoTexture;
            //
            glossiness_back = otherGlass.glossiness_back;
            texture_gloss_back = otherGlass.texture_gloss_back;
            glossiness_front = otherGlass.glossiness_front;
            texture_gloss_front = otherGlass.texture_gloss_front;
            //
            metallic_back = otherGlass.metallic_back;
            texture_metal_back = otherGlass.texture_metal_back;
            metallic_front = otherGlass.metallic_front;
            texture_metal_front = otherGlass.texture_metal_front;
            //
            glow_back = otherGlass.glow_back;
            texture_glow_back = otherGlass.texture_glow_back;
            glow_front = otherGlass.glow_front;
            texture_glow_front = otherGlass.texture_glow_front;
        }

        #endregion

        #region Values Changed - Physics

        public void PhysicsValuesChanged(Glass glassChanged)
        {
            if (!Matches(glassChanged))
                return;
            CopyPhysicsValues(glassChanged);
        }

        public void CopyPhysicsValues(Glass otherGlass)
        {
            zFightRadius = otherGlass.zFightRadius;
        }

        #endregion

        #region Glass matching

        /// <summary>
        /// Returns true if matches the specified otherGlass. Empty and null names wil never match. Exact matches are ignored by default.
        /// </summary>
        /// <param name="otherGlass">Other glass.</param>
        public bool Matches(Glass otherGlass, bool ignoreExactObjectMatch = true)
        {
            try
            {
                if (this == null)
                {
                    return false;
                }

                if (otherGlass == null)
                    return false;

                if (ignoreExactObjectMatch)
                    if (this == otherGlass)
                        return false;

                if (otherGlass.presetName == "")
                    return false;

                if (presetName == "")
                    return false;

                if (manager == null)
                    manager = GlassManager.Instance;

                if (manager.matchByMaterial)
                {
                    if (material_back == otherGlass.material_back)
                        return true;
                    if (material_front == otherGlass.material_front)
                        return true;
                }

                if (manager.matchByGlassName)
                {
                    if (presetName == otherGlass.presetName)
                        return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred trying to match Glass objects. Exception: " + e.Message);
                return false;
            }
            return false;
        }

        #endregion

        #region Depth Functions

        //	TODO: deprecate in future
        /*
        public void SetFrontFace()
        {
            if (!MaterialsReady())
                return;
            material_front.SetFloat("_CullMode", (float)UnityEngine.Rendering.CullMode.Back);
            material_back.SetFloat("_CullMode", (float)UnityEngine.Rendering.CullMode.Back);
        }
        */

        //	TODO: deprecate in future
        /*
        public void SetBackFace()
        {
            if (!MaterialsReady())
                return;
            material_front.SetFloat("_CullMode", (float)UnityEngine.Rendering.CullMode.Front);
            material_back.SetFloat("_CullMode", (float)UnityEngine.Rendering.CullMode.Front);
        }
        */

        //	TODO: deprecate in future
        /*
        public void SetAllFace()
        {
            if (!MaterialsReady())
                return;
            material_front.SetFloat("_CullMode", (float)UnityEngine.Rendering.CullMode.Off);
            material_back.SetFloat("_CullMode", (float)UnityEngine.Rendering.CullMode.Off);
        }
        */

        public void SetDepthFront(float depth)
        {
            if (!MaterialsReady())
                return;
            material_front.SetFloat("_DepthFrontDistance", depth);
            material_back.SetFloat("_DepthFrontDistance", depth);
        }

        public void SetDepthBack(float depth)
        {
            if (!MaterialsReady())
                return;
            material_front.SetFloat("_DepthBackDistance", depth);
            material_back.SetFloat("_DepthBackDistance", depth);
        }

        public void SetDepthOther(float depth)
        {
            if (!MaterialsReady())
                return;
            material_front.SetFloat("_DepthOtherDistance", depth);
            material_back.SetFloat("_DepthOtherDistance", depth);
        }

        #endregion

        #region Helper functions

        public static void FlipColour(Color colour_in, ref Color colour_out)
        {
            colour_out.r = 1f - colour_in.r;
            colour_out.g = 1f - colour_in.g;
            colour_out.b = 1f - colour_in.b;
            colour_out.a = colour_in.a;
        }

        bool MaterialsReady()
        {
            if (material_back == null)
                return false;
            if (material_front == null)
                return false;
            return true;
        }

        public void UpdateTextureDimensions_Full()
        {
            ChangedTexture("texture_distortion", texture_distortion);
            ChangedTexture("texture_aberration_front", texture_aberration_front);
            ChangedTexture("texture_aberration_back", texture_aberration_back);
            ChangedTexture("texture_extinction_front", texture_extinction_front);
            ChangedTexture("texture_extinction_back", texture_extinction_back);
            ChangedTexture("texture_gloss_front", texture_gloss_front);
            ChangedTexture("texture_gloss_back", texture_gloss_back);
            ChangedTexture("texture_metal_front", texture_metal_front);
            ChangedTexture("texture_metal_back", texture_metal_back);
            ChangedTexture("texture_glow_front", texture_glow_front);
            ChangedTexture("texture_glow_back", texture_glow_back);

            UpdateTextureDimensions();
        }

        public void UpdateTextureDimensions()
        {
            textureDimensions.Clear();
            foreach (Texture texture in allTextures.Values)
            {
                if (texture != null)
                {
                    Vector2 textureSize = new Vector2(texture.width, texture.height);
                    if (!textureDimensions.Contains(textureSize))
                        textureDimensions.Add(textureSize);
                }
            }
        }

        public void ChangedTexture(string name, Texture texture)
        {
            allTextures[name] = texture;
            UpdateTextureDimensions();
        }

        #endregion

        #region Physics Updates

        public void UpdatePhysics()
        {
            manager.PhysicsValueChanged(this);
        }

        #endregion

        #region Material Updates

        public void RemoveNullMaterials()
        {
            while (materials.Contains(null))
            {
                if (GlassManager.Instance.showDebugInfo)
                {
                    Debug.Log("Glass: Removing null material from Materials for Glass: '" + presetName + "'.");
                }
                materials.Remove(null);
            }
        }

        public void Material_Changed()
        {
            UpdateRendererMaterials();
            manager.GlassModified(this);
        }

        public void Material_BackfaceChanged()
        {
            UpdateRendererMaterials();
            manager.GlassModified(this);
        }

        public void Material_FrontfaceChanged()
        {
            UpdateRendererMaterials();
            manager.GlassModified(this);
        }

        #endregion

        #region Shader updates

        bool MaterialCanBeUpdated(Material material)
        {
            if (material == null)
            {
                if (manager != null)
                {
                    if (manager.showDebugInfo)
                    {
                        Debug.LogWarning("Glass: Unable to update material as it is null or no longer exists.");
                    }
                }
                return false;
            }
            return true;
        }

        //  textures

        internal void UpdateTexturesAndColours()
        {
            UpdateTexturesAndColours(ref material_back);
            UpdateTexturesAndColours(ref material_front);
            manager.GlassModified(this);
        }

        internal void UpdateTexturesAndColours(ref Material material)
        {
            if (!MaterialCanBeUpdated(material))
                return;
            material.SetFloat(mat_valueName_opacity, opacity);
            material.SetColor(mat_valueName_albedoColour, colour_albedo);
            material.SetTexture(mat_valueName_albedoTexture, texture_albedo);
            //  texture colour should be clear when texture isn't set
            material.SetColor(mat_valueName_albedoTextureColour, (texture_albedo != null) ? colour_albedoTexture : Color.clear);
        }

        //  bump

        internal void UpdateBump()
        {
            UpdateBump(ref material_front, bumpFront);
            UpdateBump(ref material_back, bumpBack);
            manager.GlassModified(this);
        }

        internal void UpdateBump(ref Material material, float bump)
        {
            if (!MaterialCanBeUpdated(material))
                return;
            material.SetFloat(mat_valueName_BumpMagnitude, bump);
        }

        //  distortion

        internal void UpdateDistortion()
        {
            distortion_front_shaderCopy = distortion_front;
            if (texture_distortion == null)
                distortion_front_shaderCopy.x = 0f;

            distortion_back_shaderCopy = distortion_back;
            if (texture_distortion == null)
                distortion_back_shaderCopy.x = 0f;

            UpdateDistortion(ref material_front, GlassFace.front);
            UpdateDistortion(ref material_back, GlassFace.back);

            ChangedTexture("texture_distortion", texture_distortion);

            manager.GlassModified(this);
        }

        internal void UpdateDistortion(ref Material material, GlassFace face)
        {
            if (!MaterialCanBeUpdated(material))
                return;

            material.SetTexture(mat_valueName_distortionTexture, texture_distortion);

            switch (face)
            {
                case GlassFace.front:
                    material.SetFloat(mat_valueName_enableDistortion, enableDistortion_front ? 1f : 0f);
                    material.SetVector(mat_valueName_distortionMagnitude, distortion_front_shaderCopy);
                    material.SetFloat(mat_valueName_distortionEdgeBend, distortionEdgeBend_front);
                    material.SetFloat(mat_valueName_distortionDepthFade, distortionDepthFade_front);
                    material.SetInt(mat_valueName_doubleDepthPass, enableDoubleDepth_front ? 1 : 0);
                    //material.SetInt(mat_valueName_doubleDepthPass, (enableDoubleDepth_front && distortion_front.x != 0f) ? 1 : 0);
                    break;
                case GlassFace.back:
                    material.SetFloat(mat_valueName_enableDistortion, enableDistortion_back ? 1f : 0f);
                    material.SetVector(mat_valueName_distortionMagnitude, distortion_back_shaderCopy);
                    material.SetFloat(mat_valueName_distortionEdgeBend, distortionEdgeBend_back);
                    material.SetFloat(mat_valueName_distortionDepthFade, distortionDepthFade_back);
                    material.SetInt(mat_valueName_doubleDepthPass, enableDoubleDepth_back ? 1 : 0);
                    //material.SetInt(mat_valueName_doubleDepthPass, (enableDoubleDepth_back && distortion_back.x != 0f) ? 1 : 0);

                    break;
            }
        }

        //  aberration

        internal void UpdateAberration()
        {
            UpdateAberration(ref material_front, GlassFace.front);
            UpdateAberration(ref material_back, GlassFace.back);

            ChangedTexture("texture_aberration_front", texture_aberration_front);
            ChangedTexture("texture_aberration_back", texture_aberration_back);

            manager.GlassModified(this);
        }

        internal void UpdateAberration(ref Material material, GlassFace face)
        {
            if (!MaterialCanBeUpdated(material))
                return;

            switch (face)
            {
                case GlassFace.front:
                    material.SetFloat(mat_valueName_enableAberration, (enableAberration_front || enableAberration_both) ? 1f : 0f);
                    material.SetVector(mat_valueName_aberrationIntensity, aberrationMagnitude_front);
                    material.SetColor(mat_valueName_aberration, aberration_front);
                    material.SetTexture(mat_valueName_aberrationTexture, texture_aberration_front);
                    material.SetInt(mat_valueName_capAberration, capAberration_front ? 1 : 0);
                    break;
                case GlassFace.back:
                    material.SetFloat(mat_valueName_enableAberration, (enableAberration_back || enableAberration_both) ? 1f : 0f);
                    material.SetVector(mat_valueName_aberrationIntensity, aberrationMagnitude_back);
                    material.SetColor(mat_valueName_aberration, aberration_back);
                    material.SetTexture(mat_valueName_aberrationTexture, texture_aberration_back);
                    material.SetInt(mat_valueName_capAberration, capAberration_back ? 1 : 0);
                    break;
            }
        }

        //  extinction

        internal void UpdateExtinction()
        {
            UpdateExtinction(ref material_front, GlassFace.front);
            UpdateExtinction(ref material_back, GlassFace.back);

            ChangedTexture("texture_extinction_front", texture_extinction_front);
            ChangedTexture("texture_extinction_back", texture_extinction_back);

            manager.GlassModified(this);
        }

        internal void UpdateExtinction(ref Material material, GlassFace face)
        {
            if (!MaterialCanBeUpdated(material))
                return;

            switch (face)
            {
                case GlassFace.front:
                    material.SetInt(mat_valueName_enableExtinction, (enableExtinction_front || enableExtinction_both) ? 1 : 0);
                    material.SetVector(mat_valueName_extinctionIntensity, extinctionMagnitude_front);
                    material.SetColor(mat_valueName_extinction, extinction_front);
                    material.SetTexture(mat_valueName_extinctionTexture, texture_extinction_front);
                    material.SetInt(mat_valueName_capExtinction, capExtinction_front ? 1 : 0);
                    break;
                case GlassFace.back:
                    material.SetInt(mat_valueName_enableExtinction, (enableExtinction_back || enableExtinction_both) ? 1 : 0);
                    material.SetVector(mat_valueName_extinctionIntensity, extinctionMagnitude_back);
                    material.SetColor(mat_valueName_extinction, extinction_back);
                    material.SetTexture(mat_valueName_extinctionTexture, texture_extinction_back);
                    material.SetInt(mat_valueName_capExtinction, capExtinction_back ? 1 : 0);
                    break;
            }
        }

        //  fog

        internal void UpdateFog()
        {
            UpdateFog(ref material_front, GlassFace.front);
            UpdateFog(ref material_back, GlassFace.back);
            manager.GlassModified(this);
        }

        internal void UpdateFog(ref Material material, GlassFace face)
        {
            if (!MaterialCanBeUpdated(material))
                return;

            switch (face)
            {
                case GlassFace.front:
                    material.SetInt(mat_valueName_enableFog, (enableFog_front || enableFog_both) ? 1 : 0);
                    material.SetFloat(mat_valueName_fogMagnitude, fogMagnitude_front);
                    material.SetColor(mat_valueName_fogNear, fogNear_front);
                    material.SetColor(mat_valueName_fogFar, fogFar_front);
                    break;
                case GlassFace.back:
                    material.SetInt(mat_valueName_enableFog, (enableFog_back || enableFog_both) ? 1 : 0);
                    material.SetFloat(mat_valueName_fogMagnitude, fogMagnitude_back);
                    material.SetColor(mat_valueName_fogNear, fogNear_back);
                    material.SetColor(mat_valueName_fogFar, fogFar_back);
                    break;
            }
        }

        //  surface


        internal void UpdateSurface()
        {
            UpdateSurface(ref material_front, glossiness_front, metallic_front, glow_front, texture_gloss_front, texture_metal_front, texture_glow_front);
            UpdateSurface(ref material_back, glossiness_back, metallic_back, glow_back, texture_gloss_back, texture_metal_back, texture_glow_back);

            ChangedTexture("texture_gloss_front", texture_gloss_front);
            ChangedTexture("texture_gloss_back", texture_gloss_back);
            ChangedTexture("texture_metal_front", texture_metal_front);
            ChangedTexture("texture_metal_back", texture_metal_back);
            ChangedTexture("texture_glow_front", texture_glow_front);
            ChangedTexture("texture_glow_back", texture_glow_back);

            manager.GlassModified(this);
        }

        internal void UpdateSurface(ref Material material, float gloss, float metal, float glow, Texture glossTexture, Texture metalTexture, Texture glowTexture)
        {
            if (!MaterialCanBeUpdated(material))
                return;

            material.SetFloat(mat_valueName_glossiness, gloss);
            material.SetFloat(mat_valueName_metallic, metal);
            material.SetFloat(mat_valueName_glow, glow);

            material.SetTexture(mat_valueName_glossinessTexture, glossTexture);
            material.SetTexture(mat_valueName_metallicTexture, metalTexture);
            material.SetTexture(mat_valueName_glowTexture, glowTexture);
        }

        //  depth

        internal void UpdateDepth()
        {
            UpdateDepth(ref material_front, GlassFace.front);
            UpdateDepth(ref material_back, GlassFace.back);
            manager.GlassModified(this);
        }

        internal void UpdateDepth(ref Material material, GlassFace face)
        {
            if (!MaterialCanBeUpdated(material))
                return;

            material.SetFloat(mat_valueName_depthMultiplier, depthMultiplier);
            material.SetFloat(mat_valueName_depthOffset, depthOffset);
            material.SetInt(mat_valueName_flipDepth, flipDepth ? 1 : 0);
        }

        #endregion

        #region Preset Functions

        public void LoadFromPreset(GlassPreset preset, GlassSettingsCopyList features)
        {
            if (features.Albedo())
            {
                opacity = preset.opacity;
                GlassPreset.SetColour(ref colour_albedo, preset.colour_albedo);
                texture_albedo = GetTexture(preset.texturePath_albedo);
                GlassPreset.SetColour(ref colour_albedoTexture, preset.colour_albedoTexture);

                UpdateTexturesAndColours();
            }

            if (features.Distortion())
            {
                texture_distortion = GetTexture(preset.texturePath_distortion);

                enableDistortion_front = preset.enableDistortion_front;
                enableDistortion_back = preset.enableDistortion_back;

                distortion_front.x = preset.distortion_front_bump;
                distortion_front.y = preset.distortion_front_mesh;
                distortion_front.z = preset.distortion_front_multiplier;
                distortion_front.w = preset.distortion_front_max;

                distortion_back.x = preset.distortion_back_bump;
                distortion_back.y = preset.distortion_back_mesh;
                distortion_back.z = preset.distortion_back_multiplier;
                distortion_back.w = preset.distortion_back_max;

                distortionEdgeBend_front = preset.distortion_edge_bend_front;
                distortionEdgeBend_back = preset.distortion_edge_bend_back;
                distortionDepthFade_front = preset.distortion_depth_fade_front;
                distortionDepthFade_back = preset.distortion_depth_fade_back;

                enableDoubleDepth_front = preset.enableDoubleDepth_front;
                enableDoubleDepth_back = preset.enableDoubleDepth_back;

                UpdateDistortion();
            }

            if (features.Bump())
            {
                bumpFront = preset.bump_front;
                bumpBack = preset.bump_back;

                UpdateBump();
            }

            if (features.Extinction())
            {
                enableExtinction_front = preset.enableExtinction_front;
                enableExtinction_back = preset.enableExtinction_back;
                enableExtinction_both = preset.enableExtinction_both;

                capExtinction_front = preset.capExtinction_front;
                capExtinction_back = preset.capExtinction_back;

                texture_extinction_front = GetTexture(preset.texturePath_extinction_front);
                texture_extinction_back = GetTexture(preset.texturePath_extinction_back);

                extinctionAppearance = (GlassExtinctionAppearance)preset.extinctionAppearance;

                GlassPreset.SetColour(ref extinction_front, preset.colour_extinction_front);
                GlassPreset.SetColour(ref extinction_back, preset.colour_extinction_back);
                GlassPreset.SetColour(ref extinctionFlipped_front, preset.colour_extinction_flipped_front);
                GlassPreset.SetColour(ref extinctionFlipped_back, preset.colour_extinction_flipped_back);

                extinctionMagnitude_front.x = preset.extinctionIntensity_front;
                extinctionMagnitude_front.y = preset.extinctionMin_front;
                extinctionMagnitude_front.z = preset.extinctionMax_front;

                extinctionMagnitude_back.x = preset.extinctionIntensity_back;
                extinctionMagnitude_back.y = preset.extinctionMin_back;
                extinctionMagnitude_back.z = preset.extinctionMax_back;

                UpdateExtinction();
            }

            if (features.Aberration())
            {
                enableAberration_front = preset.enableAberration_front;
                enableAberration_back = preset.enableAberration_back;
                enableAberration_both = preset.enableAberration_both;

                capAberration_front = preset.capAberration_front;
                capAberration_back = preset.capAberration_back;

                texture_aberration_front = GetTexture(preset.texturePath_aberration_front);
                texture_aberration_back = GetTexture(preset.texturePath_aberration_back);

                GlassPreset.SetColour(ref aberration_front, preset.colour_aberration_front);
                GlassPreset.SetColour(ref aberration_back, preset.colour_aberration_back);

                aberrationMagnitude_front.x = preset.aberrationIntensity_front;
                aberrationMagnitude_front.y = preset.aberrationMin_front;
                aberrationMagnitude_front.z = preset.aberrationMax_front;

                aberrationMagnitude_back.x = preset.aberrationIntensity_back;
                aberrationMagnitude_back.y = preset.aberrationMin_back;
                aberrationMagnitude_back.z = preset.aberrationMax_back;

                UpdateAberration();
            }

            if (features.Fog())
            {
                enableFog_front = preset.option_fog_front;
                enableFog_back = preset.option_fog_back;
                enableFog_both = preset.option_fog_both;

                GlassPreset.SetColour(ref fogNear_front, preset.colour_fog_near_front);
                GlassPreset.SetColour(ref fogNear_back, preset.colour_fog_near_back);
                GlassPreset.SetColour(ref fogFar_front, preset.colour_fog_far_front);
                GlassPreset.SetColour(ref fogFar_back, preset.colour_fog_far_back);

                fogMagnitude_front = preset.fog_magnitude_front;
                fogMagnitude_back = preset.fog_magnitude_back;

                UpdateFog();
            }

            if (features.Surface())
            {
                glossiness_front = preset.glossiness_front;
                glossiness_back = preset.glossiness_back;
                texture_gloss_front = GetTexture(preset.texturePath_gloss_front);
                texture_gloss_back = GetTexture(preset.texturePath_gloss_back);

                glow_front = preset.glow_front;
                glow_back = preset.glow_back;
                texture_glow_front = GetTexture(preset.texturePath_glow_front);
                texture_glow_back = GetTexture(preset.texturePath_glow_back);

                metallic_front = preset.metallic_front;
                metallic_back = preset.metallic_back;
                texture_metal_front = GetTexture(preset.texturePath_metal_front);
                texture_metal_back = GetTexture(preset.texturePath_metal_back);

                UpdateSurface();
            }

            if (features.ZFightRadius())
            {
                zFightRadius = preset.zFightingRadius;
            }

            if (features.Depth())
            {
                depthMultiplier = preset.depthMultiplier;
                depthOffset = preset.depthOffset;
                depthQuality_back = preset.depthQuality_back;
                depthQuality_front = preset.depthQuality_front;
                depthQuality_other = preset.depthQuality_other;
            }
        }

        public GlassPreset GeneratePreset(GlassSettingsCopyList features, GlassPreset existingPreset = null)
        {
            GlassPreset preset = existingPreset;
            if (preset == null)
                preset = new GlassPreset();

            preset.name = presetName;

            if (features.Albedo())
            {
                preset.opacity = opacity;
                GlassPreset.SetColour(ref preset.colour_albedo, colour_albedo);
                preset.texturePath_albedo = GetPath(texture_albedo);
                GlassPreset.SetColour(ref preset.colour_albedoTexture, colour_albedoTexture);
            }

            if (features.Distortion())
            {
                preset.enableDistortion_front = enableDistortion_front;
                preset.enableDistortion_back = enableDistortion_back;

                preset.texturePath_distortion = GetPath(texture_distortion);

                preset.distortion_front_bump = distortion_front.x;
                preset.distortion_front_mesh = distortion_front.y;
                preset.distortion_front_multiplier = distortion_front.z;
                preset.distortion_front_max = distortion_front.w;

                preset.distortion_back_bump = distortion_back.x;
                preset.distortion_back_mesh = distortion_back.y;
                preset.distortion_back_multiplier = distortion_back.z;
                preset.distortion_back_max = distortion_back.w;

                preset.distortion_edge_bend_front = distortionEdgeBend_front;
                preset.distortion_edge_bend_back = distortionEdgeBend_back;
                preset.distortion_depth_fade_front = distortionDepthFade_front;
                preset.distortion_depth_fade_back = distortionDepthFade_back;

                preset.enableDoubleDepth_front = enableDoubleDepth_front;
                preset.enableDoubleDepth_back = enableDoubleDepth_back;
            }

            if (features.Bump())
            {
                preset.bump_front = bumpFront;
                preset.bump_back = bumpBack;
            }

            if (features.Extinction())
            {
                preset.enableExtinction_front = enableExtinction_front;
                preset.enableExtinction_back = enableExtinction_back;
                preset.enableExtinction_both = enableExtinction_both;

                preset.capExtinction_front = capExtinction_front;
                preset.capExtinction_back = capExtinction_back;

                preset.texturePath_extinction_front = GetPath(texture_extinction_front);
                preset.texturePath_extinction_back = GetPath(texture_extinction_back);

                preset.extinctionAppearance = (int)extinctionAppearance;

                switch (extinctionAppearance)
                {
                    case GlassExtinctionAppearance.AsApplied:
                        FlipColour(extinction_front, ref extinctionFlipped_front);
                        FlipColour(extinction_back, ref extinctionFlipped_back);
                        break;
                    case GlassExtinctionAppearance.AsItAppears:
                        FlipColour(extinctionFlipped_front, ref extinction_front);
                        FlipColour(extinctionFlipped_back, ref extinction_back);
                        break;
                }

                GlassPreset.SetColour(ref preset.colour_extinction_front, extinction_front);
                GlassPreset.SetColour(ref preset.colour_extinction_back, extinction_back);
                GlassPreset.SetColour(ref preset.colour_extinction_flipped_front, extinctionFlipped_front);
                GlassPreset.SetColour(ref preset.colour_extinction_flipped_back, extinctionFlipped_back);

                preset.extinctionIntensity_front = extinctionMagnitude_front.x;
                preset.extinctionMin_front = extinctionMagnitude_front.y;
                preset.extinctionMax_front = extinctionMagnitude_front.z;

                preset.extinctionIntensity_back = extinctionMagnitude_back.x;
                preset.extinctionMin_back = extinctionMagnitude_back.y;
                preset.extinctionMax_back = extinctionMagnitude_back.z;
            }

            if (features.Aberration())
            {
                preset.enableAberration_front = enableAberration_front;
                preset.enableAberration_back = enableAberration_back;
                preset.enableAberration_both = enableAberration_both;

                preset.capAberration_front = capAberration_front;
                preset.capAberration_back = capAberration_back;

                preset.texturePath_aberration_front = GetPath(texture_aberration_front);
                preset.texturePath_aberration_back = GetPath(texture_aberration_back);

                GlassPreset.SetColour(ref preset.colour_aberration_front, aberration_front);
                GlassPreset.SetColour(ref preset.colour_aberration_back, aberration_back);

                preset.aberrationIntensity_front = aberrationMagnitude_front.x;
                preset.aberrationMin_front = aberrationMagnitude_front.y;
                preset.aberrationMax_front = aberrationMagnitude_front.z;

                preset.aberrationIntensity_back = aberrationMagnitude_back.x;
                preset.aberrationMin_back = aberrationMagnitude_back.y;
                preset.aberrationMax_back = aberrationMagnitude_back.z;
            }

            if (features.Fog())
            {
                preset.option_fog_front = enableFog_front;
                preset.option_fog_back = enableFog_back;
                preset.option_fog_both = enableFog_both;

                GlassPreset.SetColour(ref preset.colour_fog_near_front, fogNear_front);
                GlassPreset.SetColour(ref preset.colour_fog_near_back, fogNear_back);
                GlassPreset.SetColour(ref preset.colour_fog_far_front, fogFar_front);
                GlassPreset.SetColour(ref preset.colour_fog_far_back, fogFar_back);

                preset.fog_magnitude_front = fogMagnitude_front;
                preset.fog_magnitude_back = fogMagnitude_back;
            }

            if (features.Surface())
            {
                preset.glossiness_front = glossiness_front;
                preset.glossiness_back = glossiness_back;
                preset.texturePath_gloss_front = GetPath(texture_gloss_front);
                preset.texturePath_gloss_back = GetPath(texture_gloss_back);

                preset.glow_front = glow_front;
                preset.glow_back = glow_back;
                preset.texturePath_glow_front = GetPath(texture_glow_front);
                preset.texturePath_glow_back = GetPath(texture_glow_back);

                preset.metallic_front = metallic_front;
                preset.metallic_back = metallic_back;
                preset.texturePath_metal_front = GetPath(texture_metal_front);
                preset.texturePath_metal_back = GetPath(texture_metal_back);
            }

            if (features.ZFightRadius())
            {
                preset.zFightingRadius = zFightRadius;
            }

            if (features.Model())
            {
                preset.meshPath = GetMeshPath();
                preset.meshPrimitive = GetMeshPrimitive();
                preset.meshScale = GetMeshScale();
                preset.meshScaleFix = GetMeshScaleFix(preset.meshPrimitive);
            }

            if (features.Depth())
            {
                preset.depthMultiplier = depthMultiplier;
                preset.depthOffset = depthOffset;
                preset.depthQuality_back = depthQuality_back;
                preset.depthQuality_front = depthQuality_front;
                preset.depthQuality_other = depthQuality_other;
            }

            return preset;
        }

        string GetMeshPath()
        {
            MeshFilter foundMeshFilter = GetComponent<MeshFilter>();

            if (foundMeshFilter == null)
                return "";

            Mesh mesh = foundMeshFilter.sharedMesh;
            if (mesh == null)
                return "";

#if UNITY_EDITOR
            return AssetDatabase.GetAssetPath(mesh);
#else
			Debug.Log ("Attempting to call GetMeshPath (Editor funcion) during runtime.");
			return "";
#endif
        }

        GlassPrimitiveType GetMeshPrimitive()
        {
            MeshFilter foundMeshFilter = GetComponent<MeshFilter>();

            if (foundMeshFilter == null)
                return GlassPrimitiveType.cube;

            Mesh mesh = foundMeshFilter.sharedMesh;

            if (mesh == null)
            {
                return GlassPrimitiveType.cube;
            }

            string meshPath = "";

#if UNITY_EDITOR
            meshPath = AssetDatabase.GetAssetPath(mesh);
#else
			Debug.Log ("Attempting to call GetMeshPath (Editor funcion) during runtime.");
#endif

            string meshName = mesh.name;

            if (meshPath.Contains(unityDefaulResourcesPath))
            {
                if (meshName == "Cube")
                    return GlassPrimitiveType.cube;
                else if (meshName == "Sphere")
                    return GlassPrimitiveType.sphere;
                else if (meshName == "Cylinder")
                    return GlassPrimitiveType.cylinder;
                else if (meshName == "Quad")
                    return GlassPrimitiveType.quad;
                else if (meshName == "Plane")
                    return GlassPrimitiveType.plane;
                else if (meshName == "Capsule")
                    return GlassPrimitiveType.capsule;
                else
                    return GlassPrimitiveType.none;
            }
            else {
                return GlassPrimitiveType.none;
            }
        }

        float GetMeshScale()
        {
            return (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3;
        }

        GlassMeshScaleFix GetMeshScaleFix(GlassPrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case GlassPrimitiveType.none:
                    return GlassMeshScaleFix.custom;
                default:
                    return GlassMeshScaleFix.None;
            }
        }

        internal DepthQuality_GlassObject GetDepthQuality(GlassDepthLayer depthLayer)
        {
            switch (depthLayer)
            {
                case GlassDepthLayer.front:
                    return depthQuality_front;
                case GlassDepthLayer.back:
                    return depthQuality_back;
                default:
                case GlassDepthLayer.other:
                    return depthQuality_other;
            }
        }

        public static Texture GetTexture(string texturePath)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture)) as Texture;
#else
			Debug.Log ("Attempting to call GetTexture (Editor function) during runtime.");
			return null;
#endif
        }

        public static string GetPath(Texture texture)
        {
#if UNITY_EDITOR
            return AssetDatabase.GetAssetPath(texture);
#else
			Debug.Log ("Attempting to call GetPath (Editor function) during runtime.");
			return "";
#endif
        }

        #endregion

        #region Glass Manager Settings

        public void CopyDepthNormalTech(GlassManager manager)
        {
            Glass_GlassManager_Settings managerSettings = Glass_GlassManager_Settings.Load(manager.xmlPath, this);

            if (managerSettings == null)
            {
                managerSettings = new Glass_GlassManager_Settings();
                managerSettings.glassPresetName = presetName;
            }

            managerSettings.CopyDepthNormalTech(manager);
            managerSettings.Save(manager.xmlPath);
        }

        public void CopyQuality(GlassManager manager)
        {
            Glass_GlassManager_Settings managerSettings = Glass_GlassManager_Settings.Load(manager.xmlPath, this);

            if (managerSettings == null)
            {
                managerSettings = new Glass_GlassManager_Settings();
                managerSettings.glassPresetName = presetName;
            }

            managerSettings.CopyQuality(manager);
            managerSettings.Save(manager.xmlPath);

            depthQuality_front = GetDepthQuality(manager.depthQuality_Front, depthQuality_front);
            depthQuality_back = GetDepthQuality(manager.depthQuality_Back, depthQuality_front);
            depthQuality_other = GetDepthQuality(manager.depthQuality_Other, depthQuality_front);
        }

        DepthQuality_GlassObject GetDepthQuality(DepthQuality_GlassManager managerDepth, DepthQuality_GlassObject currentDepthQuality)
        {
            switch (managerDepth)
            {
                case DepthQuality_GlassManager.Simple:
                    return DepthQuality_GlassObject.Simple;
                case DepthQuality_GlassManager.Complex:
                    return DepthQuality_GlassObject.Complex;
                default:
                case DepthQuality_GlassManager.ObjectDefined:
                    return currentDepthQuality;
            }
        }



        #endregion

        /*  #region Render Order

        public void CopyRenderOrder(Glass other)
        {
            customRenderOrder = other.customRenderOrder;
            useCustomRenderOrder = other.useCustomRenderOrder;
            renderOrderPreference = other.renderOrderPreference;
        }

        #endregion
    */

        #region Materials

        public Material GenerateMaterial(GlassFace face)
        {
            Material newMaterial = null;
            switch (face)
            {
                case GlassFace.front:
                    newMaterial = new Material(Shader.Find(shaderPath));
                    if (material_front != null)
                    {
                        newMaterial.CopyPropertiesFromMaterial(material_front);
                        //  Workaround for Unity bug (821208)
                        newMaterial.shader = newMaterial.shader;
                    }
                    break;
                case GlassFace.back:
                    newMaterial = new Material(Shader.Find(shaderPath));
                    if (material_back != null)
                    {
                        newMaterial.CopyPropertiesFromMaterial(material_back);
                        //  Workaround for Unity bug (821208)
                        newMaterial.shader = newMaterial.shader;
                    }
                    break;
            }
            return newMaterial;
        }

        #endregion
    }
}
