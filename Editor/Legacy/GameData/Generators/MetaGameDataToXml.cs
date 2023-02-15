using System.Xml;

namespace Hypnos.Editor.GameData
{
    public static class MetaGameDataToXml
    {
        public const string XsdExtension = ".xsd";
        public const string XmlExtension = ".xml";

        public static void Generate(MetaGameData metadata, GameDataConverterSetting setting)
        {
            if (!metadata.IsFormatVaild)
            {
                return;
            }

            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            XmlElement rootElement = xmlDoc.CreateElement("ArrayOf" + metadata.dataClass);

            xmlDoc.AppendChild(declaration);
            xmlDoc.AppendChild(rootElement);

            // Loop row data.
            System.Collections.Generic.List<MetaGameDataField> fields = metadata.dataFields;
            for (int i = 0; i < metadata.rowValues.Count; i++)
            {
                XmlElement rowElement = xmlDoc.CreateElement(metadata.dataClass);

                // Loop column data.
                for (int j = 0; j < fields.Count; j++)
                {
                    XmlElement columnElement = xmlDoc.CreateElement(fields[j].name);
                    columnElement.InnerText = metadata.rowValues[i][j];
                    rowElement.AppendChild(columnElement);
                }

                rootElement.AppendChild(rowElement);
            }

            rootElement.SetAttribute("Length", metadata.rowValues.Count.ToString());

            xmlDoc.Save(setting.bytesOutputPath + "/" + metadata.sheetName + XmlExtension);
            Kernel.Log("[MetaGameDataToXml] Generate XML successfully.");
        }
    }
}