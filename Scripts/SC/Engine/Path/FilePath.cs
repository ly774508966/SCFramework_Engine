using UnityEngine;
using System.IO;
using System.Collections;

namespace SCFramework
{
    public class FilePath
    {
        static string m_PersistentDataPath;
        static string m_StreamingAssetsPath;
        static string m_PersistentDataPath4Res;

        // 外部目录  
        public static string persistentDataPath
        {
            get
            {
                if (null == m_PersistentDataPath)
                {
                    m_PersistentDataPath = Application.persistentDataPath + "/";
                }

                return m_PersistentDataPath;
            }
        }

        // 内部目录
        public static string streamingAssetsPath
        {
            get
            {
                if (null == m_StreamingAssetsPath)
                {
#if UNITY_IPHONE && !UNITY_EDITOR
                    m_StreamingAssetsPath = Application.streamingAssetsPath + "/";
#elif UNITY_ANDROID && !UNITY_EDITOR
                    m_StreamingAssetsPath = Application.streamingAssetsPath + "/";
#elif (UNITY_STANDALONE_WIN) && !UNITY_EDITOR
                    m_StreamingAssetsPath = GetParentDir(Application.dataPath, 2) + "/BuildRes/";
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
                    m_StreamingAssetsPath = Application.streamingAssetsPath + "/";
#else
                    //m_StreamingAssetsPath = GetParentDir(Application.dataPath, 1) + "/BuildRes/standalone/";
                    m_StreamingAssetsPath = Application.streamingAssetsPath + "/";
#endif
                }

                return m_StreamingAssetsPath;
            }
        }

        // 外部资源目录
        public static string persistentDataPath4Res
        {
            get
            {
                if (null == m_PersistentDataPath4Res)
                {
                    m_PersistentDataPath4Res = persistentDataPath + "Res/";

                    if (!Directory.Exists(m_PersistentDataPath4Res))
                    {
                        Directory.CreateDirectory(m_PersistentDataPath4Res);
#if UNITY_IPHONE && !UNITY_EDITOR
                        UnityEngine.iOS.Device.SetNoBackupFlag(m_PersistentDataPath4Res);
#endif
                    }
                }

                return m_PersistentDataPath4Res;
            }
        }

        // 资源路径，优先返回外存资源路径
        public static string GetResPathInPersistentOrStream(string relativePath)
        {
            string resPersistentPath = string.Format("{0}{1}", FilePath.persistentDataPath4Res, relativePath);

            if (File.Exists(resPersistentPath))
            {
                return resPersistentPath;
            }
            else
            {
                return FilePath.streamingAssetsPath + relativePath;
            }
        }

        // 上一级目录
        public static string GetParentDir(string dir, int floor = 1)
        {
            string subDir = dir;

            for (int i = 0; i < floor; ++i)
            {
                int last = subDir.LastIndexOf('/');
                subDir = subDir.Substring(0, last);
            }

            return subDir;
        }

    }
}
