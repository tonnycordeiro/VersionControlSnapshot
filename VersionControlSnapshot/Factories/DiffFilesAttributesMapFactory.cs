using VersionControlSnapshot.Models;
using VersionControlSnapshot.Model;
using VersionControlSnapshot.Utils;
using Microsoft.XmlDiffPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VersionControlSnapshot.Factories
{
    public class DiffFilesAttributesMapFactory
    {
        public const string PARENT_TAG_NAME = "DiffFilesAttributes";
        public const string MAP_FOLDER_PROPERTY_ITEM_TAG = "DiffFilesAttributesItem";
        public const string PATH_TAG = "Path";
        public const string FILE_TRANSFER_CONDITION_TAG = "FileTransferCondition";
        public const string XML_DIFF_OPTIONS_TAG = "XmlDiffOptions";
        public const char XML_DIFF_OPTIONS_CHAR_SPLIT = '|';
        private XmlDocument _xmlConfig;
        private string _cfgFullPath;

        public DiffFilesAttributesMapFactory(string cfgFullPath)
        {
            this._cfgFullPath = cfgFullPath;
            this._xmlConfig = new XmlDocument();
            if (!String.IsNullOrEmpty(cfgFullPath))
                this._xmlConfig.Load(cfgFullPath);
        }

        public DiffFilesAttibutesMapCollection GetDiffFilesAttibutesMapCollection()
        {
            FileTransferCondition fileTransferCondition;
            XmlDiffOptions xmlDiffOptions;

            List<DiffFilesAttibutesMap> diffFilesAttibutesMapList = new List<DiffFilesAttibutesMap>();

            if (!String.IsNullOrEmpty(this._cfgFullPath))
            {
                XmlNodeList elemList = this._xmlConfig.GetElementsByTagName(MAP_FOLDER_PROPERTY_ITEM_TAG);
                foreach (XmlNode xNode in elemList)
                {
                    fileTransferCondition = GetFileTransferCondition(xNode[FILE_TRANSFER_CONDITION_TAG].InnerText);
                    xmlDiffOptions = GetXmlDiffOptions(xNode[XML_DIFF_OPTIONS_TAG].InnerText);

                    diffFilesAttibutesMapList.Add(
                        new DiffFilesAttibutesMap(xNode[PATH_TAG].InnerText, fileTransferCondition, xmlDiffOptions));
                }
            }

            return new DiffFilesAttibutesMapCollection(diffFilesAttibutesMapList);
        }

        private FileTransferCondition GetFileTransferCondition(string fileTransferConditionName)
        {
            return EnumUtil.GetEnum<FileTransferCondition>(fileTransferConditionName, DiffFilesAttibutesMap.FILE_TRANSFER_CONDITION_DEFAULT);
        }

        private XmlDiffOptions GetXmlDiffOptions(string xmlDiffOptionsName)
        {
            string[] options = xmlDiffOptionsName.Split(XML_DIFF_OPTIONS_CHAR_SPLIT);
            List<XmlDiffOptions> xmlDiffOptionsList = new List<XmlDiffOptions>();
            XmlDiffOptions newXmlDiffOptions;

            foreach (string option in options)
            {
                xmlDiffOptionsList.Add(EnumUtil.GetEnum<XmlDiffOptions>(option, DiffFilesAttibutesMap.XML_DIFF_OPTIONS_DEFAULT));
            }
            xmlDiffOptionsList = xmlDiffOptionsList.Distinct().ToList<XmlDiffOptions>();

            newXmlDiffOptions = xmlDiffOptionsList.FirstOrDefault();
            for(int i=1; i< xmlDiffOptionsList.Count; i++)
            {
                newXmlDiffOptions = newXmlDiffOptions | xmlDiffOptionsList[i];
            }
            return newXmlDiffOptions;
        }
    }
}
