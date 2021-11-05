using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using PSDUnity;
using PSDUnity.Data;

namespace PSDUnity.UGUI
{
    public class TextImport : ImageImport
    {
        [Header("[可选-----------------------------------")]
        [SerializeField, CustomField("字边距")] protected  float textBorder = 0;//生成Text时,需要一定的边距
        [SerializeField, CustomField("文字锚点")] protected  TextAnchor textAnchor = TextAnchor.MiddleCenter;
        [SerializeField, CustomField("水平适配")] protected  HorizontalWrapMode text_h_wrapMode = HorizontalWrapMode.Overflow;
        [SerializeField, CustomField("垂直适配")] VerticalWrapMode text_v_wrapMode = VerticalWrapMode.Overflow;
        public override GameObject CreateTemplate()
        {
            var text = new GameObject("Text", typeof(Text)).GetComponent<Text>();
            InitTemplate(text.gameObject);
            return text.gameObject;
        }

        public override void InitTemplate(GameObject go)
        {
            var text = go.GetComponent<Text>();
            text.alignment = textAnchor;// TextAnchor.UpperLeft;
            text.horizontalOverflow = text_h_wrapMode;// HorizontalWrapMode.Overflow;
            text.verticalOverflow = text_v_wrapMode;// VerticalWrapMode.Truncate;
        }

        public override UGUINode DrawImage(ImgNode image, UGUINode parent)
        {
            UGUINode node = CreateRootNode(image.Name, AdjustTextRect( image.rect,image.fontSize), parent);
            UnityEngine.UI.Text myText = node.InitComponent<Text>();
            PSDImporterUtility.SetPictureOrLoadColor(image, myText);
            return node;
        }

        public override void DrawImage(ImgNode image, GameObject go)
        {
            var comp = go.GetComponent<Text>();
            if (!comp)
            {
                comp = go.AddComponent<Text>();
                InitTemplate(go);
            }
            PSDImporterUtility.SetPictureOrLoadColor(image, comp);
        }

        /// <summary>
        /// 调节字边距
        /// </summary>
        /// <param name="image"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        private Rect AdjustTextRect(Rect oldRect,int fontSize)
        {
            var rect = oldRect;
            rect.width += fontSize * textBorder;
            rect.height += fontSize * textBorder;
            return rect;
        }
    }
}
