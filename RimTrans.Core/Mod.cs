using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


using duduluu.System.Linq;

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
        /// All the defs for the mod.
        /// { defType: Def[] }
        /// </summary>
        public readonly Dictionary<string, List<Def>> defsMap = new Dictionary<string, List<Def>>();

        /// <summary>
        /// All the base defs for the mod.
        /// { defType: Def[] }
        /// </summary>
        public readonly Dictionary<string, List<Def>> baseMap = new Dictionary<string, List<Def>>();

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

            if (!Directory.Exists(defsPath)) return;

            var defLists = await Task.WhenAll(Directory.GetFiles(defsPath, "*.xml", SearchOption.AllDirectories).Select(async filename => {
                List<Def> result = new List<Def>();
                try {
                    using (var stream = File.OpenText(filename)) {
                        (await XDocument.LoadAsync(stream, LoadOptions.SetBaseUri, new CancellationToken()))
                            .Root
                            .Nodes()
                            .Aggregate(null as XNode, (acc, cur) => {
                                if (cur is XElement el) {
                                    result.Add(new Def(filename, el, acc as XComment));
                                }
                                return cur;
                            });
                    }
                } catch (Exception ex) {
                    Log.Error($"Failed at loading file: {filename}", ex);
                }
                return result;
            }));

            foreach (var g in from ls in defLists
                              from def in ls
                              group def by def.defType into g
                              select g) {
                this.defsMap.Add(g.Key, g.Where(def => !string.IsNullOrWhiteSpace(def.defName) && !def.isAbstract).ToList());
                this.baseMap.Add(g.Key, g.Where(def => !string.IsNullOrWhiteSpace(def.name)).ToList());
            };

#if DEBUG
            //this.defsMap.ForEach(kvp => Log.Info($"{kvp.Key}: {kvp.Value.Count}"));
            //this.baseMap.ForEach(kvp => Log.Info($"{kvp.Key}: {kvp.Value.Count}"));
#endif
        }
    }
}
