using Morpheus.Core.Encryption;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace Morpheus.Editor.GameData
{
    public static class MetaGameDataToCSharp
    {
        public const string CSharpExtension = ".cs";

        public static void Generate(MetaGameData metadata, GameDataConverterSetting setting)
        {
            if (!metadata.IsFormatVaild)
            {
                return;
            }

            // Set data class.
            CodeTypeDeclaration dataClass = new CodeTypeDeclaration(metadata.dataClass)
            {
                IsClass = true,
                TypeAttributes = System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Sealed
            };

            System.Collections.Generic.List<MetaGameDataField> fields = metadata.dataFields;
            for (int i = 0; i < fields.Count; i++)
            {
                CodeMemberField field = new CodeMemberField
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Final,
                    Name = fields[i].name,
                    Type = new CodeTypeReference("readonly " + fields[i].type.Namespace + "." + fields[i].type.Name)
                };

                dataClass.Members.Add(field);
            }

            // Set data manager class.
            CodeTypeDeclaration dataManagerClass = new CodeTypeDeclaration(setting.dataManagerTypeName)
            {
                IsPartial = true,
                IsClass = true,
                TypeAttributes = System.Reflection.TypeAttributes.Public
            };

            CodeMemberField managerField = new CodeMemberField
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = metadata.dataClass + "Sheet",
                Type = new CodeTypeReference("Dictionary<uint, " + metadata.dataClass + ">")
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
            initMethod.ReturnType = new CodeTypeReference(typeof(void));//new CodeTypeReference

            dataManagerClass.Members.Add(initMethod);

            // Set namespace.
            CodeNamespace targetNamespace = new CodeNamespace(setting.cSharpCodeNamespace);
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

            // Save
            using (FileStream fs = new FileStream(setting.cSharpCodeOutputPath + "/" + metadata.dataClass + CSharpExtension, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                provider.GenerateCodeFromCompileUnit(compileUnit, sw, options);
            }

            DebugLogger.Log("[ExcelHandler] Generate C# code successfully.");
        }
    }
}