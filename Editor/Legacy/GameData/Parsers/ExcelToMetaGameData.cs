using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Morpheus.Editor.GameData
{
    public class ExcelToMetaGameData : MetaGameDataParserBase
    {
        public const string XlsExtension = ".xls";
        public const string XlsxExtension = ".xlsx";

        private const string CommentTag = "#";
        private const string ClassNameTag = "Class";
        private const string FieldTypeTag = "Type";
        private const string FieldNameTag = "Name";

        public static string GetCellString(ICell iCell)
        {
            if (iCell == null)
            {
                return string.Empty;
            }

            if (iCell.CellType != CellType.String)
            {
                iCell.SetCellType(CellType.String);
            }

            return iCell.StringCellValue;
        }

        private string fileName;
        private string fileExtension;

        private List<ISheet> sheets = new List<ISheet>();

        public ExcelToMetaGameData(string name, string extension)
        {
            fileName = name;
            fileExtension = extension;
        }

        public override void PreParse(string filePath)
        {
            IWorkbook workbook = null;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                switch (fileExtension)
                {
                    case XlsExtension:
                        workbook = new HSSFWorkbook(fs);
                        break;
                    case XlsxExtension:
                        workbook = new XSSFWorkbook(fs);
                        break;
                    default:
                        Logger.LogError($"[ExcelToMetaGameData] The file is not xls or xlsx format. File: {fileName}");
                        break;
                }
            }

            if (workbook == null)
            {
                Logger.Log("[ExcelToMetaGameData] Workbook is null.");
                return;
            }

            int sheetNum = workbook.NumberOfSheets;
            MetadataList.Capacity = sheetNum;
            sheets.Capacity = sheetNum;
            for (int i = 0; i < sheetNum; i++)
            {
                ISheet sheet = workbook.GetSheetAt(i);
                if (sheet.SheetName.StartsWith(CommentTag))
                {
                    continue;
                }

                MetaGameData metadata = new MetaGameData()
                {
                    fileName = fileName,
                    sheetName = sheet.SheetName
                };
                ParseDataFormat(sheet, metadata);
                MetadataList.Add(metadata);
                sheets.Add(sheet);
            }
        }

        private void ParseDataFormat(ISheet sheet, MetaGameData result)
        {
            IRow classRow = null;
            IRow typeRow = null;
            IRow nameRow = null;
            int lastRowCount = sheet.LastRowNum + 1;

            for (int i = 0; i < lastRowCount; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null)
                {
                    continue;
                }

                // NOTE: Can modify start column.
                string str = GetCellString(row.GetCell(0));
                if (string.IsNullOrEmpty(str))
                {
                    continue;
                }

                if (string.Equals(str, ClassNameTag, StringComparison.OrdinalIgnoreCase))
                {
                    classRow = row;
                }
                else if (string.Equals(str, FieldTypeTag, StringComparison.OrdinalIgnoreCase))
                {
                    typeRow = row;
                }
                else if (string.Equals(str, FieldNameTag, StringComparison.OrdinalIgnoreCase))
                {
                    nameRow = row;
                }

                if (classRow != null && typeRow != null && nameRow != null)
                {
                    result.firstValueRowIndex = i + 1;
                    break;
                }
            }

            if (classRow != null && typeRow != null && nameRow != null)
            {
                FetchClass(classRow, result);
                FetchFields(typeRow, nameRow, result);
            }
        }

        private void FetchClass(IRow row, MetaGameData result)
        {
            result.SetClass(GetCellString(row.GetCell(1)));
        }

        private void FetchFields(IRow typeRow, IRow nameRow, MetaGameData result)
        {
            if (typeRow == null || nameRow == null)
            {
                //LogUtility.LogWarningFormat("[ExcelParser] typeRow or nameRow is null. Sheet: {0}", sheetInfo.sheetName);
                return;
            }

            int colNum = typeRow.LastCellNum;
            string typeStr = string.Empty;
            string nameStr = string.Empty;
            for (int i = 1; i < colNum; i++)
            {
                typeStr = GetCellString(typeRow.GetCell(i));
                if (string.IsNullOrEmpty(typeStr) || typeStr.StartsWith(CommentTag))
                {
                    continue;
                }

                Type fieldType = Type.GetType("System." + typeStr);
                if (fieldType == null)
                {
                    //LogUtility.LogWarningFormat("[ExcelParser] Can't get type at column {0}. Sheet: {1}", i - 1, sheetInfo.sheetName);
                    continue;
                }

                nameStr = GetCellString(nameRow.GetCell(i));
                if (string.IsNullOrEmpty(nameStr))
                {
                    continue;
                }

                result.AddField(i, fieldType, nameStr);
            }
        }

        public override void Parse()
        {
            if (IsParsed)
            {
                return;
            }

            int dataCount = MetadataList.Count;
            for (int i = 0; i < dataCount; i++)
            {
                int rowCount = sheets[i].LastRowNum + 1;
                for (int j = MetadataList[i].firstValueRowIndex; j < rowCount; j++)
                {
                    FetchRowValue(sheets[i].GetRow(j), MetadataList[i]);
                }
            }

            IsParsed = true;
        }

        public void FetchRowValue(IRow valueRow, MetaGameData result)
        {
            if (valueRow == null || GetCellString(valueRow.GetCell(0)).StartsWith(CommentTag))
            {
                return;
            }

            int fieldLength = result.dataFields.Count;
            string[] values = new string[fieldLength];
            for (int i = 0; i < fieldLength; i++)
            {
                values[i] = GetCellString(valueRow.GetCell(result.dataFields[i].ColumnIndex));
            }

            result.AddRowValue(values);
        }
    }
}