using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Model
{
    public enum FileTransferCondition
    {
        ALWAYS, NEVER, IF_DIFF
    }
}
