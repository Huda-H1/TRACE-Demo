#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FantasticGlass
{
    #region File Auto Renamer

    public class Glass_FileAutoRenamer
    {
        public string latestVersionStringFind = "_" + Glass.versionString;
        public string latestVersionStringReplace = "";
        //
        public List<string> previousVersionStringFindList = new List<string>();
        public string previousVersionStringFind = "";
        public List<string> previousVersionStringReplaceList = new List<string>();
        public string previousVersionStringReplace = "";
        //
        public List<string> latestVersionFiles_oldSuffix = new List<string>();
        public List<string> latestVersionFiles_withoutSuffix = new List<string>();
        public List<string> previousVersionFiles_oldSuffix = new List<string>();
        public List<string> previousVersionFiles_withoutSuffix = new List<string>();
        //
        public Dictionary<string, bool> folders = new Dictionary<string, bool>();
        public string packagePath = "";
        public static string metaString = ".meta";
        //
        public bool backupOldFiles = default_backupOldFiles;
        public bool findLatest = default_findLatest;
        public bool findPrevious = default_findPrevious;
        public bool savePrevious = default_savePrevious;
        public bool saveLatest = default_saveLatest;
        public bool deletePrevious = default_deletePrevious;
        public bool deleteLatest = default_deleteLatest;
        public bool ignoreMetaFiles = default_ignoreMetaFiles;
        public bool printDebug = default_printDebug;
        //
        public static bool default_backupOldFiles = false;
        public static bool default_findLatest = true;
        public static bool default_findPrevious = true;
        public static bool default_savePrevious = true;
        public static bool default_saveLatest = true;
        public static bool default_deletePrevious = true;
        public static bool default_deleteLatest = true;
        public static bool default_ignoreMetaFiles = false;
        public static bool default_printDebug = true;

        public Glass_FileAutoRenamer()
        {
            Init();
        }

        public void Init()
        {
            InitVersionStrings();
            InitPackagePath();
            InitFolderPaths();
        }

        void InitVersionStrings()
        {
            InitVersionStrings_Old();
        }

        void InitVersionStrings_Old()
        {
            InitVersionString_Old_Find();
            InitVersionString_Old_Replace();
        }

        void InitVersionString_Old_Find()
        {
            previousVersionStringFindList.Clear();
            //	TODO: Update this list for every release
            previousVersionStringFindList.Add("");
            previousVersionStringFindList.Add("_1_0_0");
            previousVersionStringFindList.Add("_1_1_0");
            previousVersionStringFindList.Add("_1_1_1");
        }

        void InitVersionString_Old_Replace()
        {
            previousVersionStringReplaceList.Clear();
            //
            foreach (string versionFindString in previousVersionStringFindList)
            {
                if (versionFindString == "")    //	we want everything but the blank suffix of the Find List
                {
                    previousVersionStringReplaceList.Add("_1_0_0");
                }
                else
                {
                    previousVersionStringReplaceList.Add(versionFindString);
                }
            }
        }

        public void SetDefaultOptions()
        {
            backupOldFiles = default_backupOldFiles;
            findLatest = default_findLatest;
            findPrevious = default_findPrevious;
            savePrevious = default_savePrevious;
            saveLatest = default_saveLatest;
            deletePrevious = default_deletePrevious;
            deleteLatest = default_deleteLatest;
            ignoreMetaFiles = default_ignoreMetaFiles;
            printDebug = default_printDebug;
        }

        public void ClearData()
        {
            latestVersionFiles_oldSuffix.Clear();
            latestVersionFiles_withoutSuffix.Clear();
            previousVersionFiles_oldSuffix.Clear();
            previousVersionFiles_withoutSuffix.Clear();
        }

        public bool PlatformSupportsUpdater()
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

        public void FindAndRenameAllFiles()
        {
#if UNITY_WEBGL || UNITY_WEBPLAYER
            Debug.Log("Fantastic Glass :: Unable to run update utility on this platform: '" + Application.platform.ToString() + "'.");
            return;
#else

            if (!PlatformSupportsUpdater())
            {
                Debug.Log("Fantastic Glass :: Unable to run update utility on this platform: '" + Application.platform.ToString() + "'.");
                return;
            }
            else
            {
                Debug.Log("Fantastic Glass :: Running update utility on current platform: '" + Application.platform.ToString() + "'.");
            }

            for (int i = 0; i < previousVersionStringFindList.Count; i++)
            {
                previousVersionStringFind = previousVersionStringFindList[i];
                previousVersionStringReplace = previousVersionStringReplaceList[i];

                ClearData();

                if (findLatest)
                {
                    Find_LatestVersion();
                }
                StripSuffix_LatestVersion();

                if (findPrevious)
                {
                    Find_PreviousVersion();
                }

                if (savePrevious)
                {
                    Save_PreviousVersion_NewSuffix();
                }
                if (deletePrevious)
                {
                    Delete_PreviousVersion_OldSuffix();
                }

                if (saveLatest)
                {
                    Save_LatestVersion_NewSuffix();
                }
                if (deleteLatest)
                {
                    Delete_LatestVersion_OldSuffix();
                }
            }
#endif
        }

        public void Find_LatestVersion()
        {
            foreach (string folderPath in folders.Keys)
            {
                if (folders[folderPath])
                {
                    Find_LatestVersion(folderPath);
                }
            }
        }

        public void Find_LatestVersion(string path)
        {
#if UNITY_WEBGL || UNITY_WEBPLAYER
            Debug.Log("Fantastic Glass :: Unable to run update utility on this platform: '" + Application.platform.ToString() + "'.");
            return;
#else
            DirectoryInfo pathInfo = new DirectoryInfo(path);

            foreach (FileInfo fileInfo in pathInfo.GetFiles())
            {
                if (fileInfo.Name.Contains(latestVersionStringFind))
                {
                    if (ignoreMetaFiles)
                    {
                        if (fileInfo.FullName.Contains(".meta"))
                        {
                            continue;
                        }
                    }
                    latestVersionFiles_oldSuffix.Add(fileInfo.FullName);
                    if (printDebug)
                    {
                        Debug.Log("Found(Latest Version):" + fileInfo.FullName);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Strips the suffix from files included in the latest version. This will allow us to look for matching older files.
        /// </summary>
        public void StripSuffix_LatestVersion()
        {
            foreach (string filePath in latestVersionFiles_oldSuffix)
            {
                latestVersionFiles_withoutSuffix.Add(ReplaceSuffix(filePath, latestVersionStringFind, ""));
            }
        }

        public void StripSuffix_PreviousVersion()
        {
            if (previousVersionStringFind.Length > 0)
            {
                foreach (string filePath in previousVersionFiles_oldSuffix)
                {
                    previousVersionFiles_withoutSuffix.Add(ReplaceSuffix(filePath, previousVersionStringFind, ""));
                }
            }
            else {
                foreach (string filePath in previousVersionFiles_oldSuffix)
                {
                    previousVersionFiles_withoutSuffix.Add(filePath);
                }
            }
        }

        public void Find_PreviousVersion()
        {
            foreach (string latestPath in latestVersionFiles_oldSuffix)
            {
                string previousPath = ReplaceSuffix(latestPath, latestVersionStringFind, previousVersionStringFind);
                if (File.Exists(previousPath))
                {
                    previousVersionFiles_oldSuffix.Add(previousPath);
                    if (printDebug)
                        Debug.Log("Found(Previous Version):" + previousPath);
                }
            }
        }

        void Save_PreviousVersion_NewSuffix()
        {
            foreach (string filePath in previousVersionFiles_oldSuffix)
            {
                string newFilePath = ReplaceSuffix(filePath, previousVersionStringFind, previousVersionStringReplace);
                File.Copy(filePath, newFilePath);
                if (printDebug)
                {
                    Debug.Log("Saved (Previous Version):" + newFilePath);
                }
            }
        }

        void Delete_PreviousVersion_OldSuffix()
        {
            foreach (string filePath in previousVersionFiles_oldSuffix)
            {
                File.Delete(filePath);
                if (printDebug)
                {
                    Debug.Log("Deleted (Previous Version):" + filePath);
                }
            }
        }

        void Save_LatestVersion_NewSuffix()
        {
            foreach (string filePath in latestVersionFiles_oldSuffix)
            {
                string newFilePath = ReplaceSuffix(filePath, latestVersionStringFind, latestVersionStringReplace);
                File.Copy(filePath, newFilePath);
                if (printDebug)
                {
                    Debug.Log("Saved (Previous Version):" + newFilePath);
                }
            }
        }

        void Delete_LatestVersion_OldSuffix()
        {
            foreach (string filePath in latestVersionFiles_oldSuffix)
            {
                File.Delete(filePath);
                if (printDebug)
                {
                    Debug.Log("Deleted (Previous Version):" + filePath);
                }
            }
        }

        void InitPackagePath()
        {
            if (!packagePath.Contains(Application.dataPath))
            {
                packagePath = Application.dataPath + "/" + GlassManager.default_packageName + "/";
            }
        }

        void InitFolderPaths()
        {
            //	relative paths
            folders.Add(FolderPath("Materials", true), true);
            folders.Add(FolderPath("Textures", true), true);
            folders.Add(FolderPath("Models", true), true);
            folders.Add(FolderPath("Prefabs", true), true);
            folders.Add(FolderPath("Scenes", true), true);
            folders.Add(FolderPath("Animations", true), true);
            folders.Add(FolderPath("Physics Materials", true), true);
            folders.Add(FolderPath("GUI/Resources", true), true);
            folders.Add(FolderPath("Shaders/Resources", true), true);
            //	absolute paths
            folders.Add(FolderPath("XML", false), true);
            folders.Add(FolderPath("Documentation", false), true);
        }

        public string FolderPath(string path, bool getRelative)
        {
            string folderPath = packagePath + "/" + path + "/";
            if (getRelative)
            {
                folderPath = FileUtil.GetProjectRelativePath(folderPath);
            }
            return folderPath;
        }

        public static string ReplaceSuffix(string filename, string latestSuffix, string newSuffix)
        {
            string result = filename;

            string latestSuffixFormatted = latestSuffix;
            string newSuffixFormatted = newSuffix;

            if (filename.Contains(metaString))
                result.Remove(result.IndexOf(metaString), metaString.Length);

            if (!latestSuffixFormatted.Contains("."))
                latestSuffixFormatted += ".";

            if (!newSuffixFormatted.Contains("."))
                newSuffixFormatted += ".";

            int index = filename.LastIndexOf(latestSuffixFormatted);

            if (index == -1)
                return filename;

            result = result.Remove(index, latestSuffixFormatted.Length).Insert(index, newSuffixFormatted);

            if (filename.Contains(metaString))
                result += metaString;

            return result;
        }

    }

#endregion

}

#endif
