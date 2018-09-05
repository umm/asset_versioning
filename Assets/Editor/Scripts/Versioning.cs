using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;
using UnityEditor;

namespace UnityModule.Asset
{
    public static class Versioning
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(UserData));

        [MenuItem("Assets/Versioning/Increment", priority = 300)]
        public static void Increment()
        {
            CollectAssetImporters()
                .ToList()
                .ForEach(
                    x =>
                    {
                        var userData = DeserializeUserData(x);
                        userData.Version++;
                        x.userData = SerializeUserData(userData);
                        x.SaveAndReimport();
                    }
                );
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Versioning/Decrement", priority = 301)]
        public static void Decrement()
        {
            CollectAssetImporters()
                .ToList()
                .ForEach(
                    x =>
                    {
                        var userData = DeserializeUserData(x);
                        if (userData.Version > 0)
                        {
                            userData.Version--;
                        }
                        x.userData = SerializeUserData(userData);
                        x.SaveAndReimport();
                    }
                );
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Versioning/Clear", priority = 302)]
        public static void Clear()
        {
            CollectAssetImporters()
                .ToList()
                .ForEach(
                    x =>
                    {
                        x.userData = string.Empty;
                        x.SaveAndReimport();
                    }
                );
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static UserData DeserializeUserData(AssetImporter assetImporter)
        {
            try
            {
                return (UserData) Serializer.Deserialize(assetImporter.userData.AsMemoryStream());
            }
            catch
            {
                return new UserData();
            }
        }

        private static string SerializeUserData(UserData userData)
        {
            string text;
            using (var stream = new MemoryStream())
            using (var xtw = new XmlTextWriter(stream, new UTF8Encoding(false)) {Formatting = Formatting.None})
            {
                Serializer.Serialize(xtw, userData);
                text = stream.AsString().StripXmlElementsAndAttributes();
            }

            return text;
        }

        private static IEnumerable<AssetImporter> CollectAssetImporters()
        {
            if (Selection.objects.Length == 0)
            {
                throw new InvalidOperationException("Please select target assets or directories.");
            }

            return Selection
                .objects
                // 選択済要素をパスに変換
                .Select(AssetDatabase.GetAssetPath)
                // ディレクトリのみに絞り込み
                .Where(AssetDatabase.IsValidFolder)
                // ディレクトリ以下の全アセットを検索
                .SelectMany(x => AssetDatabase.FindAssets("*", new[] {x}))
                // GUID が返ってくるので、パスに変換
                .Select(AssetDatabase.GUIDToAssetPath)
                // ディレクトリを除外
                .Where(x => !AssetDatabase.IsValidFolder(x))
                // 選択済要素のウチ、ディレクトリ以外の要素をリストに追加
                .Concat(
                    Selection
                        .objects
                        .Select(AssetDatabase.GetAssetPath)
                        .Where(x => !AssetDatabase.IsValidFolder(x))
                )
                .Select(AssetImporter.GetAtPath);
        }
    }

    [PublicAPI]
    internal static class MemoryStreamExtensions
    {
        internal static string AsString(this MemoryStream stream)
        {
            return stream.AsString(new UTF8Encoding(false));
        }

        internal static string AsString(this MemoryStream stream, Encoding encoding)
        {
            return encoding.GetString(stream.ToArray());
        }

        internal static MemoryStream AsMemoryStream(this string text)
        {
            return text.AsMemoryStream(new UTF8Encoding(false));
        }

        internal static MemoryStream AsMemoryStream(this string text, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(text));
        }

        internal static string StripXmlElementsAndAttributes(this string xmlText)
        {
            xmlText = Regex.Replace(xmlText, "<\\?xml[^?]+\\?>", string.Empty);
            xmlText = Regex.Replace(xmlText, " xmlns:(xsd|xsi)=\"[^\"]+\"", string.Empty);
            return xmlText;
        }
    }
}