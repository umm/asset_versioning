using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UnityModule.Asset
{
    public class VersioningTest
    {
        private const string FixtureAssetPath = "Assets/fixture.txt";

        [SetUp]
        public void SetUp()
        {
            var ta = new TextAsset("fixture");
            AssetDatabase.CreateAsset(ta, FixtureAssetPath);
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(FixtureAssetPath);
            AssetDatabase.Refresh();
        }

        [Test]
        public void IncrementTest()
        {
            Selection.objects = new[] {AssetDatabase.LoadAssetAtPath<Object>(FixtureAssetPath)};

            {
                var ai = AssetImporter.GetAtPath(FixtureAssetPath);
                Assert.AreEqual(string.Empty, ai.userData);
            }

            Versioning.Increment();

            {
                var ai = AssetImporter.GetAtPath(FixtureAssetPath);
                Assert.True(Regex.IsMatch(ai.userData, "<version>1</version>"));
            }

            Versioning.Increment();
            Versioning.Increment();
            Versioning.Increment();

            {
                var ai = AssetImporter.GetAtPath(FixtureAssetPath);
                Assert.True(Regex.IsMatch(ai.userData, "<version>4</version>"));
            }
        }

        [Test]
        public void DecrementTest()
        {
            Selection.objects = new[] {AssetDatabase.LoadAssetAtPath<Object>(FixtureAssetPath)};

            {
                var ai = AssetImporter.GetAtPath(FixtureAssetPath);
                Assert.AreEqual(string.Empty, ai.userData);
            }

            Versioning.Increment();
            Versioning.Increment();
            Versioning.Increment();
            Versioning.Decrement();

            {
                var ai = AssetImporter.GetAtPath(FixtureAssetPath);
                Assert.True(Regex.IsMatch(ai.userData, "<version>2</version>"));
            }

            Versioning.Decrement();
            Versioning.Decrement();

            {
                var ai = AssetImporter.GetAtPath(FixtureAssetPath);
                Assert.True(Regex.IsMatch(ai.userData, "<version>0</version>"));
            }

            Versioning.Decrement();

            {
                var ai = AssetImporter.GetAtPath(FixtureAssetPath);
                Assert.True(Regex.IsMatch(ai.userData, "<version>0</version>"));
            }
        }

        [Test]
        public void ClearTest()
        {
            Selection.objects = new[] {AssetDatabase.LoadAssetAtPath<Object>(FixtureAssetPath)};

            {
                var ai = AssetImporter.GetAtPath(FixtureAssetPath);
                Assert.AreEqual(string.Empty, ai.userData);
            }

            Versioning.Increment();
            Versioning.Clear();

            {
                var ai = AssetImporter.GetAtPath(FixtureAssetPath);
                Assert.AreEqual(string.Empty, ai.userData);
            }
        }
    }
}