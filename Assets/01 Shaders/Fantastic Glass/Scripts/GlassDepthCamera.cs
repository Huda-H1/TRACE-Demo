using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace FantasticGlass
{
    #region Depth & Normal Technique enums

    public enum GlassDepthTechnique
    {
        DEPTH_CAM_CUSTOM,
        DEPTH_CAM_UNITY
    }

    public enum GlassFrontDepthTechnique
    {
        DEPTH_FRONT_SHADER_ON,
        DEPTH_FRONT_SHADER_OFF
    }

    public enum GlassNormalTechnique
    {
        NORMAL_CAM_CUSTOM,
        NORMAL_CAM_UNITY,
        NORMAL_WORLD_CAM_SHADER
    }

    #endregion

    public class GlassDepthCamera : MonoBehaviour
    {
        #region Member Variables

        public GlassManager glassManager;
        //
        //Shader glassShader;
        //
        public int layerCamera;
        public int layerGlass;
        //
        public string textureName = "_DepthTexture";
        public RenderTexture renderTexture;
        //public Shader depthShaderBack;
        //public Shader depthShaderFront;
        public Shader depthShader;
        public List<Material> recipientMaterials = new List<Material>();
        public RenderTextureFormat textureFormat = default_textureFormat;
        //RenderTextureFormat.Depth;
        public static RenderTextureFormat default_textureFormat = RenderTextureFormat.ARGBFloat;
        //
        public GlassDepthTechnique depthTechnique = default_depthTechnique;
        public GlassNormalTechnique normalTechnique = default_normalTechnique;
        public GlassFrontDepthTechnique frontDepthTechnique = default_frontDepthTechnique;
        //
#if UNITY_5_4_OR_NEWER
        public static GlassDepthTechnique default_depthTechnique = GlassDepthTechnique.DEPTH_CAM_CUSTOM;
        public static GlassNormalTechnique default_normalTechnique = GlassNormalTechnique.NORMAL_CAM_CUSTOM;
        public static GlassFrontDepthTechnique default_frontDepthTechnique = GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF;
#else
        public static GlassDepthTechnique default_depthTechnique = GlassDepthTechnique.DEPTH_CAM_CUSTOM;
        public static GlassNormalTechnique default_normalTechnique = GlassNormalTechnique.NORMAL_CAM_CUSTOM;
        public static GlassFrontDepthTechnique default_frontDepthTechnique = GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_ON;
#endif
        public int depthTextureAniso = GlassManager.default_depthTextureAA;
        public int depthTextureAA = GlassManager.default_depthTextureAA;
        public FilterMode depthTextureFilterMode = GlassManager.default_depthTextureFilterMode;
        public CameraClearFlags depthTextureClearMode = GlassManager.default_depthTextureClearMode;
        //
        public RenderingPath default_depthRenderingPath = RenderingPath.VertexLit;
        //
        public Camera thisCamera;
        public Camera mainCamera;
        //
        public bool initialised = false;
        //
        //  DEBUGGING DEPTH IN RELEASE BUILD:
        /*
        public bool debugDepthtexture = false;
        public GameObject debugDepthObject = null;
        public float debugDepthObjectCameraDistance = 1;
        public MeshRenderer debugDepthRenderer = null;
        public MeshFilter debugDepthMeshFilter = null;
        public Material debugDepthMaterial = null;
        public Vector3 debugDepthObjectOffset = new Vector3();
        public Vector3 debugDepthObjectScale = new Vector3(0.5f, 0.5f, 0.5f);
        */

        #endregion

        #region Start

        void Start()
        {
            if (glassManager == null)
                glassManager = GlassManager.Instance;
        }

        #endregion

        #region Init

        /*
        public void Initialise(int _layerCamera, int _layerGlass, string depthTextureName, Shader _depthShaderBack, Shader _depthShaderFront, GlassManager _glassManager)
        {
            layerCamera = _layerCamera;
            layerGlass = _layerGlass;
            textureName = depthTextureName;
            depthShaderBack = _depthShaderBack;
            depthShaderFront = _depthShaderFront;
            glassManager = _glassManager;
            //
            depthTextureAA = glassManager.depthTextureAA;
            depthTextureAniso = glassManager.depthTextureAniso;
            depthTextureFilterMode = glassManager.depthTextureFilterMode;
            depthTextureClearMode = glassManager.depthTextureClearMode;
            //
            Initialise_Textures();
            Initialise_Camera();
            Initialise_Materials();
            //
            if (Application.isPlaying)
                initialised = true;
        }
        */

        public void Initialise(int _layerCamera, int _layerGlass, string depthTextureName, Shader _depthShader, GlassManager _glassManager)
        {
            layerCamera = _layerCamera;
            layerGlass = _layerGlass;
            textureName = depthTextureName;
            depthShader = _depthShader;
            glassManager = _glassManager;
            //
            depthTextureAA = glassManager.depthTextureAA;
            depthTextureAniso = glassManager.depthTextureAniso;
            depthTextureFilterMode = glassManager.depthTextureFilterMode;
            depthTextureClearMode = glassManager.depthTextureClearMode;
            //
            Initialise_Textures();
            Initialise_Camera();
            Initialise_Materials();
            //  DEBUGGING DEPTH IN RELEASE BUILD:
            //Initialise_Debug();
            //
            thisCamera.SetReplacementShader(_depthShader, "RenderType");
            //
            if (Application.isPlaying)
                initialised = true;
        }

        //  DEBUGGING DEPTH IN RELEASE BUILD:
        /*
        void Initialise_Debug()
        {
            debugDepthtexture = glassManager.debugDepthtexture;
            debugDepthObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            debugDepthObject.name = "Debug Depth Object '" + textureName + "'";//new GameObject("Debug Depth Object '" + textureName +"'");
            //
            debugDepthObject.transform.position = transform.position + transform.forward * debugDepthObjectCameraDistance + debugDepthObjectOffset;
            debugDepthObject.transform.LookAt(transform);
            debugDepthObject.transform.Rotate(transform.up, 180f);
            //
            debugDepthObject.transform.localScale = debugDepthObjectScale;
            //
            debugDepthObject.transform.parent = transform;
            //
            debugDepthRenderer = debugDepthObject.GetComponent<MeshRenderer>();
            //debugDepthMeshFilter = debugDepthObject.AddComponent<MeshFilter>();
            //debugDepthMeshFilter.sharedMesh = new Mesh()
            debugDepthMaterial = new Material(Shader.Find("Standard"));
            debugDepthMaterial.SetTexture("_MainTex", renderTexture);
            debugDepthRenderer.sharedMaterial = debugDepthMaterial;
        }*/

        public void AddRecipient(Material recipientMaterial)
        {
            if (!recipientMaterials.Contains(recipientMaterial))
                recipientMaterials.Add(recipientMaterial);
        }

        void Initialise_Textures()
        {
            //renderTexture = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 24, textureFormat);
            renderTexture = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 24, textureFormat);
            renderTexture.isPowerOfTwo = false;
            renderTexture.antiAliasing = depthTextureAA;
            renderTexture.anisoLevel = depthTextureAniso;
            renderTexture.filterMode = depthTextureFilterMode;
            //
            glassManager.SetDepthTexture(renderTexture, textureName);
        }

        void Initialise_Materials()
        {
            foreach (Material recipientMaterial in recipientMaterials)
            {
                if (recipientMaterial != null)
                {
                    recipientMaterial.SetTexture(textureName, renderTexture);
                    recipientMaterial.SetFloat(textureName + "Height", Screen.currentResolution.height);
                }
            }
        }

        public void LinkGlass(Glass glass)
        {
            foreach (Material recipientMaterial in glass.materials)
            {
                recipientMaterial.SetTexture(textureName, renderTexture);
                recipientMaterial.SetFloat(textureName + "Height", Screen.currentResolution.height);
            }
        }

        public void LinkMaterial(Material recipientMaterial)
        {
            if (recipientMaterial != null)
            {
                recipientMaterial.SetTexture(textureName, renderTexture);
                recipientMaterial.SetFloat(textureName + "Height", Screen.currentResolution.height);
            }
        }

        void Initialise_Camera()
        {
            if (thisCamera == null)
                thisCamera = GetComponent<Camera>();
            if (thisCamera == null)
                thisCamera = gameObject.AddComponent<Camera>();
            //
            FindMainCamera();
            //
            if (mainCamera != null)
                CopyCameraValues(ref mainCamera, ref thisCamera);
            //	DEPTH TECHNIQUE 1
            //thisCamera.clearFlags = CameraClearFlags.Color;
            //thisCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            //	DEPTH TECHNIQUE 2
            thisCamera.clearFlags = depthTextureClearMode;
            thisCamera.backgroundColor = new Color(0f, 0f, 0f, 1f);
            //
            thisCamera.depthTextureMode = DepthTextureMode.DepthNormals;
            thisCamera.cullingMask = layerCamera;
            thisCamera.renderingPath = default_depthRenderingPath;
            thisCamera.targetTexture = renderTexture;
            //
            thisCamera.enabled = false;
        }

        public void SetDefaultTargetTexture()
        {
            thisCamera.targetTexture = renderTexture;
        }

        void CopyCameraValues(ref Camera from, ref Camera to)
        {
            to.pixelRect = from.pixelRect;
            to.nearClipPlane = from.nearClipPlane;
            to.farClipPlane = from.farClipPlane;
            to.fieldOfView = from.fieldOfView;
            to.aspect = from.aspect;
        }

        void FindMainCamera()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera == null)
            {
                if (Camera.allCamerasCount > 0)
                {
                    mainCamera = Camera.allCameras[0];
                    if (glassManager != null)
                    {
                        if (glassManager.settings != null)
                        {
                            mainCamera.renderingPath = glassManager.settings.optimumCamera_renderingPath;
                            mainCamera.allowHDR = glassManager.settings.optimumCamera_enableHDR;
                        }
                    }
                }
                else {
                    GameObject mainCameraObject = new GameObject("MainCamera");
                    mainCamera = mainCameraObject.AddComponent<Camera>();
                    if (glassManager != null)
                    {
                        if (glassManager.settings != null)
                        {
                            mainCamera.renderingPath = glassManager.settings.optimumCamera_renderingPath;
                            mainCamera.allowHDR = glassManager.settings.optimumCamera_enableHDR;
                        }
                    }
                    Debug.LogWarning("GlassManager: No main camera was found so one was created.");
                }
            }
        }

        #endregion

        #region Render Textures

        public static RenderTexture NewRenderTexture(GlassManager _manager)
        {
            RenderTexture rt = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 24, default_textureFormat);
            rt.isPowerOfTwo = false;
            rt.antiAliasing = _manager.depthTextureAA;
            rt.anisoLevel = _manager.depthTextureAniso;
            rt.filterMode = _manager.depthTextureFilterMode;
            return rt;
        }

        #endregion

        #region Render

        public void RenderDepth()
        {
            if (!initialised)
                return;
            thisCamera.Render();
        }

        public void RenderDepth(RenderTexture destinationRenderTexture)
        {
            if (!initialised)
                return;
            thisCamera.targetTexture = destinationRenderTexture;
            thisCamera.Render();
        }

        #endregion

        #region Depth Length

        public float DepthLength()
        {
            if (!initialised)
                return 0f;
            return thisCamera.farClipPlane - thisCamera.nearClipPlane;
        }

        #endregion

        #region Clear Mode

        public void SetDepthTextureClearMode(CameraClearFlags clearMode)
        {
            if (thisCamera.clearFlags != clearMode)
            {
                depthTextureClearMode = clearMode;
                thisCamera.clearFlags = depthTextureClearMode;
            }
        }

        #endregion

        #region Techniques (Depth & Normal)

        public void SetDepthTechnique(GlassDepthTechnique technique, bool forceUpdate = false)
        {
            if (!forceUpdate)
                if (technique == depthTechnique)
                    return;

            depthTechnique = technique;
            glassManager.depthTechnique = depthTechnique;

            //    TODO: replace this workaround for bug in 5.4 if better fix found
#if UNITY_5_4_OR_NEWER
            if (glassManager.enable54Workaround)
            {
                SetFrontDepthTechnique(GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF);
                //frontDepthTechnique = GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF;
            }
#endif

            Shader.EnableKeyword(DepthKeyword());

            switch (depthTechnique)
            {
                case GlassDepthTechnique.DEPTH_CAM_CUSTOM:
                    switch (normalTechnique)
                    {
                        case GlassNormalTechnique.NORMAL_CAM_UNITY:
                            SetNormalTechnique(GlassNormalTechnique.NORMAL_CAM_CUSTOM);
                            break;
                        default:
                            break;
                    }
                    break;
                case GlassDepthTechnique.DEPTH_CAM_UNITY:
                    switch (normalTechnique)
                    {
                        case GlassNormalTechnique.NORMAL_CAM_CUSTOM:
                            SetNormalTechnique(GlassNormalTechnique.NORMAL_CAM_UNITY);
                            break;
                        default:
                            break;
                    }
                    break;
            }

            switch (depthTechnique)
            {
                case GlassDepthTechnique.DEPTH_CAM_CUSTOM:
                    Shader.DisableKeyword("DEPTH_CAM_UNITY");
                    Shader.EnableKeyword("DEPTH_CAM_CUSTOM");
                    break;
                case GlassDepthTechnique.DEPTH_CAM_UNITY:
                    Shader.DisableKeyword("DEPTH_CAM_CUSTOM");
                    Shader.EnableKeyword("DEPTH_CAM_UNITY");
                    break;
            }
        }

        public void SetFrontDepthTechnique(GlassFrontDepthTechnique technique, bool forceUpdate = false)
        {
            if (!forceUpdate)
                if (technique == frontDepthTechnique)
                    return;
			
			//    TODO: replace this workaround for bug in 5.4 if better fix found
            #if UNITY_5_4_OR_NEWER
            if (glassManager.enable54Workaround)
            {
                frontDepthTechnique = GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF;
                glassManager.frontDepthTechnique = frontDepthTechnique;
            }
            else
            {
#endif
                frontDepthTechnique = technique;
                glassManager.frontDepthTechnique = frontDepthTechnique;
#if UNITY_5_4_OR_NEWER
            }
#endif


            switch (frontDepthTechnique)
            {
                case GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_ON:
                    Shader.DisableKeyword("DEPTH_FRONT_SHADER_OFF");
                    Shader.EnableKeyword("DEPTH_FRONT_SHADER_ON");
                    break;
                case GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF:
                    Shader.DisableKeyword("DEPTH_FRONT_SHADER_ON");
                    Shader.EnableKeyword("DEPTH_FRONT_SHADER_OFF");
                    break;
            }
        }

        public void SetNormalTechnique(GlassNormalTechnique technique, bool forceUpdate = false)
        {
            if (!forceUpdate)
                if (technique == normalTechnique)
                    return;

            normalTechnique = technique;
            glassManager.normalTechnique = normalTechnique;
			
			//    TODO: replace this workaround for bug in 5.4 if better fix found
            #if UNITY_5_4_OR_NEWER
            if (glassManager.enable54Workaround)
            {
                SetFrontDepthTechnique(GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF);
                //frontDepthTechnique = GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF;
            }
#endif

            Shader.EnableKeyword(NormalKeyword());

            switch (normalTechnique)
            {
                case GlassNormalTechnique.NORMAL_CAM_CUSTOM:
                    switch (depthTechnique)
                    {
                        case GlassDepthTechnique.DEPTH_CAM_UNITY:
                            SetDepthTechnique(GlassDepthTechnique.DEPTH_CAM_CUSTOM);
                            break;
                        default:
                            break;
                    }
                    break;
                case GlassNormalTechnique.NORMAL_CAM_UNITY:
                    switch (depthTechnique)
                    {
                        case GlassDepthTechnique.DEPTH_CAM_CUSTOM:
                            SetDepthTechnique(GlassDepthTechnique.DEPTH_CAM_UNITY);
                            break;
                        default:
                            break;
                    }
                    break;
                case GlassNormalTechnique.NORMAL_WORLD_CAM_SHADER:
                    break;
            }

            switch (normalTechnique)
            {
                case GlassNormalTechnique.NORMAL_CAM_CUSTOM:
                    Shader.DisableKeyword("NORMAL_CAM_UNITY");
                    Shader.DisableKeyword("NORMAL_WORLD_CAM_SHADER");
                    Shader.EnableKeyword("NORMAL_CAM_CUSTOM");
                    break;
                case GlassNormalTechnique.NORMAL_CAM_UNITY:
                    Shader.DisableKeyword("NORMAL_CAM_CUSTOM");
                    Shader.DisableKeyword("NORMAL_WORLD_CAM_SHADER");
                    Shader.EnableKeyword("NORMAL_CAM_UNITY");
                    break;
                case GlassNormalTechnique.NORMAL_WORLD_CAM_SHADER:
                    Shader.DisableKeyword("NORMAL_CAM_CUSTOM");
                    Shader.DisableKeyword("NORMAL_CAM_UNITY");
                    Shader.EnableKeyword("NORMAL_WORLD_CAM_SHADER");
                    break;
            }
        }

        public static GlassDepthTechnique DepthTechFromNormal(GlassDepthTechnique currentDepthTechnique, GlassNormalTechnique inputNormalTechnique)
        {
            GlassDepthTechnique outputDepthTechnique = currentDepthTechnique;

            switch (inputNormalTechnique)
            {
                case GlassNormalTechnique.NORMAL_CAM_CUSTOM:
                    switch (currentDepthTechnique)
                    {
                        case GlassDepthTechnique.DEPTH_CAM_UNITY:
                            outputDepthTechnique = GlassDepthTechnique.DEPTH_CAM_CUSTOM;
                            break;
                        default:
                            break;
                    }
                    break;
                case GlassNormalTechnique.NORMAL_CAM_UNITY:
                    switch (currentDepthTechnique)
                    {
                        case GlassDepthTechnique.DEPTH_CAM_CUSTOM:
                            outputDepthTechnique = GlassDepthTechnique.DEPTH_CAM_UNITY;
                            break;
                        default:
                            break;
                    }
                    break;
                case GlassNormalTechnique.NORMAL_WORLD_CAM_SHADER:
                    break;
            }

            return outputDepthTechnique;
        }

        public static GlassNormalTechnique NormalTechFromDepth(GlassNormalTechnique currentNormalTechnique, GlassDepthTechnique inputDepthTechnique)
        {
            GlassNormalTechnique outputNormalTechnique = currentNormalTechnique;

            switch (inputDepthTechnique)
            {
                case GlassDepthTechnique.DEPTH_CAM_CUSTOM:
                    switch (currentNormalTechnique)
                    {
                        case GlassNormalTechnique.NORMAL_CAM_UNITY:
                            outputNormalTechnique = GlassNormalTechnique.NORMAL_CAM_CUSTOM;
                            break;
                        default:
                            break;
                    }
                    break;
                case GlassDepthTechnique.DEPTH_CAM_UNITY:
                    switch (currentNormalTechnique)
                    {
                        case GlassNormalTechnique.NORMAL_CAM_CUSTOM:
                            outputNormalTechnique = GlassNormalTechnique.NORMAL_CAM_UNITY;
                            break;
                        default:
                            break;
                    }
                    break;
            }

            return outputNormalTechnique;
        }

        public static void DepthNormalTechFromFrontDepth(GlassFrontDepthTechnique frontTechIn, ref GlassDepthTechnique depthTechOut, ref GlassNormalTechnique normalTechOut, bool _enable54Workaround = false)
		{
			//    TODO: replace this workaround for bug in 5.4 if better fix found
			#if UNITY_5_4_OR_NEWER
            if (_enable54Workaround)
            {
                //	No need to change anything as long as front depth is off
            }
#endif
        }

        public static GlassFrontDepthTechnique FrontDepthTechFromDepth(GlassFrontDepthTechnique currentFrontDepthTechnique, GlassDepthTechnique inputDepthTechnique, bool _enable54Workaround = false)
		{
			//    TODO: replace this workaround for bug in 5.4 if better fix found
			#if UNITY_5_4_OR_NEWER
            if (_enable54Workaround)
            {
                return GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF;
            }
#endif
            return currentFrontDepthTechnique;
        }

        public static GlassFrontDepthTechnique FrontDepthTechFromNormal(GlassFrontDepthTechnique currentFrontDepthTechnique, GlassNormalTechnique inputNormalTechnique, bool _enable54Workaround = false)
		{
			//    TODO: replace this workaround for bug in 5.4 if better fix found
			#if UNITY_5_4_OR_NEWER
            if (_enable54Workaround)
            {
                return GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF;
            }
#endif
            return currentFrontDepthTechnique;
        }

        public string DepthKeyword()
        {
            switch (depthTechnique)
            {
                case GlassDepthTechnique.DEPTH_CAM_CUSTOM:
                    return "DEPTH_CAM_CUSTOM";
                case GlassDepthTechnique.DEPTH_CAM_UNITY:
                    return "DEPTH_CAM_UNTY";
            }

            return "";
        }

        public string FrontDepthKeyword()
        {
            switch (frontDepthTechnique)
            {
                case GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_OFF:
                    return "DEPTH_FRONT_SHADER_OFF";
                case GlassFrontDepthTechnique.DEPTH_FRONT_SHADER_ON:
                    return "DEPTH_FRONT_SHADER_ON";
            }

            return "";
        }

        public string NormalKeyword()
        {
            switch (normalTechnique)
            {
                case GlassNormalTechnique.NORMAL_CAM_CUSTOM:
                    return "NORMAL_CAM_CUSTOM";
                case GlassNormalTechnique.NORMAL_CAM_UNITY:
                    return "NORMAL_CAM_UNITY";
                case GlassNormalTechnique.NORMAL_WORLD_CAM_SHADER:
                    return "NORMAL_WORLD_CAM_SHADER";
            }

            return "";
        }

        #endregion
    }
}
