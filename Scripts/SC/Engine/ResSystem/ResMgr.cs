﻿using System;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
    [TMonoSingletonAttribute("[Tools]/ResMgr")]
    public class ResMgr : TMonoSingleton<ResMgr>, IEnumeratorTaskMgr
    {

#region 字段
        private Dictionary<string, IRes>    m_ResDictionary = new Dictionary<string, IRes>();
        private List<IRes>                  m_ResList = new List<IRes>();
        [SerializeField]
        private int                         m_CurrentCoroutineCount = 0;
        private int                         m_MaxCoroutineCount = 8;//最快协成大概在6到8之间
        private TimeDebugger                m_TimeDebugger;
        private LinkedList<IEnumeratorTask> m_IEnumeratorTaskStack = new LinkedList<IEnumeratorTask>();

        private bool                        m_IsWorking = true;
        //Res 在ResMgr中 删除的问题，ResMgr定时收集列表中的Res然后删除
        private bool                        m_IsResMapDirty = false;

        #endregion

        public void InitResMgr()
        {
            string path = FilePath.streamingAssetsPath + ProjectPathConfig.EXPORT_ASSETBUNDLE_CONFIG_PATH;

            AssetDataTable.S.LoadFromFile(path);
            ABManifestHandler.manifest = ABManifestHandler.LoadInstance();
            Log.i("Init[ResMgr]");
        }

        #region 属性
        public TimeDebugger timeDebugger
        {
            get
            {
                if (m_TimeDebugger == null)
                {
                    m_TimeDebugger = new TimeDebugger("#Res");
                }
                return m_TimeDebugger;
            }
        }

        public void SetResMapDirty()
        {
            m_IsResMapDirty = true;
        }

        public void PostIEnumeratorTask(IEnumeratorTask task)
        {
            if (task == null)
            {
                return;
            }

            m_IEnumeratorTaskStack.AddLast(task);
            TryStartNextIEnumeratorTask();
        }

        public IRes GetRes(string name, bool createNew)
        {
            IRes res = null;
            if (m_ResDictionary.TryGetValue(name, out res))
            {
                return res;
            }

            if (!createNew)
            {
                return null;
            }

            res = ResFactory.Create(name);

            if (res != null)
            {
                m_ResDictionary.Add(name, res);
                m_ResList.Add(res);
            }
            return res;
        }

        public R GetRes<R>(string name) where R : IRes
        {
            IRes res = null;
            if (m_ResDictionary.TryGetValue(name, out res))
            {
                return (R)res;
            }

            return default(R);
        }

#endregion

#region Private Func

        private void Update()
        {
            if (m_IsWorking)
            {
                if (m_IsResMapDirty)
                {
                    RemoveUnusedRes();
                }
            }
        }

        private void RemoveUnusedRes()
        {
            if (!m_IsResMapDirty)
            {
                return;
            }

            m_IsResMapDirty = false;

            IRes res = null;
            for (int i = m_ResList.Count - 1; i >= 0; --i)
            {
                res = m_ResList[i];
                if (res.refCount <= 0 && res.resState != eResState.kLoading)
                {
                    if (res.ReleaseRes())
                    {
                        m_ResList.RemoveAt(i);
                        m_ResDictionary.Remove(res.name);
                        res.Recycle2Cache();
                    }
                }
            }
        }

        private void OnIEnumeratorTaskFinish()
        {
            --m_CurrentCoroutineCount;
            TryStartNextIEnumeratorTask();
        }

        private void TryStartNextIEnumeratorTask()
        {
            if (m_IEnumeratorTaskStack.Count == 0)
            {
                return;
            }

            if (m_CurrentCoroutineCount >= m_MaxCoroutineCount)
            {
                return;
            }

            IEnumeratorTask task = m_IEnumeratorTaskStack.First.Value;
            m_IEnumeratorTaskStack.RemoveFirst();

            ++m_CurrentCoroutineCount;
            StartCoroutine(task.StartIEnumeratorTask(OnIEnumeratorTaskFinish));
        }
#endregion
    }
}
