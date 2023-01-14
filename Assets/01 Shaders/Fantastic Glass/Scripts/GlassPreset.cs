using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace FantasticGlass
{
    [System.Serializable]
    [XmlRoot("GlassPreset")]
    public class GlassPreset
    {
        [XmlAttribute()]
        public string name = "";

        [XmlAttribute()]
        public string meshPath = "";
        [XmlAttribute()]
        public GlassPrimitiveType meshPrimitive = GlassPrimitiveType.none;
        //
        [XmlAttribute()]
        public bool createMaterials;
        [XmlAttribute()]
        public string frontMatPath = "";
        [XmlAttribute()]
        public string backMatPath = "";
        //
        [XmlAttribute()]
        public float opacity = 0f;
        [XmlAttribute()]
        public float[] colour_albedo;
        [XmlAttribute()]
        public string texturePath_albedo = "";
        [XmlAttribute()]
        public float[] colour_albedoTexture;
        //
        [XmlAttribute()]
        public bool enableExtinction_front = true;
        [XmlAttribute()]
        public bool enableExtinction_back = false;
        [XmlAttribute()]
        public bool enableExtinction_both = false;
        [XmlAttribute()]
        public bool capExtinction_front = false;
        [XmlAttribute()]
        public bool capExtinction_back = false;
        [XmlAttribute()]
        public string texturePath_extinction_front = "";
        [XmlAttribute()]
        public string texturePath_extinction_back = "";
        [XmlAttribute()]
        public int extinctionAppearance = 1;
        [XmlAttribute()]
        public float[] colour_extinction_front;
        [XmlAttribute()]
        public float[] colour_extinction_back;
        [XmlAttribute()]
        public float[] colour_extinction_flipped_front;
        [XmlAttribute()]
        public float[] colour_extinction_flipped_back;
        [XmlAttribute()]
        public float extinctionIntensity_front = 1f;
        [XmlAttribute()]
        public float extinctionIntensity_back = 1f;
        [XmlAttribute()]
        public float extinctionMin_front = 0f;
        [XmlAttribute()]
        public float extinctionMin_back = 0f;
        [XmlAttribute()]
        public float extinctionMax_front = 2f;
        [XmlAttribute()]
        public float extinctionMax_back = 2f;
        //
        [XmlAttribute()]
        public bool enableAberration_front = true;
        [XmlAttribute()]
        public bool enableAberration_back = false;
        [XmlAttribute()]
        public bool enableAberration_both = false;
        [XmlAttribute()]
        public string texturePath_aberration_front = "";
        [XmlAttribute()]
        public string texturePath_aberration_back = "";
        [XmlAttribute()]
        public float[] colour_aberration_front;
        [XmlAttribute()]
        public float[] colour_aberration_back;
        [XmlAttribute()]
        public float aberrationIntensity_front = 0.01f;
        [XmlAttribute()]
        public float aberrationIntensity_back = 0.01f;
        [XmlAttribute()]
        public float aberrationMin_front = 0.0f;
        [XmlAttribute()]
        public float aberrationMin_back = 0.0f;
        [XmlAttribute()]
        public float aberrationMax_front = 0.1f;
        [XmlAttribute()]
        public float aberrationMax_back = 0.1f;
        [XmlAttribute()]
        public bool capAberration_front = true;
        [XmlAttribute()]
        public bool capAberration_back = true;
        //
        [XmlAttribute()]
        public bool option_fog_front = false;
        [XmlAttribute()]
        public bool option_fog_back = false;
        [XmlAttribute()]
        public bool option_fog_both = false;
        [XmlAttribute()]
        public float[] colour_fog_near_front;
        [XmlAttribute()]
        public float[] colour_fog_near_back;
        [XmlAttribute()]
        public float[] colour_fog_far_front;
        [XmlAttribute()]
        public float[] colour_fog_far_back;
        [XmlAttribute()]
        public float fog_magnitude_front = 1f;
        [XmlAttribute()]
        public float fog_magnitude_back = 1f;
        //
        [XmlAttribute()]
        public float glossiness_front = 0.5f;
        [XmlAttribute()]
        public float glossiness_back = 0.5f;
        [XmlAttribute()]
        public string texturePath_gloss_front = "";
        [XmlAttribute()]
        public string texturePath_gloss_back = "";
        [XmlAttribute()]
        public float glow_front = 0f;
        [XmlAttribute()]
        public float glow_back = 0f;
        [XmlAttribute()]
        public string texturePath_glow_front = "";
        [XmlAttribute()]
        public string texturePath_glow_back = "";
        [XmlAttribute()]
        public float metallic_front = 0.5f;
        [XmlAttribute()]
        public float metallic_back = 0.5f;
        [XmlAttribute()]
        public string texturePath_metal_front = "";
        [XmlAttribute()]
        public string texturePath_metal_back = "";
        //
        [XmlAttribute()]
        public float[] objectRotation;
        [XmlAttribute()]
        public float[] objectPosition;
        //
        [XmlAttribute()]
        public bool physicalObject = false;
        [XmlAttribute()]
        public GlassPhysicalObjectType physicalObjectType = GlassPhysicalObjectType.box;
        [XmlAttribute()]
        public float zFightingRadius = 0.01f;
        //
        [XmlAttribute()]
        public float meshScale = 1f;
        [XmlAttribute()]
        public GlassMeshScaleFix meshScaleFix;
        //
        [XmlAttribute()]
        public bool enableDistortion_front = true;
        [XmlAttribute()]
        public bool enableDistortion_back = true;
        [XmlAttribute()]
        public string texturePath_distortion = "";
        [XmlAttribute()]
        public float distortion_back_bump = 0f;
        [XmlAttribute()]
        public float distortion_front_bump = 0f;
        [XmlAttribute()]
        public float distortion_back_mesh = 0.1f;
        [XmlAttribute()]
        public float distortion_front_mesh = -0.01f;
        [XmlAttribute()]
        public float distortion_back_multiplier = 1f;
        [XmlAttribute()]
        public float distortion_front_multiplier = 1f;
        [XmlAttribute()]
        public float distortion_back_max = 1f;
        [XmlAttribute()]
        public float distortion_front_max = 1f;
        [XmlAttribute()]
        public float distortion_edge_bend_front = 0.3f;
        [XmlAttribute()]
        public float distortion_edge_bend_back = 0.3f;
        [XmlAttribute()]
        public float distortion_depth_fade_front = 0.1f;
        [XmlAttribute()]
        public float distortion_depth_fade_back = 0.1f;
        [XmlAttribute()]
        public bool enableDoubleDepth_front = true;
        [XmlAttribute()]
        public bool enableDoubleDepth_back = true;
        //
        [XmlAttribute()]
        public float bump_front = 0f;
        [XmlAttribute()]
        public float bump_back = 0f;
        //
        [XmlAttribute()]
        public float depthMultiplier = 1f;
        [XmlAttribute()]
        public float depthOffset = 0f;
        [XmlAttribute()]
        public DepthQuality_GlassObject depthQuality_back = Glass.default_depthQuality;
        [XmlAttribute()]
        public DepthQuality_GlassObject depthQuality_front = Glass.default_depthQuality;
        [XmlAttribute()]
        public DepthQuality_GlassObject depthQuality_other = Glass.default_depthQuality;


        public GlassPreset()
        {
            colour_albedo = new float[4];
            colour_albedoTexture = new float[4];
            //
            colour_extinction_front = new float[4];
            colour_extinction_flipped_front = new float[4];
            colour_extinction_back = new float[4];
            colour_extinction_flipped_back = new float[4];
            //
            colour_aberration_front = new float[4];
            colour_aberration_back = new float[4];
            //
            colour_fog_near_front = new float[4];
            colour_fog_near_back = new float[4];
            colour_fog_far_front = new float[4];
            colour_fog_far_back = new float[4];
            //
            objectRotation = new float[3];
            objectPosition = new float[3];
        }

        public static string PresetPath(string presetName)
        {
            return Glass.GetXMLPath() + presetName + ".xml";
        }

        public static GlassPreset LoadFromName(string presetName, bool showDebug = true, bool secondAttempt = false)
        {
            return LoadFromXML(PresetPath(presetName), showDebug, secondAttempt);
        }

        public static GlassPreset LoadFromXML(string path, bool showDebug = true, bool secondAttempt = false)
        {
            if (path.Contains("/.xml") || path.Contains("GlassManagerSettings"))
            {
                if (showDebug)
                    Debug.Log("GlassPreset: Skipping file:" + path);
                return null;
            }

            if (!File.Exists(path))
            {
                if (showDebug)
                    Debug.Log("GlassPreset: File does not exists:" + path);
                return null;
            }

            XmlSerializer xmlserialiser = new XmlSerializer(typeof(GlassPreset));

            FileStream filestream = new FileStream(path, FileMode.Open);

            GlassPreset loadedPreset = null;

            try
            {
                loadedPreset = xmlserialiser.Deserialize(filestream) as GlassPreset;
            }
            catch (Exception e)
            {
                if (secondAttempt)
                {
                    if (showDebug)
                    {
                        Debug.Log("GlassPreset: Unable to fix & load preset from XML '" + path + "': " + e.Message);
                        filestream.Close();
                        return loadedPreset;
                    }
                }
                else
                {
                    filestream.Close();
                    AttemptFix(path, showDebug);
                    return LoadFromXML(path, showDebug, true);
                }
                loadedPreset = null;
            }

            if (showDebug && secondAttempt)
            {
                Debug.Log("GlassPreset: Successfully fixed / converted preset xml: '" + path + "!");
            }

            filestream.Close();
            return loadedPreset;
        }
        
        public static bool PlatformSupportsFixer()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                    return true;
                default:
                    return false;
            }
        }

        private static void AttemptFix(string filePath, bool showDebug = true)
        {
            if (showDebug)
            {
                Debug.Log("GlassPreset: Attempting to fix preset in XML: '" + filePath + "'.");
            }

#if !UNITY_WEBPLAYER && !UNITY_SAMSUNGTV
            
            if(!PlatformSupportsFixer())
            {
                if (showDebug)
                {
                    Debug.Log("Skipping fix - not supported by current platform.");
                }
                return;
            }

            string fileContents = null;

            try
            {
                fileContents = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                if (showDebug)
                    Debug.Log("GlassPreset: Unable to load preset from XML '" + filePath + "' for fixing: " + e.Message);
            }

            if (fileContents == null)
            {
                return;
            }

            ReplaceNaming(ref fileContents, "meshPrimitive", "None", "meshPrimitive", "none", showDebug);
            ReplaceNaming(ref fileContents, "meshPrimitive", "Cube", "meshPrimitive", "cube", showDebug);
            ReplaceNaming(ref fileContents, "meshPrimitive", "Sphere", "meshPrimitive", "sphere", showDebug);
            ReplaceNaming(ref fileContents, "meshPrimitive", "Capsule", "meshPrimitive", "capsule", showDebug);
            ReplaceNaming(ref fileContents, "meshPrimitive", "Cylinder", "meshPrimitive", "cylinder", showDebug);
            ReplaceNaming(ref fileContents, "meshPrimitive", "Quad", "meshPrimitive", "quad", showDebug);
            ReplaceNaming(ref fileContents, "meshPrimitive", "Plane", "meshPrimitive", "plane", showDebug);

            ReplaceNaming(ref fileContents, "physicalObjectType", "BoxCollider", "physicalObjectType", "box", showDebug);
            ReplaceNaming(ref fileContents, "physicalObjectType", "SphereCollider", "physicalObjectType", "sphere", showDebug);
            ReplaceNaming(ref fileContents, "physicalObjectType", "MeshCollider", "physicalObjectType", "mesh", showDebug);
            ReplaceNaming(ref fileContents, "physicalObjectType", "MeshCollider Convex", "physicalObjectType", "convexMesh", showDebug);

            File.WriteAllText(filePath, fileContents);
#else
            if (showDebug)
                {
                    Debug.Log("Skipping fix - not in supported platform.");
                }
                return;
#endif
        }

        private static void ReplaceNaming(ref string contents, string nameIn, string valueIn, string nameOut, string valueOut, bool showDebug = true)
        {
            if (contents.Contains(valueIn))
            {
                string stringIn = nameIn + "=\"" + valueIn + "\"";
                string stringOut = nameOut + "=\"" + valueOut + "\"";
                if (showDebug)
                {
                    Debug.Log("GlassPreset: Replacing string'" + stringIn + "' with string '" + stringOut + "'.");
                }
                contents = contents.Replace(stringIn, stringOut);
            }
        }

        public void Save()
        {
            Save(PresetPath(name));
        }

        public void Save(string path)
        {
            XmlSerializer xmlserialiser = new XmlSerializer(typeof(GlassPreset));
            FileStream fileStream = new FileStream(path, FileMode.Create);
            xmlserialiser.Serialize(fileStream, this);
            fileStream.Close();
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static void SetColour(ref Color colourColor, float[] colourFloat)
        {
            if (colourFloat == null)
                colourFloat = new float[4];
            if (colourFloat.Length >= 1)
                colourColor.r = colourFloat[0];
            if (colourFloat.Length >= 2)
                colourColor.g = colourFloat[1];
            if (colourFloat.Length >= 3)
                colourColor.b = colourFloat[2];
            if (colourFloat.Length >= 4)
                colourColor.a = colourFloat[3];
        }

        public static void SetColour(ref float[] colourFloat, Color colourColor)
        {
            if (colourFloat == null)
                colourFloat = new float[4];
            colourFloat[0] = colourColor.r;
            colourFloat[1] = colourColor.g;
            colourFloat[2] = colourColor.b;
            colourFloat[3] = colourColor.a;
        }

        public static void SetVector(ref Vector3 positionVector, float[] positionArray)
        {
            positionVector.x = positionArray[0];
            positionVector.y = positionArray[1];
            positionVector.z = positionArray[2];
        }

        public static void SetVector(ref float[] positionArray, Vector3 positionVector)
        {
            positionArray[0] = positionVector.x;
            positionArray[1] = positionVector.y;
            positionArray[2] = positionVector.z;
        }
    }

}