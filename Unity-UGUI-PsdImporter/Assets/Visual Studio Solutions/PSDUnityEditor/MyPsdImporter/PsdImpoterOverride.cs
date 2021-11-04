using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ntreev.Library.Psd;
using UnityEditor.U2D.PSD;
using UnityEditor.AssetImporters;
using PSDUnity.Data;
using UnityEditor;
using PSDUnity;
using PSDUnity.Analysis;
using System;
using Assets.Visual_Studio_Solutions.PSDUnityEditor.MyPsdImporter;
using UnityEngine.UI;
using MogulTech.Utilities;

[ScriptedImporter(1, null, new[] { "psd" })]
public class PsdImpoterOverride : PSDImporter
{
    static IEnumerator PostProcessPsdImported(string moduleName, PsdLayerNode root)
    {
        yield return new WaitForEndOfFrame();
        Debug.Log(moduleName);
        AssetDatabase.Refresh(); 
        MyPsdImporterCtrl.Instance.ProcessAtlas(moduleName);
        MyPsdImporterCtrl.Instance.RefreshImageSprite(root);
        root.Draw(GetCanvas()?.transform);
    }

    const string exporterPath = "Assets/Test/expoter.asset";
    // Start is called before the first frame update
    public override void OnImportAsset(AssetImportContext ctx)
    {
        Debug.Log("holy shitt!");

        EditorCoroutines.Execute(ImportPSD(ctx.assetPath));
    }

    static IEnumerator ImportPSD(string assetPath)
    {
        yield return new WaitForEndOfFrame();
        var canvas = GetCanvas();

        MyPsdImporterCtrl.Instance.InitEnvironment(assetPath);
        MyPsdImporterCtrl.Instance.uiRoot = canvas.transform;
        var psd = PsdDocument.Create(assetPath);
        ExportUtility.InitPsdExportEnvrioment(null, new Vector2(psd.Width, psd.Height));
        var root = MyPsdImporterCtrl.Instance.PreParsePsdLayers(psd);
        //root.Draw(canvas.transform);
        EditorCoroutines.Execute(PostProcessPsdImported(MyPsdImporterCtrl.Instance.GetModuleName(), root));
    }

    static Canvas GetCanvas()
    {
        var canvasObj = Array.Find(Selection.objects, x => x is GameObject && (x as GameObject).GetComponent<Canvas>() != null);
        Canvas canvas = null;
        if (canvasObj)
            canvas = (canvasObj as GameObject).GetComponent<Canvas>();
        else
        {
            canvas = FindObjectOfType<Canvas>();
            if (!canvas)
                canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        }
        canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.referenceResolution = new Vector2(750, 1334);
        return canvas;
    }
}