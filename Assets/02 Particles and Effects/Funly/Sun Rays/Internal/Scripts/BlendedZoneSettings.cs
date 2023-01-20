// Sun Rays by Funly, LLC
// Website: https://funly.io
// Author: Jason Ederle - jason@funly.io

using System;
using UnityEngine;

namespace  Funly.SunRays
{
  [System.Serializable]
  public class BlendedZoneSettings {
    
    public SunRaysSettings blended = new SunRaysSettings();
    public float percent = 0.0f;
    private SunRaysSettings m_From = new SunRaysSettings();
    private SunRaysSettings m_To = new SunRaysSettings();

    public BlendedZoneSettings(SunRaysSettings from, SunRaysSettings to) {
      this.m_From = from;
      this.m_To = to;
      this.percent = 1.0f;
      this.blended = new SunRaysSettings(to);
    }
    
    public void BeginTransition(SunRaysSettings to) {
      m_From = new SunRaysSettings(blended);
      m_To = to;
      percent = 0.0f;
    }

    public void UpdateBlendedSettings(float t) {
      blended.intensity = Mathf.Lerp(m_From.intensity, m_To.intensity, t);
      blended.sunShaftRadius = Mathf.Lerp(m_From.sunShaftRadius, m_To.sunShaftRadius, t);
			blended.sunShaftColor = Color.Lerp(m_From.sunShaftColor, m_To.sunShaftColor, t);
			blended.thresholdColor = Color.Lerp(m_From.thresholdColor, m_To.thresholdColor, t);
			blended.lookAtThreshold = Mathf.Lerp(m_From.lookAtThreshold, m_To.lookAtThreshold, t);
			blended.lookAtExtraIntensity = Mathf.Lerp(m_From.lookAtExtraIntensity, m_To.lookAtExtraIntensity, t);
			blended.patternRotationSpeed = Mathf.Lerp(m_From.patternRotationSpeed, m_To.patternRotationSpeed, t);
      blended.patternSize = Mathf.Lerp(m_From.patternSize, m_To.patternSize, t);
      blended.patternVisibility = Mathf.Lerp(m_From.patternVisibility, m_To.patternVisibility, t);
      
      // We don't interpolate these.
      blended.patternTexture = m_To.patternTexture;
      blended.blendingMode = m_To.blendingMode;
      
      percent = t;
    }
  }
}