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
            DefinitionData CoreDefs = DefinitionData.Load(@"D:\Game\RimWorld\Mods\Core\Defs");
            InjectionData CoreDefInjectedOriginal = InjectionData.Parse(CoreDefs);
            InjectionData CoreDefInjectedExisted_CS = InjectionData.Load(@"D:\Game\RimWorld\Mods\Core\Languages\ChineseSimplified\DefInjected");
            InjectionData CoreDefInjectedNew_CS = new InjectionData(CoreDefInjectedOriginal);
            CoreDefInjectedNew_CS.MatchExisted(CoreDefInjectedExisted_CS);
            CoreDefInjectedNew_CS.Save(@"D:\Git\RWMod\RimWorld-ChineseSimplified\DefInjected");

            //DefinitionData VgDefs = DefinitionData.Load(@"D:\Game\RimWorld\Mods\Vegetable Garden 5.3\Defs", CoreDefs);
            //InjectionData VgDefInjectedOriginal = InjectionData.Parse(VgDefs);
            //InjectionData VgDefInjectedExisted = InjectionData.Load(@"D:\Game\RimWorld\Mods\Vegetable Garden 5.3\Languages\ChineseSimplified\DefInjected");
            //InjectionData VgDefInjectedNew = new InjectionData(VgDefInjectedOriginal);
            //VgDefInjectedNew.MatchCore(CoreDefInjectedExisted_CS);
            //VgDefInjectedNew.MatchExisted(VgDefInjectedExisted);
            //VgDefInjectedNew.Save(@"D:\Game\RimWorld\Mods\Vegetable Garden 5.3\Languages\ChineseSimplified\DefInjected");
        }

        
    }
}
