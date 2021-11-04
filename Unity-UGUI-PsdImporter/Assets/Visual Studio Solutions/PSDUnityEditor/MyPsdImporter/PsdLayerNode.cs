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
        public ImgNode image;
        public string name;

        List<PsdLayerNode> childs;
        PsdLayerNode parent;
        Rect rect;
        
        static TextImport textImport = new TextImport();
        static ImageRawImageImport imageImport = new ImageRawImageImport();
        public TextImport GetTextImport()
        {
            return textImport;
        }

        public ImageRawImageImport GetImageImport()
        {
            return imageImport;
        }

        public PsdLayerNode(Rect rect)
        {
            SetRect(rect);
        }

        public void SetRect(Rect rect)
        {
            this.rect = rect;
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
            //gen ugui go
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = UnityEngine.LayerMask.NameToLayer("UI");
            go.transform.SetParent(MyPsdImporterCtrl.Instance.uiRoot, false);
            Import.SetRectTransform(rect, go.transform as RectTransform);
            if (parent)
                go.transform.SetParent(parent);
            //init component if image!=null
            if (image!=null)
            {
                if (image.type == PSDUnity.ImgType.Label)
                    GetTextImport().DrawImage(image, go);
                else
                    GetImageImport().DrawImage(image, go);
            }
 
            return go;
        }

        internal void GetImage(List<ImgNode> images)
        {
            if(image!=null&&image.type != PSDUnity.ImgType.Label)
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
