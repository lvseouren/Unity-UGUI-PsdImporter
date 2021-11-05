using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Visual_Studio_Solutions.PSDUnityEditor.MyPsdImporter
{
    public enum ImgSource
    {
        Normal,//普通
        Custom,//唯一图片（唯一命名）
        Globle//全局图片
    }

    public enum SuffixType
    {
        None,
        appendRectHash,//图片的Rect
        appendIndex,//顺序
        appendGroup//组的名字
    }

    public enum ImgType : int
    {
        Label = 0,
        Image = 1,
        AtlasImage = 2,
        Texture = 3,
        Color = 4
    }

    public class MyImgNode
    {
        public string Name;
        private int hashImage = 0;
        private int index = 0;
        private SuffixType customNameType;
        private string baseName;
        public string TextureName
        {
            get
            {
                if (forceAddress || source == ImgSource.Custom)
                {
                    if (customNameType == SuffixType.appendRectHash)
                    {
                        return Name + hashImage;
                    }
                    else if (customNameType == SuffixType.appendIndex)
                    {
                        return Name + index;
                    }
                    else if (customNameType == SuffixType.appendGroup)
                    {
                        return Name + baseName;
                    }
                    else
                        return Name;
                }
                else
                {
                    return Name;
                }
            }
        }
        public ImgType type;
        public ImgSource source;
        public bool forceAddress;
        public Rect rect;
        public Sprite sprite;
        public Texture2D texture;
        public string text = "";
        public Font font;
        public int fontSize = 0;
        public Color color = UnityEngine.Color.white;
        public MyImgNode() { }

        public MyImgNode(string baseName, Rect rect, Texture2D texture) : this(rect)
        {
            this.baseName = baseName;
            this.rect = rect;
            this.texture = texture;
            this.type = ImgType.AtlasImage;
        }
        public MyImgNode(string name, Rect rect, Color color) : this(rect)
        {
            this.Name = name;
            this.type = ImgType.Color;
            this.color = color;
        }
        public MyImgNode(string name, Rect rect, string font, int fontSize, string text, Color color) : this(rect)
        {
            this.type = ImgType.Label;
            this.Name = name;
            this.font = null; /*Debug.Log(font);*/
            this.fontSize = fontSize;
            this.text = text;
            this.color = color;
        }

        private MyImgNode(Rect rect)
        {
            this.rect = rect;
        }

        public MyImgNode SetIndex(int index)
        {
            this.index = index;
            return this;
        }

        public bool IsBigger(MyImgNode node)
        {
            return rect.width * rect.height > node.rect.width * node.rect.height;
        }

        /// <summary>
        /// 将名字转换（去除标记性字符）
        /// </summary>
        /// <returns></returns>
        public MyImgNode Analyzing(string name)
        {
            this.Name = name;
            this.customNameType = SuffixType.None;
            this.forceAddress = false;
            //添加后缀
            if (texture != null)
            {
                this.hashImage = rect.GetHashCode();
                this.texture.name = TextureName;
            }
            return this;
        }
    }
}
