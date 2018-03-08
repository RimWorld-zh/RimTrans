using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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
        public readonly string name;

        /// <summary>
        /// If the def is banned to be translated.
        /// 表示 Def 是否禁止翻译。
        /// </summary>
        public readonly bool banned;

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
                if (this.banned) {
                    return Enumerable.Empty<Schema>();
                }
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
        public Schema(string name, List<Schema> fileds = null, bool banned = false) {
            this.name = name;
            this.fileds = fileds ?? new List<Schema>();
            this.banned = banned;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">
        /// The specified xml format schema.
        /// 指定的 xml 格式模式。
        /// </param>
        public Schema(XElement element) {
            this.name = element.Name.ToString();
            this.fileds = element.Elements().Select(el => new Schema(el)).ToList();
            this.baseSchemaName = element.Attribute("Base")?.Value;
            this.banned = element.Attribute("Banned")?.Value == "True";
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
        private static readonly Dictionary<string, Schema> allSchemas =
            new Dictionary<string, Schema>();

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
                foreach (var schema in from el in XDocument.Load(path).Root.Elements()
                                           select new Schema(el)) {
                    Add(schema);
                }
            } catch (Exception ex) {
                Log.Warn($"Failed to load schema definitions file.", ex);
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
            if (allSchemas.ContainsKey(newSchema.name)) {
                foreach (var subdefinition in allSchemas.Values.Where(sd => sd.baseSchemaName == newSchema.name)) {
                    subdefinition.SetBase(newSchema);
                }
                allSchemas[newSchema.name] = newSchema;
            } else {
                allSchemas.Add(newSchema.name, newSchema);
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
            if (allSchemas.TryGetValue(name, out var schema)) {
                return schema;
            } else {
                return root;
            }
        }
    }
}
