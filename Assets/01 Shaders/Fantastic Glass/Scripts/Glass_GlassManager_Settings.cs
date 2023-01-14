using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System;

namespace FantasticGlass
{
    [System.Serializable]
    [XmlRoot("Glass_GlassManager_Settings")]
    public class Glass_GlassManager_Settings
    {
        //  Depth & Normal Techniques
        [XmlAttribute()]
        public string glassPresetName = "";
        [XmlAttribute()]
        public GlassDepthTechnique depthTechnique = GlassDepthCamera.default_depthTechnique;
        [XmlAttribute()]
        public GlassNormalTechnique normalTechnique = GlassDepthCamera.default_normalTechnique;
        [XmlAttribute()]
        public GlassFrontDepthTechnique frontDepthTechnique = GlassDepthCamera.default_frontDepthTechnique;
        [XmlAttribute()]
        public bool enable54Workaround = false;
        //  Depth Camera Settings
        [XmlAttribute()]
        public int depthTextureAA = GlassManager.default_depthTextureAA;
        [XmlAttribute()]
        public int depthTextureAniso = GlassManager.default_depthTextureAniso;
        [XmlAttribute()]
        public FilterMode depthTextureFilterMode = GlassManager.default_depthTextureFilterMode;
        [XmlAttribute()]
        public CameraClearFlags depthTextureClearMode = GlassManager.default_depthTextureClearMode;
        //  Depth Quality Settings
        [XmlAttribute()]
        public DepthQuality_GlassManager depthQuality_Front = GlassManager.default_depthQuality;
        [XmlAttribute()]
        public DepthQuality_GlassManager depthQuality_Back = GlassManager.default_depthQuality;
        [XmlAttribute()]
        public DepthQuality_GlassManager depthQuality_Other = GlassManager.default_depthQuality;
        //
        [XmlAttribute()]
        public int lastEdited = 0;

        public Glass_GlassManager_Settings()
        {

        }

        public void Edited()
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            lastEdited = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
        }

        public bool Newer(Glass_GlassManager_Settings other)
        {
            return lastEdited > other.lastEdited;
        }

        public Glass_GlassManager_Settings Newest(Glass_GlassManager_Settings left, Glass_GlassManager_Settings right)
        {
            if (left.Newer(right))
                return left;
            return right;
        }

        public override bool Equals(object o)
        {
            if (base.Equals(o))
                return true;

            if (o == null)
            {
                return (this == null);
            }

            Glass_GlassManager_Settings otherSettings = (Glass_GlassManager_Settings)o;

            if (otherSettings.glassPresetName == glassPresetName)
            {
                //  currently matching based on preset name as they should load from the same file
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static Glass_GlassManager_Settings Load(string xmlPath, Glass glassObject, bool showDebug = false)
        {
            if (glassObject == null)
                return null;
            return Load(xmlPath, glassObject.presetName, showDebug);
        }

        public static Glass_GlassManager_Settings Load(string xmlPath, GlassPreset glassPreset, bool showDebug = false)
        {
            return Load(xmlPath, glassPreset.name, showDebug);
        }

        public static Glass_GlassManager_Settings Load(string xmlPath, string glassPresetName, bool showDebug = false)
        {
            return LoadFromXML(Filename(xmlPath, glassPresetName), showDebug);
        }

        public static string Filename(string xmlPath, string glassPresetName)
        {
            return xmlPath + "/" + glassPresetName + "_GlassManagerSettings.xml";
        }

        public string Filename(string xmlPath)
        {
            return xmlPath + "/" + glassPresetName + "_GlassManagerSettings.xml";
        }

        public void Save(string xmlPath)
        {
            string filePath = Filename(xmlPath);
            XmlSerializer xmlserialiser = null;
            try
            {
                xmlserialiser = new XmlSerializer(typeof(Glass_GlassManager_Settings));
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to create serializer for Glass_GlassManager_Settings. Error Message: '" + e.Message + "'. Inner Exception: '" + e.InnerException.Message + "'.");
                return;
            }
            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            xmlserialiser.Serialize(fileStream, this);
            fileStream.Close();
        }

        public static Glass_GlassManager_Settings LoadFromXML(string path, bool showDebug = true)
        {
            if (!File.Exists(path))
            {
                if (showDebug)
                    Debug.Log("Glass_GlassManager_Settings: File does not exists:" + path);
                return null;
            }
            XmlSerializer xmlserialiser = new XmlSerializer(typeof(Glass_GlassManager_Settings));
            FileStream filestream = new FileStream(path, FileMode.Open);

            Glass_GlassManager_Settings loadedSettings = null;

            try
            {
                loadedSettings = xmlserialiser.Deserialize(filestream) as Glass_GlassManager_Settings;
            }
            catch (Exception e)
            {
                if (showDebug)
                    Debug.Log("Glass_GlassManager_Settings: Unable to load GlassManager settings from XML '" + path + "': " + e.Message);
                loadedSettings = null;
            }

            filestream.Close();
            return loadedSettings;
        }

        public void UpdateDepthTechnique()
        {
            normalTechnique = GlassDepthCamera.NormalTechFromDepth(normalTechnique, depthTechnique);
            frontDepthTechnique = GlassDepthCamera.FrontDepthTechFromDepth(frontDepthTechnique, depthTechnique, enable54Workaround);
        }

        public void UpdateNormalTechnique()
        {
            depthTechnique = GlassDepthCamera.DepthTechFromNormal(depthTechnique, normalTechnique);
            frontDepthTechnique = GlassDepthCamera.FrontDepthTechFromNormal(frontDepthTechnique, normalTechnique, enable54Workaround);
        }

        public void UpdateFrontDepthTechnique()
        {
            GlassDepthCamera.DepthNormalTechFromFrontDepth(frontDepthTechnique, ref depthTechnique, ref normalTechnique, enable54Workaround);
        }

        public void CopyDepthNormalTech(GlassManager manager)
        {
            depthTechnique = manager.depthTechnique;
            normalTechnique = manager.normalTechnique;
            frontDepthTechnique = manager.frontDepthTechnique;
            enable54Workaround = manager.enable54Workaround;
        }

        public void CopyQuality(GlassManager manager)
        {
            depthTextureAA = manager.depthTextureAA;
            depthTextureAniso = manager.depthTextureAniso;
            depthTextureFilterMode = manager.depthTextureFilterMode;
            depthTextureClearMode = manager.depthTextureClearMode;
            //
            depthQuality_Front = manager.depthQuality_Front;
            depthQuality_Back = manager.depthQuality_Back;
            depthQuality_Other = manager.depthQuality_Other;
        }
    }
}
