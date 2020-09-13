using VersionControlSnapshot.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VersionControlSnapshot.Extensions;
using VersionControlSnapshot.Managers;
using VersionControlSnapshot.Models;
using Microsoft.XmlDiffPatch;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.ComponentModel;


namespace VersionControlSnapshot.Utils
{
    public static class ExplorerFilesUtil
    {
        public const char FOLDER_SPLIT = '\\';
        public const string DRIVE_PATH_PATTERN = @"[a-zA-Z]{1}:";
        public const string BEGIN_OF_NETWORK_PATH = @"\\";

        public static string GetPathWithoutLastFolderSplit(string path)
        {
            if (path.LastOrDefault().Equals(FOLDER_SPLIT))
            {
                path = path.Substring(0, path.Length - 2);
            }
            return path;
        }

        private static string RemoveDriveFromPath(string fullPath)
        {
            string pathWithoutDrive = fullPath;

            Regex reg = new Regex(DRIVE_PATH_PATTERN);
            Match match = reg.Match(fullPath);
            string drivePath = match.Value;

            if (!String.IsNullOrEmpty(drivePath) && fullPath.IndexOf(drivePath) == 0)
                pathWithoutDrive = pathWithoutDrive.Substring(0, drivePath.Length);

            if (pathWithoutDrive.IndexOf(FOLDER_SPLIT) == 0)
                pathWithoutDrive = pathWithoutDrive.Substring(0, 1);

            return pathWithoutDrive;
        }

        public static string GetFileName(string fullPath)
        {
            int lastIndexOfFolderSplit = fullPath.LastIndexOf(ExplorerFilesUtil.FOLDER_SPLIT);
            bool isPathWithoutFileName = (lastIndexOfFolderSplit + 1 == fullPath.Length);
            if (!isPathWithoutFileName)
            {
                if (lastIndexOfFolderSplit > -1)
                    return fullPath.Substring(lastIndexOfFolderSplit);
                else
                    return fullPath;
            }
            return null;
        }

        public static string GetLastCommonPath(string path1, string path2)
        {
            path1 = RemoveDriveFromPath(path1);
            path2 = RemoveDriveFromPath(path2);

            StringBuilder commonPathsBuilder = new StringBuilder();

            string[] path1Parts = path1.Split(FOLDER_SPLIT);
            string[] path2Parts = path2.Split(FOLDER_SPLIT);

            for (int i = path1Parts.Length - 1; i >= 0; i--)
            {
                for (int j = path2Parts.Length - 1; j >= 0; j--)
                {
                    if (path1Parts[i] == path2Parts[j])
                    {
                        commonPathsBuilder.Insert(0, path1Parts[i]);
                        commonPathsBuilder.Insert(0, FOLDER_SPLIT);
                        i--;
                    }
                    else
                    {
                        if (commonPathsBuilder.Length > 0)
                            break;
                    }
                }
                if (commonPathsBuilder.Length > 0)
                    break;
            }

            return commonPathsBuilder.ToString();
        }

        public static IEnumerable<string> GetLocalFiles(string fullFolderPath)
        {
            return Directory
                .EnumerateFiles(fullFolderPath, "*", SearchOption.AllDirectories)
                .Select(Path.GetFullPath).ToList<string>();
        }

        public static IEnumerable<string> GetLocalDirectories(string fullFolderPath)
        {
            return Directory
                .EnumerateDirectories(fullFolderPath, "*", SearchOption.AllDirectories)
                .Select(Path.GetFullPath).ToList<string>();
        }

        public static void DeleteFiles(string fullFolderPath, string ignoreFilesPathWords, char separator)
        {
            List<string> matchPaths = GetLocalFiles(fullFolderPath)
                .Union(GetLocalDirectories(fullFolderPath)).ToList<string>();

            if (!String.IsNullOrEmpty(ignoreFilesPathWords))
            {
                string[] pathPartsToIgnore = ignoreFilesPathWords.Split(new char[] { separator });

                foreach (string part in pathPartsToIgnore)
                {
                    ExplorerFilesUtil.Delete(matchPaths.Where(f => f.Contains(part)));
                }
            }

        }

        public static bool HasFiles(string fullFolderPath)
        {
            return Directory
                .EnumerateFiles(fullFolderPath, "*", SearchOption.AllDirectories)
                .Select(Path.GetFullPath).Count() > 0;
        }

        public static bool IsDirectory(string fullPathFile)
        {
            return Directory.Exists(fullPathFile);
        }

        public static bool IsFile(string fullPathFile)
        {
            return File.Exists(fullPathFile);
        }

        public static Dictionary<string, FileInfo> GetFileInfosByLocalFileNameDictionary(string fullFolderPath, IEnumerable<string> fullPathFiles)
        {
            return fullPathFiles.ToDictionary(f => f.Replace(fullFolderPath + ExplorerFilesUtil.FOLDER_SPLIT, String.Empty), f => new FileInfo(f));
        }

        public static void CreateDirectory(string fullPathFile)
        {
            Directory.CreateDirectory(GetDirectoryPathFromFullPathFile(fullPathFile));
        }

        public static string GetDirectoryPathFromFullPathFile(string fullPathFile)
        {
            /*if(fullPathFile.LastIndexOf(FOLDER_SPLIT) > 0 && fullPathFile.Substring(fullPathFile.LastIndexOf(FOLDER_SPLIT)).IndexOf(".")>0 )
                return fullPathFile.Substring(0, fullPathFile.LastIndexOf(FOLDER_SPLIT) + 1);
            return null;"*/
            if (fullPathFile.LastIndexOf(FOLDER_SPLIT) > 0 && fullPathFile.LastIndexOf(FOLDER_SPLIT) < fullPathFile.Length - 1)
                return fullPathFile.Substring(0, fullPathFile.LastIndexOf(FOLDER_SPLIT) + 1);
            return fullPathFile;
        }

        public static void Delete(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        public static void Delete(IEnumerable<string> fullPaths)
        {
            foreach (string sourceFileName in fullPaths)
            {
                if (File.Exists(sourceFileName))
                {
                    File.SetAttributes(sourceFileName, FileAttributes.Normal);
                    File.Delete(sourceFileName);
                }
                else
                {
                    if (Directory.Exists(sourceFileName))
                    {
                        Directory.Delete(sourceFileName, true);
                    }
                }

            }
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
    int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32.dll")]
        private static extern Boolean CloseHandle(IntPtr hObject);

        public static void Copy(string sourceDir, string targetDir)
        {

            Directory.CreateDirectory(targetDir);
            //GrantFullAccess(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);

            foreach (var directory in Directory.GetDirectories(sourceDir))
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));

        }

        public static void CleanDirectory(string fullPathDir)
        {
            GrantFullAccess(fullPathDir);
            Delete(fullPathDir);
            Directory.CreateDirectory(fullPathDir);
        }

        public static void CreateDirectoryIfNotExists(string fullPathDir)
        {
            GrantFullAccess(fullPathDir);
            CreateDirectory(fullPathDir + ExplorerFilesUtil.FOLDER_SPLIT);
        }


        public static string GetHostNameFrom(string fullLocalNetWorkPath)
        {
            string hostName = null;
            if (IsNetworkPath(fullLocalNetWorkPath) &&
                fullLocalNetWorkPath.Substring(BEGIN_OF_NETWORK_PATH.Length).IndexOf(FOLDER_SPLIT) > BEGIN_OF_NETWORK_PATH.Length)
            {
                hostName = fullLocalNetWorkPath.Substring(BEGIN_OF_NETWORK_PATH.Length);
                hostName = hostName.Substring(0, hostName.IndexOf(FOLDER_SPLIT));
            }
            return hostName;
        }

        public static bool IsNetworkPath(string fullLocalNetWorkPath)
        {
            return fullLocalNetWorkPath.IndexOf(BEGIN_OF_NETWORK_PATH) == 0;
        }

        public static void CopyReplacingFolder(string sourceRootDir, string targetRootDir, IEnumerable<string> targetFullPathFileNames)
        {
            //GrantFullAccess(targetRootDir);
            try
            {
                foreach (string targetFullPathFile in targetFullPathFileNames)
                {
                    string sourceFullPathFile = targetFullPathFile.Replace(targetRootDir, sourceRootDir);
                    ExplorerFilesUtil.CreateDirectory(targetFullPathFile);
                    File.SetAttributes(targetFullPathFile, FileAttributes.Normal);
                    File.Copy(sourceFullPathFile, targetFullPathFile, true);
                }
            }
            catch
            {
                throw;
            }

        }


        public static void CopyNewFiles(string sourceRootDir, string targetRootDir, IEnumerable<string> sourceFullPathFileNames)
        {
            //GrantFullAccess(targetRootDir);
            foreach (string sourceFullPathFile in sourceFullPathFileNames)
            {
                string targetFullPathFile = sourceFullPathFile.Replace(sourceRootDir, targetRootDir);
                ExplorerFilesUtil.CreateDirectory(targetFullPathFile);
                File.Copy(sourceFullPathFile, targetFullPathFile, true);
            }
        }

        public static void CreateFile(string fileName, int fileSize, byte[] rawData, DateTime fileCreatedDateTime, DateTime fileModifiedDateTime, DateTime fileAccessedDateTime)
        {
            ExplorerFilesUtil.CreateDirectory(fileName);

            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {

                fs.Write(rawData, 0, (int)fileSize);
                fs.Close();
            }

            File.SetCreationTimeUtc(fileName, fileCreatedDateTime);
            File.SetLastAccessTimeUtc(fileName, fileModifiedDateTime);
            File.SetLastWriteTimeUtc(fileName, fileAccessedDateTime);
        }

        public static string GetFullPath(string rootPath, string fileName)
        {
            return String.Format("{0}{1}{2}", ExplorerFilesUtil.GetPathWithoutLastFolderSplit(rootPath), FOLDER_SPLIT, fileName);
        }

        public static void DeleteFile(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public static string GetFileContent(string localFilePath)
        {
            if (File.Exists(localFilePath))
            {
                return File.ReadAllText(localFilePath);
            }
            return null;
        }

        public static void CreateNewFile(string fullPath, string content)
        {
            using (TextWriter tw = new StreamWriter(fullPath))
            {
                tw.WriteLine(content);
                tw.Close();
            }
        }

        public static void Copy(string sourceRootDir, string targetRootDir, IEnumerable<string> fileName)
        {
            //GrantFullAccess(targetRootDir);
            foreach (string name in fileName)
            {

                string rootPath = ExplorerFilesUtil.GetFullPath(sourceRootDir, name);
                string targetPath = ExplorerFilesUtil.GetFullPath(targetRootDir, name);

                ExplorerFilesUtil.CreateDirectory(targetPath);
                File.Copy(rootPath, targetPath);
            }
        }

        public static void SetFullAccessPermissionsForEveryone(string directoryPath)
        {
            //Everyone Identity
            IdentityReference everyoneIdentity = new SecurityIdentifier(WellKnownSidType.WorldSid,
                                                       null);

            DirectorySecurity dir_security = Directory.GetAccessControl(directoryPath);

            FileSystemAccessRule full_access_rule = new FileSystemAccessRule(everyoneIdentity,
                            FileSystemRights.FullControl, InheritanceFlags.ContainerInherit |
                             InheritanceFlags.ObjectInherit, PropagationFlags.None,
                             AccessControlType.Allow);
            dir_security.AddAccessRule(full_access_rule);

            Directory.SetAccessControl(directoryPath, dir_security);
        }


        public static void GrantFullAccess(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                foreach (var file in Directory.GetFiles(directoryPath))
                    File.SetAttributes(file, FileAttributes.Normal);

                foreach (var directory in Directory.GetDirectories(directoryPath))
                    GrantFullAccess(directory);
            }

        }

        private static void AddFileSecurity(string fileName, string account,
            FileSystemRights rights, AccessControlType controlType)
        {
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            fSecurity.AddAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            File.SetAccessControl(fileName, fSecurity);
        }

        private static void RemoveFileSecurity(string fileName, string account,
            FileSystemRights rights, AccessControlType controlType)
        {
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            fSecurity.RemoveAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            File.SetAccessControl(fileName, fSecurity);
        }

        public static IEnumerable<string> GetAddedFiles(string localRootSourcePath, string localRootTargetPath)
        {
            IEnumerable<string> sourceFileNames = GetLocalFiles(localRootSourcePath);
            IEnumerable<string> targetFileNames = GetLocalFiles(localRootTargetPath).Select(f => f.Replace(localRootTargetPath, localRootSourcePath)).ToList<string>();
            return GetAddedList(sourceFileNames, targetFileNames);
        }

        public static IEnumerable<string> GetDeletedFiles(string localRootSourcePath, string localRootTargetPath)
        {
            IEnumerable<string> sourceFileNames = GetLocalFiles(localRootSourcePath).Select(f => f.Replace(localRootSourcePath, localRootTargetPath));
            IEnumerable<string> targetFileNames = GetLocalFiles(localRootTargetPath);
            return GetDeletedList(sourceFileNames, targetFileNames);
        }

        public static IEnumerable<string> GetDeletedDirectories(string localRootSourcePath, string localRootTargetPath)
        {
            IEnumerable<string> sourceFileNames = GetLocalDirectories(localRootSourcePath).Select(f => f.Replace(localRootSourcePath, localRootTargetPath));
            IEnumerable<string> targetFileNames = GetLocalDirectories(localRootTargetPath);
            return GetDeletedList(sourceFileNames, targetFileNames);
        }

        private static IEnumerable<string> GetAddedList(IEnumerable<string> sourceFileNames, IEnumerable<string> targetFileNames)
        {
            return sourceFileNames.Except(targetFileNames);
        }

        private static IEnumerable<string> GetDeletedList(IEnumerable<string> sourceFileNames, IEnumerable<string> targetFileNames)
        {
            return targetFileNames.Except(sourceFileNames);
        }

        private static IEnumerable<string> GetListExistingInBoth(IEnumerable<string> sourceFileNames, IEnumerable<string> targetFileNames)
        {
            return sourceFileNames.Intersect(targetFileNames);
        }

        public static bool FileExists(string fullPathFile)
        {
            return File.Exists(fullPathFile);
        }

        public static List<string> GetModifiedFilesFromLocalRepository(string localRootSourcePath, string localRootTargetPath, DiffFilesAttibutesMapCollection diffFilesAttibutesMapCollection, TfsManager tfsComparer)
        {
            XmlComparerManager xmlCompareManager = new XmlComparerManager();
            List<string> diffFiles = new List<string>();

            List<string> sourceFileNames = GetLocalFiles(localRootSourcePath).Select(f => f.Replace(localRootSourcePath, localRootTargetPath)).ToList<string>();
            List<string> targetFileNames = GetLocalFiles(localRootTargetPath).ToList<string>();

            List<string> filesExistingInBoth = GetListExistingInBoth(sourceFileNames, targetFileNames).ToList<string>();
            Dictionary<string, FileInfo> FileInfoByTargetName = GetFileInfosByLocalFileNameDictionary(localRootTargetPath, filesExistingInBoth);

            foreach (string fileName in FileInfoByTargetName.Keys)
            {
                FileInfo sourceFileInfo = new FileInfo(ExplorerFilesUtil.GetFullPath(localRootSourcePath, fileName));

                DiffFilesAttibutesMap diffFilesAttibutesMap = diffFilesAttibutesMapCollection.GetFileTransferCondition(fileName);

                FileTransferCondition fileTransferCondition = diffFilesAttibutesMap == null ? FileTransferCondition.ALWAYS : diffFilesAttibutesMap.FileTransferCondition;
                XmlDiffOptions xmlDiffOptions = diffFilesAttibutesMap == null ? XmlDiffOptions.None : diffFilesAttibutesMap.XMlDiffOptions;
                xmlCompareManager.XmlDiffOptions = xmlDiffOptions;

                if (sourceFileInfo.IsToTransfer(FileInfoByTargetName[fileName], fileTransferCondition))
                {
                    if (sourceFileInfo.IsDifferent(FileInfoByTargetName[fileName], xmlCompareManager, tfsComparer))
                        diffFiles.Add(FileInfoByTargetName[fileName].FullName);
                }
            }

            return diffFiles;
        }

        public static void DeleteFileOrDirectory(string fullPathFile)
        {
            if (IsDirectory(fullPathFile))
            {
                //CleanDirectory(fullPathFile);
                Delete(fullPathFile);
            }
            else
            {
                if (IsFile(fullPathFile))
                    File.Delete(fullPathFile);
            }
        }

    }
}
