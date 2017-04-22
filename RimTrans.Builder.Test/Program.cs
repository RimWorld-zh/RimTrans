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
using RimTrans.Builder.Crawler;
using RimTrans.Builder.Wiki;

namespace RimTransLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TestDefTypeCrawler();
        }

        static void TestCrawler()
        {
            DefinitionData coreDefinitionData = DefinitionData.Load(@"D:\Game\Steam\steamapps\common\RimWorld\Mods\Core\Defs");

            Capture capture = Capture.Parse(coreDefinitionData);
            capture.ProcessFieldNames(coreDefinitionData);
            coreDefinitionData.Save(@"D:\Test\Defs Processed");

            string sourceCodePath = @"D:\Translating\A17\Decomplie\Assembly-CSharp";
            Capture templates = Capture.Parse(coreDefinitionData, sourceCodePath, true);
            templates.Save(@"D:\Test\Defs Templates");

            InjectionData coreInjectionData_Original = InjectionData.Parse("Original", coreDefinitionData);
            InjectionData coreInjectionData_zhcn = new InjectionData("ChineseSimplified", coreInjectionData_Original);
            InjectionData coreInjectionData_zhcn_Existed = InjectionData.Load("ChineseSimplified", @"D:\Git\RWMod\RimWorld-ChineseSimplified\DefInjected", false);
            coreInjectionData_zhcn.MatchExisted(coreInjectionData_zhcn_Existed);
            InjectionData coreInjectionData_zhtw = new InjectionData("ChineseTraditional", coreInjectionData_Original);
            InjectionData coreInjectionData_zhtw_Existed = InjectionData.Load("ChineseTraditional", @"D:\Git\RWMod\RimWorld-ChineseTraditional\DefInjected", false);
            coreInjectionData_zhtw.MatchExisted(coreInjectionData_zhtw_Existed);

            WikiData wikiData = WikiData.Parse(coreDefinitionData, templates, coreInjectionData_zhcn, coreInjectionData_zhtw);
            wikiData.Save(@"D:\Test\Wiki Data");
            wikiData.SaveCSV(@"D:\Test\CoreData.csv");
        }

        static void TestDefTypeCrawler()
        {
            Console.Write(DefTypeCrawler.GetCode(true, false));
        }

        static void TestDefName()
        {
            DefinitionData coreDefinitionData = DefinitionData.Load(@"D:\Game\Steam\steamapps\common\RimWorld\Mods\Core\Defs");

            Capture capture = Capture.Parse(coreDefinitionData);
            capture.ProcessFieldNames(coreDefinitionData);

            Console.WriteLine("================");
            foreach (XElement curDef in from doc in coreDefinitionData.Data.Values
                                        from ele in doc.Root.Elements()
                                        select ele)
            {
                XElement defName = curDef.Element("defName");
                XAttribute attrName = curDef.Attribute("Name");
                XAttribute attrAbstract = curDef.Attribute("Abstract");
                if (defName == null)
                {
                    if (attrName == null)
                    {
                        if (attrAbstract == null)
                        {
                            //Console.WriteLine("======== 0 0 0 ========");
                            //Console.WriteLine(curDef.Name.ToString());
                        }
                        else
                        {
                            //Console.WriteLine("======== 0 0 1 ========");
                            //Console.WriteLine($"{curDef.Name.ToString()} Abstract='{attrAbstract.Value}'");
                        }
                    }
                    else
                    {
                        if (attrAbstract == null)
                        {
                            //Console.WriteLine("======== 0 1 0 ========");
                            //Console.WriteLine($"{curDef.Name.ToString()} Name='{attrName.Value}'");
                        }
                        else
                        {
                            //Console.WriteLine("======== 0 1 1 ========");
                            //Console.WriteLine($"{curDef.Name.ToString()} Name='{attrName.Value}' Abstract='{attrAbstract.Value}'");
                        }
                    }
                }
                else
                {
                    if (attrName == null)
                    {
                        if (attrAbstract == null)
                        {

                        }
                        else
                        {
                            //Console.WriteLine("======== 1 0 1 ========");
                            //Console.WriteLine($"{curDef.Name.ToString()} defName='{defName.Value}' Abstract='{attrAbstract.Value}'");
                        }
                    }
                    else
                    {
                        if (attrAbstract == null)
                        {
                            //Console.WriteLine("======== 1 1 0 ========");
                            //Console.WriteLine($"{curDef.Name.ToString()} defName='{defName.Value}' Name='{attrName.Value}'");
                        }
                        else
                        {
                            //Console.WriteLine("======== 1 1 1 ========");
                            //Console.WriteLine($"{curDef.Name.ToString()} defName='{defName.Value}' Name='{attrName.Value}' Abstract='{attrAbstract.Value}'");
                        }
                    }
                }

                if (attrAbstract != null && string.Compare(attrAbstract.Value, "False", true) == 0)
                {
                    //Console.WriteLine($"{curDef.Name.ToString()} defName='{defName.Value}'");
                }
            }
        }
    }
}
