using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FantasticGlass
{
    public enum MeshAnimatorPlaybackType
    {
        Loop,
        PingPong,
        PlayOnce
    }

    public enum MeshAnimatorPlaybackDirection
    {
        Forwards,
        Backwards
    }

    /*
    public enum MeshAnimator_FilterType
    {
        none,
        contains
    }
    */

    public class MeshAnimator : MonoBehaviour
    {
        [Header("Playback")]
        public float framerate = 30f;
        float frameInterval = 1f / 30f;
        float frameIntervalCounter = 0f;
        int currentMeshIndex = 0;
        public MeshAnimatorPlaybackDirection direction = MeshAnimatorPlaybackDirection.Forwards;
        public MeshAnimatorPlaybackType playback = MeshAnimatorPlaybackType.Loop;
        public int loopLimit = 0;
        int loopCounter = 0;
        bool didLoop = false;
        public bool playOnStart = true;
        [Header("Delay/Freeze Frame")]
        public float startDelay = 0f;
        public int startDelayFrame = -1;
        public float loopDelay = 0f;
        public float pingPongDelay = 0f;
        float delayCounter = 0f;
        bool delayed = false;
        [HideInInspector]
        public bool paused = true;
        [Header("Mesh")]
        public List<Mesh> meshes = new List<Mesh>();
        MeshFilter meshFilter;
        public List<GameObject> models = new List<GameObject>();
        List<MeshFilter> modelMeshFilters = new List<MeshFilter>();
        public MeshAnimator_FilterType meshFilterType = MeshAnimator_FilterType.none;
        public string meshNameFilter = "";
        public MeshAnimator_FilterType modelFilterType = MeshAnimator_FilterType.none;
        public string modelNameFilter = "";
        public bool autoSortModels = true;
        [Header("Debug/Editor Tools")]
        public bool showDebugLog = false;

        // Use this for initialization
        void Start()
        {
            paused = true;

            if (meshFilter == null)
            {
                meshFilter = GetComponentInChildren<MeshFilter>();
            }

            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();

#if UNITY_EDITOR
                GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                meshFilter.sharedMesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
                DestroyImmediate(tempSphere);
#endif
            }

            if (GetComponentInChildren<MeshRenderer>() == null)
            {
                /*MeshRenderer rend = */
                MeshRenderer rend = gameObject.AddComponent<MeshRenderer>();
                rend.material = new Material(Shader.Find("Standard"));
            }

            while (models.Contains(null))
                models.Remove(null);

            if (models.Count != 0)
            {
                if (autoSortModels)
                {
                    models = models.OrderBy(o => o.name).ToList();
                }

                switch (modelFilterType)
                {
                    case FantasticGlass.MeshAnimator_FilterType.contains:
                        for (int i = models.Count - 1; i >= 0; i--)
                        {
                            if (models[i].name.Contains(modelNameFilter))
                                continue;
                            models.RemoveAt(i);
                        }
                        break;
                    default:
                        break;
                }

                GenerateMeshFilters();

                switch (meshFilterType)
                {
                    case MeshAnimator_FilterType.contains:
                        //foreach (GameObject model in models) {
                        //foreach (Mesh m in model.GetComponentsInChildren<Mesh>()) {
                        //foreach (MeshFilter meshFilterInModel in model.GetComponentsInChildren<MeshFilter>()) {
                        foreach (MeshFilter meshFilterInModel in modelMeshFilters)
                        {
                            Mesh m = meshFilterInModel.sharedMesh;
                            if (m.name.Contains(meshNameFilter))
                            {
                                meshes.Add(m);
                            }
                        }
                        //}
                        break;
                    default:
                        //foreach (GameObject model in models) {
                        //foreach (Mesh m in model.GetComponentsInChildren<Mesh>()) {
                        //foreach (MeshFilter meshFilterInModel in model.GetComponentsInChildren<MeshFilter>()) {
                        foreach (MeshFilter meshFilterInModel in modelMeshFilters)
                        {
                            Mesh m = meshFilterInModel.sharedMesh;
                            meshes.Add(m);
                        }
                        //}
                        break;
                }
            }

            if (playOnStart)
            {
                //SetFrame(startDelayFrame);
                //Invoke("Play", startDelay);
                if (startDelay > 0f)
                {
                    Wait(startDelay, startDelayFrame);
                }
                else
                {
                    Play();
                }
            }

            StartCoroutine(Update_CO());
        }

        void GenerateMeshFilters()
        {
            foreach (GameObject model in models)
            {
                GameObject modelInstance = Instantiate(model);
                foreach (MeshFilter meshFilter in modelInstance.GetComponentsInChildren<MeshFilter>())
                {
                    modelMeshFilters.Add(meshFilter);
                }
                Destroy(modelInstance);
            }
        }

        public void LoadFromFolder()
        {
#if UNITY_EDITOR
            if (EditorTools.Message_static("Load Baked Animation In Folder", "Select one of the models in the folder you wish to load.", "OK", "Cancel"))
                EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", GetInstanceID());
            GUIUtility.ExitGUI();
#endif
            Debug.Log("Unable to LoadFromFolder when not it Edit mode", this);
        }

        public int LoadFromFolder(string[] filePathsToLoad, string folderString)
        {
#if UNITY_EDITOR
            if (filePathsToLoad != null)
            {
                int modelCount = 0;

                foreach (string filePath in filePathsToLoad)
                {
                    int assetPathIndex = filePath.IndexOf("Assets");
                    string localPath = filePath.Substring(assetPathIndex);

                    GameObject potentialModel = AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject)) as GameObject;

                    if (potentialModel != null)
                    {
                        models.Add(potentialModel);
                        modelCount++;
                    }
                }

                //delayedMessageString = "Loaded " + modelCount.ToString() + " models from 'Assets/" + folderString + ".";

                //Invoke("FolderLoadingComplete", 0.25f);
                //FolderLoadingComplete();

                return modelCount;
            }
            return 0;
#else
            return 0;
#endif
        }

        public void LoadFromAnimator(MeshAnimator other)
        {
            meshes.Clear();
            models.Clear();
            modelMeshFilters.Clear();

            meshes.AddRange(other.meshes);
            models.AddRange(other.models);
            modelMeshFilters.AddRange(other.modelMeshFilters);

            meshFilterType = other.meshFilterType;
            meshNameFilter = other.meshNameFilter;
            modelFilterType = other.modelFilterType;
            modelNameFilter = other.modelNameFilter;
            autoSortModels = other.autoSortModels;

            currentMeshIndex = other.currentMeshIndex;
            framerate = other.framerate;
            direction = other.direction;
            playback = other.playback;
            frameInterval = other.frameInterval;
            frameIntervalCounter = other.frameIntervalCounter;
            paused = other.paused;
        }

        public void Play()
        {
            if (showDebugLog)
                Debug.Log("MeshAnimator :: Play", this);
            paused = false;
        }

        public void Pause()
        {
            paused = true;
        }

        /// <summary>
        /// Wait the specified delay if not -1. Set the specified frame if not -1;
        /// </summary>
        /// <param name="delay">Delay.</param>
        /// <param name="frame">Frame.</param>
        public void Wait(float delay = 0, int frame = -1)
        {
            if (showDebugLog)
                Debug.Log("MeshAnimator :: Wait :: Delay: " + delay.ToString() + " Frame: " + frame.ToString(), this);
            Pause();
            SetFrame(frame);
            /*
            if (delay > 0f)
            Invoke("Play", delay);
            */
            if (delay > 0f)
            {
                delayCounter = delay;
                delayed = true;
            }
        }

        public void SetFrame(int frame = -1)
        {
            if (frame == -1)
                return;
            if (frame >= meshes.Count)
                return;
            currentMeshIndex = frame;
            UpdateMesh();
        }

        IEnumerator Update_CO()
        {
            while (true)
            {
                if (delayed)
                {
                    delayCounter -= Time.deltaTime;
                    if (delayCounter <= 0f)
                    {
                        delayed = false;
                        paused = false;
                    }
                }
                if (paused)
                {
                    //  no update needed
                }
                else if ((1f / framerate) <= 0f)
                {
                    //  no update needed
                }
                else if (meshes.Count == 0)
                {
                    //  no update needed
                }
                else if (framerate <= 0f)
                {
                    //  no update needed
                }
                else if (meshFilter == null)
                {
                    //  no update needed
                }
                else
                {
                    frameInterval = 1f / framerate;

                    frameIntervalCounter += Time.deltaTime;

                    UpdateIndex();

                    LoopUpdate();

                    UpdateMesh();
                }
                yield return new WaitForEndOfFrame();
            }
        }

        /*
        void Update()
        {
            if (paused)
                return;
            if (frameInterval <= 0f)
                return;
            if (meshes.Count == 0)
                return;
            if (framerate <= 0f)
                return;
            if (meshFilter == null)
                return;
                
            frameInterval = 1f / framerate;
            
            frameIntervalCounter += Time.deltaTime;
            
            UpdateIndex();

            LoopUpdate();

            UpdateMesh();
        }
        */

        void LoopUpdate()
        {
            if (!didLoop)
                return;

            loopCounter++;

            didLoop = false;

            if (loopCounter >= loopLimit && loopLimit > 0)
            {
                paused = true;
                loopCounter = 0;
            }
            else
            {
                if (pingPongDelay > 0f && (loopCounter % 2 == 0))
                {
                    Wait(pingPongDelay);
                }
                else if (loopDelay > 0f)
                {
                    Wait(loopDelay);
                }
            }
        }

        void UpdateMesh()
        {
            meshFilter.sharedMesh = meshes[currentMeshIndex];
        }

        void UpdateIndex()
        {
            while (frameIntervalCounter >= frameInterval)
            {
                switch (direction)
                {
                    case MeshAnimatorPlaybackDirection.Forwards:

                        currentMeshIndex++;

                        switch (playback)
                        {
                            case MeshAnimatorPlaybackType.Loop:
                                if (currentMeshIndex >= meshes.Count)
                                {
                                    currentMeshIndex = 0;
                                    didLoop = true;
                                }
                                break;
                            case MeshAnimatorPlaybackType.PingPong:
                                if (currentMeshIndex >= meshes.Count - 1)
                                {
                                    if (meshes.Count == 1)
                                    {
                                        currentMeshIndex = 0;
                                    }
                                    else
                                    {
                                        currentMeshIndex = meshes.Count - 2;
                                    }
                                    direction = MeshAnimatorPlaybackDirection.Backwards;
                                    didLoop = true;
                                }
                                break;
                            case MeshAnimatorPlaybackType.PlayOnce:
                                if (currentMeshIndex >= meshes.Count)
                                {
                                    paused = true;
                                }
                                break;
                        }
                        break;

                    case MeshAnimatorPlaybackDirection.Backwards:

                        currentMeshIndex--;

                        switch (playback)
                        {
                            case MeshAnimatorPlaybackType.Loop:
                                if (currentMeshIndex < 0)
                                {
                                    currentMeshIndex = meshes.Count - 1;
                                    didLoop = true;
                                }
                                break;
                            case MeshAnimatorPlaybackType.PingPong:
                                if (currentMeshIndex < 0)
                                {
                                    if (meshes.Count == 1)
                                    {
                                        currentMeshIndex = 0;
                                    }
                                    else
                                    {
                                        currentMeshIndex = 1;
                                    }
                                    direction = MeshAnimatorPlaybackDirection.Forwards;
                                    didLoop = true;
                                }
                                break;
                            case MeshAnimatorPlaybackType.PlayOnce:
                                if (currentMeshIndex < 0)
                                {
                                    paused = true;
                                }
                                break;
                        }

                        break;
                }


                frameIntervalCounter -= frameInterval;
            }
        }
    }

}
