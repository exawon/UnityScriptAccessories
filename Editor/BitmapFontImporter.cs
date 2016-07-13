using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Collections.Generic;

#if BITMAP_FONT_ZIP
// https://dotnetzip.codeplex.com/
using Ionic.Zip;
#endif

public class BitmapFontImporter : AssetPostprocessor
{
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets)
		{
			if (asset.EndsWith(".fnt"))
			{
				DoImport(asset, IsLtrExists(asset));
			}
			#if BITMAP_FONT_ZIP
			else if (asset.EndsWith(".zip") && IsLtrExists(asset))
			{
				DoUnzip(asset);
			}
			#endif
		}
	}

	static bool IsLtrExists(string path)
	{
		// http://kvazars.com/littera/
		var ltrPath = Path.ChangeExtension(path, ".ltr");
		return (AssetDatabase.LoadMainAssetAtPath(ltrPath) != null);
	}

	#if BITMAP_FONT_ZIP
	static void DoUnzip(string zipPath)
	{
		var fullPath = Application.dataPath + zipPath.Substring(6);
		using (ZipFile zip = ZipFile.Read(fullPath))
		{
			var dirName = Path.GetDirectoryName(fullPath);
			zip.ExtractAll(dirName, ExtractExistingFileAction.OverwriteSilently);
		}

		AssetDatabase.DeleteAsset(zipPath);
		AssetDatabase.Refresh();
	}
	#endif

	static void DoImport(string xmlPath, bool removeSourceAsset)
	{
		var fnt = AssetDatabase.LoadMainAssetAtPath(xmlPath) as TextAsset;
		var xml = new XmlDocument();
		xml.LoadXml(fnt.text);
		var info = xml.GetNode("info");
		var common = xml.GetNode("common");

		var directory = Path.GetDirectoryName(xmlPath);
		var fontPath = string.Format("{0}/{1}.fontsettings", directory, info.GetString("face"));
		var font = AssetDatabase.LoadMainAssetAtPath(fontPath) as Font;
		if (font == null)
		{
			font = new Font();
			AssetDatabase.CreateAsset(font, fontPath);

			font.material = new Material(Shader.Find("GUI/Text Shader"));
			font.material.name = "Font Material";
			AssetDatabase.AddObjectToAsset(font.material, font);
		}

		var so = new SerializedObject(font);
		so.FindProperty("m_FontSize").floatValue = info.GetFloat("size");
		so.FindProperty("m_LineSpacing").floatValue = common.GetFloat("lineHeight");
		so.ApplyModifiedProperties();

		var page = xml.GetNode("pages").FirstChild;
		var texPath = string.Format("{0}/{1}", directory, page.GetString("file"));
		var texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
		texImporter.textureType = TextureImporterType.Sprite;
		texImporter.compressionQuality = 100;
		texImporter.mipmapEnabled = false;
		texImporter.SaveAndReimport();

		var texture = AssetDatabase.LoadMainAssetAtPath(texPath) as Texture2D;
		if (texture == null)
		{
			Debug.LogErrorFormat(fnt, "{0}: not found '{1}'.", typeof(BitmapFontImporter), texPath);
			return;
		}

		Object.DestroyImmediate(font.material.mainTexture, true);
		font.material.mainTexture = Object.Instantiate<Texture2D>(texture);
		font.material.mainTexture.name = "Font Texture";
		AssetDatabase.AddObjectToAsset(font.material.mainTexture, font);

		int sw = common.GetInt("scaleW");
		int sh = common.GetInt("scaleH");
		var list = new List<CharacterInfo>();
		foreach (XmlNode ch in xml.GetNode("chars").ChildNodes)
		{
			var chInfo = new CharacterInfo();
			chInfo.index = ch.GetInt("id");
			chInfo.advance = ch.GetInt("xadvance");
			chInfo.minX = ch.GetInt("xoffset");
			chInfo.minY = -ch.GetInt("yoffset");
			chInfo.maxX = chInfo.minX + ch.GetInt("width");
			chInfo.maxY = chInfo.minY - ch.GetInt("height");

			var r = new Rect();
			r.x = ch.GetFloat("x") / sw;
			r.y = ch.GetFloat("y") / sh;
			r.width = ch.GetFloat("width") / sw;
			r.height = ch.GetFloat("height") / sh;
			chInfo.uvTopLeft = new Vector2(r.xMin, 1.0f - r.yMax);
			chInfo.uvBottomRight = new Vector2(r.xMax, 1.0f - r.yMin);

			list.Add(chInfo);
		}
		font.characterInfo = list.ToArray();

		AssetDatabase.SaveAssets();

		if (removeSourceAsset)
		{
			AssetDatabase.DeleteAsset(xmlPath);
			AssetDatabase.DeleteAsset(texPath);
		}
	}
}