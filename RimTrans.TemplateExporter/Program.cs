using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using RimTrans.Builder;
using RimTrans.Builder.Crawler;

namespace RimTrans.TemplateExporter {
    class Program {
        static void Main(string[] args) {
            
            #region Application Info

            Assembly asm = Assembly.GetExecutingAssembly();
            string title = asm.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            Console.Title = title;
            Console.OutputEncoding = Encoding.Unicode;

            #endregion

            if (args.Length != 2) {
                Log.Error();
                Log.WriteLine("Need 2 arguments: defsPath and outputPath.");
            } else if (string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1])) {
                Log.Error();
                Log.WriteLine("Need 2 arguments: defsPath and outputPath.");
            } else {
                Gen_DefsTemplate(args[0], args[1]);
            }
            Log.WriteLine();
            Log.Write("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        public static void Gen_DefsTemplate(string defsPath, string outputPath) {
            if (!Directory.Exists(defsPath)) {
                Log.Error();
                Log.WriteLine($"Directory no found: {defsPath}");
                return;
            }

            string sourceCodePath = Path.Combine(Directory.GetCurrentDirectory(), "Assembly-CSharp");
            if (Directory.Exists(sourceCodePath)) {
                Log.Info();
                Log.WriteLine();
            } else {
                sourceCodePath = null;
            }

            DefinitionData coreDefinitionData = DefinitionData.Load(defsPath);

            Capture capture = Capture.Parse(coreDefinitionData);
            capture.ProcessFieldNames(coreDefinitionData);
            coreDefinitionData.Save(Path.Combine(outputPath, "CoreDefsProcessed"));

            Capture templates = Capture.Parse(coreDefinitionData, sourceCodePath, true);
            templates.Save(Path.Combine(outputPath, "Templates"));
        }
    }
}
