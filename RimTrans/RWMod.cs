using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using RimTrans.ModX;

namespace RimTrans
{
    public class Mod
    {
        /// <summary>
        /// Initialize as Core.
        /// </summary>
        public Mod()
            :this("Core", RimTrans.Option.Where.Direct, null)
        {
        }

        /// <summary>
        /// Initialize a mod, base on some cores.
        /// </summary>
        /// <param name="modName">Must be the mod folder name.</param>
        public Mod(string modName, RimTrans.Option.Where where, params Mod[] cores)
        {
            this.Name = modName;
            this.Where = where;
            this.Cores = cores;
            this.Paths = new RimTrans.Option.Paths(modName, where);

            this.PreProcess();
        }

        /// <summary>
        /// Preprocess: load, inherit abstracts, patch genders and some others
        /// </summary>
        private void PreProcess()
        {
            // Load
            this.Defs.LoadDefs(Paths.Defs);
            this.DefInjectedExisting.LoadDefInjected(Paths.DefsInjected);
            this.KeyedExisting.LoadKeyed(Paths.Keyed);
            this.KeyedOriginal.LoadKeyed(Paths.KeyedOriginal);
            this.KeyedNew.LoadKeyed(Paths.KeyedOriginal);

            // Abstracts and Inheritance
            this.AbstrsSheet = new XElement("AbstrsSheet");
            if (Cores != null)
                foreach (var core in this.Cores)
                {
                    this.AbstrsSheet.Add(core.AbstrsSheet.Elements());
                }
            this.AbstrsSheet.Add(this.Defs.ExtractAbstrs());
            this.Defs.Inherit(this.AbstrsSheet);

            // Patch
            this.GendersSheet = new XElement("GendersSheet");
            if (Cores != null)
                foreach (var core in this.Cores)
                {
                    this.GendersSheet.Add(core.GendersSheet.Elements());
                }
            this.GendersSheet.Add(this.Defs.ExtractGenders());
            this.Defs.PatchPawnGenders(this.GendersSheet);
            this.Defs.PatchPawnRelation();
            this.Defs.PatchScenoria();
            this.Defs.PatchSkill();
            this.Defs.PatchStuffAdjective();
            this.Defs.SignIndex();

            this.InjectionsSheet = DefInjectedExisting.GetInjectionsSheet();
        }

        /// <summary>
        /// Generate: 
        /// </summary>
        public void Generate()
        {
            this.DefInjectedOriginal = this.Defs.Generate();
            if (this.Name == "Core")
            {
                this.DefInjectedOriginal.Add(@"TerrainDef\Terrain_Add.xml", XDocument.Parse(Resources.Terrain_Add, LoadOptions.PreserveWhitespace));
            }
            this.DefInjectedOriginal = this.DefInjectedOriginal.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            this.DefInjectedNew = this.DefInjectedOriginal.Clone();

            if (Cores != null)
            {
                this.DefInjectedNew.MatchCore(Cores[0].InjectionsSheet);
            }
            if (Config.IsFieldsExistingAdopt)
            {
                this.DefInjectedNew.MatchExisting(this.InjectionsSheet);
            }
            if (Config.IsFieldsInvalidHold)
            {
                this.DefInjectedNew.MatchFiles(this.DefInjectedExisting);
            }
            this.DefInjectedNew.MatchSelf(); // Match self must at last.
            this.DefInjectedNew = this.DefInjectedNew.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
        }

        /// <summary>
        /// Export
        /// </summary>
        public void Export()
        {
            this.DefInjectedNew.Export(this.Paths.DefsInjected);
            if (Config.IsFilesInvalidDelete)
            {
                this.DefInjectedNew.DeleteInvalidFiles(this.Paths.DefsInjected);
            }
            if (Config.IsFoldersEmptyDelete)
            {
                this.Paths.TargetLanguage.DeleteEmptyFolders();
            }
            this.Paths.DefsInjected.ConvertEntityReference();
        }

        /// <summary>
        /// Name of this mod. Must be the mod folder name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Location of mod.
        /// </summary>
        public RimTrans.Option.Where Where { get; private set; }

        /// <summary>
        /// Some directories of this mod.
        /// </summary>
        public RimTrans.Option.Paths Paths { get; private set; }

        /// <summary>
        /// Inheritance from these.
        /// </summary>
        private Mod[] Cores { get; set; }

        /// <summary>
        /// Storge Def documents.
        /// </summary>
        public Dictionary<string, XDocument> Defs = new Dictionary<string, XDocument>();

        /// <summary>
        /// Storge DefInjected
        /// </summary>
        public Dictionary<string, XDocument> DefInjectedExisting = new Dictionary<string, XDocument>();
        public Dictionary<string, XDocument> DefInjectedOriginal = new Dictionary<string, XDocument>();
        public Dictionary<string, XDocument> DefInjectedNew = new Dictionary<string, XDocument>();

        /// <summary>
        /// Storge Keyed
        /// </summary>
        public Dictionary<string, XDocument> KeyedExisting = new Dictionary<string, XDocument>();
        public Dictionary<string, XDocument> KeyedOriginal = new Dictionary<string, XDocument>();
        public Dictionary<string, XDocument> KeyedNew = new Dictionary<string, XDocument>();

        /// <summary>
        /// Storge abstracts.
        /// </summary>
        public XElement AbstrsSheet { get; private set; }

        /// <summary>
        /// Storge genders information of pawn.
        /// </summary>
        public XElement GendersSheet { get; private set; }

        /// <summary>
        /// The snapshot of the existing DefInjected.
        /// </summary
        public XElement InjectionsSheet { get; private set; }
        
        /// <summary>
        /// If the Mod is exist.
        /// </summary>
        public bool IsExist
        {
            get
            {
                return System.IO.Directory.Exists(this.Paths.Dir);
            }
        }
    }
}
