using System.Collections.Generic;
using System.Xml;

namespace Blanketmen.Hypnos.Editor
{
    public class XmlGenerator
    {
        public const string XsdExtension = ".xsd";
        public const string XmlExtension = ".xml";

        public void GenerateFile(SheetInfo sheetInfo, string outputPath)
        {
            if (!sheetInfo.IsValueVaild())
            {
                return;
            }

            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            XmlElement rootElement = xmlDoc.CreateElement("ArrayOf" + sheetInfo.className);

            xmlDoc.AppendChild(declaration);
            xmlDoc.AppendChild(rootElement);

            // Loop row data.
            List<SheetFieldInfo> fieldInfos = sheetInfo.fieldInfos;
            for (int i = 0; i < sheetInfo.totalValueRowCount; i++)
            {
                XmlElement rowElement = xmlDoc.CreateElement(sheetInfo.className);

                // Loop column data.
                for (int j = 0; j < fieldInfos.Count; j++)
                {
                    XmlElement columnElement = xmlDoc.CreateElement(fieldInfos[j].name);
                    columnElement.InnerText = fieldInfos[j].values[i];
                    rowElement.AppendChild(columnElement);
                }

                rootElement.AppendChild(rowElement);
            }

            rootElement.SetAttribute("Length", sheetInfo.totalValueRowCount.ToString());

            xmlDoc.Save(outputPath + "/" + sheetInfo.sheetName + XmlExtension);
            Logging.Info("Generate XML successfully.");
        }
    }
}