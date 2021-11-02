using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PG
{
    public static class PrefabProcessTools
    {
        private const string kUIRootPath = "Assets/AssetBundles/UI";
        public const string kUIModuleRootPath = "Assets/AssetBundles/UI/Modules";
        public const string kUIWidgetRootPath = "Assets/AssetBundles/UI/Widgets";
        public const string kUIGlobalRootPath = "Assets/AssetBundles/UI/Global";
        public const string kUIEffectRootPath = "Assets/AssetBundles/UI/Effect";
        private const string infoFilePath = @"D:\Documents\Work\UnityPrefabReferInfo.txt";
        /// <summary>
        ///  检查prefab是否被Assets/AssetBundles/UI下其它prefab引用
        /// </summary>
        public static void CheckAllPrefabsReferrence()
        {

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { kUIRootPath });
            int length = guids.Length;

            //遍历list，对每一个prefab检查是否引用了被查prefab(how--check whether contain guid or not)
            for (int i = 0; i < length; i++)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (filePath.StartsWith(kUIEffectRootPath))
                    continue;
                CheckPrefabIsReferByPath(filePath, guids);
            }
            
        }

        /// <summary>
        ///  检查prefab是否被Assets/AssetBundles/UI下其它prefab引用
        /// </summary>
        public static void CheckSelectedPrefabsReferrence()
        {
            UnityEngine.Object[] selectObjs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
            foreach (var selection in selectObjs)
            {
                if (ProjectWindowUtil.IsFolder(selection.GetInstanceID())) continue;
                var path = AssetDatabase.GetAssetPath(selection);
                if (!path.StartsWith(kUIRootPath))
                {
                    continue;
                }

                //获取目录下所有prefab组成的list
                string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { kUIRootPath });
                CheckPrefabIsReferByPath(path, guids);
            }
        }

        public static void CheckPrefabIsReferByPath(string path, string[] guids)
        {
            string desGuid = AssetDatabase.AssetPathToGUID(path);
            int length = guids.Length;
            string ret = path + "被引用情况：\n";
            List<string> info = new List<string>();
            info.Add(ret);
            //遍历list，对每一个prefab检查是否引用了被查prefab(how--check whether contain guid or not)
            bool finded = false;
            for (int i = 0; i < length; i++)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayCancelableProgressBar("Checking", filePath, i / length * 1.0f);

                //检查是否包含guid
                string content = File.ReadAllText(filePath);
                if (content.Contains(desGuid))
                {
                    finded = true;
                    //Object fileObject = AssetDatabase.LoadAssetAtPath(filePath, typeof(Object));
                    //Debug.Log(path + "被" + filePath + "引用到了");
                    info.Add("被" + filePath + "引用到了");
                }
            }
            if(!finded)
            {
                //Debug.Log(path + "没被引用");
                info.Add(path + "没被引用\n\n");
            }
            EditorUtility.ClearProgressBar();
            StreamWriter sw = File.AppendText(infoFilePath);
            var line = info.GetEnumerator();
            while(line.MoveNext())
            {
                sw.WriteLine((string)line.Current);
            }
           
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }
    }
}
