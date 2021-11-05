using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Visual_Studio_Solutions.PSDUnityEditor.MyPsdImporter
{
    public class ImportBase
    {
        public GameObject CreateGameObject(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            return go;
        }

        public static void SetRectTransform(Rect rect, RectTransform rectTransform)
        {
            rectTransform.pivot = Vector2.one * 0.5f;
            rectTransform.anchorMin = rectTransform.anchorMax = Vector2.one * 0.5f;
            rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
            rectTransform.anchoredPosition = new Vector2(rect.x, rect.y);
        }

        public virtual void SetComponnets(GameObject go, PsdLayerNode node)
        {

        }

        public GameObject ImportNode(PsdLayerNode node, Transform parent)
        {
            //create gameobject
            GameObject go = CreateGameObject(node.name);
            go.layer = UnityEngine.LayerMask.NameToLayer("UI");
            //init recttransform info
            go.transform.SetParent(MyPsdImporterCtrl.Instance.uiRoot, false);
            var rectTrans = go.transform as RectTransform;
            SetRectTransform(node.GetRect(), rectTrans);
            if (parent)
                go.transform.SetParent(parent);
            //init component
            SetComponnets(go, node);
            return go;
        }
    }
}
