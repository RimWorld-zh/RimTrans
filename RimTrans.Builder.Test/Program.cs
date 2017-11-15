using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;

using RimWorld;
using Verse;

using RimTrans.Builder;
using RimTrans.Builder.Xml;
using RimTrans.Builder.Crawler;

namespace RimTransLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            GenHelper.Gen_DefTypeNameOf();
            GenHelper.Gen_DefsTemplate();
        }
    }
}
