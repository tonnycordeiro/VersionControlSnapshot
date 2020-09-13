using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersionControlSnapshot.Builders;
using System.Xml;
using System.Configuration;
using VersionControlSnapshot.Adapters;

namespace VersionControlSnapshot.Factories
{
    public class SqlFileBuilderFactory : FileBuilderFactory
    {
        public const string PARENT_TAG_NAME = "SqlProperties";
        public const string SQL_PROPERTY_ITEM_TAG = "SqlPropertyItem";
        public const string SQL_ATTRIBUTE_TAG = "SQL";
        public const string FILE_NAME_ATTRIBUTE_TAG = "FileName";
        public const string CONNECTION_NAME_TAG = "ConectionName";

        public SqlFileBuilderFactory(string cfgFullPath, string rootPath) : base(cfgFullPath, rootPath)
        {
        }

        public override List<FileBuilder> CreateFileBuilder()
        {

            List<FileBuilder> fileBuilderList = new List<FileBuilder>();

            XmlNodeList elemList = this._xmlConfig.GetElementsByTagName(SQL_PROPERTY_ITEM_TAG);
            foreach (XmlNode xNode in elemList)
            {
                SqlFileBuilder sqlFileBuilder =
                    new SqlFileBuilder(this._rootPath, 
                                                      xNode[TARGET_FOLDER_ATTRIBUTE_TAG].InnerText,
                                                      xNode[SQL_ATTRIBUTE_TAG].InnerText,
                                                      xNode[FILE_NAME_ATTRIBUTE_TAG].InnerText,
                                                      xNode[CONNECTION_NAME_TAG].InnerText
                                                      );

                fileBuilderList.Add(sqlFileBuilder);
            }

            return fileBuilderList;

        }
    }
}
