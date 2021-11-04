﻿using Ntreev.Library.Psd;
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
        string moduleName;
        private string psdName;
        private static Vector2 rootSize { get; set; }

        public static MyPsdImporterCtrl Instance
        {
            get
            {
                if (instance == null)
                    instance = new MyPsdImporterCtrl();
                return instance;
            }
        }

        public void InitEnvironment(string psdPath, Vector2 size)
        {
            rootSize = size;
            string[] data = psdPath.Split('/');
            var index = Array.FindIndex(data, x => x == "Modules");
            moduleName = data[index + 1];
            psdName = data[index + 3];
            psdName = psdName.Split('.')[0];
        }

        internal string GetPrefabPath(string psdName)
        {
            return GetPrefabFolderPath() + "/" + psdName + ".prefab";
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

        string GetPrefabFolderPath()
        {
            return GetModulePath() + "/Prefabs";
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

        internal string GetPsdName()
        {
            return psdName;
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
            return moduleName;
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
                    byte[] buf = EncordToPng(texture);
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

        /// <summary>
        /// 兼容unity2017和unity5.6
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static byte[] EncordToPng(Texture2D texture)
        {
            try
            {
                var assemble = System.Reflection.Assembly.Load("UnityEngine.ImageConversionModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                if (assemble != null)
                {
                    var imageConvention = assemble.GetType("UnityEngine.ImageConversion");
                    if (imageConvention != null)
                    {
                        return imageConvention.GetMethod("EncodeToPNG", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod).Invoke(null, new object[] { texture }) as byte[];
                    }
                }
            }
            catch (Exception)
            {
                return texture.GetType().GetMethod("EncodeToPNG").Invoke(texture, null) as byte[];
            }

            return new byte[0];
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
                node.image = GenerateLayerImgNode(root, node, true);
                if (node.image.texture != null)
                {
                    if (!imgDict.ContainsKey(node.name))
                        imgDict.Add(node.name, node);
                    else
                    {
                        var preNode = imgDict[node.name];
                        if (node.image.IsBigger(preNode.image))
                            imgDict[node.name] = node;
                    }
                }
            }
            foreach (var layer in root.Childs)
            {
                node.AddChild(GenerateLayerNode(layer));
            }
            return node;
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

        public static ImgNode GenerateLayerImgNode(PsdLayer layer, PsdLayerNode node, bool forceSprite = false)
        {
            ImgNode data = null;
            var canvasRect = new MyRect(0, rootSize.x, 0, rootSize.y);
            var texture = CreateClipTexture(layer, canvasRect, out Rect clipRect);
            var rect = clipRect;
            node.SetRect(rect);
            switch (layer.LayerType)
            {
                case LayerType.Normal:
                    data = new ImgNode("", rect, texture).SetIndex(CalcuteLayerID(layer)).Analyzing(null, layer.Name);
                    break;
                case LayerType.Color:
                    if (forceSprite)
                    {
                        data = new ImgNode("", rect, texture).SetIndex(CalcuteLayerID(layer)).Analyzing(null, layer.Name);
                    }
                    else
                    {
                        data = new ImgNode(layer.Name, rect, GetLayerColor(layer)).SetIndex(CalcuteLayerID(layer));
                    }
                    break;
                case LayerType.Text:
                    var textInfo = layer.Records.TextInfo;
                    var color = new Color(textInfo.color[0], textInfo.color[1], textInfo.color[2], textInfo.color[3]);
                    data = new ImgNode(layer.Name, rect, textInfo.fontName, textInfo.fontSize, textInfo.text, color);
                    break;
                case LayerType.Complex:
                    data = new ImgNode("", rect, texture).SetIndex(CalcuteLayerID(layer)).Analyzing(null, layer.Name);
                    break;
                default:
                    break;
            }
            if (data != null)
                data.color.a *= layer.Opacity;
            return data;
        }

        /// <summary>
        /// 计算平均颜色
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static Color GetLayerColor(PsdLayer layer)
        {
            Channel red = Array.Find(layer.Channels, i => i.Type == ChannelType.Red);
            Channel green = Array.Find(layer.Channels, i => i.Type == ChannelType.Green);
            Channel blue = Array.Find(layer.Channels, i => i.Type == ChannelType.Blue);
            Channel alpha = Array.Find(layer.Channels, i => i.Type == ChannelType.Alpha);
            //Channel mask = Array.Find(layer.Channels, i => i.Type == ChannelType.Mask);

            Color[] pixels = new Color[layer.Width * layer.Height];

            for (int i = 0; i < pixels.Length; i++)
            {
                byte r = red.Data[i];
                byte g = green.Data[i];
                byte b = blue.Data[i];
                byte a = 255;

                if (alpha != null && alpha.Data[i] != 0)
                    a = (byte)alpha.Data[i];
                //if (mask != null && mask.Data[i] != 0)
                //    a *= mask.Data[i];

                int mod = i % layer.Width;
                int n = ((layer.Width - mod - 1) + i) - mod;
                pixels[pixels.Length - n - 1] = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }
            Color color = Color.white;
            foreach (var item in pixels)
            {
                color += item;
                color *= 0.5f;
            }
            return color;
        }

        private static int CalcuteLayerID(PsdLayer layer)
        {
            int id = 0;
            var parent = layer.Parent;
            if (parent != null)
            {
                id = Array.IndexOf(parent.Childs, layer);
                id += 10 * CalcuteLayerID(parent);
            }
            else
            {
                id = Array.IndexOf(layer.Document.Childs, layer);
            }
            return id;
        }

        //将画布之外的像素裁剪掉
        public static Texture2D CreateClipTexture(PsdLayer layer, MyRect canvasRect, out Rect clipRect)
        {
            clipRect = new Rect();
            Debug.Assert(layer.Width != 0 && layer.Height != 0, layer.Name + ": width = height = 0");
            if (layer.Width == 0 || layer.Height == 0) return new Texture2D(layer.Width, layer.Height);
            var clipLeft = Math.Max(layer.Left, canvasRect.left);
            var clipRight = Math.Min(layer.Right, canvasRect.right);
            var clipTop = Math.Max(layer.Top, canvasRect.top);
            var clipBot = Math.Min(layer.Bottom, canvasRect.bottom);

            if (clipTop > canvasRect.bottom || clipBot < canvasRect.top)//顶部已经在画布外，说明完全不可见了
                return null;
            if (clipLeft > layer.Left || clipRight < layer.Right || clipTop < layer.Top || clipBot > layer.Bottom)
            {
                Debug.Log("触发裁剪！");
            }
            GetRectFromLRTB(clipLeft, clipRight, clipTop, clipBot, out clipRect);

            int clipWidth = Math.Abs((int)(clipRight - clipLeft));
            int clipHeight = Math.Abs((int)(clipTop - clipBot));
            Texture2D texture = new Texture2D(clipWidth, clipHeight);
            Color32[] pixels = new Color32[clipWidth * clipHeight];

            Channel red = Array.Find(layer.Channels, i => i.Type == ChannelType.Red);
            Channel green = Array.Find(layer.Channels, i => i.Type == ChannelType.Green);
            Channel blue = Array.Find(layer.Channels, i => i.Type == ChannelType.Blue);
            Channel alpha = Array.Find(layer.Channels, i => i.Type == ChannelType.Alpha);

            layer.GetGradientColor(out Color32[] gradientColors, out int angle, out bool hasGradient);

            for (int i = 0; i < pixels.Length; i++)
            {
                //row,col:pixel-i 对应的texture2D的像素坐标值
                int row = i / clipWidth;
                int col = i % clipWidth;
                //mapRow, mapCol:原rect中的行列值（从下往上，从左往右）
                int mapRow = (int)(clipBot - layer.Top) - row - 1;
                int mapCol = col + (int)(clipLeft - layer.Left);

                var mapIndex = mapRow * layer.Width + mapCol;
                var redErr = red == null || red.Data == null || red.Data.Length <= mapIndex;
                var greenErr = green == null || green.Data == null || green.Data.Length <= mapIndex;
                var blueErr = blue == null || blue.Data == null || blue.Data.Length <= mapIndex;
                var alphaErr = alpha == null || alpha.Data == null || alpha.Data.Length <= mapIndex;

                if (mapIndex < 0 || mapIndex >= red.Data.Length)
                    Debug.Log("WTF");
                byte r = redErr ? (byte)0 : red.Data[mapIndex];
                byte g = greenErr ? (byte)0 : green.Data[mapIndex];
                byte b = blueErr ? (byte)0 : blue.Data[mapIndex];
                byte a = alphaErr ? (byte)255 : alpha.Data[mapIndex];
                if (hasGradient)
                {
                    Color32 color = GetGradientColor(gradientColors, layer.Width, layer.Height, mapRow, mapCol, angle);
                    r = color.r;
                    g = color.g;
                    b = color.b;
                }

                pixels[i] = new Color32(r, g, b, a);
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        public static Color32 GetGradientColor(Color32[] colors, int width, int height, int row, int col, int angle)
        {
            Color32 output = new Color32(colors[0].r, colors[0].g, colors[0].b, colors[0].a);
            float factor = (row + 1) * 1.0f / height;
            var c0 = colors[0].r;
            var c1 = colors[1].r;
            output.r = Convert.ToByte((int)c0 + (Math.Floor((c1 - c0) * factor)));
            c0 = colors[0].g;
            c1 = colors[1].g;
            output.g = Convert.ToByte((int)c0 + (Math.Floor((c1 - c0) * factor)));
            c0 = colors[0].b;
            c1 = colors[1].b;
            output.b = Convert.ToByte((int)c0 + (Math.Floor((c1 - c0) * factor)));
            return output;
        }

        public static Rect GetRectFromLayer(IPsdLayer psdLayer)
        {
            //rootSize = new Vector2(rootSize.x > maxSize.x ? maxSize.x : rootSize.x, rootSize.y > maxSize.y ? maxSize.y : rootSize.y);
            var left = psdLayer.Left;// psdLayer.Left <= 0 ? 0 : psdLayer.Left;
            var bottom = psdLayer.Bottom;// psdLayer.Bottom <= 0 ? 0 : psdLayer.Bottom;
            var top = psdLayer.Top;// psdLayer.Top >= rootSize.y ? rootSize.y : psdLayer.Top;
            var right = psdLayer.Right;// psdLayer.Right >= rootSize.x ? rootSize.x : psdLayer.Right;

            GetRectFromLRTB(left, right, top, bottom, out Rect rect);
            return rect;
        }

        public static void GetRectFromLRTB(int left, int right, int top, int bottom, out Rect rect)
        {
            var width = right - left;// psdLayer.Width > rootSize.x ? rootSize.x : psdLayer.Width;
            var height = bottom - top;// psdLayer.Height > rootSize.y ? rootSize.y : psdLayer.Height;

            var xMin = (right + left - rootSize.x) * 0.5f;
            var yMin = -(top + bottom - rootSize.y) * 0.5f;
            rect = new Rect(xMin, yMin, width, height);
        }
    }
}
