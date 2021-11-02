﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using PSDUnity.Data;

namespace PSDUnity.UGUI
{
    [CustomLayer(typeof(ScrollBarLayerImport))]
    public class ScrollbarLayerImportGUI : UGUI.LayerImportGUI
    {
        public override Texture Icon
        {
            get
            {
                return EditorGUIUtility.IconContent("Scrollbar Icon").image;
            }
        }
        public override void HeadGUI(Rect dirRect, GroupNode item)
        {
            base.HeadGUI(dirRect, item);
            item.direction = ((Direction)EditorGUI.EnumPopup(dirRect, item.direction));
        }
    }
}
