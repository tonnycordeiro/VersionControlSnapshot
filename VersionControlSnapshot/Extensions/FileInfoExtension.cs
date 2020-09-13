using VersionControlSnapshot.Managers;
using VersionControlSnapshot.Model;
using Microsoft.XmlDiffPatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Extensions
{
    public static class FileInfoExtension
    {
        public const string EXTENSION_XML = ".xml";
        public const string EXTENSION_DLL = ".dll";

        public static bool IsDifferent(this FileInfo fileInfo, FileInfo fileInfoToCompareWith, XmlComparerManager xmlComparer, TfsManager tfsComparer)
        {
            switch(fileInfo.Extension)
            {
                case EXTENSION_XML:
                    return xmlComparer.IsDifferent(fileInfo.FullName, fileInfoToCompareWith.FullName);
                default:
                    string difference = null;
                    return tfsComparer.IsDifferent(fileInfo.FullName, fileInfoToCompareWith.FullName, out difference);
            }
        }

        public static bool IsToTransfer(this FileInfo fileInfo, FileInfo fileInfoToCompareWith, FileTransferCondition fileTransferCondition)
        {
            switch (fileTransferCondition)
            {
                case FileTransferCondition.IF_DIFF:
                    return fileInfo.LastWriteTime.CompareTo(fileInfoToCompareWith.LastWriteTime) != 0;
                case FileTransferCondition.ALWAYS:
                    return true;
                case FileTransferCondition.NEVER:
                    return false;
                default:
                    return true;
            }
        }

        //FileTransferCondition fileTransferCondition

    }
}
