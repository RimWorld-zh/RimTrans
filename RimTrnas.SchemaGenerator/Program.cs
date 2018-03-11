using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using duduluu.System.Linq;

namespace RimTrnas.SchemaGenerator {
    class Program {
        static void Main(string[] args) {
            string pathDefs = @"D:\Games\SteamLibrary\steamapps\common\RimWorld\Mods\Core\Defs";
            string pathDefInjeted = @"C:\git\rw\zh-cn\DefInjected";

            XElement root = new XElement("SchemasDefinitions");

            Regex numberRegex = new Regex("[0-9]+");
            Directory
                .GetDirectories(pathDefInjeted)
                .ToDictionary(dir => Path.GetFileName(dir))
                .EachFor(kvp => Directory
                    .GetFiles(kvp.Value)
                    .Select(file => XDocument.Load(file).Root.Elements())
                    .Aggregate((acc, cur) => acc.Concat(cur))
                    .EachFor(
                        el => el
                            .Name
                            .ToString()
                            .Split('.')
                            .Select((s, i) => i == 0
                                ? kvp.Key
                                : numberRegex.IsMatch(s) ? "li" : s)
                            .Aggregate(root, (acc, cur) => {
                                XElement schema = acc.Element(cur);
                                if (schema == null) {
                                    acc.Add(new XElement(cur));
                                    schema = acc.Element(cur);
                                }
                                return schema;
                            })
                    )
                );
            root.Elements()
                .EachFor(el => {
                    el.Element("label")?.Remove();
                    el.Element("description")?.Remove();
                    if (el.HasElements) {
                        el.SetAttributeValue("DefType", el.Name);
                        el.Name = "Schema";
                    }
                })
                .Where((el) => !el.HasElements)
                .ToList()
                .ForEach(el => el.Remove());
            
            List<string> nonBanned = Directory
                .GetDirectories(pathDefInjeted)
                .Where(dir => Directory.GetFiles(dir, "*.xml")?.Length > 0)
                .Select(dir => Path.GetFileName(dir))
                //.EachFor(t => Console.WriteLine(t))
                .ToList();
            var banned = Directory
                .GetFiles(pathDefs, "*.xml", SearchOption.AllDirectories)
                .Select(file => XDocument.Load(file).Root.Elements())
                .ContactAll()
                .GroupBy(el => el.Name.ToString())
                .Where(g => !nonBanned.Contains(g.Key))
                .Select(g => g.Key)
                .EachFor(t => root.Add(new XElement(t)));
            root.Elements()
                .Where(el => el.Name.ToString() != "Schema")
                .EachFor(el => {
                    el.SetAttributeValue("DefType", el.Name);
                    el.Name = "Banned";
                });

            //Console.WriteLine("==== Schema Definitions ====");

            Console.WriteLine(root);
        }
    }
}
