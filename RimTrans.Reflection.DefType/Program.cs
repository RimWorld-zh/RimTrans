using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimTrans.Reflection.DefType
{
    class Program
    {
        static void Main(string[] args)
        {
            Type tDef = typeof(Def);
            SortedList<string, string> defTypeList = new SortedList<string, string>();

            foreach (Type t in Assembly.Load("Assembly-CSharp").GetTypes())
            {
                //if (t.Namespace == "RimWorld" || t.Namespace == "Verse")
                {
                    Type baseType = t.BaseType;
                    while (baseType != null)
                    {
                        if (baseType == tDef)
                        {
                            defTypeList.Add(t.Name, string.Format("        public static readonly string {0} = \"{0}\";", t.Name));
                            break;
                        }
                        baseType = baseType.BaseType;
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("        public static readonly string {0} = \"{0}\";", tDef.Name);
            foreach (string v in defTypeList.Values)
            {
                Console.WriteLine(v);
            }
        }
    }
}
