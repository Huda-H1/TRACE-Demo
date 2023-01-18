using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fader : MonoBehaviour
{
    public string sceneToLoad = "";

    public enum fadeOptions { FadeUp, FadeDown, FadeStatic, FadeUpToSceneLoad, FadeDownToSceneLoad};
    public float fadeCurrentValue = 0.0f;
    public fadeOptions fadeDirection = fadeOptions.FadeStatic;
    public float fadeUpSpeed = 2f;
    public float fadeDownSpeed = 2f;
    //public Image imageToFade = null;
    public Transform playerCameraTransform = null;

    private float lastTime = 0.0f;
    private IEnumerator coroutineForSceneLoading = null;
    private bool loadingscene = false;
    private Material materialToFade = null;


    // Start is called before the first frame update
    void Start()
    {
        lastTime = Time.time;
        if (playerCameraTransform != null)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.position = new Vector3(0, 0, 0.2f);
            plane.transform.Rotate(90.0f,180.0f,0);
            materialToFade = plane.GetComponent<MeshRenderer>().material;
            materialToFade.shader = Shader.Find("Unlit/Unlit Transparent Color");
            Color newColor = Color.black;
            newColor.a = 0.0f;
            materialToFade.color = newColor;
            plane.transform.SetParent(playerCameraTransform, false);
        }
    }

// Update is called once per frame
void Update()
    {
        if (!loadingscene) // avoid any case where a scene might be loaded twice
        {

            switch (fadeDirection)
            {
                case fadeOptions.FadeDown:
                case fadeOptions.FadeDownToSceneLoad:
                    // increase the fade
                    fadeCurrentValue += ((Time.time - lastTime) / fadeUpSpeed);
                    break;
                case fadeOptions.FadeUp:
                case fadeOptions.FadeUpToSceneLoad:
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
                //Color newColor = imageToFade.color;
                Color newColor = materialToFade.color;
                newColor.a = fadeCurrentValue;
                //imageToFade.color = newColor;
                materialToFade.color = newColor;
            }

            // if the mode is looking for a scene load and the required fade has been reached, trigger the load
            if ((fadeDirection == fadeOptions.FadeUpToSceneLoad && fadeCurrentValue <= 0.0f) ||
                fadeDirection == fadeOptions.FadeDownToSceneLoad && fadeCurrentValue >= 1.0f)
            {
                if (sceneToLoad != "")
                {
                    LoadScene(sceneToLoad);
                }
            }

            // update the last time 
            lastTime = Time.time;
        }
    }

    public void SetSceneToLoad(string sceneName)
    {
        sceneToLoad = sceneName;
    }

    private void LoadScene(string sceneName)
    {
        Debug.Log("LoadScene: " + sceneName);

        coroutineForSceneLoading = LoadAsyncScene(sceneName);

        StartCoroutine(coroutineForSceneLoading);

        loadingscene = true;
    }

    private IEnumerator LoadAsyncScene(string sceneName)
    {
        Debug.Log("Started LoadAsyncScene " + sceneName + " at timestamp : " + Time.time);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;

            Debug.Log("Loading progress: " + (asyncLoad.progress * 100) + "%    Time passed since last frame: " + Time.deltaTime);
        }

        Debug.Log("Finished LoadAsyncScene " + sceneName + " at timestamp : " + Time.time);

        loadingscene = false;
    }
}
