using System;
using System.Collections.Generic;
using UnityEngine;

namespace PantheonGames.TexturePacker
{
    public static class SpriteAtlasManager
    {
        struct SpriteLoadInfo
        {
            public string name;
            public Action<Sprite> callback;
        }

        public static event Func<string, SpriteAtlas> atlasRequested;
        public static event Action<string, Action<SpriteAtlas>> atlasAsyncRequested;
        public static event Action<SpriteAtlas> atlasReleased;
        public static event Action<SpriteAtlas> atlasRegistered;

        private static readonly Dictionary<string, List<SpriteLoadInfo>> s_LoadingSpriteAtlasTag = new Dictionary<string, List<SpriteLoadInfo>>();
        private static readonly Dictionary<string, KeyValuePair<SpriteAtlas, int>> s_CachedSpriteAtlas = new Dictionary<string, KeyValuePair<SpriteAtlas, int>>();
        private static Action<SpriteAtlas> s_SpriteAtlasLoaded = OnSpriteAtlasLoaded;

        private static readonly Dictionary<Sprite, KeyValuePair<string, int>> s_SpriteToAtlasMap = new Dictionary<Sprite, KeyValuePair<string, int>>();

        static void RecordSprite(Sprite sprite, string tag)
        {
            if (sprite == null)
                return;

            if (string.IsNullOrEmpty(tag))
                return;

            KeyValuePair<SpriteAtlas, int> info;
            if (!s_CachedSpriteAtlas.TryGetValue(tag, out info))
                return;
            s_CachedSpriteAtlas[tag] = new KeyValuePair<SpriteAtlas, int>(info.Key, info.Value + 1);
            KeyValuePair<string, int> reference;
            if (!s_SpriteToAtlasMap.TryGetValue(sprite, out reference))
                s_SpriteToAtlasMap.Add(sprite, new KeyValuePair<string, int>(tag, 1));
            else
                s_SpriteToAtlasMap[sprite] = new KeyValuePair<string, int>(tag, reference.Value + 1);
        }

        public static string GetSpriteTag(Sprite sprite)
        {
            if (sprite == null)
                return null;

            KeyValuePair<string, int> info;
            if (s_SpriteToAtlasMap.TryGetValue(sprite, out info))
                return info.Key;
            return null;
        }

        public static Sprite LoadSprite(string tag, string name)
        {
            if (string.IsNullOrEmpty(tag))
            {
                Debug.LogWarningFormat("SpriteAtlasManager.LoadSprite - tag is not valid.");
                return null;
            }

            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarningFormat("SpriteAtlasManager.LoadSprite - name is not valid.");
                return null;
            }
                
            KeyValuePair<SpriteAtlas, int> info;
            if (!s_CachedSpriteAtlas.TryGetValue(tag, out info))
            {
                var spriteAtlas = atlasRequested?.Invoke(tag);
                if (spriteAtlas != null)
                    s_CachedSpriteAtlas.Add(tag, info = new KeyValuePair<SpriteAtlas, int>(spriteAtlas, 0));
            }
            var sprite = info.Key?.GetSprite(name);
            RecordSprite(sprite, tag);
            return sprite;
        }

        public static void LoadSpriteAsync(string tag, string name, Action<Sprite> callback)
        {
            if (string.IsNullOrEmpty(tag))
            {
                Debug.LogWarningFormat("SpriteAtlasManager.LoadSpriteAsync - tag is not valid.");
                callback?.Invoke(null);
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarningFormat("SpriteAtlasManager.LoadSpriteAsync - name is not valid.");
                callback?.Invoke(null);
                return;
            }

            if (s_CachedSpriteAtlas.TryGetValue(tag, out KeyValuePair<SpriteAtlas, int> info))
            {
                var sprite = info.Key.GetSprite(name);
                RecordSprite(sprite, tag);
                callback?.Invoke(sprite);
            }
            else if (atlasAsyncRequested != null)
            {
                List<SpriteLoadInfo> loadInfos;
                if (!s_LoadingSpriteAtlasTag.TryGetValue(tag, out loadInfos))
                    s_LoadingSpriteAtlasTag.Add(tag, loadInfos = new List<SpriteLoadInfo>(2));
                loadInfos.Add(new SpriteLoadInfo { name = name, callback = callback });
                if (loadInfos.Count == 1)
                    atlasAsyncRequested.Invoke(tag, s_SpriteAtlasLoaded);
            }
            else
            {
                Debug.LogWarningFormat("SpriteAtlasManager.LoadSpriteAsync - Please add atlasAsyncRequested first.");
                callback?.Invoke(null);
            }
        }

        public static void ReleaseSprite(Sprite sprite)
        {
            if (sprite == null)
                return;

            KeyValuePair<string, int> info;
            if (!s_SpriteToAtlasMap.TryGetValue(sprite, out info))
                return;
            if (info.Value <= 1)
                s_SpriteToAtlasMap.Remove(sprite);
            else
                s_SpriteToAtlasMap[sprite] = new KeyValuePair<string, int>(info.Key, info.Value - 1);

            KeyValuePair<SpriteAtlas, int> keyValue;
            if (!s_CachedSpriteAtlas.TryGetValue(info.Key, out keyValue))
                return;
            if (keyValue.Value <= 1)
            {
                s_CachedSpriteAtlas.Remove(info.Key);
                atlasReleased?.Invoke(keyValue.Key);
            }
            else
                s_CachedSpriteAtlas[info.Key] = new KeyValuePair<SpriteAtlas, int>(keyValue.Key, keyValue.Value - 1);
        }

        private static void OnSpriteAtlasLoaded(SpriteAtlas spriteAtlas)
        {
            if (spriteAtlas == null) return;
            if (!s_CachedSpriteAtlas.ContainsKey(spriteAtlas.tag))
                s_CachedSpriteAtlas.Add(spriteAtlas.tag, new KeyValuePair<SpriteAtlas, int>(spriteAtlas, 0));
            if (s_LoadingSpriteAtlasTag.TryGetValue(spriteAtlas.tag, out List<SpriteLoadInfo> loadInfos))
            {
                for (int i = 0; i < loadInfos.Count; i++)
                {
                    var loadInfo = loadInfos[i];
                    try
                    {
                        var sprite = spriteAtlas.GetSprite(loadInfo.name);
                        RecordSprite(sprite, spriteAtlas.tag);
                        loadInfo.callback?.Invoke(sprite);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
                loadInfos.Clear();
            }
            atlasRegistered?.Invoke(spriteAtlas);
        }
    }
}
