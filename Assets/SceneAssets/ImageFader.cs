using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFader : MonoBehaviour
{
    public float fadeInSpeed = 2f;
    public float fadeOutSpeed = 2f;
    public SpriteRenderer spriteToFade = null;
    public float fadeCurrentValue = 0.0f;
    public enum fadeOptions { FadeIn, FadeOut, FadeHold };
    public fadeOptions fadeDirection = fadeOptions.FadeHold;

    private float lastTime = 0.0f;
    //public Material materialToFade = null;
    
    // Start is called before the first frame update
    void Start()
    {
        if (spriteToFade != null)
        {
            // apply the fade
            Color newColor = spriteToFade.color;
            newColor.a = fadeCurrentValue;
            spriteToFade.color = newColor;
        }

        lastTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeDirection != fadeOptions.FadeHold)
        {
            if (fadeDirection == fadeOptions.FadeIn)
            {
                fadeCurrentValue += ((Time.time - lastTime) / fadeInSpeed);

                if (fadeCurrentValue > 1.0f)
                {
                    fadeCurrentValue = 1.0f;
                    fadeDirection = fadeOptions.FadeHold;
                }
            }
            else
            {
                if (fadeDirection == fadeOptions.FadeOut)
                {
                    fadeCurrentValue -= ((Time.time - lastTime) / fadeOutSpeed);

                    if (fadeCurrentValue < 0.0f)
                    {
                        fadeCurrentValue = 0.0f;
                        fadeDirection = fadeOptions.FadeHold;
                    }
                }
            }

            // apply the fade

            if (spriteToFade != null)
            {
                Color newColor = spriteToFade.color;
                newColor.a = fadeCurrentValue;
                spriteToFade.color = newColor;
            }

            lastTime = Time.time;
        }
    }

    public void FadeIn()
    {
        fadeDirection = fadeOptions.FadeIn;
        lastTime = Time.time;
    }

    public void FadeOut()
    {
        fadeDirection = fadeOptions.FadeOut;
        lastTime = Time.time;
    }
}
