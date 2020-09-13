using VersionControlSnapshot.Model;
using Microsoft.XmlDiffPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Models
{
    public class DiffFilesAttibutesMapCollection : List<DiffFilesAttibutesMap>
    {
        public DiffFilesAttibutesMapCollection(IEnumerable<DiffFilesAttibutesMap> collection) : base(collection)
        {
        }

        public DiffFilesAttibutesMap GetFileTransferCondition(string fullPathFile)
        {
            return this.Where(dF => dF.IsFromFolder(fullPathFile)).FirstOrDefault();
        }


    }
}
