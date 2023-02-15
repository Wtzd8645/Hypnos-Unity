using Hypnos.Core.Encryption;
using NPOI.SS.UserModel;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Hypnos.Editor
{
    public class DomoExcelHandler : IExcelHandler
    {
        private const string CommentTag = "#";
        private const string TypeTag = "Type";
        private const string NameTag = "Name";

        private readonly Type KeyType = typeof(uint);
        private const string KeyName = "Key";

        public static string GetCellString(ICell iCell)
        {
            if (iCell.CellType != CellType.String)
            {
                iCell.SetCellType(CellType.String);
            }

            return iCell.StringCellValue;
        }

        private ISheet mainSheet;

        private List<Type> varTypeList = new List<Type>();
        private List<string> varNameList = new List<string>();

        //private Dictionary<string, string[]> contentDic = new Dictionary<string, string[]>();

        private int lastRowNumber = -1;
        private int firstValueRow = -1;

        public bool Parse(IWorkbook iWorkBook)
        {
            mainSheet = null;

            // Parse sheet in workbook.
            int sheetNum = iWorkBook.NumberOfSheets;
            for (int i = 0; i < sheetNum; i++)
            {
                string sheetName = iWorkBook.GetSheetName(i);
                if (sheetName.StartsWith(CommentTag))
                {
                    continue;
                }

                mainSheet = iWorkBook.GetSheetAt(i);
                break;
            }

            if (mainSheet == null)
            {
                Kernel.LogError("[ExcelHandler] Can't find main sheet.");
                return false;
            }

            lastRowNumber = mainSheet.LastRowNum;

            // Parse self-defined row.
            int rowCount = 0;
            while (rowCount <= lastRowNumber)
            {
                IRow row = mainSheet.GetRow(rowCount);
                if (row == null)
                {
                    rowCount++;
                    continue;
                }

                ICell cell = row.GetCell(0);
                if (cell == null)
                {
                    rowCount++;
                    continue;
                }

                string str = GetCellString(cell);
                if (string.Equals(str, TypeTag, StringComparison.OrdinalIgnoreCase))
                {
                    ParseTypeRow(row);
                }
                else if (string.Equals(str, NameTag, StringComparison.OrdinalIgnoreCase))
                {
                    ParseNameRow(row);
                }

                if (varTypeList.Count > 0 && varNameList.Count > 0)
                {
                    rowCount++;
                    break;
                }

                rowCount++;
            }

            if ((varTypeList.Count != varNameList.Count) || (varTypeList.Count == 0) || (varNameList.Count == 0))
            {
                Kernel.LogError("[ExcelHandler] Column num not valid. Type: " + varTypeList.Count + " Name: " + varNameList.Count);
                return false;
            }

            if (rowCount > lastRowNumber)
            {
                Kernel.LogError("[ExcelHandler] No data row to parse. LastRowNum: " + lastRowNumber);
                return false;
            }

            firstValueRow = rowCount;
            return true;
        }

        private void ParseTypeRow(IRow iRow)
        {
            string str = string.Empty;
            int columnNum = iRow.LastCellNum;
            varTypeList = new List<Type>(columnNum);

            for (int i = 1; i < columnNum; i++)
            {
                ICell cell = iRow.GetCell(i);
                if (cell == null)
                {
                    Kernel.Log("[ExcelHandler] Type row has " + (i - 1) + " column.");
                    return;
                }

                str = GetCellString(cell);
                if (string.IsNullOrEmpty(str))
                {
                    Kernel.Log("[ExcelHandler] Type row has " + (i - 1) + " column.");
                    return;
                }

                Type type = Type.GetType("System." + str);
                if (type == null)
                {
                    Kernel.LogError("[ExcelHandler] Can't find type at column " + (i - 1));
                    return;
                }

                varTypeList.Add(type);
            }
        }

        private void ParseNameRow(IRow iRow)
        {
            string str = string.Empty;
            int columnNum = iRow.LastCellNum;
            varNameList = new List<string>(columnNum);

            for (int i = 1; i < columnNum; i++)
            {
                ICell cell = iRow.GetCell(i);
                if (cell == null)
                {
                    Kernel.Log("[ExcelHandler] Name row has " + (i - 1) + " column.");
                    return;
                }

                str = GetCellString(cell);
                if (string.IsNullOrEmpty(str))
                {
                    Kernel.Log("[ExcelHandler] Name row has " + (i - 1) + " column.");
                    return;
                }

                if (varNameList.Contains(str))
                {
                    Kernel.LogError("[ExcelHandler] Name is duplicate at " + (i - 1) + " column.");
                    return;
                }

                varNameList.Add(str);
            }
        }

        public void GenerateXML(bool iIsEncrypt)
        {
            if ((varTypeList.Count != varNameList.Count) || (varTypeList.Count == 0) || (varNameList.Count == 0))
            {
                Kernel.LogError("[ExcelHandler] Column num not valid. Type: " + varTypeList.Count + " Name: " + varNameList.Count);
                return;
            }

            if (firstValueRow > lastRowNumber)
            {
                Kernel.LogError("[ExcelHandler] No data row to parse. LastRowNum: " + lastRowNumber);
                return;
            }

            // Create dictionary to verificate key.
            Dictionary<string, bool> keyDic = new Dictionary<string, bool>(lastRowNumber + 1);

            // Create XML document.
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            XmlElement rootElement = xmlDoc.CreateElement("ArrayOf" + mainSheet.SheetName);

            xmlDoc.AppendChild(declaration);
            xmlDoc.AppendChild(rootElement);

            // Loop row data.
            int rowCount = firstValueRow;
            while (rowCount <= lastRowNumber)
            {
                IRow row = mainSheet.GetRow(rowCount);
                if (row == null)
                {
                    rowCount++;
                    continue;
                }

                XmlElement rowElement = xmlDoc.CreateElement(mainSheet.SheetName);

                // Check key column.
                ICell firstColumn = row.GetCell(0);
                string key = null;

                if (!ParseCell(firstColumn, ref key))
                {
                    rowCount++;
                    continue;
                }

                if (key.StartsWith(CommentTag))
                {
                    rowCount++;
                    continue;
                }

                if (!VerificateValue(KeyType, key))
                {
                    Kernel.LogError("[ExcelHandler] Verificate value failed. [" + rowCount + ", " + 0 + "]");
                    return;
                }

                // Loop column data.
                int columnNum = varNameList.Count;
                for (int i = 0; i < columnNum; i++)
                {
                    XmlElement columnElement = xmlDoc.CreateElement(varNameList[i]);

                    // Escape column one.
                    ICell cell = row.GetCell(i + 1);
                    string str = null;

                    if (!ParseCell(cell, ref str))
                    {
                        continue;
                    }

                    if (VerificateValue(varTypeList[i], str))
                    {
                        columnElement.InnerText = str;
                        rowElement.AppendChild(columnElement);
                    }
                    else
                    {
                        Kernel.LogError("[ExcelHandler] Verificate value failed at [" + rowCount + ", " + i + "]");
                        return;
                    }
                }

                if (!rowElement.IsEmpty)
                {
                    keyDic.Add(key, false);
                    rowElement.SetAttribute("Key", key);
                    rootElement.AppendChild(rowElement);
                }

                rowCount++;
            }

            rootElement.SetAttribute("Length", keyDic.Count.ToString());

            // Output
            OutputXML(iIsEncrypt, xmlDoc);
        }

        private bool ParseCell(ICell iCell, ref string iStr)
        {
            if (iCell == null)
            {
                return false;
            }

            iStr = GetCellString(iCell);
            return !string.IsNullOrEmpty(iStr);
        }

        private bool VerificateValue(Type iType, string iStr)
        {
            if (iType == typeof(bool))
            {
                return bool.TryParse(iStr, out bool temp);
            }
            else if (iType == typeof(byte))
            {
                return byte.TryParse(iStr, out byte temp);
            }
            else if (iType == typeof(sbyte))
            {
                return sbyte.TryParse(iStr, out sbyte temp);
            }
            else if (iType == typeof(char))
            {
                return char.TryParse(iStr, out char temp);
            }
            else if (iType == typeof(decimal))
            {
                return decimal.TryParse(iStr, out decimal temp);
            }
            else if (iType == typeof(double))
            {
                return double.TryParse(iStr, out double temp);
            }
            else if (iType == typeof(float))
            {
                return float.TryParse(iStr, out float temp);
            }
            else if (iType == typeof(int))
            {
                return int.TryParse(iStr, out int temp);
            }
            else if (iType == typeof(uint))
            {
                return uint.TryParse(iStr, out uint temp);
            }
            else if (iType == typeof(long))
            {
                return long.TryParse(iStr, out long temp);
            }
            else if (iType == typeof(ulong))
            {
                return ulong.TryParse(iStr, out ulong temp);
            }
            else if (iType == typeof(short))
            {
                return short.TryParse(iStr, out short temp);
            }
            else
            {
                return iType == typeof(ushort) ? ushort.TryParse(iStr, out ushort temp) : iType == typeof(string);
            }
        }

        private void OutputXML(bool iIsEncrypt, XmlDocument iXMLDoc)
        {
            if (iIsEncrypt)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    iXMLDoc.Save(ms);
                    ms.Flush();
                    ms.Position = 0;

                    using FileStream fs = new FileStream(EditorKernel.Config.bytesOutputPath + "/" + mainSheet.SheetName + ".bytes", FileMode.Create);
                    //EncryptionUtil.EncodeByAES(fs, ms);
                }
                Kernel.Log("[ExcelHandler] Generate encrypted XML successfully.");
            }
            else
            {
                iXMLDoc.Save(EditorKernel.Config.bytesOutputPath + "/" + mainSheet.SheetName + ".xml");
                Kernel.Log("[ExcelHandler] Generate XML successfully.");
            }
        }

        public void GenerateCSharpCode()
        {
            // Set data class.
            CodeTypeDeclaration dataClass = new CodeTypeDeclaration(mainSheet.SheetName)
            {
                IsClass = true,
                TypeAttributes = System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Sealed
            };

            for (int i = 0; i < varTypeList.Count; i++)
            {
                CodeMemberField field = new CodeMemberField
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Final,
                    Name = varNameList[i],
                    Type = new CodeTypeReference("readonly " + varTypeList[i].Namespace + "." + varTypeList[i].Name)
                };

                dataClass.Members.Add(field);
            }

            // Set data manager class.
            CodeTypeDeclaration dataManagerClass = new CodeTypeDeclaration(EditorKernel.Config.dataManagerTypeName)
            {
                IsPartial = true,
                IsClass = true,
                TypeAttributes = System.Reflection.TypeAttributes.Public
            };

            CodeMemberField managerField = new CodeMemberField
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = mainSheet.SheetName + "Sheet",
                Type = new CodeTypeReference("Dictionary<uint, " + mainSheet.SheetName + ">")
            };

            dataManagerClass.Members.Add(managerField);

            // Set get value from key method.
            CodeMemberMethod getMethod = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "Get" + dataClass.Name
            };
            getMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(uint), "iKey"));
            CodeParameterDeclarationExpression outData = new CodeParameterDeclarationExpression(dataClass.Name, "iData")
            {
                Direction = FieldDirection.Out
            };
            getMethod.Parameters.Add(outData);
            CodeExpressionStatement getStatement = new CodeExpressionStatement(
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        null,
                        managerField.Name
                        ),
                    "TryGetValue",
                    new CodeExpression[2] { new CodeArgumentReferenceExpression("iKey"), new CodeArgumentReferenceExpression("out iData") }
                    )
                );
            getMethod.Statements.Add(getStatement);
            getMethod.ReturnType = new CodeTypeReference(typeof(void));//new CodeTypeReference(mainSheet.SheetName);

            dataManagerClass.Members.Add(getMethod);

            // Set initial sheet method.
            CodeMemberMethod initMethod = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "Init" + dataClass.Name
            };
            initMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(byte[]), "iEncodedData"));

            //CodeVariableDeclarationStatement decodedBytes = new CodeVariableDeclarationStatement(
            //    new CodeTypeReference(typeof(byte[])),
            //    "decodedBytes",
            //    new CodeMethodInvokeExpression(
            //        //CommonTools.DecodeByAES(iUndecodedData);
            //        new CodeTypeReferenceExpression(typeof(EncryptionUtil)),
            //        "DecodeByAES",
            //        new CodeArgumentReferenceExpression("iEncodedData")
            //        )
            //    );
            //initMethod.Statements.Add(decodedBytes);
            initMethod.Statements.Add(new CodeSnippetStatement("        using(MemoryStream ms = new MemoryStream(decodedBytes))"));
            initMethod.Statements.Add(new CodeSnippetStatement("        {"));
            initMethod.Statements.Add(new CodeSnippetStatement("        using(XmlReader xmlReader = XmlReader.Create(ms))"));
            initMethod.Statements.Add(new CodeSnippetStatement("        {"));
            CodeAssignStatement xmlserializer = new CodeAssignStatement(
                new CodeVariableReferenceExpression(typeof(System.Xml.Serialization.XmlSerializer).Name + " xmlSerializer"),
                new CodeObjectCreateExpression(typeof(System.Xml.Serialization.XmlSerializer).Name, new CodeArgumentReferenceExpression("typeof(" + dataClass.Name + ")"))
                );
            initMethod.Statements.Add(xmlserializer);

            CodeMethodInvokeExpression moveToContent = new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(
                    null,
                    "xmlReader"
                    ),
                "MoveToContent"
                );
            initMethod.Statements.Add(moveToContent);

            CodeAssignStatement dic = new CodeAssignStatement(
                new CodeFieldReferenceExpression(null, managerField.Name),
                new CodeObjectCreateExpression("Dictionary<uint, " + dataClass.Name + ">",
                    new CodeArgumentReferenceExpression("int.Parse(xmlReader.GetAttribute(0))")
                    )
                );
            initMethod.Statements.Add(dic);

            CodeMethodInvokeExpression readStartElement = new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(
                    null,
                    "xmlReader"
                    ),
                "ReadStartElement"
                );
            initMethod.Statements.Add(readStartElement);

            CodeIterationStatement whileLoop = new CodeIterationStatement(
                new CodeExpressionStatement(new CodeSnippetExpression()),
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        null,
                        "xmlReader"
                        ),
                    "IsStartElement"
                    ),
                new CodeExpressionStatement(new CodeSnippetExpression()),
                new CodeExpressionStatement(
                    new CodeMethodInvokeExpression(
                        new CodeFieldReferenceExpression(
                            null,
                            managerField.Name
                        ),
                        "Add",
                        new CodeArgumentReferenceExpression("uint.Parse(xmlReader.GetAttribute(0)), (" + dataClass.Name + ")xmlSerializer.Deserialize(xmlReader)")
                        )
                    )
                );
            initMethod.Statements.Add(whileLoop);
            initMethod.Statements.Add(new CodeSnippetStatement("        }"));
            initMethod.Statements.Add(new CodeSnippetStatement("        }"));
            initMethod.ReturnType = new CodeTypeReference(typeof(void));//new CodeTypeReference

            dataManagerClass.Members.Add(initMethod);

            // Set namespace.
            CodeNamespace targetNamespace = new CodeNamespace(EditorKernel.Config.cSharpCodeNamespace);
            targetNamespace.Types.Add(dataManagerClass);
            targetNamespace.Types.Add(dataClass);

            // Set global namespace.
            CodeNamespace globalNamespace = new CodeNamespace();
            globalNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            globalNamespace.Imports.Add(new CodeNamespaceImport("System.IO"));
            globalNamespace.Imports.Add(new CodeNamespaceImport("System.Xml"));
            globalNamespace.Imports.Add(new CodeNamespaceImport("System.Xml.Serialization"));

            // Set complie unit.
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(globalNamespace);
            compileUnit.Namespaces.Add(targetNamespace);

            // Generate.
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions
            {
                BlankLinesBetweenMembers = false,
                BracingStyle = "C"
            };

            using (FileStream fs = new FileStream(EditorKernel.Config.cSharpCodeOutputPath + "/" + mainSheet.SheetName + ".cs", FileMode.Create))
            {
                using StreamWriter sw = new StreamWriter(fs);
                provider.GenerateCodeFromCompileUnit(compileUnit, sw, options);
            }
            Kernel.Log("[ExcelHandler] Generate C# code successfully.");
        }
    }
}
