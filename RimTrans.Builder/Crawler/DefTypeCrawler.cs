using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimTrans.Builder.Crawler {
    /// <summary>
    /// Get all DefTypes for coding the source file 'DefTypeNameOf.cs'.
    /// </summary>
    public static class DefTypeCrawler {
        /// <summary>
        /// Get partial source code or only names.
        /// </summary>
        /// <param name="formating"></param>
        /// <param name="sorting"></param>
        public static string GenCode(bool formating, bool sorting) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(formating ?
                "        public static readonly string Def = \"Def\";" :
                "Def");
            var allSubDefTypes = sorting ?
                typeof(Def).AllSubclasses().OrderBy(t => t.Name) :
                typeof(Def).AllSubclasses();
            foreach (Type curDefType in allSubDefTypes) {
                sb.AppendLine(formating ?
                    $"        public static readonly string {curDefType.Name} = \"{curDefType.Name}\";" :
                    curDefType.Name);
            }
            return sb.ToString();
        }
    }
}
