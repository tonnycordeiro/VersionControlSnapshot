using VersionControlSnapshot.Factories;
using VersionControlSnapshot.Models;
using VersionControlSnapshot.Utils;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace VersionControlSnapshot.Managers
{
    public class TfsManager : IDisposable
    {
        public const char SERVER_FOLDER_SPLIT = '/';

        private const string SERVER_PATH_STARTS_WITH = "$";
        private const string CHECKIN_ADD_COMMENT = "(ADD)";
        private const string CHECKIN_EDIT_COMMENT = "(EDIT)";
        private const string CHECKIN_DEL_COMMENT = "(DEL)";


        private Workspace _currentWorkspace;
        private VersionControlServer _versionControlServer;
        private TfsTeamProjectCollection _tfsTeamProjectCollection;
        private string _localProjectPath;
        private string _serverProjectPath;

        public TfsManager(string collectionUri, string localProjectPath, string serverProjectPath, string userName, string password, string owner, string workspaceName)
        {
            NetworkCredential cred = new NetworkCredential(userName, password);

            _tfsTeamProjectCollection = new TfsTeamProjectCollection(new Uri(collectionUri), cred);
            Console.WriteLine("EnsureAuthenticated start");
            _tfsTeamProjectCollection.EnsureAuthenticated();
            Console.WriteLine("EnsureAuthenticated end");

            Console.WriteLine("GetService start");
            _versionControlServer = _tfsTeamProjectCollection.GetService(typeof(VersionControlServer)) as VersionControlServer;
            Console.WriteLine("GetService end");

            _localProjectPath = localProjectPath;
            _serverProjectPath = serverProjectPath;

            Console.WriteLine("DownloadProjectFromTfs start");

            if (String.IsNullOrEmpty(owner))
            {
                if (userName.IndexOf("\\") > 0)
                    owner = userName.Substring(userName.IndexOf("\\") + 1);
                else
                    owner = userName;
            }

            _currentWorkspace = DownloadProjectFromTfs(owner, workspaceName);
            Console.WriteLine("DownloadProjectFromTfs end");

            Console.WriteLine("EnsureUpdateWorkspaceInfoCache start");
            Workstation.Current.EnsureUpdateWorkspaceInfoCache(_versionControlServer, _versionControlServer.AuthorizedUser);
            Console.WriteLine("EnsureUpdateWorkspaceInfoCache end");

        }

        public Workspace DownloadProjectFromTfs(string workspaceOwner, string workspaceName)//string workspaceOwner
        {
            try
            {
                Workspace workspace = null;


                if (String.IsNullOrEmpty(workspaceName))
                    workspaceName = System.Environment.MachineName;

                try
                {
                    workspace = _versionControlServer.GetWorkspace(workspaceName, workspaceOwner); 
                }
                catch
                {
                    try
                    {
                        workspace = _versionControlServer.GetWorkspace(this._localProjectPath);
                    }
                    catch
                    {
                        ExplorerFilesUtil.CreateDirectoryIfNotExists(this._localProjectPath);
                        workspace = _versionControlServer.CreateWorkspace(workspaceName, workspaceOwner);
                        var workfolder = new WorkingFolder(this._serverProjectPath, this._localProjectPath);
                        workspace.CreateMapping(workfolder);
                    }
                }

                workspace.Get();

                return workspace;
            }
            catch
            {
                throw;
            }
        }

        public static void BuildWindowsCredentials(String url, String username, String password)
        {
            ProcessFactory processFactory = new ProcessFactory();

            Process rdcProcess = processFactory.getWindowsCredentialsProcess(url, username, password);
            rdcProcess.Start();
        }

        public void DownloadLatestVersionProject()
        {
            try
            {
                ItemSet items = _versionControlServer.GetItems(this._serverProjectPath, VersionSpec.Latest, RecursionType.Full);
                foreach (Item item in items.Items)
                {
                    string relativePath = item.ServerItem.Replace(this._serverProjectPath, this._localProjectPath).Replace(SERVER_FOLDER_SPLIT, ExplorerFilesUtil.FOLDER_SPLIT);
                    if (item.ItemType == ItemType.Folder)
                    {
                        Directory.CreateDirectory(relativePath);
                    }
                    else
                    {
                        item.DownloadFile(relativePath);
                    }
                }

                List<string> notMappedFiles = this.GetNotMappedFiles();
                ExplorerFilesUtil.Delete(notMappedFiles);
            }
            catch
            {
                throw;
            }
        }

        public bool IsDifferent(string sourcePath, string targetPath, out string difference)
        {
            IDiffItem sourceItem;
            IDiffItem targetItem;
            string fileName;
            difference = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                DiffOptions diffOptions = new DiffOptions();
                diffOptions.UseThirdPartyTool = false;
                diffOptions.OutputType = DiffOutputType.Unified;

                diffOptions.StreamWriter = new System.IO.StreamWriter(memoryStream);

                sourceItem = GetDiffItem(sourcePath, _versionControlServer);
                targetItem = GetDiffItem(targetPath, _versionControlServer);

                fileName = ExplorerFilesUtil.GetFileName(sourcePath) ?? sourcePath;

                Difference.DiffFiles(_versionControlServer,
                                        sourceItem,
                                        targetItem,
                                        diffOptions,
                                        fileName,
                                        true);

                diffOptions.StreamWriter.Flush();

                difference = Encoding.ASCII.GetString(memoryStream.ToArray());

            }

            return difference.Length > 0;
        }


        private IDiffItem GetDiffItem(string path, VersionControlServer vcServer)
        {
            IDiffItem sourceItem;

            if (path.StartsWith(SERVER_PATH_STARTS_WITH))
            {
                sourceItem = Difference.CreateTargetDiffItem(vcServer, path, VersionSpec.Latest, 0, VersionSpec.Latest);
            }
            else
            {
                sourceItem = new DiffItemLocalFile(path, Encoding.UTF8.CodePage, DateTime.Now, false);
            }

            return sourceItem;
        }

        public void CheckIn(string commentMessage)
        {
            try
            {
                PendingChange[] pendingChanges = this._currentWorkspace.GetPendingChanges().Where(pci => pci.LocalItem.StartsWith(this._localProjectPath)).ToArray<PendingChange>();
                if (pendingChanges.Count() > 0)
                    _currentWorkspace.CheckIn(changes: pendingChanges, comment: commentMessage);
            }
            catch
            {
                throw;
            }
        }

        public void CheckIn(List<string> modifiedFileNamesFromLocalRepository, List<string> addedFullPathFiles, List<string> deletedFullPathFiles, string targetRepositoryPath, string checkInComment)
        {
            try
            {
                if (modifiedFileNamesFromLocalRepository.Count > 0)
                {
                    this.EditFilesInWorkspace(this._localProjectPath, modifiedFileNamesFromLocalRepository);
                    ExplorerFilesUtil.CopyReplacingFolder(targetRepositoryPath, this._localProjectPath, modifiedFileNamesFromLocalRepository);

                    this.CheckIn(String.Format("{0} {1}", checkInComment, CHECKIN_EDIT_COMMENT));
                }

                if (addedFullPathFiles.Count > 0)
                {
                    this.AddFilesInWorkspace(targetRepositoryPath, addedFullPathFiles);

                    this.CheckIn(String.Format("{0} {1}", checkInComment, CHECKIN_ADD_COMMENT));
                }

                if (deletedFullPathFiles.Count > 0)
                {
                    this.DeleteFilesAndDirectoriesInWorkspace(deletedFullPathFiles);

                    this.CheckIn(String.Format("{0} {1}", checkInComment, CHECKIN_DEL_COMMENT));
                }

            }
            catch
            {
                throw;
            }
        }


        private string GetFileChanged(string localFullPathFile)
        {
            string diff = null;
            var serverPath = _currentWorkspace.GetServerItemForLocalItem(localFullPathFile);
            var serverVersion = new DiffItemVersionedFile(_versionControlServer, serverPath, VersionSpec.Latest);
            var localVersion = new DiffItemLocalFile(localFullPathFile, Encoding.UTF8.CodePage, DateTime.Now, false);
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                var diffOptions = new DiffOptions
                {
                    Flags = DiffOptionFlags.EnablePreambleHandling,
                    OutputType = DiffOutputType.Unified,
                    TargetEncoding = Encoding.UTF8,
                    SourceEncoding = Encoding.UTF8,
                    StreamWriter = writer
                };

                Difference.DiffFiles(_versionControlServer, serverVersion, localVersion, diffOptions, serverPath, true);
                writer.Flush();

                diff = Encoding.UTF8.GetString(stream.ToArray());
            }

            return diff ?? String.Empty;
        }

        public void AddFilesInWorkspace(string sourceRootPath, IEnumerable<string> sourceFullPathFiles)
        {
            try
            {
                
                sourceRootPath = ExplorerFilesUtil.GetPathWithoutLastFolderSplit(sourceRootPath);
                Console.WriteLine("sourceRootPath");
                Console.WriteLine(sourceRootPath);
                foreach (string sourcePath in sourceFullPathFiles)
                {
                    Console.WriteLine("targetPath");
                    string targetPath = this._localProjectPath + sourcePath.Replace(sourceRootPath, string.Empty);
                    Console.WriteLine(targetPath);
                    ExplorerFilesUtil.CreateDirectory(targetPath);
                    File.Copy(sourcePath, targetPath, true);
                    _currentWorkspace.PendAdd(targetPath);
                }
            }
            catch
            {
                throw;
            }
        }

        public void DeleteFilesAndDirectoriesInWorkspace(IEnumerable<string> localFullPathFiles)
        {
            try
            {
                foreach (string localPath in localFullPathFiles)
                {
                    string serverPath = ConvertLocalPathToServerPath(localPath);


                    if (!String.IsNullOrEmpty(serverPath))
                    {
                        _currentWorkspace.PendDelete(serverPath);
                        ExplorerFilesUtil.DeleteFileOrDirectory(localPath);
                    }
                }
            }
            catch
            {
                throw;
            }
        }


        public List<string> GetNotMappedFiles()
        {
            List<string> notMappedFiles = new List<string>();
            List<string> fullPathFiles = ExplorerFilesUtil.GetLocalFiles(this._localProjectPath).ToList<string>();

            foreach (string fullPathFile in fullPathFiles)
            {
                if (String.IsNullOrEmpty(ConvertLocalPathToServerPath(fullPathFile)))
                    notMappedFiles.Add(fullPathFile);
            }

            return notMappedFiles;
        }

        private string ConvertLocalPathToServerPath(string localPath)
        {
            bool hasServerVersion = true;
            string serverPath = this._currentWorkspace.GetServerItemForLocalItem(localPath);
            try
            {
                DiffItemVersionedFile serverVersion = new DiffItemVersionedFile(_versionControlServer, serverPath, VersionSpec.Latest);

            }
            catch
            {
                hasServerVersion = false;
            }

            if (!hasServerVersion && ExplorerFilesUtil.IsFile(localPath))
                return null;
            else
                return serverPath;
        }

        public void EditFilesInWorkspace(string sourceRootPath, IEnumerable<string> sourceFullPathFiles)
        {
            try
            {
                sourceRootPath = ExplorerFilesUtil.GetPathWithoutLastFolderSplit(sourceRootPath);
                foreach (string sourcePath in sourceFullPathFiles)
                {
                    string targetPath = this._localProjectPath + sourcePath.Replace(sourceRootPath, string.Empty);
                    string serverPath = ConvertLocalPathToServerPath(targetPath);
                    if (!String.IsNullOrEmpty(serverPath))
                        _currentWorkspace.PendEdit(serverPath);
                }
            }
            catch
            {
                throw;
            }
        }

        public List<string> GetServerFiles()
        {
            ItemSet items = _versionControlServer.GetItems(this._serverProjectPath, RecursionType.Full);
            return items.Items.Where(i => i.ItemType == ItemType.File).Select(item => item.ServerItem.ToString()
                                 .Replace(this._serverProjectPath, String.Empty)).ToList<string>();
        }

        public void Dispose()
        {
            _tfsTeamProjectCollection.Dispose();
        }

        public void GetDiffByFolders(string targetRepositoryPath, out List<string> modifiedFileNamesFromLocalRepository, out List<string> addedFullPathFiles, out List<string> deletedFullPathFiles, DiffFilesAttibutesMapCollection diffFilesAttibutesMapCollection)
        {
            modifiedFileNamesFromLocalRepository = ExplorerFilesUtil.GetModifiedFilesFromLocalRepository(targetRepositoryPath, this._localProjectPath, diffFilesAttibutesMapCollection, this).ToList<string>();
            addedFullPathFiles = ExplorerFilesUtil.GetAddedFiles(targetRepositoryPath, this._localProjectPath).ToList<string>();
            deletedFullPathFiles = ExplorerFilesUtil.GetDeletedFiles(targetRepositoryPath, this._localProjectPath)
                .Union(ExplorerFilesUtil.GetDeletedDirectories(targetRepositoryPath, this._localProjectPath))
                .ToList<string>();
        }
    }
}
