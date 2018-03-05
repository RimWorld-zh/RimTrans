using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace RimTrans.Core {
    /// <summary>
    /// Schema definition is used for parsing specified defs and extract injections.
    /// 模式定义用于解析特定的 def 并提取 injection。
    /// </summary>
    public class SchemaDefinition {
        /// <summary>
        /// The target field name or def type name.
        /// 目标字段名或 Def 类型名。
        /// </summary>
        public readonly string name;

        /// <summary>
        /// Fields, inherited members are not included.
        /// 字段，不包括继承的成员。
        /// </summary>
        private readonly List<SchemaDefinition> fileds;

        /// <summary>
        /// Get all fields, including inherited members.
        /// 获取所有字段，包括继承的成员。
        /// </summary>
        public IEnumerable<SchemaDefinition> Fields {
            get {
                if (this.BaseDefinition != null) {
                    return this.fileds.Concat(this.BaseDefinition.Fields);
                } else {
                    return this.fileds;
                }
            }
        }

        /// <summary>
        /// The name for base definition.
        /// 基定义名称。
        /// </summary>
        public readonly string baseDefinitionName;

        /// <summary>
        /// The base definition.
        /// 基定义。
        /// </summary>
        public SchemaDefinition BaseDefinition { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SchemaDefinition(string name, List<SchemaDefinition> fileds = null) {
            this.name = name;
            this.fileds = fileds ?? new List<SchemaDefinition>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">
        /// The specified xml format schema definition.
        /// 指定的 xml 格式模式定义。
        /// </param>
        public SchemaDefinition(XElement element) {
            this.name = element.Name.ToString();
            this.fileds = element.Elements().Select(el => new SchemaDefinition(el)).ToList();
            this.baseDefinitionName = element.Attribute("Base")?.Value;
        }

        /// <summary>
        /// Set base definition.
        /// 设置基定义。
        /// </summary>
        /// <param name="baseDefinition">
        /// The base schema definition.
        /// 基定义。
        /// </param>
        public void SetBase(SchemaDefinition baseDefinition) {
            this.BaseDefinition = baseDefinition;
        }
    }

    /// <summary>
    /// The schema collection.
    /// 模式合集。
    /// </summary>
    public static class Schema {
        /// <summary>
        /// The root base schema definition (Def).
        /// 根基模式定义，（Def）。
        /// </summary>
        private readonly static SchemaDefinition rootBaseDefinition =
            new SchemaDefinition("Def", new List<SchemaDefinition> {
                new SchemaDefinition("label"),
                new SchemaDefinition("description"),
            });

        /// <summary>
        /// All of schema definitions.
        /// 所有模式定义。
        /// </summary>
        private static readonly Dictionary<string, SchemaDefinition> definitions =
            new Dictionary<string, SchemaDefinition>();

        /// <summary>
        /// Get all of schema definitions.
        /// 获取所有模式定义。
        /// </summary>
        private static Dictionary<string, SchemaDefinition> Definitions => definitions;

        /// <summary>
        /// Load schema definitions xml document.
        /// 载入模式定义 xml 文档。
        /// </summary>
        /// <param name="path">
        /// The path to the xml document.
        /// xml 文档的路径。
        /// </param>
        public static void Load(string path) {
            try {
                foreach (var definition in from el in XDocument.Load(path).Root.Elements()
                                           select new SchemaDefinition(el)) {
                    AddDefinition(definition);
                }
            } catch (Exception ex) {
                Log.Warn($"Failed to load schema definitions file.", ex);
            }
        }

        /// <summary>
        /// Get the schema definition by specified name.
        /// Will return the root base definition (Def) when the specified definition no found.
        /// 获取指定名称的模式定义。
        /// 找不到指定定义时会返回根基定义（Def）。
        /// </summary>
        /// <param name="name">
        /// The name for the schema definition.
        /// 模式定义的名称。
        /// </param>
        /// <returns></returns>
        public static SchemaDefinition GetDefinition(string name) {
            if (Definitions.TryGetValue(name, out var definition)) {
                return definition;
            } else {
                return rootBaseDefinition;
            }
        }

        /// <summary>
        /// Add a definition to schema.
        /// 添加一个定义到模式。
        /// </summary>
        /// <param name="definition">
        /// The schema definition need to be added.
        /// 需要添加的模式定义。
        /// </param>
        public static void AddDefinition(SchemaDefinition definition) {
            definition.SetBase(GetDefinition(definition.baseDefinitionName));
            definitions.Add(definition.name, definition);
        }
    }
}
