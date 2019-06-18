using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace RimTrans.Reflection {
  class Program {
    static void Main(string[] args) {
      ClassInfo.Crawl();
      var result = new Dictionary<string, object> {
        { "classes", ClassInfo.classesOf },
        { "enums", EnumInfo.enumsOf },
      };
      File.WriteAllText("./type-package.json", JsonConvert.SerializeObject(result, Formatting.Indented).Replace("\r\n", "\n"));
    }
  }
}
