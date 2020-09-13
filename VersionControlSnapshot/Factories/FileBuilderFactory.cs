using VersionControlSnapshot.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VersionControlSnapshot.Factories
{
    public abstract class FileBuilderFactory
    {
        public const string TARGET_FOLDER_ATTRIBUTE_TAG = "TargetFolder";

        protected XmlDocument _xmlConfig;
        private string _cfgFullPath;
        protected string _rootPath;
        private bool _isValid;

        public bool IsValid { get { return _isValid ; } set { _isValid = value; } }

        public FileBuilderFactory(string cfgFullPath, string rootPath)
        {
            this._cfgFullPath = cfgFullPath;
            this._rootPath = rootPath;
            this.IsValid = false;

            this._xmlConfig = new XmlDocument();
            if (!String.IsNullOrEmpty(this._cfgFullPath))
            {
                IsValid = true;
                this._xmlConfig.Load(cfgFullPath);
            }

        }

        public abstract List<FileBuilder> CreateFileBuilder();

    }
}
