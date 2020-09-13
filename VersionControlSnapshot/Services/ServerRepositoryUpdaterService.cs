using VersionControlSnapshot.Factories;
using VersionControlSnapshot.Managers;
using VersionControlSnapshot.Models;
using VersionControlSnapshot.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Services
{
    public class ServerRepositoryUpdaterService
    {
        private const char IGNORE_FILES_PATH_SEPARATOR = '|';
        private int maximunNumberOfTries;

        public ServerRepositoryUpdaterService()
        {
            maximunNumberOfTries = Properties.Settings.Default.NumberOfTries;
        }

        public void stop()
        {
        }

        public void start()
        {
            UpdateServerRepository();

        }

        public void UpdateServerRepository()
        {
            TfsManager tfsManager;

            List<string> modifiedFileNamesFromLocalRepository = new List<string>();
            List<string> addedFullPathFiles = new List<string>();
            List<string> deletedFullPathFiles = new List<string>();

            string tempRepositoryPathToCurrentFiles = Properties.Settings.Default.TempRepositoryPathToCurrentFiles;
            string localRepositoryPath = Properties.Settings.Default.LocalRepositoryPath;
            string collectionURI = Properties.Settings.Default.CollectionURI;
            string serverRepositoryPath = Properties.Settings.Default.ServerRepositoryPath;
            string diffFilesAttributesMap_FilePath = Properties.Settings.Default.DiffFilesAttributesMap_FilePath;
            string networkPathAccessSettings_FilePath = Properties.Settings.Default.NetworkPathAccessSettings_FilePath;
            string tfsUserName = Properties.Settings.Default.TfsUserName;
            string tfsPassword = Properties.Settings.Default.TfsPassword;
            string tfsOwner = Properties.Settings.Default.TfsOwner;
            string workspaceName = Properties.Settings.Default.WorkspaceName;
            bool downloadLatestVersion = Properties.Settings.Default.DownloadFilesFromTFS;

            bool hasErrors = false;

            string checkInComment = Properties.Settings.Default.CheckInComment;

            DiffFilesAttibutesMapCollection diffFilesAttibutesMapCollection;
            DiffFilesAttributesMapFactory diffFilesAttributesMapFactory;

            string logFileName = Properties.Settings.Default.LogFileName;
            string logFile = ExplorerFilesUtil.GetFullPath(tempRepositoryPathToCurrentFiles, logFileName);
            string logFullPathFileInLocalRepositoryPath = null;

            ExplorerFilesUtil.CleanDirectory(tempRepositoryPathToCurrentFiles);

            for (int tryTime = 1; tryTime <= maximunNumberOfTries; tryTime++)
            {
                try
                {

                    if (!String.IsNullOrEmpty(networkPathAccessSettings_FilePath))
                    {
                        Console.WriteLine("Start ProvideNetworkAccess");
                        NetworkPathAccessManager networkPathAccessManager = new NetworkPathAccessManager(networkPathAccessSettings_FilePath);
                        networkPathAccessManager.ProvideNetworkAccess();
                        Console.WriteLine("End ProvideNetworkAccess");
                    }

                    BuilFiles();
                    break;
                }
                catch (Exception ex)
                {
                    if (tryTime == maximunNumberOfTries)
                    {
                        Console.WriteLine(String.Format("{0}{1}", "Error: ", ex.ToString()));
                        CreateErrorLogFile(logFile, ex);
                        hasErrors = true;
                    }
                }
            }

            for (int tryTime = 1; tryTime <= maximunNumberOfTries; tryTime++)
            {
                try
                {
                    tfsManager = new TfsManager(collectionURI, localRepositoryPath, serverRepositoryPath, tfsUserName, tfsPassword, tfsOwner, workspaceName);

                    if (downloadLatestVersion)
                    {
                        Console.WriteLine("Start DownloadLatestVersionProject");
                        tfsManager.DownloadLatestVersionProject();
                        Console.WriteLine("End DownloadLatestVersionProject");
                    }

                    if (!hasErrors)
                    {
                        diffFilesAttributesMapFactory = new DiffFilesAttributesMapFactory(diffFilesAttributesMap_FilePath);
                        diffFilesAttibutesMapCollection = diffFilesAttributesMapFactory.GetDiffFilesAttibutesMapCollection();
                        tfsManager.GetDiffByFolders(tempRepositoryPathToCurrentFiles, out modifiedFileNamesFromLocalRepository,
                            out addedFullPathFiles, out deletedFullPathFiles, diffFilesAttibutesMapCollection);

                        CreateSuccessfulLogFile(logFile, String.Format("Adicionados: {0} itens - Excluídos: {1} itens - Editados: {2} itens",
                            addedFullPathFiles.Count, deletedFullPathFiles.Count, modifiedFileNamesFromLocalRepository.Count));
                    }

                    logFullPathFileInLocalRepositoryPath = logFile.Replace(tempRepositoryPathToCurrentFiles, localRepositoryPath);
                    if (ExplorerFilesUtil.FileExists(logFullPathFileInLocalRepositoryPath))
                        modifiedFileNamesFromLocalRepository.Add(logFullPathFileInLocalRepositoryPath);
                    else
                        addedFullPathFiles.Add(logFile);

                    if (hasErrors)
                        checkInComment = checkInComment + " (ERROR) ";
                    else
                    {
                        if ((modifiedFileNamesFromLocalRepository.Count + addedFullPathFiles.Count + deletedFullPathFiles.Count) == 1)
                            checkInComment = checkInComment + " (NO CHANGES) ";
                    }

                    Console.WriteLine("Start Check-in");
                    tfsManager.CheckIn(modifiedFileNamesFromLocalRepository, addedFullPathFiles, deletedFullPathFiles, tempRepositoryPathToCurrentFiles, checkInComment);
                    Console.WriteLine("Check-in done");

                    break;
                }
                catch (Exception ex)
                {
                    if (tryTime == maximunNumberOfTries)
                    {
                        Console.WriteLine("Error: " + ex.ToString());
                        CreateErrorLogFile(logFile, ex);
                    }
                }
            }
        }


        private void BuilFiles()
        {
            string tempRepositoryPathToCurrentFiles = Properties.Settings.Default.TempRepositoryPathToCurrentFiles;
            string processCommandLinePropertiesSource_FilePath = Properties.Settings.Default.ProcessCommandLinePropertiesSource_FilePath;
            string mapFolderPropertiesSource_FilePath = Properties.Settings.Default.MapFolderPropertiesSource_FilePath;
            string sqlPropertiesSource_FilePath = Properties.Settings.Default.SqlPropertiesSource_FilePath;
            string ignoreFilesPathWords = Properties.Settings.Default.IgnoreFilesPathWords;

            try
            {
                FileBuilderManager fileBuilderManager = new FileBuilderManager();

                fileBuilderManager.AddFactory(
                    new ProcessCommandLineFileBuilderFactory(processCommandLinePropertiesSource_FilePath,
                                                             tempRepositoryPathToCurrentFiles));

                fileBuilderManager.AddFactory(
                    new MapFolderFileBuilderFactory(mapFolderPropertiesSource_FilePath,
                                                             tempRepositoryPathToCurrentFiles));

                fileBuilderManager.AddFactory(
                    new SqlFileBuilderFactory(sqlPropertiesSource_FilePath,
                                                             tempRepositoryPathToCurrentFiles));

                fileBuilderManager.ExecuteFileBuilders();

                ExplorerFilesUtil.DeleteFiles(tempRepositoryPathToCurrentFiles, ignoreFilesPathWords, IGNORE_FILES_PATH_SEPARATOR);
            }
            catch
            {
                throw;
            }
        }

        private void CreateSuccessfulLogFile(string logPathFile, string message)
        {
            StringBuilder erroMessageStrBuilder = new StringBuilder();
            erroMessageStrBuilder.AppendLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
            erroMessageStrBuilder.AppendLine();
            erroMessageStrBuilder.AppendLine("Sucesso:");
            erroMessageStrBuilder.AppendLine(message);

            ExplorerFilesUtil.CreateNewFile(logPathFile, erroMessageStrBuilder.ToString());
        }

        private void CreateErrorLogFile(string logPathFile, Exception ex)
        {
            StringBuilder erroMessageStrBuilder = new StringBuilder();
            erroMessageStrBuilder.AppendLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
            erroMessageStrBuilder.AppendLine();
            erroMessageStrBuilder.AppendLine("Message:");
            erroMessageStrBuilder.AppendLine(ex.Message);
            erroMessageStrBuilder.AppendLine();
            erroMessageStrBuilder.AppendLine("StackTrace:");
            erroMessageStrBuilder.AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                erroMessageStrBuilder.AppendLine();
                erroMessageStrBuilder.AppendLine("InnerException:");
                erroMessageStrBuilder.AppendLine("Message:");
                erroMessageStrBuilder.AppendLine(ex.InnerException.Message);
                erroMessageStrBuilder.AppendLine();
                erroMessageStrBuilder.AppendLine("StackTrace:");
                erroMessageStrBuilder.AppendLine(ex.InnerException.StackTrace);
            }

            ExplorerFilesUtil.CreateNewFile(logPathFile, erroMessageStrBuilder.ToString());
        }
    }
}
