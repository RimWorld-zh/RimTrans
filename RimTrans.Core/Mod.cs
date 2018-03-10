using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;


using duduluu.System.Linq;

namespace RimTrans.Core {
    public class Mod {
        /// <summary>
        /// The path to the directory for the mod.
        /// Mod 的目录路径。
        /// </summary>
        public readonly string path;

        /// <summary>
        /// The dependency mods of the mod.
        /// 此 Mod 所依赖的 Mod。
        /// </summary>
        public readonly List<Mod> dependencies = new List<Mod>();

        /// <summary>
        /// All the defs for the mod, use def type name for key.
        /// Mod 的所有 Def, Def 的类型名称作为键。
        /// </summary>
        public readonly Dictionary<string, List<Def>> defsMap = new Dictionary<string, List<Def>>();

        /// <summary>
        /// All the base defs for the mod.
        /// { defType: Def[] }
        /// </summary>
        public readonly Dictionary<string, List<Def>> baseMap = new Dictionary<string, List<Def>>();

        /// <summary>
        /// Create a new <see cref="Mod"/> from a directory.
        /// 从目录创建新的 <see cref="Mod"/>。
        /// </summary>
        /// <param name="path">
        /// Path to the directory for the mod.
        /// Mod 的目录路径。
        /// </param>
        /// <returns></returns>
        public static Mod Load(string path) {
            var result = new Mod(path);
            result.LoadDefsAsync().Wait();
            return result;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">
        /// Path to the directory for the mod.
        /// Mod 的目录路径。
        /// </param>
        private Mod(string path) {
            this.path = path;
        }

        /// <summary>
        /// Asynchronously load all defs for the mod.
        /// 异步地加载 Mod 所有 Def。
        /// </summary>
        /// <returns></returns>
        private async Task LoadDefsAsync() {
            string defsPath = Path.Combine(this.path, "Defs");

            if (!Directory.Exists(defsPath)) return;

            // Task.Run is faster
            var defLists = await Task.WhenAll(Directory.GetFiles(defsPath, "*.xml", SearchOption.AllDirectories).Select(filename => Task.Run<List<Def>>(() => {
                List<Def> result = new List<Def>();
                try {
                    XDocument.Load(filename).Root.Nodes().Aggregate(null, (XNode acc, XNode cur) => {
                        if (cur is XElement el) {
                            result.Add(new Def(filename, el, acc as XComment));
                        }
                        return cur;
                    });
                } catch (Exception ex) {
                    Log.Error($"Failed at loading file: {filename}", ex);
                }
                return result;
            })));

            foreach (var g in from ls in defLists
                              from def in ls
                              group def by def.defType into g
                              select g) {
                this.defsMap.Add(g.Key, g.Where(def => !string.IsNullOrWhiteSpace(def.defName) && !def.isAbstract).ToList());
                this.baseMap.Add(g.Key, g.Where(def => !string.IsNullOrWhiteSpace(def.name)).ToList());
            };

            this.AddDependencies(this);
#if DEBUG
            //this.defsMap.ForEach(kvp => Log.Info($"{kvp.Key}: {kvp.Value.Count}"));
            //this.baseMap.ForEach(kvp => Log.Info($"{kvp.Key}: {kvp.Value.Count}"));
#endif
        }

        /// <summary>
        /// Add a dependency mod for the mod and return self
        /// 为此 Mod 添加一个依赖 Mod，并返回自身。
        /// </summary>
        public Mod AddDependencies(Mod dependency) {
            this.dependencies.Add(dependency);
            return this;
        }
    }
}
