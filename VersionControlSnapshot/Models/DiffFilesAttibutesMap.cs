using VersionControlSnapshot.Model;
using Microsoft.XmlDiffPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Models
{
    public class DiffFilesAttibutesMap
    {
        private string _folderPath;
        private FileTransferCondition _fileTransferCondition;
        private XmlDiffOptions _xMlDiffOptions;

        public const FileTransferCondition FILE_TRANSFER_CONDITION_DEFAULT = FileTransferCondition.ALWAYS;
        public const XmlDiffOptions XML_DIFF_OPTIONS_DEFAULT = XmlDiffOptions.None;

        public DiffFilesAttibutesMap(string folderPath, 
            FileTransferCondition fileTransferCondition = FILE_TRANSFER_CONDITION_DEFAULT, XmlDiffOptions xMlDiffOptions = XmlDiffOptions.None)
        {
            FolderPath = folderPath;
            FileTransferCondition = fileTransferCondition;
            XMlDiffOptions = xMlDiffOptions;
        }

        public string FolderPath { get { return _folderPath; } set { _folderPath = value; } }

        public FileTransferCondition FileTransferCondition {
            get { return _fileTransferCondition; } set { _fileTransferCondition = value; } }

        public XmlDiffOptions XMlDiffOptions { get { return _xMlDiffOptions; } set { _xMlDiffOptions = value; } }


        public FileTransferCondition GetFileTransferCondition(string fullPathFile)
        {
            if (IsFromFolder(fullPathFile))
                return this.FileTransferCondition;
            return FILE_TRANSFER_CONDITION_DEFAULT;
        }

        public XmlDiffOptions GetXmlDiffOptions(string fullPathFile)
        {
            if(IsFromFolder(fullPathFile))
                return this.XMlDiffOptions;
            return XML_DIFF_OPTIONS_DEFAULT;
        }

        public bool IsFromFolder(string fullPathFile)
        {
            return (fullPathFile.Contains(this.FolderPath));
        }
    }
}
