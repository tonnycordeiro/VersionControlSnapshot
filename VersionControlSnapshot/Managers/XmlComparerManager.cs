using Microsoft.XmlDiffPatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VersionControlSnapshot.Managers
{
    public class XmlComparerManager
    {
        private XmlDocument _diffXmlDocument;
        private XmlDiffOptions _xmlDiffOptions;
        private XmlDiff _xmldiff;

        public XmlDiffOptions XmlDiffOptions { get { return _xmlDiffOptions; } set { _xmlDiffOptions = value; _xmldiff = new XmlDiff(this.XmlDiffOptions); } }

        public XmlComparerManager(XmlDiffOptions xmlDiffOptions = XmlDiffOptions.None)
        {
            this.XmlDiffOptions = xmlDiffOptions;
            _xmldiff = new XmlDiff(this.XmlDiffOptions);
        }

        //XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreNamespaces | XmlDiffOptions.IgnorePrefixes

        public bool IsDifferent(string sourceLocalPath, string targetLocalPath)
        {
            return !_xmldiff.Compare(sourceLocalPath, targetLocalPath, false);
        }

        private bool GetDiffXmlDocument(string sourceLocalPath, string targetLocalPath)
        {
            XmlDocument diffXmlDocument = new XmlDocument();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                XmlWriter diffGramWriter = XmlWriter.Create(memoryStream);
                GenerateDiffGram(sourceLocalPath, targetLocalPath, diffGramWriter);
                diffGramWriter.Flush();

                diffXmlDocument.Load(memoryStream);
                return HasDifferences(diffXmlDocument);
            }
        }


        private static bool HasDifferences(XmlDocument diffXmlDocument)
        {
            return diffXmlDocument.HasChildNodes && diffXmlDocument.ChildNodes[0].HasChildNodes;
        }

        private bool GenerateDiffGram(string originalFile, string finalFile,
                                            XmlWriter diffGramWriter)
        {
            
            bool bIdentical = _xmldiff.Compare(originalFile, finalFile, false);
            diffGramWriter.Close();

            return bIdentical;
        }

        public void PatchUp(string originalFile, string diffGramFile, string outputFile)
        {
            XmlDocument sourceDoc = new XmlDocument(new NameTable());
            sourceDoc.Load(originalFile);
            XmlTextReader diffgramReader = new XmlTextReader(diffGramFile);

            XmlPatch xmlPatch = new XmlPatch();
            xmlPatch.Patch(sourceDoc, diffgramReader);

            XmlTextWriter output = new XmlTextWriter(outputFile, Encoding.UTF8);
            sourceDoc.Save(output);
            output.Close();
        }

    }
}
