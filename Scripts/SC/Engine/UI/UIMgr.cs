﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace SCFramework
{
    [TMonoSingletonAttribute("[Tools]/UIMgr")]
    public partial class UIMgr : TMonoSingleton<UIMgr>
    {
        public delegate bool PanelCloseFilter(int panelID);

        #region 字段
        private EventSystem                 m_UIEventSystem = ObjectPool<EventSystem>.S.Allocate();

        private UIRoot                      m_UIRoot;
        private int                         m_NextPanelID = 0;
        private List<PanelInfo>             m_ActivePanelInfoList = new List<PanelInfo>();//当前正在运行的所有界面列表
        private Dictionary<int, PanelInfo>  m_ActivePanelInfoMap = new Dictionary<int, PanelInfo>();

        private List<PanelInfo>             m_CachedPanelList = new List<PanelInfo>();
        private bool                        m_PanelSortingOrderDirty = false;
        private bool                        m_IsPanelInfoListChange = false;

        #endregion

        private int nextPanelID
        {
            get
            {
                return ++m_NextPanelID;
            }
        }
        public UIRoot uiRoot
        {
            get { return m_UIRoot; }
        }

        public EventSystem uiEventSystem
        {
            get
            {
                return m_UIEventSystem;
            }
        }

        public override void OnSingletonInit()
        {
            if (m_UIRoot == null)
            {
                UIRoot root = GameObject.FindObjectOfType<UIRoot>();
                if (root == null)
                {
                    root = LoadUIRoot();
                }

                m_UIRoot = root;
                if (m_UIRoot == null)
                {
                    Log.e("Error:UIRoot Is Null.");
                }
                ObjectPool<PanelInfo>.S.maxCacheCount = 10;
                ObjectPool<PanelInfo.OpenParam>.S.maxCacheCount = 20;
            }
        }

        private UIRoot LoadUIRoot()
        {
            ResLoader loader = ResLoader.Allocate(null);
            loader.Add2Load(ProjectPathConfig.UI_ROOT_PATH);
            loader.LoadSync();

            IRes res = ResMgr.S.GetRes(ProjectPathConfig.UI_ROOT_PATH, false);
            if (res == null || res.asset == null)
            {
                return null;
            }

            GameObject prefab = res.asset as GameObject;
            if (prefab == null)
            {
                return null;
            }

            GameObject uiRoot = GameObject.Instantiate(prefab);
            loader.ReleaseAllRes();
            loader.Recycle2Cache();
            return uiRoot.GetComponent<UIRoot>();
        }

        #region Public Func

        public void SetPanelSortingOrderDirty()
        {
            m_PanelSortingOrderDirty = true;
        }

        public void AttachPage<T>(int panelID, T uiID, Transform parent, Action<AbstractPage> listener, bool singleton)
            where T : IConvertible
        {
            LoadPageAsync(panelID, uiID.ToInt32(null), parent, listener, singleton);
        }

        public AbstractPanel FindPanel<T>(T uiID) where T : IConvertible
        {
            PanelInfo info = GetPanelFromActive(uiID.ToInt32(null));
            if (info == null)
            {
                return null;
            }
            return info.Panel;
        }

        public int FindTopPanel<T>(T[] filter = null) where T : IConvertible
        {
            int result = -1;
            if (filter == null)
            {
                if (m_ActivePanelInfoList.Count > 0)
                {
                    result = m_ActivePanelInfoList[m_ActivePanelInfoList.Count - 1].uiID;
                }
            }
            else
            {
                for (int i = m_ActivePanelInfoList.Count - 1; i >= 0; --i)
                {
                    if (IsValueInArray(filter, m_ActivePanelInfoList[i].uiID))
                    {
                        result = m_ActivePanelInfoList[i].uiID;
                        break;
                    }
                }
            }

            return result;
        }

        public AbstractPage FindPanelPage<T>(int panelID, T uiID) where T : IConvertible
        {
            PanelInfo info = FindPanelInfoByPanelID(panelID);
            if (info == null)
            {
                return null;
            }

            return info.GetPage(uiID.ToInt32(null));
        }

        public void OpenDependPanel<T>(int panelID, T uiID, Action<AbstractPanel> callBack, params object[] args)
            where T : IConvertible
        {
            PanelInfo panelInfo = FindPanelInfoByPanelID(panelID);
            if (panelInfo == null)
            {
                Log.e("OpenDependPanel Not Find PanelID:" + panelID);
                return;
            }
            OpenDependPanel(uiID.ToInt32(null), panelInfo, callBack, args);
        }

        public void OpenPanel<T>(T uiID, params object[] args) where T : IConvertible
        {
            OpenPanel(uiID, PanelType.Auto, null, args);
        }

        public void OpenPanel<T>(T uiID, Action<AbstractPanel> callBack, params object[] args) where T : IConvertible
        {
            OpenPanel(uiID, PanelType.Auto, callBack, args);
        }

        public void OpenTopPanel<T>(T uiID, Action<AbstractPanel> callBack, params object[] args) where T : IConvertible
        {
            OpenPanel(uiID, PanelType.Top, callBack, args);
        }

        public void OpenBottomPanel<T>(T uiID, Action<AbstractPanel> callBack, params object[] args) where T : IConvertible
        {
            OpenPanel(uiID, PanelType.Bottom, callBack, args);
        }

        public void OpenPanel<T>(T uiID, PanelType panelType, Action<AbstractPanel> listener, params object[] args)
    where T : IConvertible
        {
            PanelInfo panelInfo = LoadPanelInfo(uiID.ToInt32(null));

            if (panelInfo == null)
            {
                return;
            }

            panelInfo.sortIndex = m_UIRoot.RequireNextPanelSortingOrder(panelType);
            panelInfo.AddMaster(panelInfo.panelID, args);

            if (panelInfo.isReady)
            {
                ReSortPanel();
                panelInfo.AddOpenCallback(listener);
            }
            else
            {
                panelInfo.AddOpenCallback(listener);
                panelInfo.LoadPanelResAsync();
            }
        }

        public void ClosePanel<T>(T uiID) where T : IConvertible
        {
            int eID = uiID.ToInt32(null);
            bool hasChange = false;
            for (int i = m_ActivePanelInfoList.Count - 1; i >= 0; --i)
            {
                if (m_ActivePanelInfoList[i].uiID == eID)
                {
                    ClosePanelInfo(m_ActivePanelInfoList[i]);
                    hasChange = true;
                }
            }

            if (hasChange)
            {
                ReSortPanel();
            }
        }

        public void ClosePanel(AbstractPanel panel)
        {
            if (panel == null)
            {
                return;
            }

            PanelInfo panelInfo = FindPanelInfoByPanelID(panel.panelID);

            //该面板的管理失效，直接移除
            if (panelInfo == null)
            {
                panelInfo = GetPanelFromCache(panel.uiID, false);

                if (panelInfo == null)
                {
                    Log.e("Not Find PanelInfo For Panel.");
                    panel.OnPanelClose(true);
                    GameObject.Destroy(panel.gameObject);
                }
                return;
            }

            ClosePanelInfo(panelInfo);
            ReSortPanel();
        }

        public void ShortCachePanel<T>(T uiID, int cacheCount) where T : IConvertible
        {
            UIData data = UIDataTable.Get(uiID.ToInt32(null));
            if (data == null)
            {
                return;
            }

            data.shortCacheCount = cacheCount;
        }

        public void UnShortCachePanel<T>(T uiID, bool clean = true) where T : IConvertible
        {
            UIData data = UIDataTable.Get(uiID.ToInt32(null));
            if (data == null)
            {
                return;
            }

            data.shortCacheCount = 0;

            if (data.cacheCount > 0)
            {
                return;
            }

            if (!clean)
            {
                return;
            }

            for (int i = m_CachedPanelList.Count - 1; i >= 0; --i)
            {
                if (i >= m_CachedPanelList.Count)
                {
                    continue;
                }

                PanelInfo panelInfo = m_CachedPanelList[i];

                UIData data2 = UIDataTable.Get(panelInfo.uiID);
                if (data2.cacheCount < 1)
                {
                    m_CachedPanelList.RemoveAt(i);

                    GameObject.Destroy(panelInfo.Panel.gameObject);

                    ObjectPool<PanelInfo>.S.Recycle(panelInfo);
                }
            }
        }

        public void DestroyAllPanel(PanelCloseFilter filter = null)
        {
            for (int i = m_ActivePanelInfoList.Count - 1; i >= 0; --i)
            {
                if (i >= m_ActivePanelInfoList.Count)
                {
                    continue;
                }

                PanelInfo panelInfo = m_ActivePanelInfoList[i];

                if (filter != null)
                {
                    if (!filter(panelInfo.panelID))
                    {
                        continue;
                    }
                }

                RemovePanelInfo(panelInfo);

                m_UIRoot.ReleasePanelSortingOrder(panelInfo.sortIndex);

                panelInfo.ClosePanel(true);

                ObjectPool<PanelInfo>.S.Recycle(panelInfo);
            }

            for (int i = m_CachedPanelList.Count - 1; i >= 0; --i)
            {
                if (i >= m_CachedPanelList.Count)
                {
                    continue;
                }

                PanelInfo panelInfo = m_CachedPanelList[i];

                m_CachedPanelList.RemoveAt(i);

                GameObject.Destroy(panelInfo.Panel.gameObject);

                ObjectPool<PanelInfo>.S.Recycle(panelInfo);
            }
        }

        //编辑器面板强行删除面板
        public void OnPanelForceDestroy(AbstractPanel panel)
        {
            if (panel == null)
            {
                return;
            }

            PanelInfo panelInfo = FindPanelInfoByPanelID(panel.panelID);

            if (panelInfo == null)
            {
                return;
            }

            RemovePanelInfo(panelInfo);
            CheckNeedClosePanel(panelInfo.panelID);
            ObjectPool<PanelInfo>.S.Recycle(panelInfo);
            ReSortPanel();
        }

        public void CloseDependPanel<T>(int master, T uiID) where T : IConvertible
        {
            int eID = uiID.ToInt32(null);
            List<int> needClosePanel = new List<int>();
            for (int i = m_ActivePanelInfoList.Count - 1; i >= 0; --i)
            {
                if (m_ActivePanelInfoList[i].uiID == eID)
                {
                    m_ActivePanelInfoList[i].RemoveMaster(master);
                    if (m_ActivePanelInfoList[i].nextMaster <= 0)
                    {
                        needClosePanel.Add(m_ActivePanelInfoList[i].panelID);
                    }
                }
            }

            if (needClosePanel.Count > 0)
            {
                for (int i = needClosePanel.Count - 1; i >= 0; --i)
                {
                    PanelInfo info = FindPanelInfoByPanelID(needClosePanel[i]);
                    if (info != null)
                    {
                        ClosePanelInfo(info);
                    }
                }
            }

            ReSortPanel();
        }

        #endregion

        #region Private Func
        private AbstractPage ProcessAttachPage(int panelID, int uiID, GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            PanelInfo panelInfo = FindPanelInfoByPanelID(panelID);
            if (panelInfo == null || panelInfo.Panel == null)
            {
                Log.e("AttachPage Failed To Find Panel:" + panelID);
                return null;
            }

            panelInfo.SetSortingOrderDirty();

            AbstractPage page = go.GetComponent<AbstractPage>();
            if (page == null)
            {
                Log.e("Failed To Find Page In UI:" + uiID);
                go.SetActive(true);
                SetPanelSortingOrderDirty();
                return null;
            }

            UIData panelData = UIDataTable.Get(uiID);

            if (panelData.panelClassType != null)
            {
                if (page.GetType() != panelData.panelClassType)
                {
                    Log.e("ERROR: Prefab Bind C# Class Is Not Same With Define:" + panelData.name);
                }
            }

            page.parentPage = panelInfo.Panel;

            page.uiID = uiID;

            go.SetActive(true);

            page.OpenPage();

            SetPanelSortingOrderDirty();

            return page;
        }

        private void LoadPageAsync(int panelID, int uiID, Transform parent, Action<AbstractPage> listener, bool singleton)
        {
            if (parent == null)
            {
                Log.e("Failed to Open Page, parent is Null.");
                return;
            }

            PanelInfo panelInfo = FindPanelInfoByPanelID(panelID);
            if (panelInfo == null)
            {
                Log.e("Not Find Panel To Attach Page:" + panelID);
                return;
            }

            panelInfo.LoadPageResAsync(uiID, parent, singleton, listener);
        }


        private void OpenPanel(int uiID, PanelType panelType, Action<AbstractPanel> listener, params object[] args)
        {
            PanelInfo panelInfo = LoadPanelInfo(uiID);

            if (panelInfo == null)
            {
                return;
            }

            panelInfo.sortIndex = m_UIRoot.RequireNextPanelSortingOrder(panelType);
            panelInfo.AddMaster(panelInfo.panelID, args);

            if (panelInfo.isReady)
            {
                ReSortPanel();
                panelInfo.AddOpenCallback(listener);
            }
            else
            {
                panelInfo.AddOpenCallback(listener);
                panelInfo.LoadPanelResAsync();
            }
        }

        private void OpenDependPanel(int uiID, PanelInfo masterInfo, Action<AbstractPanel> listener, params object[] args)
        {
            if (masterInfo == null)
            {
                Log.e("DependPanel Can not open with null Parent.");
                return;
            }

            PanelInfo panelInfo = LoadPanelInfo(uiID);

            if (panelInfo == null)
            {
                return;
            }

            panelInfo.AddMaster(masterInfo.panelID, args);

            if (panelInfo.isReady)
            {
                ReSortPanel();
                panelInfo.AddOpenCallback(listener);
            }
            else
            {
                panelInfo.AddOpenCallback(listener);
                panelInfo.LoadPanelResAsync();
            }
        }

        private void ClosePanelInfo(PanelInfo panelInfo)
        {
            if (panelInfo == null)
            {
                return;
            }

            //删除对自己的引用
            panelInfo.RemoveMaster(panelInfo.panelID);

            if (panelInfo.nextMaster > 0)
            {
                //重新调整层级就行
            }
            else             //该面板已经没有任何依赖
            {
                UIData data = UIDataTable.Get(panelInfo.uiID);

                bool destroy = true;
                if (data != null && data.cacheCount > 0)
                {
                    if (GetActiveAndCachedUICount(panelInfo.uiID) <= data.cacheCount)
                    {
                        destroy = false;
                    }
                }

                RemovePanelInfo(panelInfo);

                //恢复层级记录
                m_UIRoot.ReleasePanelSortingOrder(panelInfo.sortIndex);

                //处理是否真正销毁面板逻辑
                if (destroy)
                {
                    panelInfo.ClosePanel(destroy);
                }
                else
                {
                    m_CachedPanelList.Add(panelInfo);
                    panelInfo.ClosePanel(destroy);
                }

                //该面板的删除将影响它的依赖面板
                CheckNeedClosePanel(panelInfo.panelID);

                if (destroy)
                {
                    ObjectPool<PanelInfo>.S.Recycle(panelInfo);
                }
            }
        }

        private void CheckNeedClosePanel(int panelID)
        {
            //该面板的删除将影响它的依赖面板
            RemoveMaster(panelID);

            List<int> needClosePanel = new List<int>();

            for (int i = m_ActivePanelInfoList.Count - 1; i >= 0; --i)
            {
                if (m_ActivePanelInfoList[i].nextMaster <= 0)
                {
                    needClosePanel.Add(m_ActivePanelInfoList[i].panelID);
                }
            }

            for (int i = needClosePanel.Count - 1; i >= 0; --i)
            {
                PanelInfo info = FindPanelInfoByPanelID(needClosePanel[i]);
                if (info != null)
                {
                    ClosePanelInfo(info);
                }
            }
        }

        private void ReSortPanel()
        {
            m_IsPanelInfoListChange = false;
            m_PanelSortingOrderDirty = false;

            SortActivePanelInfo();

            ProcessPanelGameObjectActiveState();

            for (int i = 0; i < m_ActivePanelInfoList.Count; ++i)
            {
                m_ActivePanelInfoList[i].OpenPanel();
                //上面的代码导致内部状态改变
                if (m_IsPanelInfoListChange)
                {
                    m_IsPanelInfoListChange = true;
                    m_PanelSortingOrderDirty = true;
                    break;
                }
            }

            EventSystem.S.Send(EngineEventID.OnPanelUpdate);
        }

        private PanelInfo LoadPanelInfo(int uiID)
        {
            UIData data = UIDataTable.Get(uiID);
            if (data == null)
            {
                Log.e("Failed to OpenPanel, Not Find UIData for UIID:" + uiID);
                return null;
            }

            bool needAdd = true;
            PanelInfo panelInfo = GetPanelFromCache(uiID, true);

            if (panelInfo == null)
            {
                //缓存中没有，判断当前Panel 是否只支持单例
                if (data.isSingleton)
                {
                    //去当前所有已经开启的面板中寻找
                    panelInfo = GetPanelFromActive(uiID);
                }

                if (panelInfo == null)
                {
                    panelInfo = ObjectPool<PanelInfo>.S.Allocate();
                    panelInfo.Set(uiID, nextPanelID);
                }
                else
                {
                    needAdd = false;
                }
            }

            if (needAdd)
            {
                AddPanelInfo(panelInfo);
            }

            return panelInfo;
        }

        private void RemoveMaster(int master)
        {
            for (int i = m_ActivePanelInfoList.Count - 1; i >= 0; --i)
            {
                m_ActivePanelInfoList[i].RemoveMaster(master);
            }
        }

        private void AddPanelInfo(PanelInfo panelInfo)
        {
            if (panelInfo == null)
            {
                return;
            }

            if (m_ActivePanelInfoMap.ContainsKey(panelInfo.panelID))
            {
                Log.e("Already Add Panel to Mgr.");
                return;
            }

            m_ActivePanelInfoList.Add(panelInfo);
            m_ActivePanelInfoMap.Add(panelInfo.panelID, panelInfo);
        }

        private void RemovePanelInfo(PanelInfo panelInfo)
        {
            if (panelInfo == null)
            {
                return;
            }

            if (!m_ActivePanelInfoMap.ContainsKey(panelInfo.panelID))
            {
                Log.w("Already Remove Panel:" + panelInfo.uiID);
                return;
            }

            m_ActivePanelInfoMap.Remove(panelInfo.panelID);
            m_ActivePanelInfoList.Remove(panelInfo);
            m_IsPanelInfoListChange = true;
        }

        //处理面板隐藏逻辑
        private void ProcessPanelGameObjectActiveState()
        {
            int mask = 0;
            for (int i = m_ActivePanelInfoList.Count - 1; i >= 0; --i)
            {
                PanelInfo panelInfo = m_ActivePanelInfoList[i];

                if (panelInfo.sortIndex < 0)
                {
                    panelInfo.SetActive(false, false);
                    continue;
                }

                if (((mask ^ (int)PanelHideMask.Hide) & (int)PanelHideMask.Hide) == 0)
                {
                    panelInfo.SetActive(false, false);
                }
                else
                {
                    if (((mask ^ (int)PanelHideMask.UnInteractive) & (int)PanelHideMask.UnInteractive) == 0)
                    {
                        panelInfo.SetActive(true, false);
                    }
                    else
                    {
                        panelInfo.SetActive(true, true);
                    }
                }

                mask |= panelInfo.HideMask;
            }
        }

        private void SortActivePanelInfo()
        {
            m_ActivePanelInfoList.Sort(PanelCompare);

            int index = 0;
            int sortingOrder = 0;
            for (int i = 0; i < m_ActivePanelInfoList.Count; ++i)
            {
                if (m_ActivePanelInfoList[i].Panel != null)
                {
                    m_ActivePanelInfoList[i].SetSiblingIndexAndSortingOrder(index++, sortingOrder);
                    sortingOrder = m_ActivePanelInfoList[i].MaxSortingOrder;
                }
            }
        }

        private int PanelCompare(PanelInfo a, PanelInfo b)
        {
            return a.sortIndex - b.sortIndex;
        }

        private PanelInfo GetPanelFromCache(int uiID, bool remove)
        {
            for (int i = m_CachedPanelList.Count - 1; i >= 0; --i)
            {
                if (m_CachedPanelList[i].uiID == uiID)
                {
                    PanelInfo panel = m_CachedPanelList[i];
                    if (remove)
                    {
                        m_CachedPanelList.RemoveAt(i);
                    }
                    return panel;
                }
            }

            return null;
        }

        private PanelInfo GetPanelFromActive(int uiID)
        {
            for (int i = m_ActivePanelInfoList.Count - 1; i >= 0; --i)
            {
                if (m_ActivePanelInfoList[i].uiID == uiID)
                {
                    PanelInfo panel = m_ActivePanelInfoList[i];
                    return panel;
                }
            }
            return null;
        }

        private bool IsValueInArray<T>(T[] array, int id) where T : IConvertible
        {
            for (int i = 0; i < array.Length; ++i)
            {
                if (array[i].ToInt32(null) == id)
                {
                    return true;
                }
            }

            return false;
        }

        //Cache 和 Active的所有该UI 总数
        private int GetActiveAndCachedUICount(int uiID)
        {
            int result = 0;
            for (int i = m_CachedPanelList.Count - 1; i >= 0; --i)
            {
                if (m_CachedPanelList[i].uiID == uiID)
                {
                    ++result;
                }
            }

            for (int i = m_ActivePanelInfoList.Count - 1; i >= 0; --i)
            {
                if (m_ActivePanelInfoList[i].uiID == uiID)
                {
                    ++result;
                }
            }

            return result;
        }

        private PanelInfo FindPanelInfoByPanelID(int panelID)
        {
            PanelInfo panelInfo = null;

            if (!m_ActivePanelInfoMap.TryGetValue(panelID, out panelInfo))
            {
                //Log.w("Not Find Panel:" + panelID);
                return null;
            }

            return panelInfo;
        }

        private GameObject InstantiateUIPrefab(GameObject prefab)
        {
            return GameObject.Instantiate(prefab, uiRoot.hideRoot, false) as GameObject;
        }

        private void InitPanelParem(GameObject go)
        {
            InitOpenUIParam(go, m_UIRoot.panelRoot);
        }

        private void InitOpenUIParam(GameObject go, Transform parent)
        {
            if (go == null)
            {
                return;
            }

            Vector3 anchorPos = Vector3.zero;
            Vector2 sizeDel = Vector2.zero;
            Vector3 scale = Vector3.one;

            RectTransform rtTr = go.GetComponent<RectTransform>();
            if (rtTr != null)
            {
                anchorPos = rtTr.anchoredPosition;
                sizeDel = rtTr.sizeDelta;
                scale = rtTr.localScale;
            }

            rtTr.SetParent(parent, false);

            if (rtTr != null)
            {
                rtTr.anchoredPosition = anchorPos;
                rtTr.sizeDelta = sizeDel;
                rtTr.localScale = scale;
            }
        }

        #endregion

        #region Mono生命周期
        private void Update()
        {
            if (m_PanelSortingOrderDirty || m_IsPanelInfoListChange)
            {
                ReSortPanel();
            }
        }

        #endregion
    }

}
