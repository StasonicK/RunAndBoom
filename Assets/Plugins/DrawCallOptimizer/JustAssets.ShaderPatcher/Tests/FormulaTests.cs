using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssert;
using JustAssets.ShaderPatcher.Parts;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace JustAssets.ShaderPatcher.Tests
{
    public class FormulaTests
    {
        [Test]
        public void TestLineBreaks()
        {
            var parsed = new ShaderParser(LoadResource("ShaderCode"));
            new ShaderPatcher(parsed, (prio, message) => Console.WriteLine($"[{prio}] {message}")).PatchInAtlasSupport();
        }

        private static string LoadResource(string fileName)
        {
            var folders = GetSubFolders("Assets");
            var folder = folders.First(x => x.EndsWith("Tests/Properties"));
            var assetGUIDs = AssetDatabase.FindAssets(fileName, new[] {folder});
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs.First());
            return AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath).text;
        }

        private static IEnumerable<string> GetSubFolders(string currentFolder)
        {
            yield return currentFolder;
            foreach (var subFolder in AssetDatabase.GetSubFolders(currentFolder))
            {
                foreach (var folder in GetSubFolders(subFolder))
                    yield return folder;
            }
        }

        [Test]
        public void TestComplex()
        {
            var formula = CGCommandParser.Parse(new StringSpan(LoadResource("ComplexFormula")), 0, "\r\n", new List<string>()).First();

            var parts = formula.Cast<ICollectionPart>().Parts;
            parts.Count.ShouldBeEqualTo(2);
            parts[0].ShouldBeOfType<MethodHeader>();
        }

        [Test]
        public void TestDoubleBracket()
        {
            var formula = CGCommandParser.Parse(new StringSpan("(( temp_output_132_0_g47 + 2.0 )).xx;"), 0, "\r\n",new List<string>()).First();

            var parts = formula.Cast<ICollectionPart>().Parts;
            parts.Count.ShouldBeEqualTo(4);
            parts[0].ShouldBeOfType<Brackets>().As<Brackets>().Parts[0].ShouldBeOfType<Brackets>()
                .As<Brackets>().Parts.Count.ShouldBeEqualTo(3);
        }

        [Test]
        public void TestStructWithMacros()
        {
            var macroWords = new List<string> {"INTERNAL_DATA"};
            
            var content = CGCommandParser.Parse(new StringSpan(LoadResource("StructBody")), 0, "\r\n", macroWords).FirstOrDefault();

            var commands = content.Cast<Struct>().Parameters;
            commands.Count.ShouldBeEqualTo(6);
            commands.ForEach(x=> x.ShouldBeOfType<IParameter>());
        }


        [Test]
        public void TestIndenting()
        {
            ICollection<string> macroWords = new List<string>();
            var content = CGCommandParser.Parse(new StringSpan(LoadResource("Indenting")), 1, "\r\n", macroWords).ToList();
         
            //content.Parts.Count.ShouldBeEqualTo(1);
            //content.Parts[0].ShouldBeOfType<Content>();


        }
    }

    public static class Extensions
    {
        public static T As<T>(this object that) where T : class
        {
            return that as T;
        }
    }
}