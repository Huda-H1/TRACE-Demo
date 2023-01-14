#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_5_3_OR_NEWER	//	Unity 5.3
using UnityEditor.SceneManagement;
#endif
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace FantasticGlass
{
    public enum EditorToolsPreset_Option
    {
        Hidden,
        NoOption,
        ItemChanged,
        ItemDeleted,
        ItemAdded,
        DeleteAll,
        DeleteItem,
        LoadItem,
        SaveItem
    }

    /// <summary>
    /// N.B. Do not put this in an 'Editor' folder as it will make it inaccessible.
    /// </summary>
    public class EditorTools
    {
        #region Member Variables

        public string filename = "";
        //
        Editor editor = null;
        //
        public int scrollItemLength = 9;
        public float maxScrollHeight = 128f;
        public float minScrollHeight = 19f;
        public int pixelsPerScrollItem = 19;
        public float scrollPixelHeightOffset = 6;
        //
        public Color curveColour = new Color(0f, 1f, 0f);
        public float curveHeightMin = 19f;
        public float curveHeightMax = 38f;
        public bool enableDebugLogging = false;
        //
        private GUIStyle style_wordwrap;
        private GUIStyle style_bold;
        private GUIStyle style_wordwrap_bold;
        //
        private GUIStyle tempGUIStyle;
        //
        public static float defaultFadeTime = 2f;
        public static float defaultFadeTime_Max = 2f;
        //
        public float drawInterval = 0.03f;
        public float lastDrawTime;

        #endregion

        #region Constructor

        public EditorTools(string name, Editor _editor = null)
        {
            filename = name;

            editor = _editor;

            style_wordwrap = new GUIStyle();
            style_wordwrap.wordWrap = true;

            style_wordwrap_bold = new GUIStyle();
            style_wordwrap_bold.fontStyle = FontStyle.Bold;
            style_wordwrap_bold.wordWrap = true;

            style_bold = new GUIStyle(); ;
            style_bold.fontStyle = FontStyle.Bold;
        }

        public static EditorTools Create(UnityEngine.Object obj, Editor _editor = null)
        {
            string name = "";
            if (obj != null)
                name = obj.name + "_" + obj.GetInstanceID().ToString();
            else if (_editor != null)
                name = _editor.name + "_" + _editor.GetInstanceID().ToString();
            else
                name = "UnknownObject";
            return new EditorTools(name, _editor);
        }

        public void StartGUI()
        {
            drawInterval = (float)EditorApplication.timeSinceStartup - lastDrawTime;
            lastDrawTime = (float)EditorApplication.timeSinceStartup;
        }

        public void EndGUI()
        {
            editor.Repaint();
        }

        public bool ShowFadeOut(ref float counter, bool startShowing, int fadeTime = -1, int fadeSpeed = -1, float minimumValue = 0)
        {
            if (startShowing)
            {
                IncreaseTimer(ref counter, fadeTime);
            }
            else
            {
                DecreaseTimer(ref counter, fadeSpeed, minimumValue);
            }
            return (counter > minimumValue);
        }

        public string FlippableString(string stringOn, string stringOff, float counter)
        {
            if (counter > 0)
            {
                return stringOn;
            }
            else
            {
                return stringOff;
            }
        }

        public string FadeOutString(string stringOn, string stringOff, ref float counter, bool turnedOn, float fadeTime = -1)
        {
            if (fadeTime == -1)
                fadeTime = defaultFadeTime;
            if (turnedOn)
            {
                IncreaseTimer(ref counter, fadeTime);
            }
            else
            {
                DecreaseTimer(ref counter);
            }
            return (counter > 0) ? stringOn : stringOff;
        }

        public void FlippableLabel(string label, int stringCounter, bool wordWrap = false, bool bold = false)
        {
            if (stringCounter > 0)
            {
                Label(label, wordWrap, bold);
            }
        }

        public void FadeOutLabel(string label, bool startShowingLabel, ref float counter, bool wordWrap = false, bool bold = false, float fadeTime = -1)
        {
            if (fadeTime == -1)
                fadeTime = defaultFadeTime;
            if (startShowingLabel)
            {
                IncreaseTimer(ref counter, fadeTime);
            }
            else if (counter > 0)
            {
                DecreaseTimer(ref counter);
                if (counter > 0)
                {
                    Label(label, wordWrap, bold);
                }
            }
        }

        public void FadeOutLabel(string label, string stringValue, bool startShowingLabel, ref float counter, float fadeTime = -1)
        {
            if (fadeTime == -1)
                fadeTime = defaultFadeTime;
            if (startShowingLabel)
            {
                IncreaseTimer(ref counter, fadeTime);
            }
            else
            {
                DecreaseTimer(ref counter);
            }
            if (counter > 0)
            {
                Label(label, stringValue);
            }
        }

        public void IncreaseTimer(ref float counter, float amountIncrease = -1, float maximumValue = -1)
        {
            if (counter == maximumValue)
                return;
            if (amountIncrease == -1)
                amountIncrease = defaultFadeTime;
            if (maximumValue == -1)
                maximumValue = defaultFadeTime_Max;
            counter += amountIncrease;
            if (counter > maximumValue)
                counter = maximumValue;
            if (enableDebugLogging)
                Debug.Log("counter increased(+) to " + counter.ToString());
        }

        public void DecreaseTimer(ref float counter, float amountDecrease = -1, float minimumValue = 0)
        {
            if (counter == minimumValue)
                return;
            if (amountDecrease == -1)
                amountDecrease = drawInterval;
            counter -= amountDecrease;
            if (counter < minimumValue)
                counter = minimumValue;
            if (enableDebugLogging)
                Debug.Log("counter decreased(-) to " + counter.ToString());
        }

        #endregion

        #region Dirty

        public void SetDirty(UnityEngine.Object obj, bool markSceneDirty = true)
        {
            if (Application.isPlaying)
                return;
#if UNITY_EDITOR
            if (obj != null)
                EditorUtility.SetDirty(obj);
#if UNITY_5_3_OR_NEWER
            if (markSceneDirty)
                EditorSceneManager.MarkAllScenesDirty();
#endif
#endif
        }

        public void StartEdit(UnityEngine.Object obj, string editLabel)
        {
            if (obj == null)
                return;
            Undo.RecordObject(obj, editLabel);
        }

        public void FinishEdit(UnityEngine.Object obj, bool markSceneDirty = true)
        {
            if (obj == null)
                return;
            SetDirty(obj, markSceneDirty);
        }

        public void EndEdit(UnityEngine.Object obj, bool markSceneDirty = true)
        {
            FinishEdit(obj, markSceneDirty);
        }

        #endregion

        #region Change Check

        public void BeginChangeCheck()
        {
            EditorGUI.BeginChangeCheck();
        }

        public void StartChangeCheck()
        {
            BeginChangeCheck();
        }

        public bool EndChangeCheck()
        {
            try
            {
                return EditorGUI.EndChangeCheck();
            }
            catch (Exception e)
            {
                Debug.LogError("Exception in EditorTools->EndChangeCheck. Message:" + e.Message);
                return false;
            }
        }

        public bool StopChangeCheck()
        {
            return EndChangeCheck();
        }

        public bool FinishChangeCheck()
        {
            return EndChangeCheck();
        }

        #endregion

        #region Show List

        private bool ShowList(string label, ref List<UnityEngine.Object> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(null);
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<GameObject> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(null);
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<int> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(0);
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<string> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add("");
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<Transform> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(null);
            }
            //
            return show;
        }

        public bool ShowList(string label, ref List<Vector3> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(new Vector3());
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<Vector2> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(new Vector2());
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<Material> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(null);
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<Renderer> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            if (listSize == 0)
                return show;
            //
            listSize = Mathf.Max(1, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(null);
            }
            //
            return show;
        }

        private bool ShowList(string label, ref List<Glass> list, ref bool show)
        {
            show = EditorGUILayout.Foldout(show, label);
            if (!show)
                return show;
            //
            int listSize = EditorGUILayout.IntField("   size", list.Count);
            //
            //if (listSize == 0)
            //    return show;
            //
            listSize = Mathf.Max(0, listSize);
            //
            while (listSize < list.Count)
            {
                list.RemoveAt(list.Count - 1);
            }
            //
            while (listSize > list.Count)
            {
                list.Add(null);
            }
            //
            return show;
        }

        #endregion

        #region GUI List

        //  Vector2

        public bool GUI_List(string label, ref List<Vector2> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.Vector2Field("0", list[0]);
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem) + scrollPixelHeightOffset;
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.Vector2Field(i2.ToString(), list[i2]);
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  Vector3

        public bool GUI_List(string label, ref List<Vector3> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.Vector3Field("0", list[0]);
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem) + scrollPixelHeightOffset;
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.Vector3Field(i2.ToString(), list[i2]);
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  Material

        public bool GUI_List(string label, ref List<Material> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.ObjectField("0", list[0], typeof(Material), true) as Material;
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem) + scrollPixelHeightOffset;
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.ObjectField(i2.ToString(), list[i2], typeof(Material), true) as Material;
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  Renderer

        public bool GUI_List(string label, ref List<Renderer> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.ObjectField("0", list[0], typeof(Renderer), true) as Renderer;
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem) + scrollPixelHeightOffset;
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.ObjectField(i2.ToString(), list[i2], typeof(Renderer), true) as Renderer;
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }


        //  Glass
        public bool GUI_List(string label, ref List<Glass> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.ObjectField(list[0], typeof(Glass), true) as Glass;
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem) + scrollPixelHeightOffset;
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.ObjectField(i2.ToString(), list[i2], typeof(Glass), true) as Glass;
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  Transform

        public bool GUI_List(string label, ref List<Transform> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            ;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.ObjectField(list[0], typeof(Transform), true) as Transform;
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem) + scrollPixelHeightOffset;
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.ObjectField(i2.ToString(), list[i2], typeof(Transform), true) as Transform;
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  int

        public bool GUI_List(string label, ref List<int> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.IntField(list[0]);
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem) + scrollPixelHeightOffset;
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.IntField(i2.ToString(), list[i2]);
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        //  string

        public bool GUI_List(string label, ref List<string> list, ref bool show, ref Vector2 scrollPosition)
        {
            if (!ShowList(label, ref list, ref show))
                return false;
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.TextField(list[0]);
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem) + scrollPixelHeightOffset;
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            list[i2] = EditorGUILayout.TextField(i2.ToString(), list[i2]);
                        }
                    }
                }
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            return true;
        }

        #endregion

        #region GUI List - ReOrderable

        /// <summary>
        /// A re-orderable list of Glass.
        /// Returns the resulting index of the last moved object. -1 if no move.
        /// </summary>
        /// <returns>The resulting index of the last moved object. -1 if no move.</returns>
        /// <param name="label">Label.</param>
        /// <param name="list">List.</param>
        /// <param name="show">Show.</param>
        /// <param name="scrollPosition">Scroll position.</param>
        /// <param name="topString">Top string.</param>
        /// <param name="bottomString">Bottom string.</param>
        public void GUI_ReorderableList(string label, ref List<Glass> list, ref bool show, ref Vector2 scrollPosition,
                                       ref int movedObjectNewIndex, ref int movedObjectPreviousIndex,
                                       string topString = "Top", string bottomString = "Bottom")
        {
            if (!ShowList(label, ref list, ref show))
            {
                movedObjectPreviousIndex = -1;
                movedObjectNewIndex = -1;
                return;
            }
            //
            if (list.Count == 1)
            {
                list[0] = EditorGUILayout.ObjectField(list[0], typeof(Glass), true) as Glass;
            }
            else {
                StartSection();
                //
                float adjustedMinScrollHeight = Mathf.Max(minScrollHeight, Mathf.Min(list.Count, scrollItemLength) * pixelsPerScrollItem) + scrollPixelHeightOffset;
                //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxScrollHeight), GUILayout.MinHeight(adjustedMinScrollHeight));
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(adjustedMinScrollHeight));
                int itemIteration = Mathf.Min(list.Count, Mathf.Max(0, Mathf.Abs(scrollItemLength)));
                //
                int moveUp = -1;
                int moveDown = -1;
                int moveTop = -1;
                int moveBottom = -1;
                //
                for (int i = 0; i < list.Count; i += itemIteration)
                {
                    for (int i2 = i; i2 < (i + itemIteration); i2++)
                    {
                        if (i2 < list.Count)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(i2 == 0 ? topString : (i2 == list.Count - 1) ? bottomString : i2.ToString(), GUILayout.MaxWidth(90f));
                            ListOrderButtons_Up(i2, list.Count, ref moveTop, ref moveUp, topString, bottomString);
                            list[i2] = EditorGUILayout.ObjectField(list[i2], typeof(Glass), true) as Glass;
                            ListOrderButtons_Down(i2, list.Count, ref moveBottom, ref moveDown, bottomString);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                //
                if (moveTop != -1)  //  top is 0
                {
                    if (moveTop > 0)
                    {
                        Glass movedItem = list[moveTop];
                        list.Remove(movedItem);
                        list.Insert(0, movedItem);
                        movedObjectPreviousIndex = moveTop;
                        movedObjectNewIndex = list.IndexOf(movedItem);
                    }
                }
                if (moveBottom != -1)   // bottom = highest index
                {
                    if (moveBottom < (list.Count - 1))
                    {
                        Glass movedItem = list[moveBottom];
                        list.RemoveAt(moveBottom);
                        list.Add(movedItem);
                        movedObjectPreviousIndex = moveBottom;
                        movedObjectNewIndex = list.IndexOf(movedItem);
                    }
                }
                if (moveUp != -1)  //  up = lower index
                {
                    if (moveUp > 0)
                    {
                        Glass movedItem = list[moveUp];
                        list.Remove(movedItem);
                        list.Insert(moveUp - 1, movedItem);
                        movedObjectPreviousIndex = moveUp;
                        movedObjectNewIndex = list.IndexOf(movedItem);
                    }
                }
                if (moveDown != -1) //  down = higher index
                {
                    if (moveDown < (list.Count - 1))
                    {
                        Glass movedItem = list[moveDown];
                        list.Remove(movedItem);
                        list.Insert(moveDown + 1, movedItem);
                        movedObjectPreviousIndex = moveDown;
                        movedObjectNewIndex = list.IndexOf(movedItem);
                    }
                }
                //
                GUILayout.EndScrollView();
                //
                EndSection();
            }
            movedObjectPreviousIndex = -1;
            movedObjectNewIndex = -1;
        }

        private void ListOrderButtons_Up(int itemIndex, int itemCount, ref int topIndex, ref int upIndex, string topString = "Top", string bottomString = "Bottom")
        {
            if (itemIndex > 0)
            {
                //string itemLabel = itemIndex < (itemCount - 1) ? itemIndex.ToString() : bottomString;
                if (Button(topString, "", false))
                {
                    topIndex = itemIndex;
                }
                if (Button("↑", "", false))
                {
                    upIndex = itemIndex;
                }
            }
            else
            {
                string blankButtonString = "-";
                for (int i = 0; i < topString.Length; i++)
                    blankButtonString += "-";
                //Button(blankButtonString, "", false);
                //Button("-", "", false);
            }
        }

        private void ListOrderButtons_Down(int itemIndex, int itemCount, ref int bottomIndex, ref int downIndex, string bottomString = "Bottom")
        {
            if (itemIndex < itemCount - 1)
            {
                if (Button("↓", "", false))
                {
                    downIndex = itemIndex;
                }
                if (Button(bottomString, "", false))
                {
                    bottomIndex = itemIndex;
                }
            }
            else
            {
                string blankButtonString = "-";
                for (int i = 0; i < bottomString.Length; i++)
                    blankButtonString += "-";
                //Button("-", "", false);
                //Button(blankButtonString, "", false);
            }
        }

        #endregion

        #region Button

        public bool Button(string buttonLabel, string leftHandLabel = "", bool expandWidth = true)
        {
            bool pressed = false;
            GUILayout.BeginHorizontal(leftHandLabel, GUIStyle.none);
            GUILayout.FlexibleSpace();
            pressed = GUILayout.Button(buttonLabel, GUILayout.ExpandWidth(expandWidth));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return pressed;
        }

        #endregion

        #region Sections

        /// <summary>
        /// Displays a togglable indented section.
        /// REMEMBER to call EndSection() at section end.
        /// </summary>
        /// <param name="sectionTitle"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ShowSection(string sectionTitle, ref bool value)
        {
            value = EditorGUILayout.Foldout(value, sectionTitle);
            if (value)
                StartSection();
            return value;
        }

        /// <summary>
        /// Starts the indented section.
        /// REMEMBER to call EndSection() at section end.
        /// </summary>
        public void StartSection()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            EditorGUI.indentLevel++;
        }

        /// <summary>
        /// Ends the indented section.
        /// REMEMBER to call StartSection() first.
        /// </summary>
        /// <param name="showDivider">If set to <c>true</c> show divider.</param>
        /// <param name="_dividerHeight">Divider height.</param>
        public void EndSection(bool showDivider = false, int _dividerHeight = 1)
        {
            if (showDivider)
                Divider(_dividerHeight);
            //
            GUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        #endregion

        #region Labels

        public void Label(string label, bool wordWrap = false, bool bold = false)
        {
            if (wordWrap)
            {
                if (bold)
                {
                    EditorGUILayout.LabelField(label, style_wordwrap_bold);
                }
                else
                {
                    EditorGUILayout.LabelField(label, style_wordwrap);
                }
            }
            else if (bold)
            {
                EditorGUILayout.LabelField(label, style_bold);
            }
            else
            {
                EditorGUILayout.LabelField(label);
            }
        }

        public void BoldLabel(string label, bool wordWrap = false)
        {
            if (wordWrap)
            {
                EditorGUILayout.LabelField(label, style_wordwrap_bold);
            }
            else
            {
                EditorGUILayout.LabelField(label, style_bold);
            }
        }

        /// <summary>
        /// This displays a NON-editable string. Use the String or Text options for editable versions.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value"></param>
        public void Label(string label, string value)
        {
            EditorGUILayout.LabelField(label, value);
        }

        #endregion

        #region Decorations

        public void HorizontalLine(int _height = 1)
        {
            Divider(_height);
        }

        public void Divider(int _height = 1)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(_height) });
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public void Space()
        {
            EditorGUILayout.Space();
        }

        #endregion

        #region Inspector Settings

        public void GUI_InspectorSettings()
        {
            IntOption("Items Per Scroll View", ref scrollItemLength, 2, 128);
            //
            FloatOption("Scroll View height MIN", ref minScrollHeight, 0f, 512f);
            FloatOption("Scroll View height MAX", ref maxScrollHeight, 0f, 512f);
            //
            IntOption("Pixels per Scroll Item", ref pixelsPerScrollItem, 1, 128);
            //
            FloatOption("Scroll Pixel Height Offset", ref scrollPixelHeightOffset);
            //
            ColourOption("Curve Colour", ref curveColour);
            FloatOption("Curve Graph height MIN", ref curveHeightMin, 19f, 128);
            FloatOption("Curve Graph height MAX", ref curveHeightMax, 19f, 128);
        }

        #endregion

        #region Options (singular values)

        public void SetBold(bool setBold = true)
        {
            if (setBold)
            {
                tempGUIStyle = new GUIStyle(EditorStyles.label);
                EditorStyles.label.fontStyle = EditorStyles.boldLabel.fontStyle;
                EditorStyles.label.font = EditorStyles.boldLabel.font;
            }
        }

        public void UnsetBold(bool setBold = true)
        {
            if (setBold)
            {
                EditorStyles.label.fontStyle = tempGUIStyle.fontStyle;
                EditorStyles.label.font = tempGUIStyle.font;
            }
        }

        public bool BoolOption(string label, ref bool value, bool bold = false, bool longTitle = false)
        {
            if (longTitle)
            {
                EditorGUILayout.BeginHorizontal();
                Label(label, true, bold);
                EditorGUILayout.Toggle("", value, EditorStyles.toggle);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                SetBold(bold);
                value = EditorGUILayout.Toggle(label, value, EditorStyles.toggle);
                UnsetBold(bold);
            }
            return value;
        }

        public bool BoolOption(string label, bool value)
        {
            return EditorGUILayout.Toggle(label, value);
        }

        public float FloatOption(string label, ref float value)
        {
            value = EditorGUILayout.FloatField(label, value);
            return value;
        }

        public float FloatOption(string label, float value)
        {
            return EditorGUILayout.FloatField(label, value);
        }

        public float FloatOption(string label, ref float value, float min, float max)
        {
            value = EditorGUILayout.Slider(label, value, min, max);
            return value;
        }

        public int IntOption(string label, ref int value)
        {
            value = EditorGUILayout.IntField(label, value);
            return value;
        }

        public int IntOption(string label, int value)
        {
            return EditorGUILayout.IntField(label, value);
        }

        public int IntOption(string label, ref int value, int min, int max)
        {
            value = EditorGUILayout.IntSlider(label, value, min, max);
            return value;
        }

        public Vector2 VectorOption(string label, ref Vector2 value)
        {
            value = EditorGUILayout.Vector2Field(label, value);
            return value;
        }

        public Vector2 VectorOption(string label, Vector2 value)
        {
            return EditorGUILayout.Vector2Field(label, value);
        }

        public Vector3 VectorOption(string label, ref Vector3 value)
        {
            value = EditorGUILayout.Vector3Field(label, value);
            return value;
        }

        public Vector3 VectorOption(string label, Vector3 value)
        {
            return EditorGUILayout.Vector3Field(label, value);
        }

        public Vector4 VectorOption(string label, ref Vector4 value)
        {
            value = EditorGUILayout.Vector4Field(label, value);
            return value;
        }

        public Vector4 VectorOption(string label, Vector4 value)
        {
            return EditorGUILayout.Vector4Field(label, value);
        }

        public AnimationCurve CurveOption(string label, ref AnimationCurve value, Rect range)
        {
            value = EditorGUILayout.CurveField(label, value, curveColour, range, GUILayout.MinHeight(curveHeightMin), GUILayout.MaxHeight(curveHeightMax));
            return value;
        }

        public string StringOption(string label, ref string value, bool editable = true)
        {
            string initialValue = value;
            value = EditorGUILayout.TextField(label, value);
            if (!editable)
                value = initialValue;
            return value;
        }

        public string TextOption(string label, ref string value, bool editable = true)
        {
            string initialValue = value;
            value = StringOption(label, ref value);
            if (!editable)
                value = initialValue;
            return value;
        }

        public string LabelOption(string label, ref string value, bool editable = true)
        {
            string initialValue = value;
            value = StringOption(label, ref value);
            if (!editable)
                value = initialValue;
            return value;
        }

        public Shader ShaderOption(string label, ref Shader value)
        {
            value = EditorGUILayout.ObjectField(label, value, typeof(Shader), true) as Shader;
            return value;
        }

        public Color ColourOption(string label, ref Color colour)
        {
            colour = EditorGUILayout.ColorField(label, colour);
            return colour;
        }

        public Color ColourOption(string label, Color colour)
        {
            return EditorGUILayout.ColorField(label, colour);
        }

        /// <summary>
        /// Pass in the current enum value and cast the returned value back to your enum type.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="currentEnum"></param>
        /// <returns></returns>
		public Enum EnumOption(string label, Enum currentEnum)
        {
            return EditorGUILayout.EnumPopup(label, (Enum)currentEnum);
        }

        /// <summary>
        /// Shows a selection of labels in a pop-up.
        ///	Sets the referenced value and index to the latest selection.
        ///	Returns the chosen value.
        /// </summary>
        /// <param name="label">Label.</param>
        /// <param name="itemLabels">Item labels.</param>
        /// <param name="itemValues">Item values.</param>
        /// <param name="value">Value.</param>
        public int Popup(string label, string[] itemLabels, int[] itemValues, ref int currentIndex, ref int currentValue)
        {
            currentIndex = EditorGUILayout.Popup(label, currentIndex, itemLabels);
            currentValue = itemValues[currentIndex];
            return currentValue;
        }

        public GameObject GameObjectOption(string label, ref GameObject obj)
        {
            obj = EditorGUILayout.ObjectField(label, obj, typeof(GameObject), true) as GameObject;
            return obj;
        }

        public GameObject GameObjectOption(string label, GameObject obj)
        {
            return EditorGUILayout.ObjectField(label, obj, typeof(GameObject), true) as GameObject;
        }

        public Camera CameraOption(string label, ref Camera cam)
        {
            cam = EditorGUILayout.ObjectField(label, cam, typeof(Camera), true) as Camera;
            return cam;
        }

        public GlassDepthCamera GlassDepthCamOption(string label, ref GlassDepthCamera glassDepthCam)
        {
            glassDepthCam = EditorGUILayout.ObjectField(label, glassDepthCam, typeof(GlassDepthCamera), true) as GlassDepthCamera;
            return glassDepthCam;
        }

        public Glass GlassOption(string label, ref Glass value)
        {
            value = EditorGUILayout.ObjectField(label, value, typeof(Glass), true) as Glass;
            return value;
        }

        public Glass GlassOption(string label, Glass value)
        {
            return EditorGUILayout.ObjectField(label, value, typeof(Glass), true) as Glass;
        }

        public Mesh MeshOption(string label, ref Mesh mesh)
        {
            mesh = EditorGUILayout.ObjectField(label, mesh, typeof(Mesh), true) as Mesh;
            return mesh;
        }

        public Material MaterialOption(string label, ref Material material)
        {
            material = EditorGUILayout.ObjectField(label, material, typeof(Material), true) as Material;
            return material;
        }

        public Texture TextureOption(string label, ref Texture texture)
        {
            texture = EditorGUILayout.ObjectField(label, texture, typeof(Texture), true) as Texture;
            return texture;
        }

        public Texture TextureOption(string label, Texture texture)
        {
            return EditorGUILayout.ObjectField(label, texture, typeof(Texture), true) as Texture;
        }

        public LayerMask LayerOption(string label, ref int layer)
        {
            string[] layerNames = new string[32];
            for (int i = 0; i < 32; i++)
            {
                layerNames[i] = LayerMask.LayerToName(i);
            }
            layer = EditorGUILayout.MaskField(label, layer, layerNames);
            return layer;
        }

        #endregion

        #region Messages

        /// <summary>
        /// A version of the Message function that can be called without an instance of EditorTools.
        /// </summary>
        /// <returns><c>true</c>, if message appeared and 'ok' was clicked, <c>false</c> otherwise returns false.</returns>
        /// <param name="title">Title.</param>
        /// <param name="message">Message.</param>
        /// <param name="ok">Ok.</param>
        /// <param name="cancel">Cancel.</param>
        public static bool Message_static(string title, string message, string ok = "OK", string cancel = "")
        {
            if (Application.isPlaying)
            {
                Debug.Log("[CANNOT show (static) message when Application is Playing: Message - Title:" + title + ". Message: " + message + ".");
                return false;
            }
            Debug.Log("(static) Message - Title:" + title + "; Message: " + message + ";");
            if (cancel.Length > 0)
            {
                return EditorUtility.DisplayDialog(title, message, ok, cancel);
            }
            else {
                return EditorUtility.DisplayDialog(title, message, ok);
            }
        }

        public bool Message(string title, string message, string ok = "OK", string cancel = "")
        {
            if (Application.isPlaying)
            {
                if (enableDebugLogging)
                    Debug.Log("[CANNOT show message when Application is Playing: Message - Title:" + title + ". Message: " + message + ".");
                return false;
            }
            if (enableDebugLogging)
                Debug.Log("Message - Title:" + title + "; Message: " + message + ";");
            if (cancel.Length > 0)
            {
                return EditorUtility.DisplayDialog(title, message, ok, cancel);
            }
            else {
                return EditorUtility.DisplayDialog(title, message, ok);
            }
        }

        public int MessageComplex(string title, string message, string yes = "Yes", string no = "No", string cancel = "Cancel")
        {
            if (enableDebugLogging)
                Debug.Log("Message (Complex) - Title: " + title + "; Message: " + message + ";");
            return EditorUtility.DisplayDialogComplex(title, message, yes, no, cancel);
        }

        #endregion

        #region Components (Objects saved as Binary files)

        public bool SaveComponent(object obj, string suffix)
        {
            BinaryFormatter bf = new BinaryFormatter();
            string componentPath = Application.persistentDataPath + "/" + filename + suffix + ".gd";
            FileStream file = File.Create(componentPath);
            bool success = false;
            try
            {
                bf.Serialize(file, obj);
                success = true;
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to serialize '" + componentPath + "'. Reason: " + e.Message);
            }
            finally
            {
                file.Close();
            }
            return success;
        }

        public void SaveComponent(object obj, string customFilename, string fileType)
        {
            BinaryFormatter bf = new BinaryFormatter();
            string componentPath = Application.persistentDataPath + "/" + customFilename + fileType;
            FileStream file = File.Create(componentPath);
            try
            {
                bf.Serialize(file, obj);
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to SAVE component: '" + componentPath + "' during Deserialize. Reason: " + e.Message);
            }
            finally
            {
                file.Close();
            }
        }

        public object LoadComponent(string suffix)
        {
            object obj = null;
            if (File.Exists(Application.persistentDataPath + "/" + filename + suffix + ".gd"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                string componentPath = Application.persistentDataPath + "/" + filename + suffix + ".gd";
                FileStream file = File.Open(componentPath, FileMode.Open);
                try
                {
                    obj = bf.Deserialize(file);
                }
                catch (SerializationException e)
                {
                    Debug.LogError("Failed to LOAD component: '" + componentPath + "' during Deserialize. Reason: " + e.Message);
                }
                finally
                {
                    file.Close();
                }
            }
            return obj;
        }

        public bool DeleteComponent(string suffix)
        {
            string componentPath = Application.persistentDataPath + "/" + filename + suffix + ".gd";
            if (File.Exists(componentPath))
            {
                try
                {
                    File.Delete(componentPath);
                }
                catch
                {
                    Debug.LogError("Failed to DELETE component: '" + componentPath + "' during Deserialize.");
                }
                finally
                {
                }
                return true;
            }
            else {
                Debug.LogError("Tried to delete file that does not exist at path: " + Application.persistentDataPath + "/" + filename + suffix + ".gd");
            }
            return false;
        }

        #endregion

        #region Prefabs

        public static GameObject LoadDefaultPrefab(string path)
        {
            if (!path.Contains(".Prefab"))
                if (!path.Contains(".prefab"))
                    path += ".Prefab";
            return EditorGUIUtility.Load(path) as GameObject;
        }

        #endregion

        #region Defaults

        public static UnityEngine.Object LoadDefault(string path)
        {
            return EditorGUIUtility.Load(path);
        }

        #endregion

        #region Presets

        public void PresetList_Basic(string label, string listFilePath, ref string currentItem, ref int currentItemIndex, ref List<string> presetList)
        {
            if (currentItemIndex == -1)
            {
                currentItemIndex = 0;
                if (presetList.Count > currentItemIndex)
                {
                    currentItem = presetList[currentItemIndex];
                }
            }
            //
            if (currentItem == null)
            {
                if (currentItemIndex > -1)
                {
                    if (presetList.Count > currentItemIndex)
                    {
                        currentItem = presetList[currentItemIndex];
                    }
                }
            }
            //
            if (presetList == null)
            {
                presetList = new List<string>();
            }
            //
            currentItemIndex = EditorGUILayout.Popup(label, currentItemIndex, presetList.ToArray());
            if (presetList.Count > currentItemIndex)
            {
                currentItem = presetList[currentItemIndex];
            }
        }

        public EditorToolsPreset_Option PresetList(string label, string listFilePath, ref string currentItem, ref int currentItemIndex, ref List<string> presetList, ref bool showPreset, bool showLoadButton = true, bool showSaveButton = true, bool showDeleteSingleButton = true, bool showDeleteAllButton = false)
        {
            if (currentItemIndex == -1)
            {
                currentItemIndex = 0;
                if (presetList.Count > currentItemIndex)
                {
                    currentItem = presetList[currentItemIndex];
                }
            }

            string currentItemString = "empty";
            if (currentItem != null)
            {
                if (currentItem.Length > 0)
                {
                    currentItemString = currentItem;
                }
            }
            else
            {
                if (currentItemIndex > -1)
                {
                    if (presetList.Count > currentItemIndex)
                    {
                        currentItem = presetList[currentItemIndex];
                        currentItemString = currentItem;
                    }
                }
            }

            if (!ShowSection(label + " (" + currentItemString + ")", ref showPreset))
            {
                return EditorToolsPreset_Option.Hidden;
            }

            /*
            if (presetList == null)
                presetList = LoadComponent (listFilePath) as List<string>;
            else if (presetList.Count == 0)
                presetList = LoadComponent (listFilePath) as List<string>;
            */

            if (presetList == null)
            {
                presetList = new List<string>();
                //SaveComponent(presetList, listFilename);
            }
            //
            currentItem = EditorGUILayout.TextField("Preset Name", currentItem);
            //
            EditorGUI.BeginChangeCheck();
            currentItemIndex = EditorGUILayout.Popup("Preset List", currentItemIndex, presetList.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                if (presetList.Count > 0)
                {
                    currentItem = presetList[currentItemIndex];
                }
                EndSection();
                return EditorToolsPreset_Option.ItemChanged;
            }

            //  SAVE
            if (showSaveButton)
            {
                if (Button("Save"))
                {
                    if (currentItem == null)
                    {
                        EditorUtility.DisplayDialog("Saving Preset FAILED", "Please give the current preset a name.", "OK");
                        EndSection();
                        return EditorToolsPreset_Option.NoOption;
                    }
                    if (presetList.Count == 0)
                    {
                        EndSection();
                        return EditorToolsPreset_Option.SaveItem;
                    }
                    if (presetList.Contains(currentItem))
                    {
                        if (currentItemIndex != presetList.LastIndexOf(currentItem))
                        {
                            EditorUtility.DisplayDialog("Unable To Save Preset", "A preset already exists with that name.", "OK");
                            EndSection();
                            return EditorToolsPreset_Option.NoOption;
                        }
                    }
                    //
                    EndSection();
                    return EditorToolsPreset_Option.SaveItem;
                }
            }
            //  LOAD
            if (showLoadButton)
            {
                if (Button("Load"))
                {
                    EndSection();
                    return EditorToolsPreset_Option.LoadItem;
                }
            }
            //  DELETE (Single)
            if (showDeleteSingleButton)
            {
                if (Button("Delete (PERMANENT)"))
                {
                    if (EditorUtility.DisplayDialog("Delete Delected Preset?", "Are you sure you wish to PERMANENTLY delete this prefab?", "Yes", "No"))
                    {
                        EndSection();
                        return EditorToolsPreset_Option.DeleteItem;
                    }
                    else {
                        EndSection();
                        return EditorToolsPreset_Option.NoOption;
                    }
                }
            }
            //  DELETE (All)
            if (showDeleteAllButton)
            {
                if (Button("Delete ALL (PERMANENT)"))
                {
                    if (EditorUtility.DisplayDialog("Delete ALL Presets?", "Are you sure you wish to PEMANENTLY delete ALL prefabs?", "Yes", "No"))
                    {
                        EndSection();
                        return EditorToolsPreset_Option.DeleteAll;
                    }
                    else {
                        EndSection();
                        return EditorToolsPreset_Option.NoOption;
                    }
                }
            }

            EndSection();
            return EditorToolsPreset_Option.NoOption;
        }

        //  SAVE / DELETE
        //  use the public functions to save presets, as it handles the whole process
        private bool SavePreset(object presetObject, string presetName, string listFilename)
        {
            return SavePreset(presetObject, PresetFilename(presetName, listFilename));
        }

        private bool SavePreset(object presetObject, string presetFilename)
        {
            return SaveComponent(presetObject, presetFilename);
        }

        private bool DeletePreset(string presetName, string listFilename)
        {
            return DeletePreset(PresetFilename(presetName, listFilename));
        }

        private bool DeletePreset(string presetFilename)
        {
            return DeleteComponent(presetFilename);
        }

        //  SAVE/DELETE W/POST
        //	These versions of functions handle the whole process, including managing the Editor GUI aspect.

        public bool SavePreset(object presetObject, string presetName, int presetIndex, ref List<string> presetList, string listFilename)
        {
            //if (SavePreset(presetObject, presetName, listFilename))
            //{
            return SavedPreset(presetName, presetIndex, ref presetList, listFilename);
            //}
            //return false;
        }

        public bool DeletePreset(string presetName, string listFilename, int presetIndex, ref List<string> presetlist)
        {
            //if (DeletePreset(presetName, listFilename))
            //{
            return DeletedPreset(presetName, presetIndex, ref presetlist, listFilename);
            //}
            //return false;
        }

        //  POST - SAVE/DELETE

        public bool SavedPreset(string currentPresetName, int currentPresetIndex, ref List<string> presetList, string listFilename)
        {
            int indexOfPreset = presetList.IndexOf(currentPresetName);
            if (indexOfPreset < 0 || indexOfPreset >= presetList.Count || presetList.Count == 0)
            {
                presetList.Add(currentPresetName);
                //SaveComponent(presetList, listFilename);
            }
            return true;
        }

        public bool DeletedPreset(string presetName, int presetIndex, ref List<string> presetList, string listFilename)
        {
            if (presetList.IndexOf(presetName) == presetIndex)
            {
                presetList.Remove(presetName);
                //SaveComponent(presetList, listFilename);
                return true;
            }
            else if (presetList.Contains(presetName))
            {
                if (enableDebugLogging)
                    Debug.Log("Attempting to delete preset:" + presetName + ". However, the index does not match. Deleting name match...");
                presetList.Remove(presetName);
                //SaveComponent(presetList, listFilename);
                return true;
            }
            else {
                return false;
            }
        }

        public string PresetFilename(string presetName, string listFilename)
        {
            return filename + listFilename + presetName;
        }

        #endregion
    }
}

#endif
