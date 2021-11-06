using PantheonGames.TexturePacker;
using PantheonGamesEditor.TexturePacker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PG
{
    public static class UIModuleSpriteAtlas
    {
        public static void Run(string path, Action<string, float> callback)
        {
            callback?.Invoke("Find UI SpriteAtlas...", 0);
            var guids = AssetDatabase.FindAssets("t:PantheonGames.TexturePacker.SpriteAtlas", new[] { path });
            if (guids.Length > 1)
                throw new InvalidOperationException($"{path} contains {guids.Length} SpriteAtlas, this is not allowed.");
            SpriteAtlas spriteAtlas;
            if (guids.Length == 0)
            { 
                spriteAtlas = ScriptableObject.CreateInstance<SpriteAtlas>();
            }
            else
                spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guids[0]));

            FillSpriteAtlas(spriteAtlas, path);
            callback?.Invoke("Fill UI SpriteAtlas...", 0.5f);
            if (AssetDatabase.Contains(spriteAtlas))
                AssetDatabase.SaveAssets();
            else
            {
                spriteAtlas.SizeConstraints = SizeConstraints.POT;
                AssetDatabase.CreateAsset(spriteAtlas, Path.Combine(path, "SpriteAtlas.asset"));
            }
             
            callback?.Invoke("Packing UI SpriteAtlas...", 1);
            TPGenerator.PackTextures(spriteAtlas);
            AssetDatabase.Refresh();
            spriteAtlas.RefreshDict();
        }

        public static void ModifySpritePivot(string path)
        {
            var guids = AssetDatabase.FindAssets("t:texture", new[] { path });
            foreach (var guid in guids)
            {
                var filePath = AssetDatabase.GUIDToAssetPath(guid);
                if (filePath.Contains("Sprite"))
                    continue;
                try
                {
                    TextureFormatUtility.SetPreferredSpriteSettings(filePath, (settings, spriteSettings) =>
                    {
                        settings.spriteBorder = spriteSettings.spriteBorder;
                    }, importer => false);
                    TextureFormatUtility.SetPreferredPlatformSettings(filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            HashSet<Sprite> sprites = new HashSet<Sprite>();
            var folders = FindAtlasFolder(path, sprites);
            foreach(var sprite in sprites)
            {
                TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite.texture));
                TextureImporterSettings texSettings = new TextureImporterSettings();
                ti.ReadTextureSettings(texSettings);
                texSettings.spriteAlignment = (int)SpriteAlignment.Custom;
                if (texSettings.spritePivot.y != 0.4f)
                {
                    texSettings.spritePivot = new Vector2(0.5f, 0.4f);
                    ti.SetTextureSettings(texSettings);
                    ti.SaveAndReimport();
                }
                else
                    break;
            }
        }

        static void FillSpriteAtlas(SpriteAtlas spriteAtlas, string path)
        {
            HashSet<Sprite> sprites = new HashSet<Sprite>();
            var folders = FindAtlasFolder(path, sprites);
            HashSet<Object> objects = new HashSet<Object>();
            foreach (var folder in folders)
                objects.Add(AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder));
            Sprite[] refSprites = FindAllPrefabReferenceSprites(path);
            var externalSprites = refSprites.Where(sprite => !sprites.Contains(sprite));
            foreach (var sprite in externalSprites)            
                objects.Add(sprite);
            
            spriteAtlas.Objects = objects.ToArray();
        }

        public static string[] FindAtlasFolder(string path, HashSet<Sprite> sprites)
        {
            List<string> atlasFolders = new List<string>();
            foreach (var subPath in AssetDatabase.GetSubFolders(path))
            {
                var fileName = Path.GetFileName(subPath);
                if (fileName.StartsWith(UIModuleProcessor.kAtlasName))
                {
                    atlasFolders.Add(subPath);
                    var guids = AssetDatabase.FindAssets("t:Sprite", new[] { subPath });
                    foreach (var guid in guids)
                    {
                        var spritePath = AssetDatabase.GUIDToAssetPath(guid);
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                        sprites.Add(sprite);
                    }
                }
            }
            return atlasFolders.ToArray();
        }

        static Sprite[] FindAllPrefabReferenceSprites(string path)
        {
            List<Sprite> sprites = new List<Sprite>();
            var guids = AssetDatabase.FindAssets("t:prefab", new[] { path });
            foreach (var guid in guids)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                sprites.AddRange(FindPrefabReferenceSprites(prefabPath));
            }
            return sprites.ToArray();
        }

        static Sprite[] FindPrefabReferenceSprites(string path)
        {
            List<Sprite> sprites = new List<Sprite>();
            var dependencies = AssetDatabase.GetDependencies(path);
            foreach (var dependence in dependencies)
            {
                if (File.Exists(Path.ChangeExtension(dependence, ".tpsheet")))
                    continue;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(dependence);
                if (sprite) sprites.Add(sprite);
            }
            return sprites.ToArray();
        }
    }
}
