using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

public class ExcelImporter : AssetPostprocessor
{
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string assetPathName in importedAssets)
		{
			if (Path.GetExtension(assetPathName).StartsWith(".xls"))
			{
				if (assetPathName.Contains("/Prefabs/"))
				{
					ImportExcelForPrefab(assetPathName);
				}
				else
				{
					ImportExcelForAsset(assetPathName);
				}
			}
		}
	}

	static void ImportExcelForPrefab(string excelPathName)
	{
		List<GameObject> list = new List<GameObject>();

		string projectPathName = Directory.GetCurrentDirectory() + "\\";
		string folderPathName = projectPathName + Path.GetDirectoryName(excelPathName).Replace('/', '\\');
		foreach (string filePathName in Directory.GetFiles(folderPathName, "*.prefab"))
		{
			string assetPath = filePathName.Replace(projectPathName, "").Replace('\\', '/');
			list.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject);
		}

		using (FileStream fs = File.Open(excelPathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
#if UNITY_EDITOR_OSX
			IWorkbook book = new HSSFWorkbook(fs);
#else
			IWorkbook book = new XSSFWorkbook(fs);
#endif
			foreach (ISheet sheet in book)
			{
				if (!IsAlphaNumeric(sheet.SheetName))
				{
					continue;
				}

				var dataType = DataManager.GetType(sheet.SheetName);
				if (dataType == null || !dataType.IsSubclassOf(typeof(Component)))
				{
					Debug.LogError(string.Format("{0}({1}): Not found component type.", excelPathName, sheet.SheetName), GetAsset(excelPathName));
					continue;
				}

				ICell anchorCell = FindDataAnchor(sheet);
				if (anchorCell == null)
				{
					Debug.LogError(string.Format("{0}({1}): Not found data anchor.", excelPathName, sheet.SheetName), GetAsset(excelPathName));
					continue;
				}

				int prefabColumn = FindHeader("prefab", anchorCell);
				if (prefabColumn == 0)
				{
					Debug.LogError(string.Format("{0}({1}): Not found prefab column.", excelPathName, sheet.SheetName), GetAsset(excelPathName));
					continue;
				}

				var headers = GetHeaders(dataType, anchorCell);
				if (headers.Count == 0)
				{
					continue;
				}

				for (int rowIndex = anchorCell.RowIndex + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
				{
					IRow dataRow = sheet.GetRow(rowIndex);
					if (IsRowEmpty(dataRow))
					{
						break;
					}

					string prefabName = GetCellValue(dataRow.GetCell(prefabColumn)).ToString();
					GameObject prefab = list.Find(p => p.name == prefabName);
					if (prefab == null)
					{
						continue;
					}

					Object data = prefab.GetComponent(dataType);
					DataFromRow(excelPathName, headers, dataRow, data);
					EditorUtility.SetDirty(prefab);
				}
			}

			fs.Close();
		}

		AssetDatabase.SaveAssets();
	}

	static void ImportExcelForAsset(string excelPathName)
	{
		string excelName = Path.GetFileNameWithoutExtension(excelPathName);
		var masterType = DataManager.GetType(excelName);
		if (masterType == null)
		{
			Debug.LogError(string.Format("{0}: Not found data type. {1}", excelPathName, excelName), GetAsset(excelPathName));
			return;
		}

		FieldInfo listField = masterType.GetField("list");
		if (listField == null)
		{
			Debug.LogError(string.Format("{0}: Not found 'list' in '{1}'.", excelPathName, masterType), GetAsset(excelPathName));
			return;
		}

		var dataType = listField.FieldType.GetGenericArguments()[0];
		IList list = System.Activator.CreateInstance(listField.FieldType) as IList;

		using (FileStream fs = File.Open(excelPathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
#if UNITY_EDITOR_OSX
			IWorkbook book = new HSSFWorkbook(fs);
#else
			IWorkbook book = new XSSFWorkbook(fs);
#endif
			foreach (ISheet sheet in book)
			{
				if (!IsAlphaNumeric(sheet.SheetName))
				{
					continue;
				}

				ICell anchorCell = FindDataAnchor(sheet);
				if (anchorCell == null)
				{
					Debug.LogError(string.Format("{0}({1}): Not found data anchor.", excelPathName, sheet.SheetName), GetAsset(excelPathName));
					continue;
				}

				var headers = GetHeaders(dataType, anchorCell);
				if (headers.Count == 0)
				{
					continue;
				}

				for (int rowIndex = anchorCell.RowIndex + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
				{
					IRow dataRow = sheet.GetRow(rowIndex);
					if (IsRowEmpty(dataRow))
					{
						break;
					}

					object data = System.Activator.CreateInstance(dataType);
					DataFromRow(excelPathName, headers, dataRow, data);
					list.Add(data);
				}

				string assetPathName = string.Format("{0}/{1}({2}).asset", Path.GetDirectoryName(excelPathName), Path.GetFileNameWithoutExtension(excelPathName), sheet.SheetName);
				var asset = AssetDatabase.LoadAssetAtPath(assetPathName, masterType) as ScriptableObject;
				if (asset == null)
				{
					asset = ScriptableObject.CreateInstance(masterType);
					asset.hideFlags = HideFlags.NotEditable;

					listField.SetValue(asset, list);
					AssetDatabase.CreateAsset(asset, assetPathName);
				}
				else
				{
					listField.SetValue(asset, list);
					EditorUtility.SetDirty(asset);
				}
			}

			fs.Close();
		}

		AssetDatabase.SaveAssets();
	}

	static Object GetAsset(string assetPathName)
	{
		return AssetDatabase.LoadAssetAtPath(assetPathName, typeof(Object));
	}

	static bool IsAlphaNumeric(string str)
	{
		for (int i = 0; i < str.Length; i++)
		{
			if (!char.IsLetterOrDigit(str, i))
			{
				return false;
			}
		}
		return true;
	}

	static ICell FindDataAnchor(ISheet sheet)
	{
		foreach (IRow row in sheet)
		{
			foreach (ICell cell in row)
			{
				if (cell.CellType == CellType.String && cell.StringCellValue.Equals("^data"))
				{
					return cell;
				}
			}
		}
		return null;
	}

	static int FindHeader(string header, ICell anchorCell)
	{
		for (int columnIndex = anchorCell.ColumnIndex + 1; columnIndex < anchorCell.Row.LastCellNum; columnIndex++)
		{
			ICell headerCell = anchorCell.Row.GetCell(columnIndex);
			if (!IsCellExcluded(headerCell))
			{
				object cellValue = GetCellValue(headerCell);
				if (cellValue.ToString() == header)
				{
					return columnIndex;
				}
			}
		}
		return 0;
	}

	static Dictionary<int, FieldInfo> GetHeaders(System.Type dataType, ICell anchorCell)
	{
		var headerDictionary = new Dictionary<int, FieldInfo>();
		for (int columnIndex = anchorCell.ColumnIndex + 1; columnIndex < anchorCell.Row.LastCellNum; columnIndex++)
		{
			ICell headerCell = anchorCell.Row.GetCell(columnIndex);
			if (!IsCellExcluded(headerCell))
			{
				object cellValue = GetCellValue(headerCell);
				FieldInfo fi = dataType.GetField(cellValue.ToString());
				if (fi != null)
				{
					headerDictionary.Add(columnIndex, fi);
				}
			}
		}
		return headerDictionary;
	}

	static void DataFromRow(string excelPathName, Dictionary<int, FieldInfo> headers, IRow row, object data)
	{
		foreach (var h in headers)
		{
			ICell dataCell = row.GetCell(h.Key);
			if (IsCellExcluded(dataCell))
			{
				continue;
			}

			try
			{
				object cellValue = GetCellValue(dataCell);
				FieldInfo fi = h.Value;
				if (fi.FieldType.IsEnum)
				{
					fi.SetValue(data, System.Enum.Parse(fi.FieldType, cellValue.ToString()));
				}
				else if (fi.FieldType == typeof(string))
				{
					fi.SetValue(data, cellValue.ToString());
				}
				else
				{
					fi.SetValue(data, System.Convert.ChangeType(cellValue, fi.FieldType));
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError(string.Format("{0}({1},{2}): {3}", excelPathName, row.Sheet.SheetName, GetCellAddress(row.RowNum, h.Key), e), GetAsset(excelPathName));
			}
		}
	}

	static bool IsRowEmpty(IRow row)
	{
		if (row != null)
		{
			foreach (ICell cell in row)
			{
				if (!IsCellEmpty(cell))
				{
					return false;
				}
			}
		}
		return true;
	}

	static bool IsCellEmpty(ICell cell)
	{
		if (cell != null)
		{
			return (cell.CellType == CellType.Blank);
		}
		return true;
	}

	static bool IsCellExcluded(ICell cell)
	{
		if (cell != null)
		{
			return (cell.CellStyle.IsHidden || cell.CellStyle.GetFont(cell.Sheet.Workbook).IsStrikeout);
		}
		return true;
	}

	static object GetCellValue(ICell cell)
	{
		if (cell == null)
		{
			return null;
		}

		return GetCellValue(cell, cell.CellType);
	}

	static object GetCellValue(ICell cell, CellType cellType)
	{
		switch (cellType)
		{
		case CellType.Numeric:
			return cell.NumericCellValue;

		case CellType.String:
			return cell.StringCellValue.Trim();

		case CellType.Boolean:
			return cell.BooleanCellValue;

		case CellType.Formula:
			return GetCellValue(cell, cell.CachedFormulaResultType);
		}

		return null;
	}

	static string GetCellAddress(ICell cell)
	{
		return GetCellAddress(cell.RowIndex, cell.ColumnIndex);
	}

	static string GetCellAddress(int row, int column)
	{
		string address = "";
		do
		{
			address = (char)('A' + (column % 26)) + address;
			column /= 26;
		} while (column > 0);

		address += (row + 1).ToString();
		return address;
	}
}