// Sun Rays by Funly, LLC
// Website: https://funly.io
// Author: Jason Ederle - jason@funly.io

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SunRays
{
  // Image effect that creates sun rays (aka God rays) on a camera.
  // You can create zones that will enable or disable the image effect by
  // adding the SunRaysZone.cs script to an object in your scene and scaling it
  // to the size you want to enable the feature on. If there are no zones setup
  // the image effect will always be enabled. 
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  public class SunRaysImageEffect : MonoBehaviour
  {
    public enum EffectResolution
    {
      High = 1,
      Medium = 2,
      Low = 4,
    }

    // Public.

    [Tooltip("Transform of a directional light, or some other object where the rotation can be used to determine light directionality.")]
    public Transform lightTransform;

    [Tooltip("Duration in seconds to crossfade between zones.")]
    public float transitionDuration = 1.5f;

    [Tooltip("Resolutoin of the effect framebuffer. Higher resolutions produce sharper more stable sun rays, but may be slower on some hardware.")]
    public EffectResolution resolution = EffectResolution.High;

    [Tooltip("Default settings used when camera isn't inside a SunRayZone.")]
    public SunRaysSettings globalZoneSettings = new SunRaysSettings();

    // Private.

    private Material m_LightMat;
    private List<SunRaysZone> m_Zones = new List<SunRaysZone>();
    private const int m_BlurPasses = 2;
    private float m_ZoneTransitionBeginTime = -100;
    private BlendedZoneSettings m_BlendedSettings;
    private bool m_DidRenderFirstFrame = false;
    private const string k_BlendModeAdditiveKeyword = "BLEND_MODE_ADDITIVE";

    // Editor script managed values.

    [SerializeField, HideInInspector]
    public Shader lightShader;

    [SerializeField, HideInInspector]
    public bool didConfigureDefaultValues = false;

    public void Awake()
    {
      m_BlendedSettings = new BlendedZoneSettings(globalZoneSettings, globalZoneSettings);
    }

    void Update()
    {
      if (m_BlendedSettings == null)
      {
        m_BlendedSettings = new BlendedZoneSettings(globalZoneSettings, globalZoneSettings);
      }

      ContinueAnimatingZoneTransition();
    }

    public void EnteredZone(SunRaysZone zone)
    {
      if (!zone)
      {
        return;
      }

      // Make sure we can't ever get a duplictae enter event.
      m_Zones.Remove(zone);
      m_Zones.Add(zone);

      // Transition into this zone.
      TransitionToZoneSettings(zone.zoneSettings, m_DidRenderFirstFrame);
    }

    public void ExitedZone(SunRaysZone zone)
    {
      if (!zone)
      {
        return;
      }

      SunRaysZone oldActiveZone = GetActiveZone();
      m_Zones.Remove(zone);

      if (zone == oldActiveZone)
      {
        TransitionToZoneSettings(GetActiveZoneSettings(), true);
      }
    }

    public SunRaysZone GetActiveZone()
    {
      if (m_Zones.Count == 0)
      {
        return null;
      }

      return m_Zones[m_Zones.Count - 1];
    }

    public SunRaysSettings GetActiveZoneSettings()
    {
      SunRaysZone zone = GetActiveZone();
      return zone ? zone.zoneSettings : globalZoneSettings;
    }

    // Transition into a new sun rays zone configuration, with optional animated transition.
    public void TransitionToZoneSettings(SunRaysSettings settings, bool animated)
    {
      if (animated)
      {
        m_ZoneTransitionBeginTime = Time.time;
        m_BlendedSettings.BeginTransition(settings);
      }
      else
      {
        m_ZoneTransitionBeginTime = Time.time - 100.0f;
        m_BlendedSettings.BeginTransition(settings);
      }
    }

    void ContinueAnimatingZoneTransition()
    {
      float percent = Mathf.Abs((Time.time - m_ZoneTransitionBeginTime) / transitionDuration);
      percent = Mathf.Clamp01(percent);

      m_BlendedSettings.UpdateBlendedSettings(percent);
    }

    float CalculateLookAtIntensity()
    {
      Transform viewer = GetComponent<Camera>().transform;
      Vector3 distantLightPosition = GetDistantLightPosition();

      Vector3 dirToSun = (distantLightPosition - viewer.position).normalized;
      float lookIntensity = Mathf.Clamp01(Vector3.Dot(dirToSun, viewer.forward));

      float lookAtThreshold = m_BlendedSettings.blended.lookAtThreshold;
      float smoothIntensity = 0;
      if (lookIntensity > lookAtThreshold)
      {
        // Find the percentage of lookAt intensity we're at.
        float lookAtPercent = Mathf.Clamp01(lookIntensity - lookAtThreshold) / (1.0f - lookAtThreshold);
        smoothIntensity = Mathf.SmoothStep(0.0f, 1.0f, lookAtPercent);
      }

      float extraIntensity = m_BlendedSettings.blended.lookAtExtraIntensity * smoothIntensity;

      //Debug.Log("Dot: " + lookIntensity + " Look at extra intensity: " + extraIntensity);

      return extraIntensity;
    }

    Vector3 GetDistantLightPosition()
    {
      return lightTransform.forward * -100000.0f;
    }

    Vector3 GetDirectionToLight()
    {
      return lightTransform.forward * -1.0f;
    }

    float GetGlobalIntensity(Vector3 lookDirection, Vector3 toLightDirection)
    {
      float value = Vector3.Dot(lookDirection, toLightDirection);

      if (value >= 0)
      {
        return 1.0f;
      }
      else
      {
        return 0.0f;
      }
    }

    bool IsValidConfiguration()
    {
      return lightShader != null && lightTransform != null;
    }

    void SyncShaderKeywords(Material mat, SunRaysSettings settings)
    {
      if (settings.blendingMode == SunRaysSettings.BlendingMode.Add)
      {
        if (!mat.IsKeywordEnabled(k_BlendModeAdditiveKeyword))
        {
          mat.EnableKeyword(k_BlendModeAdditiveKeyword);
        }
      }
      else
      {
        if (mat.IsKeywordEnabled(k_BlendModeAdditiveKeyword))
        {
          mat.DisableKeyword(k_BlendModeAdditiveKeyword);
        }
      }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
      if (!IsValidConfiguration())
      {
        Graphics.Blit(src, dst);
        return;
      }

      // Effect resolution of the effect buffers.
      float resolutionDivider = (int)resolution;
      int width = Mathf.FloorToInt(Screen.width / resolutionDivider);
      int height = Mathf.FloorToInt(Screen.height / resolutionDivider);

      Camera activeCamera = this.GetComponent<Camera>();

      if (m_LightMat == null)
      {
        m_LightMat = new Material(lightShader);
      }

      SunRaysSettings settings = m_BlendedSettings.blended;
      float lightIntensity = settings.intensity + CalculateLookAtIntensity();

      // Texture with just skybox.
      RenderTexture skyBuffer = GetTemporaryRenderBuffer(width, height);
      RenderTexture.active = skyBuffer;
      GL.ClearWithSkybox(false, activeCamera);

      float globalIntensity = GetGlobalIntensity(activeCamera.transform.forward, GetDirectionToLight());

      // Render luminosity info.
      RenderTexture lightBuffer = GetTemporaryRenderBuffer(width, height);
      m_LightMat.SetColor("_SunColor", settings.sunShaftColor);
      m_LightMat.SetVector("_SunViewSpace", activeCamera.WorldToViewportPoint(GetDistantLightPosition()));
      m_LightMat.SetColor("_ThresholdColor", settings.thresholdColor);
      m_LightMat.SetFloat("_Intensity", lightIntensity);
      m_LightMat.SetFloat("_SunShaftRadius", settings.sunShaftRadius);
      m_LightMat.SetTexture("_SkyboxTex", skyBuffer);
      m_LightMat.SetTexture("_PatternTex", settings.patternTexture);
      m_LightMat.SetFloat("_PatternRotationSpeed", settings.patternRotationSpeed);
      m_LightMat.SetFloat("_PatternSize", settings.patternSize);
      m_LightMat.SetFloat("_PatternIntensity", settings.patternVisibility);
      m_LightMat.SetFloat("_GlobalIntensity", globalIntensity);

      SyncShaderKeywords(m_LightMat, settings);

      // Create luminance light mask.
      Graphics.Blit(src, lightBuffer, m_LightMat, 1);

      // Blur mask.
      RenderTexture blurTemp = GetTemporaryRenderBuffer(width, height);

      float blurRadius = settings.sunShaftRadius * (1.0f / 768.0f);

      for (int i = 0; i < m_BlurPasses; i++)
      {
        m_LightMat.SetVector("_BlurStepRadius", new Vector4(blurRadius, blurRadius, 0.0f, 0.0f));
        Graphics.Blit(lightBuffer, blurTemp, m_LightMat, 2);
        blurRadius = settings.sunShaftRadius * (((i * 2.0f + 1.0f) * 6.0f)) / 768.0f;

        m_LightMat.SetVector("_BlurStepRadius", new Vector4(blurRadius, blurRadius, 0.0f, 0.0f));
        Graphics.Blit(blurTemp, lightBuffer, m_LightMat, 2);
        blurRadius = settings.sunShaftRadius * (((i * 2.0f + 2.0f) * 6.0f)) / 768.0f;
      }

      RenderTexture.ReleaseTemporary(blurTemp);

      // Combine mask with frame.
      m_LightMat.SetTexture("_LightTex", lightBuffer);

      Graphics.Blit(src, dst, m_LightMat, 0);

      RenderTexture.ReleaseTemporary(skyBuffer);
      RenderTexture.ReleaseTemporary(lightBuffer);

      m_DidRenderFirstFrame = true;
    }

    RenderTexture GetTemporaryRenderBuffer(int width, int height)
    {
      var format = GetComponent<Camera>().allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
      return RenderTexture.GetTemporary(width, height, 0, format, RenderTextureReadWrite.sRGB, 2);
    }

    // TODO - REMOVEME
    static void DrawBorder(
      RenderTexture dest,
      Material material)
    {
      float x1;
      float x2;
      float y1;
      float y2;

      RenderTexture.active = dest;
      bool invertY = true; // source.texelSize.y < 0.0ff;
                           // Set up the simple Matrix
      GL.PushMatrix();
      GL.LoadOrtho();

      for (int i = 0; i < material.passCount; i++)
      {
        material.SetPass(i);

        float y1_; float y2_;
        if (invertY)
        {
          y1_ = 1.0f; y2_ = 0.0f;
        }
        else
        {
          y1_ = 0.0f; y2_ = 1.0f;
        }

        // left
        x1 = 0.0f;
        x2 = 0.0f + 1.0f / (dest.width * 1.0f);
        y1 = 0.0f;
        y2 = 1.0f;
        GL.Begin(GL.QUADS);

        GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
        GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
        GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
        GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

        // right
        x1 = 1.0f - 1.0f / (dest.width * 1.0f);
        x2 = 1.0f;
        y1 = 0.0f;
        y2 = 1.0f;

        GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
        GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
        GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
        GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

        // top
        x1 = 0.0f;
        x2 = 1.0f;
        y1 = 0.0f;
        y2 = 0.0f + 1.0f / (dest.height * 1.0f);

        GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
        GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
        GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
        GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

        // bottom
        x1 = 0.0f;
        x2 = 1.0f;
        y1 = 1.0f - 1.0f / (dest.height * 1.0f);
        y2 = 1.0f;

        GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
        GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
        GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
        GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

        GL.End();
      }

      GL.PopMatrix();
    }
  }
}


