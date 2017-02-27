using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using RimTrans.Builder;
using RimTrans.Builder.Xml;

namespace RimTransLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DefinitionData Core_Defs = DefinitionData.Load(@"D:\Game\RimWorld\Mods\Core\Defs");
            InjectionData Core_Original_DefInjected = InjectionData.Parse(Core_Defs);
            //Core_Original_DefInjected.Debug();
            //InjectionData Core_CS_DefInject = InjectionData.Load(@"D:\Game\RimWorld\Mods\Core\Languages\ChineseSimplified\DefInjected");
            //defs.DebugCore();

            //XmlHelper.ReadExtraFieldNames(@"D:\Game\RimWorld\Mods\Core\Languages\ChineseSimplified\Strings\Names\Animal_Female.txt");
            //XmlHelper.ReadExtraFieldNamesParent(@"D:\Game\RimWorld\Mods\Core\Languages\ChineseSimplified\Strings\Names\Animal_Male.txt");
            //XmlHelper.Debug();
        }

        
    }
}
