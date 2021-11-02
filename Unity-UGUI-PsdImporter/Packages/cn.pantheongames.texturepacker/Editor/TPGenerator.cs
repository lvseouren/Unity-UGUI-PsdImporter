using PantheonGames.TexturePacker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace PantheonGamesEditor.TexturePacker
{
    public static class TPGenerator
    {
        private const string kTexturePackerEditorKey = "TexturePackerPath";
        private const string kValidTexturePackerFileName = "TexturePacker.exe";
        private const string kTexturePackerMacPath = "/Applications/TexturePacker.app/Contents/MacOS/TexturePacker";

        public const int kAllowedMaxSize = 4096;
        public const string kTexturePackerDataExtension = ".tpsheet";
        public const string kTexturePackerSheetExtension = ".png";

        private static readonly string kTempTexturePath = Path.Combine(Directory.GetCurrentDirectory(), "TPCached");

        private static readonly TextureImporterSettings s_TextureSettings = new TextureImporterSettings();

        private static readonly Vector2[] s_Pivots = 
        {
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(0f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(1f, 0f),
        };

        private static string GetApplicationPath()
        {
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
                return kTexturePackerMacPath;
            var path = EditorPrefs.GetString(kTexturePackerEditorKey);
            if (string.IsNullOrEmpty(path) || !IsValidApplicationPath(path)) return string.Empty;
            return path;
        }

        private static bool IsValidApplicationPath(string path)
        {
            var fileName = Path.GetFileName(path);
            return fileName.Equals(kValidTexturePackerFileName) && File.Exists(path);
        }

        public static string PackTextures(SpriteAtlas spriteAtlas)
        {
            Dictionary<string, HashSet<string>> paths;
            if (spriteAtlas.Objects == null || spriteAtlas.Objects.Length == 0 ||
                (paths = TexturePackerUtility.GetSpritePaths(spriteAtlas)).Count == 0)
            {
                Debug.Log("There's no asset need to be Packed.");
                return null;
            }

            var applicationPath = GetApplicationPath();
            if (string.IsNullOrEmpty(applicationPath))
            {
                applicationPath = EditorUtility.OpenFilePanelWithFilters("Choose Texture Packer Application",
                    Application.dataPath,
                    new []
                        {kValidTexturePackerFileName, Path.GetExtension(kValidTexturePackerFileName).Substring(1)});
                if (string.IsNullOrEmpty(applicationPath) || !IsValidApplicationPath(applicationPath))
                    throw new Exception("Can not find Texture Packer Application");
                EditorPrefs.SetString(kTexturePackerEditorKey, applicationPath);
            }

            if (!TexturePackerUtility.IsPackable(spriteAtlas))
                throw new ArgumentException("Each of Objects for packing is not valid.");

            var collections = new Dictionary<string, HashSet<string>>();
            if (TexturePackerUtility.HaveSameNames(paths, collections))
            {
                var sb = new StringBuilder();
                foreach (var kv in collections)
                {
                    if (kv.Value.Count > 1)
                    {
                        sb.AppendLine(kv.Key);
                        foreach (var path in kv.Value)
                            sb.AppendLine(path);
                    }
                }
                throw new ArgumentException("Can not include same name: \n" + sb);
            }

            return PackTextures(spriteAtlas, paths, applicationPath);
        }

        private static void DisplayProgressBar(string content, float progress)
        {
            EditorUtility.DisplayProgressBar("Packing Textures", content, progress);
        }

        private static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }

        private static string GetDataPath(SpriteAtlas spriteAtlas)
        {
            var output = Path.GetDirectoryName(AssetDatabase.GetAssetPath(spriteAtlas));
            return Path.Combine(output, spriteAtlas.name + kTexturePackerDataExtension);
        }

        private static string GetSheetPath(SpriteAtlas spriteAtlas)
        {
            var output = Path.GetDirectoryName(AssetDatabase.GetAssetPath(spriteAtlas));
            return Path.Combine(output, spriteAtlas.name + kTexturePackerSheetExtension);
        }

        private static string PackTextures(SpriteAtlas spriteAtlas, Dictionary<string, HashSet<string>> paths, string applicationPath)
        {
            try
            {
                DisplayProgressBar("Prepare to pack textures...", 0);
                var dataPath = GetDataPath(spriteAtlas);
                var sheetPath = GetSheetPath(spriteAtlas);
                var spriteInfo = new Dictionary<string, SpriteMetaData>();
                RecordSpriteInfo(paths, spriteInfo, (content, index, count) => DisplayProgressBar(content, index / (float)count * 0.2f));

                var tempAtlasDataPath = Path.Combine(kTempTexturePath, spriteAtlas.name + kTexturePackerDataExtension);
                try
                {
                    if (Directory.Exists(kTempTexturePath)) Directory.Delete(kTempTexturePath, true);
                    Directory.CreateDirectory(kTempTexturePath);
                    
                    ClipToSourcePath(paths, (content, index, count) => DisplayProgressBar(content, index / (float)count * 0.2f + 0.2f));
                    // Before process TexturePacker, move data file to temp file to avoid modify sprite pivot or border incorrect.
                    if (File.Exists(dataPath)) File.Move(dataPath, tempAtlasDataPath);
                    DisplayProgressBar("Process TexturePacker...", 0.4f);
                    var arguments = CreateArguments(spriteAtlas, dataPath, sheetPath);
                    var result = ProcessCommand(applicationPath, arguments);
                    Debug.Log(result);
                }
                catch (Exception e)
                {
                    if (File.Exists(tempAtlasDataPath) && !File.Exists(dataPath))
                        File.Move(tempAtlasDataPath, dataPath);
                    throw e;
                }
                finally
                {
                    Directory.Delete(kTempTexturePath, true);
                }
                
                ApplySpriteInfo(sheetPath, spriteInfo, (content, index, count) => DisplayProgressBar(content, index / (float)count * 0.4f + 0.4f));
                DisplayProgressBar("Import Assets...", 0.8f);
                AssetDatabase.ImportAsset(sheetPath, ImportAssetOptions.ForceUpdate);
                // fix: Unity 2020.3.4f1 can not import sprites with npot scale.
                var importer = (TextureImporter) AssetImporter.GetAtPath(sheetPath);
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();
                AssetDatabase.ImportAsset(dataPath, ImportAssetOptions.ForceUpdate);
                DisplayProgressBar("Format Sprite Sheet...", 0.8f);
                spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(spriteAtlas.tag);
                FormatSprite(sheetPath, spriteAtlas.FormatMaxSize);
                InjectToSpriteAtlas(spriteAtlas, sheetPath, (content, index, count) => DisplayProgressBar(content, index / (float)count * 0.2f + 0.8f));
                return sheetPath;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                ClearProgressBar();
            }
        }

        private static void InjectToSpriteAtlas(SpriteAtlas spriteAtlas, string sheetPath, Action<string, int, int> callback = null)
        {
            var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheetPath);
            using var so = new SerializedObject(spriteAtlas);
            so.Update();
            var property = so.FindProperty("m_Sprites");
            property.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++)
            {
                callback?.Invoke("Injecting SpriteAtlas: " + sprites[i].name, i + 1, sprites.Length);
                property.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
            if (so.ApplyModifiedPropertiesWithoutUndo())
                AssetDatabase.SaveAssets();
        }

        private static void RecordSpriteInfo(Dictionary<string, HashSet<string>> paths, Dictionary<string, SpriteMetaData> spriteInfo, Action<string, int, int> callback = null)
        {
            var index = 0;
            spriteInfo.Clear();
            foreach (var kv in paths)
            {
                index++;
                callback?.Invoke("Recording Sprite Info: " + kv.Key, index, paths.Count);
                var importer = AssetImporter.GetAtPath(kv.Key) as TextureImporter;
                if (!importer) continue;
                if (importer.textureType == TextureImporterType.Sprite)
                {
                    if (importer.spriteImportMode == SpriteImportMode.Multiple)
                    {
                        var sheets = importer.spritesheet;
                        foreach (var sheet in sheets)
                        {
                            if (kv.Value.Count == 0 || kv.Value.Contains(sheet.name))
                                spriteInfo.Add(sheet.name, sheet);
                        }
                    }
                    else
                    {
                        var tex = AssetDatabase.LoadAssetAtPath<Texture>(kv.Key);
                        importer.ReadTextureSettings(s_TextureSettings);
                        spriteInfo.Add(tex.name, new SpriteMetaData
                        {
                            name = tex.name,
                            alignment = s_TextureSettings.spriteAlignment,
                            border = s_TextureSettings.spriteBorder,
                            pivot = s_TextureSettings.spritePivot,
                            rect = new Rect(0, 0, tex.width, tex.height)
                        });
                    }
                }
                else
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture>(kv.Key);
                    spriteInfo.Add(tex.name, new SpriteMetaData
                    {
                        name = tex.name,
                        alignment = (int)SpriteAlignment.Center,
                        border = Vector4.zero,
                        pivot = s_Pivots[0],
                        rect = new Rect(0, 0, tex.width, tex.height)
                    });
                }
            }
        }

        private static void ClipTexture(Texture2D texture, SpriteMetaData data)
        {
            var colors = texture.GetPixels((int)data.rect.x, (int)data.rect.y, (int)data.rect.width, (int)data.rect.height);
            var tex = new Texture2D((int)data.rect.width, (int)data.rect.height);
            tex.SetPixels(colors);
            var bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            using var fs = File.Create(Path.Combine(kTempTexturePath, data.name + ".png"));
            var bw = new BinaryWriter(fs);
            bw.Write(bytes);
            bw.Close();
        }

        private static void ClipToSourcePath(Dictionary<string, HashSet<string>> paths, Action<string, int, int> callback = null)
        {
            var index = 0;
            foreach (var kv in paths)
            {
                index++;
                var importer = AssetImporter.GetAtPath(kv.Key) as TextureImporter;
                if (!importer) continue;
                if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Multiple)
                {
                    var isReadable = importer.isReadable;
                    if (!isReadable)
                    {
                        callback?.Invoke("Reimporting Texture: " + kv.Key, index, paths.Count);
                        importer.isReadable = true;
                        importer.SaveAndReimport();
                    }

                    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(kv.Key);
                    var sheets = importer.spritesheet;
                    foreach (var sheet in sheets)
                    {
                        if (kv.Value.Count == 0 || kv.Value.Contains(sheet.name))
                        {
                            callback?.Invoke("Clipping Texture Sprite: " + sheet.name, index, paths.Count);
                            ClipTexture(texture, sheet);
                        }
                    }

                    if (importer.isReadable != isReadable)
                    {
                        callback?.Invoke("Reverting Texture: " + kv.Key, index, paths.Count);
                        importer.isReadable = isReadable;
                        importer.SaveAndReimport();
                    }
                }
                else
                {
                    callback?.Invoke("Coping Texture: " + kv.Key, index, paths.Count);
                    var absPath = Path.Combine(Directory.GetCurrentDirectory(), kv.Key);
                    var destPath = Path.Combine(kTempTexturePath, Path.GetFileName(kv.Key));
                    File.Copy(absPath, destPath);
                }
            }
        }

        private static string CreateArguments(SpriteAtlas spriteAtlas, string dataPath, string sheetPath)
        {
            // https://www.codeandweb.com/texturepacker/documentation/texture-settings
            var sb = new StringBuilder();
            sb.Append(kTempTexturePath);
            AddArgument(sb, "format", "unity-texture2d");
            AddArgument(sb, "data", Path.Combine(Directory.GetCurrentDirectory(), dataPath));
            AddArgument(sb, "sheet", Path.Combine(Directory.GetCurrentDirectory(), sheetPath));
            
            AddArgument(sb, "texture-format", "png");
            AddArgument(sb, "png-opt-level", 0);
            AddArgument(sb, "opt", "RGBA8888");
            AddArgument(sb, "alpha-handling", "ClearTransparentPixels");
            
            AddArgument(sb, "max-width", spriteAtlas.MaxSize.x);
            AddArgument(sb, "max-height", spriteAtlas.MaxSize.y);
            if (spriteAtlas.FixedSize.x > 0 && spriteAtlas.FixedSize.y > 0)
            {
                AddArgument(sb, "width", spriteAtlas.FixedSize.x);
                AddArgument(sb, "height", spriteAtlas.FixedSize.y);
            }
            AddArgument(sb, "size-constraints", Enum.GetName(typeof(SizeConstraints), spriteAtlas.SizeConstraints));
            if (spriteAtlas.ForceSquared) AddArgument(sb, "force-squared");
            
            AddArgument(sb, "algorithm", Enum.GetName(typeof(Algorithm), spriteAtlas.Algorithm));
            switch (spriteAtlas.Algorithm)
            {
                case Algorithm.Basic:
                    AddArgument(sb, "basic-sort-by", Enum.GetName(typeof(BasicSorting), spriteAtlas.Sorting));
                    AddArgument(sb, "basic-order", Enum.GetName(typeof(BasicSortingOrder), spriteAtlas.SortingOrder));
                    AddArgument(sb, "pack-mode", Enum.GetName(typeof(PackMode), spriteAtlas.PackMode));
                    break;
                case Algorithm.MaxRects:
                    AddArgument(sb, "maxrects-heuristics",
                        Enum.GetName(typeof(HeuristicsMode), spriteAtlas.HeuristicsMode));
                    AddArgument(sb, "pack-mode", Enum.GetName(typeof(PackMode), spriteAtlas.PackMode));
                    break;
                case Algorithm.Polygon:
                    AddArgument(sb, "align-to-grid", spriteAtlas.AlignToGrid);
                    break;
            }
            
            AddArgument(sb, "trim-mode", Enum.GetName(typeof(TrimMode), spriteAtlas.TrimMode));
            AddArgument(sb, "trim-margin", spriteAtlas.TrimMargin);
            if (spriteAtlas.TrimMode == TrimMode.Polygon)
                AddArgument(sb, "tracer-tolerance", spriteAtlas.TracerTolerance);
            else
                AddArgument(sb, "trim-threshold", spriteAtlas.AlphaThreshold);

            AddArgument(sb, "extrude", spriteAtlas.Extrude);
            AddArgument(sb, "border-padding", spriteAtlas.BorderPadding);
            AddArgument(sb, "shape-padding", spriteAtlas.ShapePadding);
            return sb.ToString();
        }

        private static void AddArgument(StringBuilder sb, string key)
        {
            sb.Append(" ");
            sb.Append("--");
            sb.Append(key);
        }

        private static void AddArgument<T>(StringBuilder sb, string key, T value)
        {
            AddArgument(sb, key);
            sb.Append(" ");
            sb.Append(value);
        }

        private static string ProcessCommand(string applicationPath, string arguments)
        {
            var encoding = Encoding.UTF8;
            if (Application.platform == RuntimePlatform.WindowsEditor)
                encoding = Encoding.GetEncoding("gb2312");
            var info = new ProcessStartInfo(applicationPath)
            {
                Arguments = arguments,
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = encoding,
                StandardErrorEncoding = encoding
            };
            
            var process = Process.Start(info);
            var error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error) && error.Contains("error"))
                throw new Exception(error);

            return process.StandardOutput.ReadToEnd();
        }

        private static void FormatSprite(string path, int maxSize)
        {
            if (TextureFormatUtility.SetPreferredPlatformSettings(path, (apply, revert) => {
                apply.maxTextureSize = Mathf.Min(maxSize, kAllowedMaxSize);
            })) AssetDatabase.Refresh();
        }

        private static void ApplySpriteInfo(string path, Dictionary<string, SpriteMetaData> spriteInfo, Action<string, int, int> callback = null)
        {
            var sb = new StringBuilder();
            var spriteLines = new List<string>();
            var tpPath = Path.ChangeExtension(path, ".tpsheet");
            using (var sr = new StreamReader(tpPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#", StringComparison.OrdinalIgnoreCase)
                        || line.StartsWith(":", StringComparison.OrdinalIgnoreCase)
                        || string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                    {
                        // Always enable import borders
                        if (line.StartsWith(":borders=", StringComparison.OrdinalIgnoreCase))
                            line = ":borders=enabled";
                        sb.AppendLine(line);
                    }
                    else spriteLines.Add(line);
                }
            }
            for (int i = 0; i < spriteLines.Count; i++)
            {
                string line = spriteLines[i];
                int nameIndex = line.IndexOf(';');
                string spriteName = line.Substring(0, nameIndex);
                callback?.Invoke("Apply Sprite Info: " + spriteName, i + 1, spriteLines.Count);
                if (spriteInfo.TryGetValue(spriteName, out SpriteMetaData metaData))
                {
                    string spriteData = line.Substring(nameIndex + 1);
                    string[] spriteDatas = spriteData.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                    string rectStr = spriteDatas[0];
                    string pivotStr = spriteDatas[1];
                    string borderStr = spriteDatas[2];

                    int[] rect = Array.ConvertAll(rectStr.Split(';'), Convert.ToInt32);
                    float[] pivot = Array.ConvertAll(pivotStr.Split(';'), Convert.ToSingle);
                    int[] border = Array.ConvertAll(borderStr.Split(';'), Convert.ToInt32);

                    var metaPivot = GetPivot(metaData);

                    var originLeft = metaData.rect.width * 0.5f;
                    var destLeft = rect[2] * pivot[0];
                    var cutWidth = originLeft - destLeft;
                    var pivotWidth = metaPivot.x * metaData.rect.width;
                    var destPivotX = (pivotWidth - cutWidth) / rect[2];

                    var originBottom = metaData.rect.height * 0.5f;
                    var destBottom = rect[3] * pivot[1];
                    var cutHeight = originBottom - destBottom;
                    var pivotHeight = metaPivot.y * metaData.rect.height;
                    var destPivotY = (pivotHeight - cutHeight) / rect[3];

                    pivot[0] = destPivotX;
                    pivot[1] = destPivotY;

                    border[0] = Mathf.RoundToInt(Mathf.Max(0, metaData.border.x - cutWidth));
                    border[1] = Mathf.RoundToInt(Mathf.Max(0, metaData.border.z - (metaData.rect.width - rect[2] - cutWidth)));
                    border[2] = Mathf.RoundToInt(Mathf.Max(0, metaData.border.w - (metaData.rect.height - rect[3] - cutHeight)));
                    border[3] = Mathf.RoundToInt(Mathf.Max(0, metaData.border.y - cutHeight));

                    spriteDatas[1] = string.Join(";", pivot);
                    spriteDatas[2] = string.Join(";", border);
                    spriteData = string.Join("; ", spriteDatas);
                    line = $"{spriteName};{spriteData}";
                }
                sb.AppendLine(line);
            }
            using (StreamWriter sw = new StreamWriter(tpPath))
                sw.Write(sb);
        }

        private static Vector2 GetPivot(SpriteMetaData metaData)
        {
            switch (metaData.alignment)
            {
                case (int)SpriteAlignment.Custom:
                    return metaData.pivot;
                default:
                    return s_Pivots[metaData.alignment];
            }
        }
    }
}
