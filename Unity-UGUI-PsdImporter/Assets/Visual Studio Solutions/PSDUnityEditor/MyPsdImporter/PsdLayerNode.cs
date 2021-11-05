using Ntreev.Library.Psd;
using PSDUnity.Analysis;
using PSDUnity.Data;
using PSDUnity.UGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Visual_Studio_Solutions.PSDUnityEditor.MyPsdImporter
{
    public class PsdLayerNode
    {
        public MyImgNode image;
        public string name;

        List<PsdLayerNode> childs;
        PsdLayerNode parent;
        Rect rect;

        public PsdLayerNode(Rect rect)
        {
            SetRect(rect);
        }

        public void SetRect(Rect rect)
        {
            this.rect = rect;
        }

        public Rect GetRect()
        {
            return rect;
        }

        public PsdLayerNode(IPsdLayer layer):this(ExportUtility.GetRectFromLayer(layer))
        {
            name = MyPsdImporterCtrl.Instance.GetRegularName(layer.Name);
        }

        public void AddChild(PsdLayerNode node)
        {
            if (childs == null)
                childs = new List<PsdLayerNode>();
            childs.Add(node);
            node.parent = this;
        }
        //生成

        GameObject DrawUGUIGameobject(Transform parent)
        {
            var go = LayerImportFactory.Instance.GetLayerImporter(this).ImportNode(this, parent);
            return go;
        }

        internal void GetImage(List<MyImgNode> images)
        {
            if(image!=null&&image.type != ImgType.Label)
                images.Add(image);
            if(childs!=null)
                foreach (var child in childs)
                    child.GetImage(images);
        }

        public void Draw(Transform parent)
        {
            var go = DrawUGUIGameobject(parent);
            if(childs!=null)
            {
                foreach(var child in childs)
                {
                    child.Draw(go.transform);
                }
            }
        }
    }
}
