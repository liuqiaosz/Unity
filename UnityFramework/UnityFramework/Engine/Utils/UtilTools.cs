using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace UnityFramework.Engine.Utils
{
    /**
     * 
     * 工具类
     * 
     **/
    public class UtilTools
    {
        /**
         * 获取路径中的后缀
         * xxxx/xxxx/xxxx.png 返回png
         **/
        public static string GetFileSuffix(string Path)
        {
            return Path.Substring(Path.LastIndexOf(".") + 1);
        }

        /**
         * 获得不同平台下的StreamingAssets路径
         * 
         **/
        public static string GetStreamingAssetsPath()
        {
#if UNITY_ANDROID   //安卓
            return "jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IPHONE  //iPhone
            return Application.dataPath + "/Raw/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR  //windows平台和web平台

            return "file://" + Application.dataPath + "/StreamingAssets/";
#else
            return string.Empty;

#endif
        }
    }

}
