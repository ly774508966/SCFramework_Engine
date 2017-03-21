using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SCFramework
{
    public class ProjectPathConfig
    {
        #region 工程目录
        public static string APP_CONFIG_PATH = "Resources/Demo/Config/AppConfig";
        #endregion

        #region AssetBundle 相关

        public const string AB_MANIFEST_NAME = "StreamingAssets";

        public static string AssetBundleUrl2Name(string url)
        {
            string parren = Application.dataPath + "/" + "StreamingAssets/";
            return url.Replace(parren, "");
        }

        public static string AssetBundleName2Url(string name)
        {
            string parren = Application.dataPath + "/" + "StreamingAssets/";
            return parren + name;
        }

        //导入目录
        public const string IMPORT_ROOT_FOLDER = "Assets/Resources/Engine";

        private const string IMPORT_TEXTURE_ROOT_FOLDER = "Assets/Resources/Engine/Texture";
        private const string IMPORT_UI_ROOT_FOLDER = "Assets/Resources/Engine/UI";

        public static string[] IMPORT_ROOT_FOLDERS =
        {
            IMPORT_TEXTURE_ROOT_FOLDER,
            IMPORT_UI_ROOT_FOLDER
        };

        //导出目录
        public const string EXPORT_ROOT_FOLDER = "Assets/StreamingAssets/";

        public const string EXPORT_ASSETBUNDLE_CONFIG_PATH = "StreamingAssets/asset_bindle_config.bin";
        #endregion
    }
}
