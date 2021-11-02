﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

using PSDUnity;
namespace PSDUnity.UGUI
{
    public abstract class ImageImport: Import
    {
        public abstract UGUINode DrawImage(Data.ImgNode image, UGUINode parent);
        public abstract void DrawImage(Data.ImgNode image, GameObject go);

        public abstract void InitTemplate(GameObject go);
    }
}
