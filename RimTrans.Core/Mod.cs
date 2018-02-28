using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RimTrans.Core {
    public class Mod {
        /// <summary>
        /// The path to the directory for the mod
        /// </summary>
        public readonly string path;

        /// <summary>
        /// The dependency mods of the mod.
        /// </summary>
        public readonly List<Mod> dependencies = new List<Mod>();

        /// <summary>
        /// All the xml documents for the mod.
        /// </summary>
        //public readonly List<XDocument> documents = new List<XDocument>();

        /// <summary>
        /// All the defs for the mod.
        /// { defType: { defName: defElement } }
        /// </summary>
        public readonly Dictionary<string, Dictionary<string, Def>> defsMap = new Dictionary<string, Dictionary<string, Def>>();

        /// <summary>
        /// All the base defs for the mod.
        /// { defType: { name: defElement } }
        /// </summary>
        public readonly Dictionary<string, Dictionary<string, Def>> baseMap = new Dictionary<string, Dictionary<string, Def>>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">The path to the directory for the mod.</param>
        public Mod(string path) {
            this.path = path;
        }

        /// <summary>
        /// Add dependency to the mod.
        /// </summary>
        /// <param name="dependency">The dependency mod</param>
        /// <returns>self</returns>
        public Mod AddDependencies(Mod dependency) {
            this.dependencies.Add(dependency);
            return this;
        }

        /// <summary>
        /// Asynchronously load all defs for the mod.
        /// </summary>
        /// <returns></returns>
        public async Task LoadDefsAsync() {
            string defsPath = Path.Combine(this.path, "Defs");

            if (!Directory.Exists(defsPath)) {
                return;
            }

            var defGroups = await Task.WhenAll(
                Directory
                    .GetFiles(defsPath, "*.xml", SearchOption.AllDirectories)
                    .Select(filename => Task.Run(() => {
                        using (var sr = File.OpenText(filename)) {
                            var document = XDocument.Load(sr, LoadOptions.SetBaseUri);
                            return document.Root.Elements().Select(element => new Def(filename, element, element.NodesBeforeSelf().Last() as XComment));
                        }
                    }))
            );

            var defsAll = from g in defGroups
                          from d in g
                          select d;

            foreach (var def in defsAll) {
                // Add to defs map
                if (!string.IsNullOrWhiteSpace(def.defName) && !def.isAbstract) {
                    if (!this.defsMap.TryGetValue(def.defType, out var subDefsMap)) {
                        subDefsMap = new Dictionary<string, Def>();
                        this.defsMap.Add(def.defType, subDefsMap);
                    }
                    if (!subDefsMap.ContainsKey(def.defName)) {
                        subDefsMap.Add(def.defName, def);
                    } else if (string.Compare(def.filename, subDefsMap[def.defType].filename, true) > -1) {
                        subDefsMap[def.defType] = def;
                    }
                }
                // Add to base map
                if (!string.IsNullOrWhiteSpace(def.name)) {
                    if (!this.baseMap.TryGetValue(def.defType, out var subBaseMap)) {
                        subBaseMap = new Dictionary<string, Def>();
                        this.baseMap.Add(def.defType, subBaseMap);
                    }
                    if (!subBaseMap.ContainsKey(def.name)) {
                        subBaseMap.Add(def.name, def);
                    } else if (string.Compare(def.filename, subBaseMap[def.name].filename, true) > -1) {
                        subBaseMap[def.name] = def;
                    }
                }

             }
         }
    }
}
