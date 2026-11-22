using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AchievementHelper.UI;

internal static class IconLoader
{
    private static readonly Dictionary<string, Sprite?> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly MethodInfo? LoadImageMethod = AccessTools.Method(
        "UnityEngine.ImageConversion:LoadImage",
        new[] { typeof(Texture2D), typeof(byte[]) });

    public static Sprite? UndefeatedSprite => GetSprite("crossed-swords.png");
    public static Sprite? TrophySprite => GetSprite("trophy-cup.png");

    private static Sprite? GetSprite(string fileName)
    {
        if (Cache.TryGetValue(fileName, out Sprite? cached))
        {
            return cached;
        }

        Sprite? sprite = LoadEmbeddedSprite(fileName);
        Cache[fileName] = sprite;
        return sprite;
    }

    private static Sprite? LoadEmbeddedSprite(string fileName)
    {
        try
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string? resourceName = null;
            foreach (string name in assembly.GetManifestResourceNames())
            {
                if (name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = name;
                    break;
                }
            }

            if (resourceName == null)
            {
                Plugin.Log.LogWarning($"Embedded icon resource not found: {fileName}");
                return null;
            }

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return null;
            }

            byte[] data = new byte[stream.Length];
            int offset = 0;
            while (offset < data.Length)
            {
                int read = stream.Read(data, offset, data.Length - offset);
                if (read <= 0)
                {
                    break;
                }
                offset += read;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            bool loaded = LoadImageMethod != null &&
                          LoadImageMethod.Invoke(null, new object[] { texture, data }) is true;
            if (!loaded)
            {
                UnityEngine.Object.Destroy(texture);
                Plugin.Log.LogWarning($"Failed to decode PNG icon: {fileName}");
                return null;
            }

            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Error loading embedded icon {fileName}: {ex.Message}");
            return null;
        }
    }
}
