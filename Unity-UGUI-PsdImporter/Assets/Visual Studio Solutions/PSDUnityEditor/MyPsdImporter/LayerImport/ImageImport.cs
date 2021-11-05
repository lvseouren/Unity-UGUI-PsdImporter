using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Visual_Studio_Solutions.PSDUnityEditor.MyPsdImporter
{
    public class ImageImport:ImportBase
    {
        T InitComponent<T>(GameObject go)where T: Component
        {
            var pic = go.GetComponent<T>();
            if (pic == null)
            {
                pic = go.AddComponent<T>();
            }
            return pic;
        }
        public override void SetComponnets(GameObject go, PsdLayerNode node)
        {
            var image = node.image;

            Graphic pic;
            if (image.type == ImgType.Texture)
            {
                pic = InitComponent<RawImage>(go);
            }
            else
            {
                pic = InitComponent<Image>(go);
            }
            pic.raycastTarget = false;

            MyPsdImporterCtrl.SetPictureOrLoadColor(image, pic);
        }
    }
}
