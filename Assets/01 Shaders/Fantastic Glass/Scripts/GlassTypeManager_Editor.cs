#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Xml.Serialization;
using System.IO;
using UnityEditorInternal;

namespace FantasticGlass
{

    /// <summary>
    /// Glass editor.
    /// NOTE: Do not place this file in an 'Editor' folder as it will break.
    /// </summary>
    [CustomEditor(typeof(GlassRenderOrderManager))]
    [CanEditMultipleObjects]
    public class GlassTypeManager_Editor : Editor
    {
        #region Member Variables

        EditorTools tools;

        GlassRenderOrderManager manager;

        ReorderableList list;

        bool listModified = false;

        Color[] listColours = new Color[] { new Color(0.6f,0.4f,0.4f),
            new Color(0.4f, 0.6f, 0.4f),
            new Color(0.4f, 0.4f, 0.6f),

            new Color(0.8f,0.4f,0.4f),
            new Color(0.4f, 0.8f, 0.4f),
            new Color(0.4f, 0.4f, 0.8f),

            new Color(0.6f,0.2f,0.4f),
            new Color(0.2f, 0.6f, 0.4f),
            new Color(0.2f, 0.4f, 0.6f) };
        int listColourIndex = 0;

        #endregion

        void OnEnable()
        {
            manager = target as GlassRenderOrderManager;

            if (tools == null)
            {
                tools = EditorTools.Create(manager, this);
            }

            manager.RefreshGlassTypes();

            list = new ReorderableList(manager.glassTypes, typeof(GlassType), true, false, false, false);
            list.drawHeaderCallback = HeaderCallBackDelegate;
            list.drawElementCallback = ElementCallBackDelegate;
            list.onReorderCallback = ReorderCallBackDelegate;
            list.onChangedCallback = ChangedCallBackDelegate;
            list.onSelectCallback = SelectCallBackDelegate;
        }

        private void SelectCallBackDelegate(ReorderableList list)
        {
            listModified = true;
        }

        private void ChangedCallBackDelegate(ReorderableList list)
        {
            listModified = true;
        }

        private void HeaderCallBackDelegate(Rect rect)
        {
            EditorGUI.LabelField(rect, "Render Order");
        }

        private void ReorderCallBackDelegate(ReorderableList list)
        {
            listModified = true;
            //manager.RenderOrderFromOrder();

            //manager.RefreshGlassTypes();
        }

        void IncreaseListColour()
        {
            listColourIndex++;
            if (listColourIndex >= listColours.Length)
                listColourIndex = 0;
        }

        void ElementCallBackDelegate(Rect rect, int index, bool isActive, bool isFocused)
        {
            float elementWidth = rect.width;

            float elementY = rect.y + 2f;

            float buttonOffset = 5f;



            GlassType element = manager.GetGlassType(index);//manager.glassTypes[index];
            if (element == null)
            {
                manager.RemoveNullypes();
                return;
            }


            if (!element.MatchingRenderOrder(manager.GetGlassTypeBefore(element)))
            {
                IncreaseListColour();
            }
            GUI.backgroundColor = listColours[listColourIndex];


            float indexLabelWidth = index.ToString().Length * 10f;
            float doubleRenderWidth = element.isGlass ? 20f : 0f;
            float renderQueueWidth = element.renderOrder.ToString().Length * 8.5f;
            float renderBackWidth = element.isGlass ? 45f : 0f;
            float renderFrontWidth = renderBackWidth;
            float copyCountWidth = element.HasCopies() ? 30f : 0f;
            float revertButtonWidth = element.CanRevert() ? 60f : 0f;

            float nameLabelWidth = elementWidth
                - indexLabelWidth
                - renderQueueWidth
                - doubleRenderWidth
                - copyCountWidth
                - buttonOffset
                - renderFrontWidth
                - renderBackWidth
                - revertButtonWidth;



            Rect indexLabelRect = new Rect(rect.x, elementY,
                indexLabelWidth, EditorGUIUtility.singleLineHeight);

            Rect nameLabelRect = new Rect(indexLabelRect.xMax, elementY,
                nameLabelWidth, EditorGUIUtility.singleLineHeight);

            Rect copyCountRect = new Rect(nameLabelRect.xMax, elementY,
                copyCountWidth, EditorGUIUtility.singleLineHeight);

            Rect renderQueueRect = new Rect(copyCountRect.xMax, elementY,
                renderQueueWidth, EditorGUIUtility.singleLineHeight);

            Rect doubleRenderRect = new Rect(renderQueueRect.xMax + buttonOffset, elementY,
                doubleRenderWidth, EditorGUIUtility.singleLineHeight);

            Rect renderBackRect = new Rect(doubleRenderRect.xMax + buttonOffset, elementY,
                renderBackWidth, EditorGUIUtility.singleLineHeight);

            Rect renderFrontRect = new Rect(renderBackRect.xMax, elementY,
                renderFrontWidth, EditorGUIUtility.singleLineHeight);


            Rect revertButtonRect = new Rect(renderQueueRect.xMax + buttonOffset, elementY,
                revertButtonWidth, EditorGUIUtility.singleLineHeight);



            EditorGUI.LabelField(indexLabelRect, (index + 1).ToString());

            EditorGUI.LabelField(nameLabelRect, element.presetName);

            if (element.HasCopies())
            {
                EditorGUI.LabelField(copyCountRect, "(" + element.CopyNumber() + ")");
            }

            int newOrder = EditorGUI.IntField(renderQueueRect, element.renderOrder);
            if (newOrder != element.renderOrder)
            {
                element.SetRenderOrder(newOrder);
                ////manager.SortByRenderOrder();
                //listModified = true;
            }

            if (element.isGlass)
            {
                if (!element.isCopy)
                {
                    if (GUI.Button(doubleRenderRect, "+"))
                    {
                        manager.AddCopy(element);
                        listModified = true;
                    }
                }
                else
                {
                    if (GUI.Button(doubleRenderRect, "-"))
                    {
                        manager.RemoveCopy(element);
                        listModified = true;
                    }
                }
            }
            //element.renderTwice = EditorGUI.ToggleLeft(doubleRenderRect, "2", element.renderTwice);

            if (element.isGlass)
            {
                bool newRenderBack = EditorGUI.ToggleLeft(renderBackRect, "back", element.renderBack);
                bool newRenderFront = EditorGUI.ToggleLeft(renderFrontRect, "front", element.renderFront);

                if (newRenderBack != element.renderBack)
                {
                    listModified = true;
                }

                if (newRenderFront != element.renderFront)
                {
                    listModified = true;
                }

                element.renderBack = newRenderBack;
                element.renderFront = newRenderFront;
            }

            if (element.CanRevert())
            {
                if (GUI.Button(revertButtonRect, "Revert"))
                {
                    element.RevertRenderQueue();
                    //manager.SortByRenderOrder();
                    listModified = true;
                }
            }


            GUI.backgroundColor = Color.white;
        }

        public override void OnInspectorGUI()
        {
            tools.StartGUI();

            tools.StartEdit(manager, "Changed Glass Render Order");

            listColourIndex = 0;
            list.DoLayoutList();
            GUI.backgroundColor = Color.white;

            if (tools.Button("Clean Up (no data loss)"))
            {
                GlassRenderOrderManager.Instance.RefreshGlassTypes();
                listModified = true;
            }

            if (listModified)
            {
                manager.RenderOrderFromOrder();
                manager.SortByRenderOrder();
                manager.UpdateMaterials();
                //manager.RefreshGlassTypes();
                /*
                tools.SetDirty(manager);
                tools.SetDirty(GlassManager.Instance);
                */
                listModified = false;
            }

            tools.FinishEdit(manager);

            /*

            tools.Label("Order - Name - (Copy) - Queue - [Copy]");

            foreach (GlassType g in manager.glassTypes)
            {
                tools.Label(g.presetName + " : " + g.renderOrder);
            }

            if (tools.Button("Sort By Render Order"))
            {
                manager.SortByRenderOrder();
            }

            if (tools.Button("Render Order From Sorting Order"))
            {
                manager.RenderOrderFromOrder();
            }

            if (tools.Button("Find"))
            {
                manager.FindGlassTypes();
            }

            if (tools.Button("Refresh"))
            {
                manager.RefreshGlassTypes();
            }

            */

            tools.EndGUI();
        }
    }
}

#endif