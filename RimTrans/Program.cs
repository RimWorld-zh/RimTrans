using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace RimTrans
{
    class Program
    {
        static void Main(string[] args)
        {
            Mod core = new Mod();
            core.Generate();

            Mod vg = new Mod("706426033", Option.Where.Workshop, core);
            vg.Generate();
            vg.Export();
            Console.WriteLine(vg.Paths.Dir);
            Console.WriteLine(vg.Paths.DefsInjected);
            Console.WriteLine(Path.GetDirectoryName(vg.Paths.DefsInjected));
            //Console.WriteLine(vg.InjectionsSheet);
            foreach (var doc in vg.DefInjectedNew.Values)
            {
                //Console.WriteLine("================================");
                //Console.WriteLine(doc);
            }
        }
    }
}
