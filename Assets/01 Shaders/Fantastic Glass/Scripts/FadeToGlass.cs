using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace FantasticGlass
{

    public class FadeToGlass : MonoBehaviour
    {
        #region Member Variables

        [Header("Glass")]
        public List<Glass> glassToFade = new List<Glass>();
        //public bool fadeMatchingGlass = true;

        [Header("Fade")]
        public GlassFade_FadeType fadeType = GlassFade_FadeType.FadeIn;
        public float delay = 0f;
        float delayCounter = 0f;
        bool delayed = true;
        public float duration = 1f;
        float durationCounter = 0f;
        float fadeInAmount = 0f;

        [Header("Materials")]
        public List<Material> nonGlassMaterials = new List<Material>();
        Dictionary<Glass, List<Material>> nonGlassMaterialsDic = new Dictionary<Glass, List<Material>>();
        Dictionary<Glass, List<Material>> glassMaterialsDic = new Dictionary<Glass, List<Material>>();
        Dictionary<Glass, List<Material>> allMaterialsDic = new Dictionary<Glass, List<Material>>();
        Dictionary<Material, int> originalRenderQueue = new Dictionary<Material, int>();
        public bool destroyHiddenMaterialsOnFinish = true;
        public bool moveNonGlassToFront = true;
        //
        public StandardShaderRenderMode nonGlassFading = StandardShaderRenderMode.Fade;
        public StandardShaderRenderMode nonGlassFadedIn = StandardShaderRenderMode.Opaque;
        public StandardShaderRenderMode nonGlassFadedOut = StandardShaderRenderMode.Fade;

        [Header("Execution")]
        public bool playOnAwake = false;
        bool paused = true;
        //bool createOnAwake = true;

        [Header("Physics")]
        public GlassFade_GravityChange gravity_DelayStart = GlassFade_GravityChange.noChange;
        public GlassFade_GravityChange gravity_FadeStart = GlassFade_GravityChange.noChange;
        public GlassFade_GravityChange gravity_FadeFinish = GlassFade_GravityChange.enableGravity;

        #endregion


        #region Start

        public void Start()
        {
            Setup();
            if (playOnAwake)
                Play();
        }

        /*
        public void Start()
        {           
            lock (this)
            {
                if (fadeMatchingGlass)
                {
                    List<Glass> otherGlass = new List<Glass>(GameObject.FindObjectsOfType<Glass>());
                    otherGlass.Remove(glass);

                    List<FadeToGlass> faders = new List<FadeToGlass>(GameObject.FindObjectsOfType<FadeToGlass>());
                    faders.Remove(this);

                    foreach (FadeToGlass fader in faders)
                    {
                        otherGlass.Remove(fader.glass);
                    }

                    for (int i = otherGlass.Count - 1; i >= 0; i--)
                    {
                        Glass otherGlassToCompare = otherGlass[i];
                        if (glass.Matches(otherGlassToCompare))
                            continue;
                        otherGlass.Remove(otherGlassToCompare);
                    }

                    foreach (Glass otherGlassToFade in otherGlass)
                    {
                        Create(otherGlassToFade, this);
                    }
                }
            }
            //
            if (playOnAwake)
            {
                Play();
            }
        }
        */

        /*
        public void EnabledInEditor()
        {
            createOnAwake = true;
        }
        */

        #endregion


        #region Setup

        void Setup()
        {
            FindMatchingGlass();

            ClearPrivateData();

            UpdateGlassMaterials();

            foreach (Glass glass in glassToFade)
            {
                StoreMaterials(glass);
            }

            StoreOriginalRenderQueues();
        }

        void FindMatchingGlass()
        {
            List<Glass> originalGlass = new List<Glass>();
            foreach (Glass glass in glassToFade)
            {
                bool foundMatch = false;
                foreach (Glass otherGlass in originalGlass)
                {
                    if (otherGlass.Matches(glass))
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                {
                    originalGlass.Add(glass);
                }
            }
            foreach (Glass foundGlass in GameObject.FindObjectsOfType<Glass>())
            {
                if (glassToFade.Contains(foundGlass))
                    continue;
                foreach (Glass otherGlass in originalGlass)
                {
                    if (otherGlass.Matches(foundGlass))
                    {
                        glassToFade.Add(foundGlass);
                        break;
                    }
                }
            }
        }

        void ClearPrivateData()
        {
            nonGlassMaterialsDic.Clear();
            glassMaterialsDic.Clear();
            allMaterialsDic.Clear();
        }

        void UpdateGlassMaterials()
        {
            foreach (Glass glass in glassToFade)
            {
                glass.UpdateRendererMaterials();
            }
        }

        void StoreMaterials(Glass glass)
        {
            StoreGlassMaterials(glass);
            StoreNonGlassMaterials(glass);
            StoreCombinedMaterials(glass);
        }

        void StoreGlassMaterials(Glass glass)
        {
            if (!glassMaterialsDic.ContainsKey(glass))
                glassMaterialsDic.Add(glass, new List<Material>());

            foreach (Material m in glass.GlassMaterials())
            {
                glassMaterialsDic[glass].Add(m);
            }
        }

        void StoreNonGlassMaterials(Glass glass)
        {
            if (!nonGlassMaterialsDic.ContainsKey(glass))
                nonGlassMaterialsDic.Add(glass, new List<Material>());

            foreach (Material m in glass.NonGlassMaterials())
            {
                nonGlassMaterialsDic[glass].Add(m);
            }

            foreach (Material m in nonGlassMaterials)
            {
                if (!nonGlassMaterialsDic[glass].Contains(m))
                    nonGlassMaterialsDic[glass].Add(m);
            }
        }

        void StoreCombinedMaterials(Glass glass)
        {
            if (!allMaterialsDic.ContainsKey(glass))
                allMaterialsDic.Add(glass, new List<Material>());

            allMaterialsDic[glass].AddRange(glassMaterialsDic[glass]);
            allMaterialsDic[glass].AddRange(nonGlassMaterialsDic[glass]);
        }

        /*
        public void SetGlass(Glass glassToFade)
        {
            Setup(glassToFade, nonGlassMaterials.ToArray(), duration, delay);
        }
        */

        /*
        void Setup_Basic(Glass glassToFade, float fadeDuration = 1f, float delayBeforeFade = 0f)
        {
            duration = fadeDuration;
            delay = delayBeforeFade;
            glass = glassToFade;
            //
            if (glass != null)
            {
                objectToFade = glass.GetComponentInChildren<Renderer>();
            }
        }
        */

        /*
        void Setup(Glass glassToFade, Material[] otherMaterials, float fadeDuration = 1f, float delayBeforeFade = 0f)
        {
            Setup_Basic(glassToFade, fadeDuration, delayBeforeFade);
            //
            if (otherMaterials != null)
            {
                nonGlassMaterials.Clear();
                foreach (Material m in otherMaterials)
                {
                    nonGlassMaterials.Add(m);
                }
            }
            //
            glass.UpdateRendererMaterials();
            //
            foreach (Material m in objectToFade.sharedMaterials)
            {
                if (glass.materials.Contains(m))
                {
                    if (!glassMaterials.Contains(m))
                    {
                        glassMaterials.Add(m);
                    }
                }
                else
                {
                    if (!nonGlassMaterials.Contains(m))
                    {
                        nonGlassMaterials.Add(m);
                    }
                }
                //
                if (!allMaterials.Contains(m))
                {
                    allMaterials.Add(m);
                }
            }
            //
            foreach (Material m in nonGlassMaterials)
            {
                if (!allMaterials.Contains(m))
                    allMaterials.Add(m);
            }
            //
            try
            {
                StoreOriginalRenderQueue();
            }
            catch (Exception e)
            {
                Debug.Log("Error in storing original render queue '" + e.Message + "'");
            }
            //
            //objectToFade.sharedMaterials = allMaterials.ToArray();
            objectToFade.sharedMaterials = GetArray(allMaterials);
            //
            delayCounter = 0f;
            durationCounter = 0f;
            EnsureMaterials();
            //
            switch (fadeType)
            {
                case GlassFade_FadeType.FadeOut:
                    Update_Fade_Out();
                    break;
                default:
                case GlassFade_FadeType.FadeIn:
                    Update_Fade_In();
                    break;
            }
            //
            MoveNonGlassToRenderQueueFront();
        }
        */

        void AssignAllMaterials()
        {
            foreach (Glass glass in glassToFade)
            {
                glass.GetComponent<Renderer>().sharedMaterials = GetAllMaterialArray(glass);
            }
        }

        void StoreOriginalRenderQueues()
        {
            foreach (List<Material> materials in allMaterialsDic.Values)
            {
                foreach (Material m in materials)
                {
                    if (!originalRenderQueue.ContainsKey(m))
                        originalRenderQueue.Add(m, m.renderQueue);
                    else
                        originalRenderQueue[m] = m.renderQueue;
                }
            }
        }

        void MoveNonGlassToRenderQueueFront()
        {
            int foremostGlassRenderQueue = int.MinValue;
            foreach (List<Material> materials in glassMaterialsDic.Values)
            {
                foreach (Material m in materials)
                {
                    foremostGlassRenderQueue = Mathf.Max(m.renderQueue, foremostGlassRenderQueue);
                }
            }
            foreach (List<Material> materials in nonGlassMaterialsDic.Values)
            {
                foreach (Material m in materials)
                {
                    if (m.renderQueue < foremostGlassRenderQueue)
                    {
                        m.renderQueue += foremostGlassRenderQueue;
                    }
                }
            }
        }

        void RestoreRenderQueues()
        {
            foreach (List<Material> materials in nonGlassMaterialsDic.Values)
            {
                foreach (Material m in materials)
                {
                    m.renderQueue = originalRenderQueue[m];
                }
            }
        }

        /*
        void Setup(Glass glassToFade, float fadeDuration = 1f, float delayBeforeFade = 0f)
        {
            Setup_Basic(glassToFade, fadeDuration, delayBeforeFade);
            //
            nonGlassMaterials.Clear();
            //
            foreach (Material m in objectToFade.sharedMaterials)
            {
                nonGlassMaterials.Add(m);
            }
        }
        */

        /*
        void Setup(Glass glassToFade, FadeToGlass faderToCopy)
        {
            fadeMatchingGlass = false;

            Setup(glassToFade, faderToCopy.nonGlassMaterials.ToArray(), faderToCopy.duration, faderToCopy.delay);

            gravityState_Start = faderToCopy.gravityState_Start;
            gravityState_FadeStart = faderToCopy.gravityState_FadeStart;
            gravityState_FadeFinish = faderToCopy.gravityState_FadeFinish;

            fadeType = faderToCopy.fadeType;

            removeHiddenMaterials = faderToCopy.removeHiddenMaterials;

            playOnAwake = faderToCopy.playOnAwake;
        }
        */

        #endregion


        #region Create (static)

        /*
        public static FadeToGlass Create(Glass glassToFade)
        {
            if (glassToFade == null)
                return null;
            FadeToGlass fadeInstance = glassToFade.gameObject.AddComponent<FadeToGlass>();
            fadeInstance.createOnAwake = false;
            return fadeInstance;
        }
        */

        /*
        public static FadeToGlass Create(Glass glassToFade, FadeToGlass faderToCopy)
        {
            if (glassToFade == null)
                return null;
            FadeToGlass fadeInstance = glassToFade.gameObject.AddComponent<FadeToGlass>();
            fadeInstance.createOnAwake = false;
            fadeInstance.Setup(glassToFade, faderToCopy);
            if (fadeInstance.playOnAwake)
            {
                fadeInstance.Start();
            }
            return fadeInstance;
        }
        */

        #endregion

        #region FadeIn / Fade Out (static)

        /// <summary>
        /// Fades the Glass in. NOTE: You do not need to do anything with the returned FadeToGlass object.
        /// </summary>
        /// <returns>Fade To Glass object.</returns>
        /*
        public static FadeToGlass FadeIn(Material[] fadeInFrom, Glass fadeInTo, float fadeInDuration = 1f, float delayBeforeFadeIn = 0f)
        {
            FadeToGlass ftg = Create(fadeInTo);
            ftg.fadeType = GlassFade_FadeType.FadeIn;
            ftg.Setup(fadeInTo, fadeInFrom, fadeInDuration, delayBeforeFadeIn);
            return ftg;
        }
        */

        /// <summary>
        /// Fades the Glass Out. NOTE: You do not need to do anything with the returned FadeToGlass object.
        /// </summary>
        /// <returns>Fade To Glass object.</returns>
        /*
        public static FadeToGlass FadeOut(Glass fadeOutFrom, Material[] fadeOutTo, float fadeOutDuration = 1f, float delayBeforeFadeOut = 0f)
        {
            FadeToGlass ftg = Create(fadeOutFrom);
            ftg.fadeType = GlassFade_FadeType.FadeOut;
            ftg.Setup(fadeOutFrom, fadeOutTo, fadeOutDuration, delayBeforeFadeOut);
            return ftg;
        }
        */

        /// <summary>
        /// Fades the Glass in. NOTE: You do not need to do anything with the returned FadeToGlass object.
        /// </summary>
        /// <returns>Fade To Glass object.</returns>
        /*
        public static FadeToGlass FadeIn(Glass glassToFadeIn, float fadeInDuration = 1f, float delayBeforeFadeIn = 0f)
        {
            FadeToGlass ftg = Create(glassToFadeIn);
            ftg.Setup(glassToFadeIn, fadeInDuration, delayBeforeFadeIn);
            return ftg;
        }
        */

        /// <summary>
        /// Fades the Glass Out. NOTE: You do not need to do anything with the returned FadeToGlass object.
        /// </summary>
        /// <returns>Fade To Glass object.</returns>
        /*
        public static FadeToGlass FadeOut(Glass glassToFadeOut, float fadeOutDuration = 1f, float delayBeforeFadeOut = 0f)
        {
            FadeToGlass ftg = Create(glassToFadeOut);
            ftg.Setup(glassToFadeOut, fadeOutDuration, delayBeforeFadeOut);
            return ftg;
        }
        */

        #endregion


        #region Play / Pause / Stop

        public void Play()
        {
            RestoreRenderQueues();
            MoveNonGlassToRenderQueueFront();

            SetNonGlassBlendMode_Play();

            paused = false;

            SetGravity(gravity_DelayStart);

            AssignAllMaterials();

            StartCoroutines();
        }

        public void Resume()
        {
            Play();
        }

        public void Pause()
        {
            paused = true;
        }

        public void Stop()
        {
            paused = true;

            delayCounter = 0f;
            durationCounter = 0f;

            SetGravity(gravity_FadeFinish);
            RestoreRenderQueues();
            SetNonGlassBlendMode_Stop();
        }

        #endregion


        #region Complete - Delay

        void DelayComplete()
        {
            durationCounter = delayCounter - delay; //	account for any time past the delay
            delay = 0f;
            delayed = false;
            Fade_Started();
        }

        #endregion


        #region Coroutines - Start / Stop

        void StartCoroutines()
        {
            StartCoroutine(Update_Delay());
            StartCoroutine(Update_Duration());
            StartCoroutine(Update_Fade());
        }

        void StopCoroutines()
        {
            StartCoroutine(Update_Delay());
            StartCoroutine(Update_Duration());
            StartCoroutine(Update_Fade());
        }

        #endregion

        #region Coroutines - Updates

        IEnumerator Update_Delay()
        {
            while (true)
            {
                if (!paused)
                {
                    if (delayed)
                    {
                        /*
                        delayCounter += Time.deltaTime;
                        switch (fadeType)
                        {
                            case GlassFade_FadeType.FadeIn:
                                Update_Fade_In();
                                break;
                            case GlassFade_FadeType.FadeOut:
                                Update_Fade_Out();
                                break;
                        }
                        if (delayCounter >= delay)
                        {
                            DelayComplete();
                        }
                        */
                        delayCounter += Time.deltaTime;
                        if (EnsureMaterials())
                        {
                            switch (fadeType)
                            {
                                case GlassFade_FadeType.FadeIn:
                                    Update_Fade_In();
                                    break;
                                case GlassFade_FadeType.FadeOut:
                                    Update_Fade_Out();
                                    break;
                            }
                        }
                        if (delayCounter >= delay)
                        {
                            DelayComplete();
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator Update_Duration()
        {
            while (true)
            {
                if (!paused)
                {
                    if (!delayed)
                    {
                        durationCounter += Time.deltaTime;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator Update_Fade()
        {
            while (true)
            {
                if (!paused)
                {
                    if (!delayed)
                    {
                        EnsureMaterials();
                        switch (fadeType)
                        {
                            case GlassFade_FadeType.FadeIn:
                                Update_Fade_In();
                                break;
                            case GlassFade_FadeType.FadeOut:
                                Update_Fade_Out();
                                break;
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }

        #endregion


        #region Gravity State

        void SetGravity(GlassFade_GravityChange newGravityState)
        {
            switch (newGravityState)
            {
                case GlassFade_GravityChange.enableGravity:
                    EnableGravity();
                    break;
                case GlassFade_GravityChange.disableGravity:
                    DisableGravity();
                    break;
                default:
                case GlassFade_GravityChange.noChange:
                    break;
            }
        }

        #endregion


        #region Fade - Started

        void Fade_Started()
        {
            SetGravity(gravity_FadeStart);
            SetNonGlassBlendMode_Fading();
        }

        void EnableGravity(bool enableGravity = true)
        {
            foreach (Glass glass in glassToFade)
            {
                foreach (Rigidbody rb in glass.GetComponentsInChildren<Rigidbody>())
                {
                    rb.useGravity = enableGravity;
                }
            }
        }

        void DisableGravity()
        {
            EnableGravity(false);
        }

        #endregion

        #region Fade - Update

        void Update_Fade_In()
        {
            fadeInAmount = durationCounter / duration;
            SetFade(fadeInAmount);
            if (fadeInAmount >= 1f)
            {
                FadeIn_Complete();
            }
        }

        void Update_Fade_Out()
        {
            fadeInAmount = 1f - (durationCounter / duration);
            SetFade(fadeInAmount);
            if (fadeInAmount <= 0f)
            {
                FadeOut_Complete();
            }
        }

        void SetFade(float fadeInAmount)
        {
            if (fadeInAmount > 1f)
            {
                fadeInAmount = 1f;
            }
            else if (fadeInAmount < 0f)
            {
                fadeInAmount = 0f;
            }
            SetFade(fadeInAmount, 1f - fadeInAmount);
        }

        #endregion

        #region Fade - Set

        public void SetFade(float glassFadeAmount, float nonGlassFadeAmount)
        {
            foreach (List<Material> materials in glassMaterialsDic.Values)
            {
                foreach (Material m in materials)
                {
                    m.SetFloat("_Alpha", glassFadeAmount);
                }
            }
            foreach (List<Material> materials in nonGlassMaterialsDic.Values)
            {
                foreach (Material m in materials)
                {
                    m.SetFloat("_Alpha", nonGlassFadeAmount);
                    m.color = new Color(m.color.r, m.color.g, m.color.b, 1f - glassFadeAmount);
                }
            }
        }

        #endregion

        #region Material Checks

        bool MissingMaterials()
        {
            /*
            List<Material> activeMaterials = new List<Material>();
            try
            {
                activeMaterials.AddRange(objectToFade.sharedMaterials);
            }
            catch (Exception e)
            {
                Debug.Log("Error in finding missing materials: '" + e.Message);
            }
            foreach (Material m in allMaterials)
            {
                if (!activeMaterials.Contains(m))
                {
                    return true;
                }
            }
            */
            foreach (Glass glass in glassToFade)
            {
                Renderer glassRenderer = glass.GetComponent<Renderer>();
                List<Material> activeMaterials = new List<Material>(glassRenderer.sharedMaterials);
                foreach (Material intendedMaterial in allMaterialsDic[glass])
                {
                    if (!activeMaterials.Contains(intendedMaterial))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool EnsureMaterials()
        {
            if (MissingMaterials())
            {
                //objectToFade.sharedMaterials = GetArray(allMaterials);
                AssignAllMaterials();
                return true;
            }
            return false;
        }

        #endregion

        #region Fade - Complete

        void FadeIn_Complete()
        {
            Stop();
            if (destroyHiddenMaterialsOnFinish)
            {
                foreach (Glass glass in glassToFade)
                {
                    glass.GetComponent<Renderer>().sharedMaterials = GetGlassMaterialArray(glass);
                }
                //objectToFade.sharedMaterials = GetArray(glassMaterials);
            }
            SetNonGlassBlendMode_FadedOut();
            Fade_Complete();
        }

        void FadeOut_Complete()
        {
            Stop();
            if (destroyHiddenMaterialsOnFinish)
            {
                foreach (Glass glass in glassToFade)
                {
                    glass.GetComponent<Renderer>().sharedMaterials = GetNonGlassMaterialArray(glass);
                }
                //objectToFade.sharedMaterials = GetArray(nonGlassMaterials);
            }
            SetNonGlassBlendMode_FadedIn();
            Fade_Complete();
        }

        void Fade_Complete()
        {
            RestoreRenderQueues();
            SetGravity(gravity_FadeFinish);
        }

        #endregion

        #region Helpers

        Material[] GetGlassMaterialArray(Glass glass)
        {
            return GetArray(glassMaterialsDic[glass]);
        }

        Material[] GetNonGlassMaterialArray(Glass glass)
        {
            return GetArray(nonGlassMaterialsDic[glass]);
        }

        Material[] GetAllMaterialArray(Glass glass)
        {
            return GetArray(allMaterialsDic[glass]);
        }

        static Material[] GetArray(List<Material> materialList)
        {
            Material[] mArray = new Material[materialList.Count];
            for (int i = 0; i < materialList.Count; i++)
            {
                mArray[i] = materialList[i];
            }
            return mArray;
        }

        #endregion

        #region Non Glass Blend Mode

        void SetNonGlassBlendMode_Play()
        {
            SetNonGlassBlendMode_Fading();
        }

        void SetNonGlassBlendMode_Stop()
        {
            switch (fadeType)
            {
                case GlassFade_FadeType.FadeIn:
                    SetNonGlassBlendMode_FadedIn();
                    break;
                case GlassFade_FadeType.FadeOut:
                    SetNonGlassBlendMode_FadedOut();
                    break;
            }
        }

        void SetNonGlassBlendMode_Fading()
        {
            foreach (Material m in nonGlassMaterials)
            {
                SetNonGlassBlendMode(nonGlassFading, m);
            }
        }

        void SetNonGlassBlendMode_FadedIn()
        {
            foreach (Material m in nonGlassMaterials)
            {
                SetNonGlassBlendMode(nonGlassFadedIn, m, true);
            }
        }

        void SetNonGlassBlendMode_FadedOut()
        {
            foreach (Material m in nonGlassMaterials)
            {
                SetNonGlassBlendMode(nonGlassFadedOut, m, true);
            }
        }

        void SetNonGlassBlendMode(StandardShaderRenderMode blendMode, Material sharedMaterial, bool finalBlendMode = false)
        {
            switch (blendMode)
            {
                case StandardShaderRenderMode.Opaque:
                    sharedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    sharedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    sharedMaterial.SetInt("_ZWrite", 1);
                    sharedMaterial.DisableKeyword("_ALPHATEST_ON");
                    sharedMaterial.DisableKeyword("_ALPHABLEND_ON");
                    sharedMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    if (finalBlendMode)
                        sharedMaterial.renderQueue = -1;
                    sharedMaterial.SetInt("_Mode", 0);
                    break;
                case StandardShaderRenderMode.Cutout:
                    sharedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    sharedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    sharedMaterial.SetInt("_ZWrite", 1);
                    sharedMaterial.EnableKeyword("_ALPHATEST_ON");
                    sharedMaterial.DisableKeyword("_ALPHABLEND_ON");
                    sharedMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    if (finalBlendMode)
                        sharedMaterial.renderQueue = 2450;
                    sharedMaterial.SetInt("_Mode", 1);
                    break;
                case StandardShaderRenderMode.Fade:
                    sharedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    sharedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    sharedMaterial.SetInt("_ZWrite", 0);
                    sharedMaterial.DisableKeyword("_ALPHATEST_ON");
                    sharedMaterial.EnableKeyword("_ALPHABLEND_ON");
                    sharedMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    if (finalBlendMode)
                        sharedMaterial.renderQueue = 3000;
                    sharedMaterial.SetInt("_Mode", 2);
                    break;
                case StandardShaderRenderMode.Transparent:
                    sharedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    sharedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    sharedMaterial.SetInt("_ZWrite", 0);
                    sharedMaterial.DisableKeyword("_ALPHATEST_ON");
                    sharedMaterial.DisableKeyword("_ALPHABLEND_ON");
                    sharedMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    if (finalBlendMode)
                        sharedMaterial.renderQueue = 3000;
                    sharedMaterial.SetInt("_Mode", 3);
                    break;
                case StandardShaderRenderMode.NoChange:
                default:
                    break;
            }
        }
        #endregion


        #region OnDestroy

        void OnDestroy()
        {
            RestoreRenderQueues();
        }

        #endregion
    }

}