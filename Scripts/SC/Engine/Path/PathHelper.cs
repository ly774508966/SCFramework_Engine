using System;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SCFramework
{
    public class PathHelper
    {
        // 内部目录
        private static string m_StreamingAssetsPath;
        // 外部目录
        private static string m_PersistentDataPath;
        // 外部资源目录
        private static string m_PersistentDataPath4Res;
        // 外部缓存的头像目录
        private static string m_PersistentDataPath4Photo;

        public static string FileNameWithoutSuffix(string name)
        {
            if (name == null)
            {
                return null;
            }

            int endIndex = name.LastIndexOf('.');
            if (endIndex > 0)
            {
                return name.Substring(0, endIndex);
            }
            return name;
        }

        public static string FullAssetPath2Name(string fullPath)
        {
            string name = FileNameWithoutSuffix(fullPath);
            if (name == null)
            {
                return null;
            }

            int endIndex = name.LastIndexOf('/');
            if (endIndex > 0)
            {
                return name.Substring(endIndex + 1);
            }
            return name;
        }

        // 外部资源目录
        public static string persistentDataPath4Res
        {
            get
            {
                if (null == m_PersistentDataPath4Res)
                {
                    m_PersistentDataPath4Res = persistentDataPath + "Res\\";

                    if (!Directory.Exists(m_PersistentDataPath4Res))
                    {
                        Directory.CreateDirectory(m_PersistentDataPath4Res);
                    }
                }

                return m_PersistentDataPath4Res;
            }
        }

        // 外部头像缓存目录
        public static string persistentDataPath4Photo
        {
            get
            {
                if (null == m_PersistentDataPath4Photo)
                {
                    m_PersistentDataPath4Photo = persistentDataPath + "Photos\\";

                    if (!Directory.Exists(m_PersistentDataPath4Photo))
                    {
                        Directory.CreateDirectory(m_PersistentDataPath4Photo);
                    }
                }

                return m_PersistentDataPath4Photo;
            }
        }

        // 外部目录
        public static string persistentDataPath
        {
            get
            {
                if (null == m_PersistentDataPath)
                {

#if UNITY_IPHONE && !UNITY_EDITOR
                m_PersistentDataPath = Application.persistentDataPath + "/";
#elif UNITY_ANDROID && !UNITY_EDITOR
                // UIElementDatabase.assetbundle所在目录就是读写目录
                string uiPath = string.Format("{0}{1}", Application.persistentDataPath, "/Res/UI/UIElementDatabase.assetbundle");
                Log.i("persistentDataPath uiPath=" + uiPath + " Application.persistentDataPath = " + Application.persistentDataPath);

                if (File.Exists(uiPath))
                {
                    m_PersistentDataPath = Application.persistentDataPath + "/";
                }
                else
                {
                    // /data/data or /data/user/0 ..为读写目录
                    string innerStoragePath = "";//AndroidSDKHelper.GetInnerStoragePath();
                    Log.i("innerStoragePath = " + innerStoragePath);
                    if (!string.IsNullOrEmpty(innerStoragePath))
                    {
                        string fullPath = Application.persistentDataPath + "/";
                        string uiPathOnDataData = string.Format("{0}{1}", innerStoragePath, "Res/UI/UIElementDatabase.assetbundle");
                        if (File.Exists(uiPathOnDataData))
                        {
                            m_PersistentDataPath = innerStoragePath;
                        }
                        else if (fullPath != innerStoragePath)
                        {
                            m_PersistentDataPath = Application.persistentDataPath + "/";
                        }
                        else
                        {
                            m_PersistentDataPath = innerStoragePath;
                        }
                    }
                    else
                    {
                        m_PersistentDataPath = Application.persistentDataPath + "/";
                    }
                }
#elif (UNITY_STANDALONE_WIN) && !UNITY_EDITOR
                m_PersistentDataPath = Application.persistentDataPath + "/";
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
                m_PersistentDataPath = Application.persistentDataPath + "/";
#else
                    m_PersistentDataPath = Application.persistentDataPath + "/";
#endif
                    // 缓存第一次获取到的读写目录
                    //PlayerPrefs.SetString(PlayerPrefsKey.persistentDataPath, m_PersistentDataPath);

                    m_PersistentDataPath = m_PersistentDataPath.Replace('/', '\\');
                }

                return m_PersistentDataPath;
            }
        }
    }
}
