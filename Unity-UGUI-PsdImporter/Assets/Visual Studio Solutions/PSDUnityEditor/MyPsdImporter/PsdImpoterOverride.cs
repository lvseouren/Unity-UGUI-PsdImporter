using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ntreev.Library.Psd;
using UnityEditor.U2D.PSD;
using UnityEditor.AssetImporters;
using UnityEditor;
using System;
using Assets.Visual_Studio_Solutions.PSDUnityEditor.MyPsdImporter;
using UnityEngine.UI;
using MogulTech.Utilities;

[ScriptedImporter(1, null, new[] { "psd" })]
public class PsdImpoterOverride : PSDImporter
{
    //class MyAllPostprocessor : AssetPostprocessor
    //{
    //    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    //    {
    //        foreach (string str in importedAssets)
    //        {
    //            Debug.Log("Reimported Asset: " + str);
    //        }
    //        foreach (string str in deletedAssets)
    //        {
    //            Debug.Log("Deleted Asset: " + str);
    //        }

    //        for (int i = 0; i < movedAssets.Length; i++)
    //        {
    //            Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
    //        }
    //    }
    //}

    static IEnumerator PostProcessPsdImported(string moduleName, PsdLayerNode root, string psdName)
    {
        yield return new WaitForEndOfFrame();
        Debug.Log(moduleName);
        AssetDatabase.Refresh(); 
        MyPsdImporterCtrl.Instance.FormatSprites(moduleName);
        MyPsdImporterCtrl.Instance.UpdateAtlas(moduleName);
        EditorCoroutines.Execute(FinalProcessPsdImported(moduleName, root, psdName));
    }

    static IEnumerator FinalProcessPsdImported(string moduleName, PsdLayerNode root, string psdName)
    {
        yield return new WaitForEndOfFrame();
        Debug.Log(moduleName);
        AssetDatabase.Refresh();
        MyPsdImporterCtrl.Instance.RefreshImageSprite(root);

        GameObject source = AssetDatabase.LoadAssetAtPath("Assets/AssetBundles/UI/Modules/Global/Prefabs/FullPanel.prefab", typeof(GameObject)) as GameObject;
        GameObject objSource = (GameObject)PrefabUtility.InstantiatePrefab(source);
        objSource.transform.SetParent(MyPsdImporterCtrl.Instance.uiRoot);
        objSource.transform.localPosition = Vector3.zero;
        objSource.transform.localScale = Vector3.one;

        Transform contentNode = objSource.transform.Find("content");
        root.Draw(contentNode);

        string variantAssetPath = MyPsdImporterCtrl.Instance.GetPrefabPath(psdName);
        PrefabUtility.SaveAsPrefabAsset(objSource, variantAssetPath);
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

        var psd = PsdDocument.Create(assetPath);
        MyPsdImporterCtrl.Instance.InitEnvironment(assetPath, new Vector2(psd.Width, psd.Height));

        MyPsdImporterCtrl.Instance.uiRoot = canvas.transform;
        var root = MyPsdImporterCtrl.Instance.PreParsePsdLayers(psd);
        //root.Draw(canvas.transform);
        EditorCoroutines.Execute(PostProcessPsdImported(MyPsdImporterCtrl.Instance.GetModuleName(), root, MyPsdImporterCtrl.Instance.GetPsdName()));
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