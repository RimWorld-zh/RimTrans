using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using RimTrans.Builder;

namespace RimTransLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DefinitionData Core_Defs = DefinitionData.Load(@"D:\Game\RimWorld\Mods\Core\Defs");
            DefinitionData Rimsenal_Feral_Defs = DefinitionData.Load(@"D:\Game\Steam\steamapps\common\RimWorld\Mods\Rimsenal_Feral", Core_Defs);
            //defs.DebugCore();
        }

        
    }
}
