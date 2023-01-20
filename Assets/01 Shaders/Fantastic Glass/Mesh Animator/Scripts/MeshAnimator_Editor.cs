#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace FantasticGlass
{
    
    [CustomEditor(typeof(MeshAnimator))]
    [CanEditMultipleObjects]
    public class MeshAnimator_Editor : Editor
{
    EditorTools tools = null;
    
        MeshAnimator animator = null;
        int objectPickerID = 0;
        
        bool folderLoaded = false; 
        string[] filePathsToLoad = null;
        string folderString = "";
        int modelCount = 0;
        
        void OnEnable()
        {
            if(tools==null)
                tools = new EditorTools("MeshAnimator_Editor", this);
                
            if(animator==null)
                animator = (MeshAnimator)target;
                
            if(objectPickerID==0)
                objectPickerID = this.GetInstanceID()+1;
        }
        
        void FinishedLoadingFromFolder()
        {
        }
                
        public override void OnInspectorGUI()
    {    
        if(folderLoaded)
            {
                EditorTools.Message_static("Loading Complete", "Loaded " + modelCount.ToString() + " models from 'Assets/" + folderString + ".");
                folderLoaded = false;
                folderString = "";
                filePathsToLoad = null;
        }
    
       DrawDefaultInspector();
       
       if(tools.Button("Load Folder"))
       {    
            if(tools.Message("Load Baked Animation In Folder", "Select one of the models in the folder you wish to load.", "OK", "Cancel"))
                    EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", objectPickerID);
                GUIUtility.ExitGUI();
       }
       
       Event currentEvent = Event.current;
       
            if(currentEvent.type==EventType.ExecuteCommand || currentEvent.type == EventType.ValidateCommand)
            {
                if(currentEvent.commandName == "ObjectSelectorClosed")
                {
                    if(EditorGUIUtility.GetObjectPickerControlID()==objectPickerID)
                    {
                    Object pickedObject = EditorGUIUtility.GetObjectPickerObject();
                    
                    if(pickedObject!=null)
                    {
                        folderString = AssetDatabase.GetAssetPath(pickedObject);
                            folderString = folderString.Replace(Path.GetFileName(folderString), "");
                            folderString = folderString.Replace("Assets/", "");
                        
                            filePathsToLoad = Directory.GetFiles(Application.dataPath+"/"+folderString);
                            
                            bool replaceExisting = tools.Message("Load Folder", "Replace any existing models?", "Yes", "No");

                            tools.StartEdit(animator, "Mesh Animator Loaded models");

                            if(replaceExisting)
                            {
                                animator.models.Clear();
                            }
                            
                            modelCount = animator.LoadFromFolder(filePathsToLoad, folderString);

                            tools.EndEdit(animator);

                            folderLoaded = true;
                            
                            GUIUtility.ExitGUI();
                    }
                    
                    }
                }
            }
        
        if(animator.paused)
        {
            if(tools.Button("Play"))
            {
                animator.Play();
            }
        }
        else
        {
            if(tools.Button("Pause"))
            {
                animator.Pause();
            }
        }
        
    }

}

}

#endif