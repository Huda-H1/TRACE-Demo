#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace FantasticGlass
{
    public class GlassMaterialInspector : ShaderGUI
    {

        Material material;

        EditorTools tools;

        bool showAdvancedSettings = false;

        override public void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            material = materialEditor.target as Material;

            if (tools == null)
            {
                tools = new EditorTools("GlassMaterialEditor" + material.GetInstanceID());
            }

            tools.Label("It is recommended that you edit Glass objects in their Glass inspector panel rather than directly within the Materials they use.", true);

            tools.Divider();

            tools.Label("The settings below are not currently documented and editing them may result in adverse effects.", true);

            tools.Divider();

            if (tools.ShowSection("I Understand - Show The Settings", ref showAdvancedSettings))
            {
                base.OnGUI(materialEditor, properties);

                tools.EndSection();
            }
        }
    }

}

#endif
