using PantheonGames.TexturePacker;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PantheonGamesEditor.TexturePacker
{
    [CustomEditor(typeof(SpriteAtlas))]
    public sealed class SpriteAtlasEditor : Editor
    {
        private const string kXIcon = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABoSURBVDhPnY3BDcAgDAOZhS14dP1O0x2C/LBEgiNSHvfwyZabmV0jZRUpq2zi6f0DJwdcQOEdwwDLypF0zHLMa9+NQRxkQ+ACOT2STVw/q8eY1346ZlE54sYAhVhSDrjwFymrSFnD2gTZpls2OvFUHAAAAABJRU5ErkJggg==";

        private static Texture2D s_XIcon;

        private static GUIContent[] s_MaxSizesContent;

        private SerializedProperty m_Tag;
        private SerializedProperty m_Sprites;
        
        private SerializedProperty m_MaxSize;
        private SerializedProperty m_FixedSize;
        private SerializedProperty m_SizeConstraints;
        private SerializedProperty m_ForceSquared;
        
        private SerializedProperty m_Algorithm;
        private SerializedProperty m_Sorting;
        private SerializedProperty m_SortingOrder;
        private SerializedProperty m_HeuristicsMode;
        private SerializedProperty m_PackMode;
        private SerializedProperty m_AlignToGrid;
        
        private SerializedProperty m_TrimMode;
        private SerializedProperty m_TrimMargin;
        private SerializedProperty m_AlphaThreshold;
        private SerializedProperty m_TracerTolerance;
        
        private SerializedProperty m_Extrude;
        private SerializedProperty m_BorderPadding;
        private SerializedProperty m_ShapePadding;
        
        private SerializedProperty m_FormatMaxSize;
        private SerializedProperty m_Objects;

        private ReorderableList m_ObjectReorderableList;
        private ReorderableList m_SpritesReorderableList;
        private bool m_PackedSpriteFoldout;

        private bool m_Buildable = true;
        private readonly List<bool> m_CachedPackables = new List<bool>();
        private readonly Dictionary<string, HashSet<string>> m_CachedPaths = new Dictionary<string, HashSet<string>>();
        private readonly StringBuilder m_StringBuilder = new StringBuilder();

        private static void InitXIcon()
        {
            if (!s_XIcon) s_XIcon = Base64ToTexture(kXIcon);
        }

        private static void InitMaxSizeContent()
        {
            if (s_MaxSizesContent != null) return;
            s_MaxSizesContent = new GUIContent[SpriteAtlas.validMaxSizes.Length];
            for (int i = 0; i < SpriteAtlas.validMaxSizes.Length; i++)
                s_MaxSizesContent[i] = new GUIContent(SpriteAtlas.validMaxSizes[i].ToString());
        }

        private void OnEnable()
        {
            InitXIcon();
            InitMaxSizeContent();
            m_Tag = serializedObject.FindProperty("m_Tag");
            m_Sprites = serializedObject.FindProperty("m_Sprites");
            
            m_MaxSize = serializedObject.FindProperty("m_MaxSize");
            m_FixedSize = serializedObject.FindProperty("m_FixedSize");
            m_SizeConstraints = serializedObject.FindProperty("m_SizeConstraints");
            m_ForceSquared = serializedObject.FindProperty("m_ForceSquared");
            
            m_Algorithm = serializedObject.FindProperty("m_Algorithm");
            m_Sorting = serializedObject.FindProperty("m_Sorting");
            m_SortingOrder = serializedObject.FindProperty("m_SortingOrder");
            m_HeuristicsMode = serializedObject.FindProperty("m_HeuristicsMode");
            m_PackMode = serializedObject.FindProperty("m_PackMode");
            m_AlignToGrid = serializedObject.FindProperty("m_AlignToGrid");
            
            m_TrimMode = serializedObject.FindProperty("m_TrimMode");
            m_TrimMargin = serializedObject.FindProperty("m_TrimMargin");
            m_AlphaThreshold = serializedObject.FindProperty("m_AlphaThreshold");
            m_TracerTolerance = serializedObject.FindProperty("m_TracerTolerance");
            
            m_Extrude = serializedObject.FindProperty("m_Extrude");
            m_BorderPadding = serializedObject.FindProperty("m_BorderPadding");
            m_ShapePadding = serializedObject.FindProperty("m_ShapePadding");
            
            m_FormatMaxSize = serializedObject.FindProperty("m_FormatMaxSize");
            m_Objects = serializedObject.FindProperty("m_Objects");

            m_ObjectReorderableList = new ReorderableList(serializedObject, m_Objects, true, true, true, true)
            {
                elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 3,
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Objects for Packing"),
                drawElementCallback = DrawElement,
                onRemoveCallback = OnRemoveElement
            };

            m_SpritesReorderableList = new ReorderableList(serializedObject, m_Sprites, true, true, true, true)
            {
                elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 3,
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, m_Sprites.displayName),
                drawElementCallback = DrawSpriteElement
            };

            CheckSpritePaths();
        }

        private void OnRemoveElement(ReorderableList list)
        {
            m_Objects.DeleteArrayElementAtIndex(list.index);
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var property = m_Objects.GetArrayElementAtIndex(index);
            var isValid = m_CachedPackables[index];
            var position = new Rect(rect.x, rect.y + EditorGUIUtility.standardVerticalSpacing, rect.width, EditorGUIUtility.singleLineHeight);
            if (!isValid) position.width -= EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property);
            if (!isValid) GUI.DrawTexture(new Rect(position.x + position.width, position.y, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), s_XIcon);
        }

        private static Texture2D Base64ToTexture(string base64)
        {
            var t = new Texture2D(1, 1) {hideFlags = HideFlags.HideAndDontSave};
            t.LoadImage(Convert.FromBase64String(base64));
            return t;
        }

        private void CheckSpritePaths()
        {
            m_CachedPackables.Clear();
            m_CachedPaths.Clear();
            for (var i = 0; i < m_Objects.arraySize; i++)
            {
                var asset = m_Objects.GetArrayElementAtIndex(i).objectReferenceValue;
                var packable = TexturePackerUtility.IsPackable(asset) && !ContainsAsset(asset, i);
                m_CachedPackables.Add(packable);
                if (!packable) continue;
                TexturePackerUtility.CalculateSpriteNames(asset, m_CachedPaths);
            }

            m_Buildable = true;
            foreach (var packable in m_CachedPackables)
            {
                m_Buildable = packable;
                if (!m_Buildable) break;
            }

            m_StringBuilder.Length = 0;
            foreach (var kv in m_CachedPaths)
            {
                if (kv.Value.Count > 1)
                {
                    m_StringBuilder.AppendLine(kv.Key);
                    foreach (var path in kv.Value)
                        m_StringBuilder.AppendLine(path);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(m_Tag);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_MaxSize);
            EditorGUILayout.PropertyField(m_FixedSize);
            EditorGUILayout.PropertyField(m_SizeConstraints);
            EditorGUILayout.PropertyField(m_ForceSquared);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_Algorithm);
            if (m_Algorithm.enumValueIndex ==
                Array.IndexOf(m_Algorithm.enumNames, Enum.GetName(typeof(Algorithm), Algorithm.Basic)))
            {
                EditorGUILayout.PropertyField(m_Sorting);
                EditorGUILayout.PropertyField(m_SortingOrder);
                EditorGUILayout.PropertyField(m_PackMode);
            }
            else if (m_Algorithm.enumValueIndex == Array.IndexOf(m_Algorithm.enumNames,
                Enum.GetName(typeof(Algorithm), Algorithm.MaxRects)))
            {
                EditorGUILayout.PropertyField(m_HeuristicsMode);
                EditorGUILayout.PropertyField(m_PackMode);
            }
            else
            {
                EditorGUILayout.PropertyField(m_AlignToGrid);
            }
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_TrimMode);
            EditorGUILayout.PropertyField(m_TrimMargin);
            if (m_TrimMode.enumValueIndex !=
                Array.IndexOf(m_TrimMode.enumNames, Enum.GetName(typeof(TrimMode), TrimMode.Polygon)))
                EditorGUILayout.PropertyField(m_AlphaThreshold);
            else
                EditorGUILayout.PropertyField(m_TracerTolerance);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_Extrude);
            EditorGUILayout.PropertyField(m_BorderPadding);
            EditorGUILayout.PropertyField(m_ShapePadding);

            EditorGUILayout.Separator();
            EditorGUILayout.IntPopup(m_FormatMaxSize, s_MaxSizesContent, SpriteAtlas.validMaxSizes);
            
            EditorGUI.BeginChangeCheck();
            m_ObjectReorderableList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
                CheckSpritePaths();
                
            if (!m_Buildable)
                EditorGUILayout.HelpBox("Each of Objects for packing is not valid.", MessageType.Error);
            else if (m_StringBuilder.Length > 0)
                EditorGUILayout.HelpBox("Can not include same name: \n" + m_StringBuilder, MessageType.Error);
            else if (GUILayout.Button("Generate Texture"))
                TPGenerator.PackTextures(target as SpriteAtlas);

            m_PackedSpriteFoldout = EditorGUILayout.Foldout(m_PackedSpriteFoldout, $"Packed Sprites ({m_Sprites.arraySize})");
            if (m_PackedSpriteFoldout)
            {
                using (new EditorGUI.DisabledScope(true))
                    m_SpritesReorderableList.DoLayoutList();
            }

            CheckDragInfo();
            if (serializedObject.ApplyModifiedProperties())
                AssetDatabase.SaveAssets();
        }

        private bool ContainsAsset(Object asset, int index = -1)
        {
            if (!asset) return false;
            if (index < 0) index = m_Objects.arraySize;
            for (int i = 0; i < index; i++)
            {
                var property = m_Objects.GetArrayElementAtIndex(i);
                if (property.objectReferenceValue == asset) return true;
            }
            return false;
        }

        private void CheckDragInfo()
        {
            var evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                    var visualMode = DragAndDropVisualMode.Rejected;
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        var asset = DragAndDrop.objectReferences[i];
                        if (TexturePackerUtility.IsPackable(asset))
                        {
                            visualMode = DragAndDropVisualMode.Copy;
                            evt.Use();
                            break;
                        }
                    }
                    DragAndDrop.visualMode = visualMode;
                    break;
                case EventType.DragPerform:
                    bool isDirty = false;
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        var asset = DragAndDrop.objectReferences[i];
                        if (TexturePackerUtility.IsPackable(asset) && !ContainsAsset(asset))
                        {
                            m_Objects.arraySize += 1;
                            m_Objects.GetArrayElementAtIndex(m_Objects.arraySize - 1).objectReferenceValue = asset;
                            isDirty = true;
                        }
                    }
                    if (isDirty) CheckSpritePaths();
                    evt.Use();
                    break;
            }
        }

        private void DrawSpriteElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty property = m_Sprites.GetArrayElementAtIndex(index);
            var asset = property.objectReferenceValue;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.standardVerticalSpacing, rect.width, EditorGUIUtility.singleLineHeight), property, EditorGUIUtility.TrTempContent(asset != null ? asset.name : "Missing"));
        }
    }
}
