using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

using duduluu.System.Linq;

namespace RimTrans.Core {
    /// <summary>
    /// Schema is used for parsing specified defs and extract injections.
    /// 模式用于解析特定的 def 并提取 injection。
    /// </summary>
    public class Schema {
        /// <summary>
        /// The target field name or def type name.
        /// 目标字段名或 Def 类型名。
        /// </summary>
        public readonly string defType;

        /// <summary>
        /// Fields, inherited members are not included.
        /// 字段，不包括继承的成员。
        /// </summary>
        private readonly List<Schema> fileds;

        /// <summary>
        /// Get all fields, including inherited members.
        /// 获取所有字段，包括继承的成员。
        /// </summary>
        public IEnumerable<Schema> Fields {
            get {
                if (this.BaseSchema != null) {
                    return this.fileds.Concat(this.BaseSchema.Fields);
                } else {
                    return this.fileds;
                }
            }
        }

        /// <summary>
        /// The name for base schema.
        /// 基定义名称。
        /// </summary>
        public readonly string baseSchemaName;

        /// <summary>
        /// The base schema.
        /// 基定义。
        /// </summary>
        public Schema BaseSchema { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Schema(string defType, List<Schema> fileds = null) {
            this.defType = defType;
            this.fileds = fileds ?? new List<Schema>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">
        /// The specified xml format schema.
        /// 指定的 xml 格式模式。
        /// </param>
        public Schema(XElement element) {
            this.defType = element.Attribute("DefType").Value;
            //this.fileds = element.Elements().Select(el => new Schema(el)).ToList();
            this.baseSchemaName = element.Attribute("Base")?.Value;
        }

        /// <summary>
        /// Set base schema.
        /// 设置基模式。
        /// </summary>
        /// <param name="baseSchema">
        /// The base schema.
        /// 基模式。
        /// </param>
        public void SetBase(Schema baseSchema) {
            this.BaseSchema = baseSchema;
        }
    }

    /// <summary>
    /// The schema collection.
    /// 模式合集。
    /// </summary>
    public static class SchemaCollection {
        /// <summary>
        /// The root schema (Def).
        /// 根模式（Def）。
        /// </summary>
        private readonly static Schema root =
            new Schema("Def", new List<Schema> {
                new Schema("label"),
                new Schema("description"),
            });

        /// <summary>
        /// All of schemas.
        /// 所有模式。
        /// </summary>
        private static readonly Dictionary<string, Schema> schemas =
            new Dictionary<string, Schema>();

        /// <summary>
        /// All of banned def type name for translating.
        /// 所有禁止翻译的 Def 类型名称。
        /// </summary>
        private static readonly List<string> banneds = new List<string>();

        /// <summary>
        /// Initialize the schema collection.
        /// 初始化模式合集。
        /// </summary>
        public static void Initialize() {
            Directory
                .GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Schemas"), "*.xml")
                .EachFor(file => Load(file));
        }

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
                var root = XDocument.Load(path).Root;
                root.Elements("Schema").EachFor(el => Add(new Schema(el)));
                root.Elements("Banned").EachFor(el => banneds.Add(el.Attribute("DefType").Name.ToString()));
            } catch (Exception ex) {
                Log.Warn($"Failed to load schema definitions file: '{path}'.", ex);
            }
        }

        /// <summary>
        /// Add a schema to the collection.
        /// 添加一个模式到合集。
        /// </summary>
        /// <param name="newSchema">
        /// The schema need to be added.
        /// 需要添加的模式。
        /// </param>
        private static void Add(Schema newSchema) {
            if (schemas.ContainsKey(newSchema.defType)) {
                foreach (var subdefinition in schemas.Values.Where(sd => sd.baseSchemaName == newSchema.defType)) {
                    subdefinition.SetBase(newSchema);
                }
                schemas[newSchema.defType] = newSchema;
            } else {
                schemas.Add(newSchema.defType, newSchema);
            }
            newSchema.SetBase(Get(newSchema.baseSchemaName));
        }

        /// <summary>
        /// Get the schema by specified name.
        /// If the specified schema no found, it will return the root schema (Def).
        /// 获取指定名称的模式。
        /// 找不到指定模式时会返回根模式（Def）。
        /// </summary>
        /// <param name="name">
        /// The name for the schema.
        /// 模式的名称。
        /// </param>
        /// <returns></returns>
        public static Schema Get(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                return root;
            }
            if (schemas.TryGetValue(name, out var schema)) {
                return schema;
            } else {
                return root;
            }
        }
    }
}
