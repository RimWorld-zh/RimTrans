using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimTrans.Builder;
using RimTrans.Builder.Crawler;

namespace RimTransLibTest {
    public static class GenHelper {
        public static void Gen_DefTypeNameOf() {
            Console.Write(DefTypeCrawler.GenCode(true, false));
        }

        public static void Gen_DefsTemplate() {
            DefinitionData coreDefinitionData = DefinitionData.Load(@"D:\Games\SteamLibrary\steamapps\common\RimWorld\Mods\Core\Defs");

            Capture capture = Capture.Parse(coreDefinitionData);
            capture.ProcessFieldNames(coreDefinitionData);
            coreDefinitionData.Save(@"C:\git\rw\RimWorld-Defs-Templates\CoreDefsProcessed");

            string sourceCodePath = @"C:\git\rw\RimWorld-Decompile\Assembly-CSharp";
            Capture templates = Capture.Parse(coreDefinitionData, sourceCodePath, true);
            templates.Save(@"C:\git\rw\RimWorld-Defs-Templates\Templates");
        }
    }
}
