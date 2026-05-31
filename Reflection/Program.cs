using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace RimTrans.Reflection {
  class Program {
    static void Main(string[] args) {
      //ClassInfo.Crawl();
      //var result = new Dictionary<string, object> {
      //  { "classes", ClassInfo.classesOf },
      //  { "enums", EnumInfo.enumsOf },
      //};
      //File.WriteAllText("../Core/type-package.json", JsonConvert.SerializeObject(result, Formatting.Indented).Replace("\r\n", "\n"));

      Test();
    }

    static void Test() {
      var type = typeof(int);
      Console.WriteLine(Nullable.GetUnderlyingType(type) != null);

      Crawler.Crawl(new Dictionary<string, string> {
        { "core", @"H:\workspaces\rw\type-meta\core.json" },
        { @"H:\workspaces\rw\1127530465\Assemblies\Rimatomics.dll", @"H:\workspaces\rw\type-meta\Rimatomics.json" }
      });
    }
  }
}
