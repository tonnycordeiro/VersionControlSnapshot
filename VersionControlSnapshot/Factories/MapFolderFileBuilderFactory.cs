using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersionControlSnapshot.Builders;
using System.Xml;

namespace VersionControlSnapshot.Factories
{
    public class MapFolderFileBuilderFactory : FileBuilderFactory
    {
        public const string PARENT_TAG_NAME = "MapFolderProperties";
        public const string MAP_FOLDER_PROPERTY_ITEM_TAG = "MapFolderPopertyItem";
        public const string SOURCE_ATTRIBUTE_TAG = "Source";

        public MapFolderFileBuilderFactory(string cfgFullPath, string rootPath) : base(cfgFullPath, rootPath)
        {
        }

        public override List<FileBuilder> CreateFileBuilder()
        {
            List<FileBuilder> fileBuilderList = new List<FileBuilder>();

            XmlNodeList elemList = this._xmlConfig.GetElementsByTagName(MAP_FOLDER_PROPERTY_ITEM_TAG);
            foreach (XmlNode xNode in elemList)
            {
                MapFolderFileBuilder mapFolderFileBuilder =
                    new MapFolderFileBuilder(this._rootPath,
                                                      xNode[SOURCE_ATTRIBUTE_TAG].InnerText,
                                                      xNode[TARGET_FOLDER_ATTRIBUTE_TAG].InnerText
                                                      );

                fileBuilderList.Add(mapFolderFileBuilder);
            }

            return fileBuilderList;
        }
    }
}
