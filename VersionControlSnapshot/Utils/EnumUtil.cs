using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionControlSnapshot.Utils
{
    public static class EnumUtil
    {
        public static T GetEnum<T>(string enumName, T enumDefault)
        {
            T enumParsed = enumDefault;
            if (Enum.GetNames(typeof(T)).Contains(enumName))
            {
                enumParsed = (T)Enum.Parse(typeof(T), enumName);
            }

            return enumParsed;
        }
    }
}
