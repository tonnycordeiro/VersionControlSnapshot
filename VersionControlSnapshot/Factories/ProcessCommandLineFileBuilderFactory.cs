using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersionControlSnapshot.Builders;
using System.Xml;
using System.Xml.Linq;
using VersionControlSnapshot.Utils;
using VersionControlSnapshot.Data;
using System.Text.RegularExpressions;

namespace VersionControlSnapshot.Factories
{
    public class ProcessCommandLineFileBuilderFactory : FileBuilderFactory
    {
        public const string PARENT_TAG_NAME = "ProcessCommandLinePorperties";
        public const string PROCESS_COMMAND_LINE_PROPERTY_ITEM_TAG = "ProcessCommandLinePopertyItem";
        public const string PROCESS_FILE_NAME_ATTRIBUTE_TAG = "ProcessFileName";
        public const string PROCESS_ARGUMENT_ATTRIBUTE_TAG = "ProcessArguments";
        public const string TEMP_REPOSITORY_PATH_KEY_IN_CONFIG = "[TEMP_REPOSITORY_PATH]";
        public const string CFG_LOCAL_FILE_KEY_IN_CONFIG = "[CFG_FILES_PATH]";
        public const string TARGET_FOLDER_KEY_IN_CONFIG = "[TARGET_FOLDER]";
        public const string TARGET_FOLDER_FULL_PATH_KEY_IN_CONFIG = "[TARGET_FOLDER_FULL_PATH]";

        public ProcessCommandLineFileBuilderFactory(string cfgFullPath, string rootPath) : base(cfgFullPath, rootPath)
        {
        }

        public override List<FileBuilder> CreateFileBuilder()
        {

            List <FileBuilder> fileBuilderList = new List<FileBuilder>();

            XmlNodeList elemList = this._xmlConfig.GetElementsByTagName(PROCESS_COMMAND_LINE_PROPERTY_ITEM_TAG);
            foreach (XmlNode xNode in elemList)
            {
                string processArguments =
                    ReplaceStrWithTargetFolderPathValue(xNode[PROCESS_ARGUMENT_ATTRIBUTE_TAG].InnerText,
                                                        xNode[TARGET_FOLDER_ATTRIBUTE_TAG].InnerText);

                ProcessCommandLineFileBuilder processCommandLineFileBuilder =
                    new ProcessCommandLineFileBuilder(this._rootPath,
                                                      xNode[TARGET_FOLDER_ATTRIBUTE_TAG].InnerText,
                                                      xNode[PROCESS_FILE_NAME_ATTRIBUTE_TAG].InnerText,
                                                      processArguments
                                                     );

                fileBuilderList.Add(processCommandLineFileBuilder);
            }

            return fileBuilderList;
        }

        private string ReplaceStrWithTargetFolderPathValue(string strWithKeysToReplace, string targetFolderPath)
        {
            return strWithKeysToReplace
                .Replace(TARGET_FOLDER_FULL_PATH_KEY_IN_CONFIG, ExplorerFilesUtil.GetFullPath(this._rootPath, targetFolderPath));
        }
    }
}
