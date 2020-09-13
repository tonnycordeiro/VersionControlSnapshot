using VersionControlSnapshot.Builders;
using VersionControlSnapshot.Factories;
using VersionControlSnapshot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Managers
{
    public class FileBuilderManager
    {
        

        private List<FileBuilderFactory> _fileBuilderFactoryList;
        private List<FileBuilder> _fileBuilderList;
        private List<string> _rootPathsWithErrorList;

        public List<string> RootPathsWithError { get { return _rootPathsWithErrorList; } private set { _rootPathsWithErrorList = value; } }

        public FileBuilderManager()
        {
            _fileBuilderFactoryList = new List<FileBuilderFactory>();
            _fileBuilderList = new List<FileBuilder>();
            _rootPathsWithErrorList = new List<string>();
        }

        public void AddFactory(FileBuilderFactory fileBuilderFactory)
        {
            _fileBuilderFactoryList.Add(fileBuilderFactory);
        }

        private void GenerateFileBuilders()
        {
            foreach(FileBuilderFactory factory in _fileBuilderFactoryList)
            {
                if(factory.IsValid)
                    _fileBuilderList.AddRange(factory.CreateFileBuilder());
            }
        }

        public void ExecuteFileBuilders()
        {
            GenerateFileBuilders();
            foreach (FileBuilder builder in _fileBuilderList)
            {
                try
                {
                    Console.WriteLine(String.Format("{0}{1}", "Start ", builder.GetType().Name));
                    builder.BuildFiles();
                    Console.WriteLine(String.Format("{0}{1}", "End ", builder.GetType().Name));
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }

            foreach (FileBuilder builder in 
                        _fileBuilderList.Where(fb => fb.GetType() == typeof(ProcessCommandLineFileBuilder)))
            {
                ProcessCommandLineFileBuilder processBuilder = (ProcessCommandLineFileBuilder)builder;
                try { 
                    if(processBuilder.Process != null && !processBuilder.Process.HasExited)
                        processBuilder.Process.WaitForExit();

                    if (!ExplorerFilesUtil.HasFiles(processBuilder.TargetFullPath))
                    {
                        throw new Exception(String.Format("Arquivos não gerados ao executar o processo: {0} \ncom os argumentos: {1}",
                            processBuilder.ProcessFileName, processBuilder.ProcessArguments));
                    }
                }
                catch
                {
                    throw;
                }
            }

            

        }
    }
}
