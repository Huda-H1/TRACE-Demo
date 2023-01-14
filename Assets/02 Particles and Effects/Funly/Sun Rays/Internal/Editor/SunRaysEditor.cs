// Sun Rays by Funly, LLC
// Website: https://funly.io
// Author: Jason Ederle - jason@funly.io

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if SKY_STUDIO_PRESENT
using Funly.SkyStudio;
#endif

namespace Funly.SunRays {
  [CustomEditor(typeof(SunRaysImageEffect))]
  public class SunRaysEditor : Editor {
    
    void OnEnable()
    {	
      SunRaysImageEffect effect = this.target as SunRaysImageEffect;
      if (effect.lightShader == null) {
        SetupShader();
      }

      if (effect.lightTransform == null) {
        SetupLightTransform();
      }
      
      if (effect.lightTransform == null) {
        Debug.LogError("Sun Rays Effect requires a transform to use for light direction. " +
          "Create a directional light in your scene and assign it to SunRaysEffect.lightTransform " + 
          "or use anothe objects transform to adjust the sun rays direction.");
      }

      if (effect.didConfigureDefaultValues == false) {
        SetupDefaultObjects();
      }
    }

    void SetupShader() {
      SunRaysImageEffect effect = this.target as SunRaysImageEffect;
      effect.lightShader = Shader.Find("Hidden/Funly/Sun Rays/Rendering");

      if (effect.lightShader == null) {
        Debug.LogError("Missing required shader for SunRaysEffect: 'Hidden/Funly/Sun Rays/Rendering'");
      }

      EditorUtility.SetDirty(this.target);
    }

    void SetupLightTransform() {
      SunRaysImageEffect effect = this.target as SunRaysImageEffect;

      Transform bestTransform = null;
      Transform skyStudioSun = CheckForSkyStudioSunTransorm();
      if (skyStudioSun != null) {
        bestTransform = skyStudioSun;
      } else {
        bestTransform = CheckForDirectionalLightTransform();
      }

      effect.lightTransform = bestTransform;
      EditorUtility.SetDirty(this.target);
    }

    Transform CheckForDirectionalLightTransform() {
      Light[] lights = FindObjectsOfType<Light>();
      
      if (lights == null || lights.Length == 0) {
        return null;
      }

      foreach (Light light in lights) {
        if (light.type == LightType.Directional) {
          return light.transform;
        }
      }

      return null;
    }

    Transform CheckForSkyStudioSunTransorm() {
      #if SKY_STUDIO_PRESENT
        Debug.Log("Sky Studio is present!");
        TimeOfDayController tc = FindObjectOfType<TimeOfDayController>();
        if (tc == null) {
          return null;
        }

        return tc.transform.Find("Sun/Light");
      #else
        Debug.Log("Sky Studio not present!");
        return null;
      #endif
    }

    void SetupDefaultObjects() {
      SunRaysImageEffect effect = this.target as SunRaysImageEffect;
      effect.globalZoneSettings.patternTexture = GetImageWithName("DefaultSunRaysPattern");
      
      effect.didConfigureDefaultValues = true;
      EditorUtility.SetDirty(this.target);
    }
    
    Texture2D GetImageWithName(string imgName) {
      string[] ids = AssetDatabase.FindAssets("t:texture2d " + imgName);
      if (ids == null || ids.Length == 0) {
        return null;
      }

      string assetPath = AssetDatabase.GUIDToAssetPath(ids[0]);
      if (assetPath == null) {
        return null;
      }

      return AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
    }
  }
}
