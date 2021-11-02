using System;
using UnityEngine;
using UnityEngineInternal;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using PSDUnity;

namespace PSDUnity.UGUI
{
    public class ImageRawImageImport : ImageImport
    {
        public override GameObject CreateTemplate()
        {
            return new GameObject("Image", typeof(RectTransform));
        }

        public override void InitTemplate(GameObject go)
        {
           
        }

        public override UGUINode DrawImage(Data.ImgNode image, UGUINode parent)
        {
            if (image.type == ImgType.Texture)
            {
                return DrawRawImage(image, parent);
            }
            else
            {
                return DrawSpriteImage(image, parent);
            }
        }

        public UGUINode DrawSpriteImage(Data.ImgNode image, UGUINode parent)
        {
            UGUINode node = CreateRootNode(image.Name, image.rect, parent);
            UnityEngine.UI.Image pic = node.InitComponent<UnityEngine.UI.Image>();
            PSDImporterUtility.SetPictureOrLoadColor(image, pic);
            return node;
        }

        public UGUINode DrawRawImage(Data.ImgNode image, UGUINode parent)
        {
            UGUINode node = CreateRootNode(image.Name, image.rect, parent);
            UnityEngine.UI.RawImage pic = node.InitComponent<UnityEngine.UI.RawImage>();
            PSDImporterUtility.SetPictureOrLoadColor(image, pic);
            return node;
        }

        public override void DrawImage(Data.ImgNode image, GameObject go)
        {
            if (image.type == ImgType.Texture)
            {
                DrawRawImage(image, go);
            }
            else
            {
                DrawSpriteImage(image, go);
            }
        }

        public void DrawRawImage(Data.ImgNode image, GameObject go)
        {
            var pic = go.GetComponent<RawImage>();
            if(!pic)
            {
                pic = go.AddComponent<RawImage>();
            }
            PSDImporterUtility.SetPictureOrLoadColor(image, pic);
        }

        public void DrawSpriteImage(Data.ImgNode image, GameObject go)
        {
            var pic = go.GetComponent<Image>();
            if (!pic)
            {
                pic = go.AddComponent<Image>();
            }
            PSDImporterUtility.SetPictureOrLoadColor(image, pic);
        }

    }

}
