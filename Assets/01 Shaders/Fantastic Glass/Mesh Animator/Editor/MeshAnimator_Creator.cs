#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace FantasticGlass
{

    public class MeshAnimator_Creator : EditorWindow
    {
        bool folderLoaded = false; 
        bool showPicker = false;
        bool readyToClose = false;
        string[] filePathsToLoad = null;
        string folderString = "";
        int modelCount = 0;
        MeshAnimator currentAnimator = null;
        public EditorTools tools = null;
        List<GameObject> objectList = new List<GameObject>();
        
        [MenuItem("GameObject/Create Other/Mesh Animator...", false, 0)]
        static void Init()
        {
            if (Application.isPlaying)
            {
                Debug.Log("Not loading MeshAnimator_Creator during Play mode.");
                return;
            }
            //
            //MeshAnimator_Creator window = ScriptableObject.CreateInstance(typeof(MeshAnimator_Creator)) as MeshAnimator_Creator;
            MeshAnimator_Creator window = (MeshAnimator_Creator)EditorWindow.GetWindow (typeof (MeshAnimator_Creator));
            
            window.tools = new EditorTools("MeshAnimator_Creator");
            
            window.Show();
            
            window.CreateMeshAnimator();
        }
        
        public void CreateMeshAnimator()
        {
            if(Selection.gameObjects.Length==0)
            {
                GameObject obj = new GameObject("Animated Mesh");
                CreateMeshAnimator(obj);
            }
            else
            {
                GameObject firstObj = Selection.gameObjects[0];
                objectList.Clear();
                foreach(GameObject obj in Selection.gameObjects)
                {
                    if(obj!=firstObj)
                    {
                        objectList.Add(obj);
                        //CreateMeshAnimator(obj, firstAnim);
                    }
                }
                //MeshAnimator firstAnim =  CreateMeshAnimator(firstObj);
                CreateMeshAnimator(firstObj);
            }
        }
            
    public void CreateMeshAnimator(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if(meshFilter==null)
        {
                meshFilter = obj.AddComponent<MeshFilter>();
                #if UNITY_EDITOR
                GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                meshFilter.sharedMesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
                DestroyImmediate(tempSphere);
                #endif
        };
        
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        if(meshRenderer==null)
        {
            meshRenderer = obj.AddComponent<MeshRenderer>();     
            meshRenderer.material = new Material(Shader.Find("Standard"));
        }
        
        MeshAnimator meshAnimator = obj.GetComponent<MeshAnimator>();
        if(meshAnimator==null)
            meshAnimator = obj.AddComponent<MeshAnimator>();
        
        //meshAnimator.LoadFromFolder();
            LoadFromFolder(meshAnimator);
        
            //return meshAnimator;
        }
        
        public MeshAnimator CreateMeshAnimator(GameObject obj, MeshAnimator animator)
        {
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if(meshFilter==null)
            {
                meshFilter = obj.AddComponent<MeshFilter>();
                MeshFilter existingMeshFilter = animator.GetComponent<MeshFilter>();
                if(existingMeshFilter!=null)
                {
                    meshFilter.sharedMesh = existingMeshFilter.sharedMesh;
                }
            }
            
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if(meshRenderer==null)
            {
                meshRenderer = obj.AddComponent<MeshRenderer>();
                MeshRenderer existingRenderer = animator.GetComponent<MeshRenderer>();
                if(existingRenderer!=null)
                {
                    meshRenderer.sharedMaterial = existingRenderer.sharedMaterial;
                }
            }   
            
            MeshAnimator meshAnimator = obj.GetComponent<MeshAnimator>();
            if(meshAnimator==null)
                meshAnimator = obj.AddComponent<MeshAnimator>();
            
            meshAnimator.LoadFromAnimator(animator);
            
            return meshAnimator;
        }
        
            public void LoadFromFolder(MeshAnimator meshAnimator)
        {
                if(tools.Message("Load Baked Animation In Folder", "Select one of the models in the folder you wish to load.", "OK", "Cancel"))
                {
                    //EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", GetInstanceID());
                    currentAnimator = meshAnimator;
                    showPicker = true;
                }
                else
                {
                    Close();
                }
        }

    void OnGUI()
        {           
            
            if(readyToClose)
            {
                /*
                tools.Label("Mesh Animator Creator has finished!");
                if(tools.Button("Close"))
                {
                    this.Close();
                }
                */
                this.Close();
                GUIUtility.ExitGUI();
                return;
            }
            else
            {
                tools.Label("Mesh Animator Creator is Active...");
            }
            
            if(showPicker)
            {
                EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", GetInstanceID());
                showPicker = false;
                GUIUtility.ExitGUI();
                return;
            }
        
            if(folderLoaded)
            {
                EditorTools.Message_static("Loading Complete", "Loaded " + modelCount.ToString() + " models from 'Assets/" + folderString + ".");
                
                folderLoaded = false;
                folderString = "";
                filePathsToLoad = null;
                
                foreach(GameObject obj in objectList)
                {
                    CreateMeshAnimator(obj, currentAnimator);
                }
                
                readyToClose = true;
                
                GUIUtility.ExitGUI();
                
                //this.Close();
                //Destroy(this);
                return;
            }
        
        Event currentEvent = Event.current;
            
            if(currentEvent.type==EventType.ExecuteCommand || currentEvent.type == EventType.ValidateCommand)
            {
                if(currentEvent.commandName == "ObjectSelectorClosed")
                {
                    if(EditorGUIUtility.GetObjectPickerControlID()==GetInstanceID())
                    {
                        Object pickedObject = EditorGUIUtility.GetObjectPickerObject();
                        
                        if(pickedObject!=null)
                        {
                            folderString = AssetDatabase.GetAssetPath(pickedObject);
                            folderString = folderString.Replace(Path.GetFileName(folderString), "");
                            folderString = folderString.Replace("Assets/", "");
                            
                            filePathsToLoad = Directory.GetFiles(Application.dataPath+"/"+folderString);
                            
                            bool replaceExisting = tools.Message("Load Folder", "Replace any existing models?", "Yes", "No");
                            
                            if(replaceExisting)
                            {
                                currentAnimator.models.Clear();
                            }
                            
                            modelCount = currentAnimator.LoadFromFolder(filePathsToLoad, folderString);
                            
                            folderLoaded = true;
                            
                            GUIUtility.ExitGUI();
                            return;
                        }
                        
                    }
                }
            }
    }

}

}

#endif