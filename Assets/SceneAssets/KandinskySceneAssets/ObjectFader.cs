using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFader : MonoBehaviour
{
    public enum fadeOptions { FadeUp, FadeDown, FadeStatic };
    public float fadeCurrentValue = 0.0f;
    public fadeOptions fadeDirection = fadeOptions.FadeStatic;
    public float fadeUpSpeed = 2f;
    public float fadeDownSpeed = 2f;

    private float lastTime = 0.0f;
    private Material materialToFade = null;


    // Start is called before the first frame update
    void Start()
    {
        lastTime = Time.time;

        materialToFade = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        switch (fadeDirection)
        {
            case fadeOptions.FadeDown:
                // increase the fade
                fadeCurrentValue += ((Time.time - lastTime) / fadeUpSpeed);
                break;
            case fadeOptions.FadeUp:
                // decrease the fade
                fadeCurrentValue -= ((Time.time - lastTime) / fadeDownSpeed);
                break;
            case fadeOptions.FadeStatic:
                // do nothing
                break;
        }

        // range check the fade value
        if (fadeCurrentValue > 1.0f)
            fadeCurrentValue = 1.0f;
        else
            if (fadeCurrentValue < 0.0f)
            fadeCurrentValue = 0.0f;

        //if (imageToFade != null)
        if (materialToFade != null)
        {
            // apply the fade
            Color newColor = materialToFade.color;
            newColor.a = fadeCurrentValue;
            materialToFade.color = newColor;
        }

        // update the last time 
        lastTime = Time.time;
        
    }
}
