using PSDUnity.UGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Visual_Studio_Solutions.PSDUnityEditor.MyPsdImporter
{
    public class TextImport:ImportBase
    {
        protected float textBorder = 0;//生成Text时,需要一定的边距
        protected TextAnchor textAnchor = TextAnchor.MiddleCenter;
        protected HorizontalWrapMode text_h_wrapMode = HorizontalWrapMode.Overflow;
        VerticalWrapMode text_v_wrapMode = VerticalWrapMode.Overflow;

        public void InitComponent(Text comp)
        {
            comp.alignment = textAnchor;// TextAnchor.UpperLeft;
            comp.horizontalOverflow = text_h_wrapMode;// HorizontalWrapMode.Overflow;
            comp.verticalOverflow = text_v_wrapMode;// VerticalWrapMode.Truncate;
        }

        public override void SetComponnets(GameObject go, PsdLayerNode node)
        {
            var comp = go.GetComponent<Text>();
            if (!comp)
            {
                comp = go.AddComponent<Text>();
                InitComponent(comp);
            }
            MyPsdImporterCtrl.SetPictureOrLoadColor(node.image, comp);
        }
    }
}
