using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SCFramework
{
    [TMonoSingletonAttribute("[Tools]/SceneMgr")]
    public class SceneMgr : TMonoSingleton<SceneMgr>
    {
        private ResLoader m_CurrentLoader;

        public bool SwitchSceneSync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            ResLoader nextLoader = ResLoader.Allocate();

            //提前加入可以起到缓存已经加载的资源的作用，防止释放后又加载的重复动作
            if (!AddSceneAB2Loader(sceneName, nextLoader))
            {
                return false;
            }

            if (m_CurrentLoader != null)
            {
                m_CurrentLoader.ReleaseAllRes();
                m_CurrentLoader.Recycle2Cache();
                m_CurrentLoader = null;
            }

            m_CurrentLoader = nextLoader;

            m_CurrentLoader.LoadSync();

            try
            {
                SceneManager.LoadScene(sceneName, mode);
            }
            catch (Exception e)
            {
                Log.e("SceneManager LoadSceneSync Failed! SceneName:" + sceneName);
                Log.e(e);
                UnloadSceneAssetBundle(sceneName);
                return false;
            }

            UnloadSceneAssetBundle(sceneName);
            return true;
        }

        public void SwitchSceneAsync(string sceneName, Action<string, bool> loadCallback = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            ResLoader nextLoader = ResLoader.Allocate();

            //可以起到缓存已经加载的资源的作用，防止释放后又加载的重复动作
            if (!AddSceneAB2Loader(sceneName, nextLoader))
            {
                if (loadCallback != null)
                {
                    loadCallback(sceneName, false);
                }
                return;
            }

            if (m_CurrentLoader != null)
            {
                m_CurrentLoader.ReleaseAllRes();
                m_CurrentLoader.Recycle2Cache();
                m_CurrentLoader = null;
            }

            m_CurrentLoader = nextLoader;

            m_CurrentLoader.LoadAsync(() =>
            {
                StartCoroutine(OnSceneResLoadFinish(sceneName, loadCallback, mode));
            });
        }

        private IEnumerator OnSceneResLoadFinish(string sceneName, Action<string, bool> loadCallback, LoadSceneMode mode)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);
            yield return op;

            if (!op.isDone)
            {
                Log.e("SceneManager LoadSceneAsync Not Done! SceneName:" + sceneName);
                UnloadSceneAssetBundle(sceneName);

                if (loadCallback != null)
                {
                    loadCallback(sceneName, false);
                }
                yield break;
            }

            UnloadSceneAssetBundle(sceneName);

            if (loadCallback != null)
            {
                loadCallback(sceneName, true);
            }
        }

        private void UnloadSceneAssetBundle(string sceneName)
        {
            string abName = GetSceneAssetBundleName(sceneName);
            if (string.IsNullOrEmpty(abName))
            {
                return;
            }
            m_CurrentLoader.ReleaseRes(abName);
        }

        private bool AddSceneAB2Loader(string sceneName, ResLoader loader)
        {
            string abName = GetSceneAssetBundleName(sceneName);

            if(string.IsNullOrEmpty(abName))
            {
                return false;
            }

            loader.Add2Load(abName);

            return true;
        }

        private string GetSceneAssetBundleName(string sceneName)
        {
            AssetData config = AssetDataTable.S.GetAssetData(sceneName);

            if (config == null)
            {
                Log.e("Not Find AssetData For Asset:" + sceneName);
                return string.Empty;
            }

            string assetBundleName = AssetDataTable.S.GetAssetBundleName(config.assetBundleIndex);

            if (string.IsNullOrEmpty(assetBundleName))
            {
                Log.e("Not Find AssetBundle In Config:" + config.assetBundleIndex);
                return string.Empty;
            }

            return assetBundleName;
        }
    }
}
