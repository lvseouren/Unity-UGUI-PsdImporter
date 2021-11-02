using PantheonGames.TexturePacker;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace PantheonGamesEditor.TexturePacker
{
    public class TexturePackerImporter : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            List<string> addedAssets = new List<string>(importedAssets);
            addedAssets.AddRange(movedAssets);
            if (addedAssets.Count > 0)
            {
                foreach (var path in addedAssets)
                {
                    if (Path.GetExtension(path).Equals(".asset", StringComparison.OrdinalIgnoreCase))
                    {
                        SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                        if (spriteAtlas != null)
                        {
                            using (var so = new SerializedObject(spriteAtlas))
                            {
                                so.Update();
                                var tag = so.FindProperty("m_Tag");
                                tag.stringValue = path;
                                so.ApplyModifiedPropertiesWithoutUndo();
                            }
                        }
                    }
                }
            }
        }
    }
}
