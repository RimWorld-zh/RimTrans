using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using duduluu.System.Linq;

namespace RimTrnas.SchemaGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Regex numberRegex = new Regex("[0-9]+");
            string path = @"C:\git\rw\zh-cn\DefInjected";
            XElement root = new XElement("SchemaDefinitions");
            Directory
                .GetDirectories(path)
                .ToDictionary(dir => Path.GetFileName(dir))
                .ForEach(kvp => Directory
                    .GetFiles(kvp.Value)
                    .Select(file => XDocument.Load(file).Root.Elements())
                    .Aggregate((acc, cur) => acc.Concat(cur))
                    .ForEach(
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

            Console.WriteLine();
            root.Elements()
                .ForEach(el => {
                    el.Element("label")?.Remove();
                    el.Element("description")?.Remove();
                })
                .Where(el => el.HasElements)
                .ForEach(el => {
                    Console.WriteLine(el);
                    Console.WriteLine();
                });
            Console.WriteLine();
        }
    }
}
