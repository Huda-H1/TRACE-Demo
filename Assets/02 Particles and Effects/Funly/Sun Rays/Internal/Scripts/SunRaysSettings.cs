// Sun Rays by Funly, LLC
// Website: https://funly.io
// Author: Jason Ederle - jason@funly.io

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SunRays
{
  [Serializable]
  public class SunRaysSettings {
    
    public enum BlendingMode  {
      Screen = 0,
      Add = 1,
    }
    
    [Header("Sun Ray Properties")]

    [Tooltip("Blending algoritm ued when merging sun rays into the final image.")]
    public BlendingMode blendingMode = BlendingMode.Screen;
    
    [Range(0.0f, 20.0f)]
    public float intensity = .5f;

    [Range(0.0f, 6.0f)]
    public float sunShaftRadius = 1.5f;

    [Tooltip("Tint color applied to the sun rays. This should be a lighter color since it's multiplied against the sun rays mask.")]
    public Color sunShaftColor = new Color(249.0f / 255.0f, 228.0f / 255.0f, 160.0f / 255.0f);

    [Tooltip("Threshold color used to control if sunshafts appear or not. Darker colors show more sun rays, lighter colors show less.")]
    public Color thresholdColor = new Color(222.0f / 255.0f, 189.0f / 255.0f, 166.0f / 255.0f);

    [Header("Pattern Texture")]

    [Tooltip("Optional texture used to create a rolling pattern in sun light beams.")]
    public Texture2D patternTexture;

    [Tooltip("Size of the pattern texture, typically this is roughly the same size as the sun shaft radius.")]
    [Range(0.0f, 6.0f)]
    public float patternSize = 2.0f;

    [Tooltip("Intensity of the pattern texture. Value of 1 makes pattern fully visible, 0 makes it invisible.")]
    [Range(0.0f, 1.0f)]
    public float patternVisibility = 0;

    [Tooltip("Speed at which the pattern animation will rotate at. Slower speeds usually look more natural.")]
    [Range(-.5f, .5f)]
    public float patternRotationSpeed = .15f;

    [Header("Looking At Sun Reaction")]

    [Tooltip("Threashold for how wide or narrow, the look-at angle is. Value of 1 is narrow, and 0 is wide.")]
    [Range(0, 1)]
    public float lookAtThreshold = .8f;

    [Tooltip("The amount of extra itensity to apply to sun shafts, when camera is looking at the sun light.")]
    public float lookAtExtraIntensity = .1f;

    public SunRaysSettings() {}

    // Copy constructor.
    public SunRaysSettings(SunRaysSettings s) {
      intensity = s.intensity;
      sunShaftRadius = s.sunShaftRadius;
      sunShaftColor = s.sunShaftColor;
      thresholdColor = s.thresholdColor;
      lookAtExtraIntensity = s.lookAtExtraIntensity;
      lookAtThreshold = s.lookAtThreshold;
      patternTexture = s.patternTexture;
      patternSize = s.patternSize;
      patternVisibility = s.patternVisibility;
      patternRotationSpeed = s.patternRotationSpeed;
      blendingMode = s.blendingMode;
    }
    
  }

}

