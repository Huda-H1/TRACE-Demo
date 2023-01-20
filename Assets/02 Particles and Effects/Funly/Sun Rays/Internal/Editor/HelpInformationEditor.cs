using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SunRays
{
  [CustomEditor(typeof(Funly.SunRays.HelpInformation))]
  public class HelpInformationEditor : Editor
  {
    private static void OpenDiscordChat()
    {
      Application.OpenURL("http://bit.ly/2Gjrywv");
    }

    private static void OpenVideoTutorials()
    {
      Application.OpenURL("http://bit.ly/2xIzfaO");
    }

    [MenuItem("Window/Sky Studio/Help/Review Sky Studio...")]
    private static void OpenAssetStorePage()
    {
      Application.OpenURL("http://bit.ly/2XDXsJV");
    }

    public override void OnInspectorGUI()
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PrefixLabel("Review Sun Rays");
      bool didClick = GUILayout.Button(new GUIContent("Open Sun Rays Store Page..."));
      if (didClick)
      {
        OpenAssetStorePage();
      }
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PrefixLabel("Tutorial Videos");
      didClick = GUILayout.Button(new GUIContent("Open Tutorials..."));
      if (didClick)
      {
        OpenVideoTutorials();
      }
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PrefixLabel("Chat Support");
      didClick = GUILayout.Button(new GUIContent("Join Discord for help..."));
      if (didClick)
      {
        OpenDiscordChat();
      }
      EditorGUILayout.EndHorizontal();
    }
  }
}
