using Ntreev.Library.Psd;
using PantheonGames.TexturePacker;
using PG;
using PSDUnity.Analysis;
using PSDUnity.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEditor.U2D.Common;
using UnityEngine;

namespace Assets.Visual_Studio_Solutions.PSDUnityEditor.MyPsdImporter
{
    public class MyPsdImporterCtrl
    {
        static MyPsdImporterCtrl instance;
        internal Transform uiRoot;
        Dictionary<string, PsdLayerNode> imgDict;

        public static MyPsdImporterCtrl Instance
        {
            get
            {
                if (instance == null)
                    instance = new MyPsdImporterCtrl();
                return instance;
            }
        }
        public PsdLayerNode PreParsePsdLayers(PsdDocument psd)
        {
            if (imgDict == null)
                imgDict = new Dictionary<string, PsdLayerNode>();
            imgDict.Clear();
            PsdLayerNode root = new PsdLayerNode(psd);
            
            foreach (PsdLayer layer in Enumerable.Reverse(psd.Childs))
            {
                root.AddChild(GenerateLayerNode(layer));
            }

            CreateTextures();
            return root;
        }

        #region resource process
        string GetModulePath()
        {
            return UIModuleProcessor.kUIModuleRootPath +"/" + GetModuleName();
        }

        string GetAtlasFolderPath()
        {
            return GetModulePath() + "/Atlas";
        }

        Sprite GetSprite(string name)
        {
            Sprite sprite = null;
            bool replaced = false;
            string path = GetModulePath();
            var guids = AssetDatabase.FindAssets("t:PantheonGames.TexturePacker.SpriteAtlas", new[] { path });
            for (int i = 0; i < guids.Length; ++i)
            {
                SpriteAtlas target = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guids[i]));
                sprite = target.GetSprite(name);
                if (sprite)
                {
                    replaced = true;
                    break;
                }
                else
                {
                    Debug.Log("wtf,no such sprite:" + name);
                }
            }
            if (!replaced)
            {
                SpriteAtlas globalAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(PG.UIModuleProcessor.kUIGlobalRootPath + "/GlobalAtlas.asset");              
                sprite = globalAtlas?.GetSprite(name);
            }
            return sprite;
        }

        internal void RefreshImageSprite(PsdLayerNode root)
        {
            List<ImgNode> images = new List<ImgNode>();
            root.GetImage(images);
            for (int i = 0; i < images.Count; ++i)
                images[i].sprite = GetSprite(GetRegularName(images[i].Name));
        }

        public string GetModuleName()
        {
            return "Home";
        }

        string GetAtlasPath()
        {
            return GetModulePath() + "/" + GetModuleName() + "Atlas.png";
        }

        string GetPrefabPath()
        {
            return GetModulePath() + "/Prefabs";
        }
        public void CreateTextures()
        {
            foreach(var node in imgDict)
            {
                var texture = node.Value.image.texture;
                if(texture)
                {
                    // Need to load the image first
                    byte[] buf = ExportUtility.EncordToPng(texture);
                    var atlasRoot = GetAtlasFolderPath();
                    if(!Directory.Exists(atlasRoot))
                        Directory.CreateDirectory(atlasRoot);

                    var name = GetRegularName(node.Key);
                    var path = string.Format(atlasRoot + "/{0}.png", name);
                    //if (!File.Exists(path))
                    {
                        File.WriteAllBytes(path, buf);

                        try
                        {
                            TextureFormatUtility.SetPreferredSpriteSettings(path, (settings, spriteSettings) =>
                            {
                                settings.spriteBorder = spriteSettings.spriteBorder;
                            }, importer => false);
                            TextureFormatUtility.SetPreferredPlatformSettings(path);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                        finally
                        {

                        }
                    }
                }
            }
            //ProcessAtlas(null);
        }

        public void ProcessAtlas(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
                moduleName = GetModuleName();
            UIModuleProcessor.FormatSpecificUIModule(moduleName);

            UIModuleSpriteAtlas.Run(UIModuleProcessor.kUIModuleRootPath + "/" + moduleName, null);
        }
        #endregion
        //遍历所有图层（有图的，非group），剔除外围的透明像素得到一个真正的rect
        public void ExtractLayerRectRectAndImage(string psdFile)
        {
            //FindTightRectJob.Execute()
        }

        public PsdLayerNode GenerateLayerNode(PsdLayer root)
        {
            PsdLayerNode node = new PsdLayerNode(root);
            if (!root.IsGroup)
            {
                node.image = ExportUtility.GenerateLayerImgNode(root, node, true);
                if (!imgDict.ContainsKey(node.name))
                    imgDict.Add(node.name, node);
                else
                {
                    var preNode = imgDict[node.name];
                    if (node.image.IsBigger(preNode.image))
                        imgDict[node.name] = node;
                }
            }
            foreach (var layer in root.Childs)
            {
                node.AddChild(GenerateLayerNode(layer));
            }
            return node;
        }

        //get layer's untransparent pixel rect(相对于图层原来的rect数据）
        public static Rect GetRectFromLayer(IPsdLayer psdLayer)
        {
            Rect ret = new Rect();

            return ret;
        }

        public string GetRegularName(string name)
        {
            var splitCharIndex = name.IndexOf('@');

            if (splitCharIndex >= 0)
                name = name.Substring(0, splitCharIndex);
            splitCharIndex = name.IndexOf(':');

            if (splitCharIndex >= 0)
                name = name.Substring(0, splitCharIndex);
            return name;
        }
    }
}
