﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
    public class UIData
    {
        protected string    m_Name;
        protected int       m_UIID;
        protected Type      m_PanelClassType;
        protected int       m_CacheCount = 0;
        private int         m_ShortCacheCount = 0;
        protected bool      m_IsSingleton = true;//默认都是单例
        protected bool      m_IsABMode = false;

        public UIData(int pID, Type pType, string pName, bool abMode)
        {
            m_UIID = pID;
            m_PanelClassType = pType;
            m_Name = pName;
            m_IsABMode = abMode;
            ProcessNameAsABMode();
        }

        public UIData(int pID, Type pType, string pName, bool singleton, int cacheCount, bool abMode)
        {
            m_UIID = pID;
            m_PanelClassType = pType;
            m_Name = pName;
            m_CacheCount = cacheCount;
            m_IsSingleton = singleton;
            m_IsABMode = abMode;
            ProcessNameAsABMode();
        }

        public string name
        {
            get { return m_Name; }
        }

        public int uiID
        {
            get { return m_UIID; }
        }

        public Type panelClassType
        {
            get { return m_PanelClassType; }
        }

        public int cacheCount
        {
            get { return m_CacheCount + m_ShortCacheCount; }
        }

        public int shortCacheCount
        {
            get { return m_ShortCacheCount; }
            set { m_ShortCacheCount = value; }
        }

        public bool isSingleton
        {
            get { return m_IsSingleton; }
        }

        public virtual string fullPath
        {
            get
            {
                if (m_IsABMode)
                {
                    return m_Name;
                }

                return string.Format(prefixPath, m_Name);
            }
        }

        protected virtual string prefixPath
        {
            get
            {
                return "";
            }
        }

        protected void ProcessNameAsABMode()
        {
            if (m_IsABMode)
            {
                m_Name = PathHelper.FullAssetPath2Name(m_Name);
            }
        }
    }

    public class PanelData : UIData
    {
        public static string PREFIX_PATH = "";

        public PanelData(int pID, Type pType, string pName, bool abMode) : base(pID, pType, pName, abMode)
        {
        }

        public PanelData(int pID, Type pType, string pName, bool singleton, int cacheCount, bool abMode) 
            : base(pID, pType, pName, singleton, cacheCount, abMode)
        {
        }

        protected override string prefixPath
        {
            get
            {
                return PREFIX_PATH;
            }
        }
    }

    public class PageData : UIData
    {
        public static string PREFIX_PATH = "";

        public PageData(int pID, Type pType, string pName, bool abMode) : base(pID, pType, pName, false, -1, abMode)
        {
        }

        protected override string prefixPath
        {
            get
            {
                return PREFIX_PATH;
            }
        }
    }

    public class UIDataTable
    {
        private static Dictionary<int, UIData> m_UIDataMap = new Dictionary<int, UIData>();
        private static Dictionary<string, UIData> m_UINameDataMap = new Dictionary<string, UIData>();

        private static bool s_IsABMode = false;

        public static void SetABMode(bool abMode)
        {
            s_IsABMode = abMode;
        }

        public static void AddPanelData<T>(T uiID, Type pType, string pName, bool singleton = true, int cacheCount = 0)
            where T : IConvertible
        {
            Add(new PanelData(uiID.ToInt32(null), pType, pName, singleton, cacheCount, s_IsABMode));
        }

        public static void AddPageData<T>(T uiID, Type pType, string pName) where T : IConvertible
        {
            Add(new PageData(uiID.ToInt32(null), pType, pName, s_IsABMode));
        }

        public static void Add(UIData data)
        {
            if (data == null)
            {
                return;
            }

            if (m_UIDataMap.ContainsKey(data.uiID))
            {
                Log.w("Already Add UIData:" + data.uiID);
                return;
            }

            m_UIDataMap.Add(data.uiID, data);

            string shortName = data.name;
            int folderIndex = shortName.LastIndexOf('/');
            if (folderIndex >= 0)
            {
                shortName = shortName.Substring(folderIndex + 1);
            }
            m_UINameDataMap.Add(shortName, data);
        }

        public static UIData Get<T>(T uiID) where T : IConvertible
        {
            UIData result = null;

            if (m_UIDataMap.TryGetValue(uiID.ToInt32(null), out result))
            {
                return result;
            }

            return null;
        }

        public static UIData Get(string name)
        {
            UIData result = null;

            if (m_UINameDataMap.TryGetValue(name, out result))
            {
                return result;
            }

            return null;
        }

        public static int PanelName2UIID(string name)
        {
            UIData data = Get(name);
            if (data != null)
            {
                return data.uiID;
            }

            return -1;
        }

    }
}
