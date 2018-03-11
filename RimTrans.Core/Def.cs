using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace RimTrans.Core {
    /// <summary>
    /// Store
    /// </summary>
    public class Def {
        /// <summary>
        /// The path to the document of the def.
        /// Def 文档的路径。
        /// </summary>
        public readonly string filename;

        /// <summary>
        /// The xml element that defined the def in the document.
        /// 文档中定义 Def 的 xml 元素。
        /// </summary>
        public readonly XElement element;

        /// <summary>
        /// The valid comment content before the element.
        /// 位于元素之前的有效的注释内容。
        /// </summary>
        public readonly string comment;

        /// <summary>
        /// The type name of the def, such as "ThingDef", "PawnKindDef"....
        /// Def的类型名称，比如“ThingDef”、“PawnKindDef”……
        /// </summary>
        public readonly string defType;

        /// <summary>
        /// The name (ID) of the def.
        /// Def 的名称（ID）。
        /// </summary>
        public readonly string defName;

        /// <summary>
        /// The name of the def, used to inherit.
        /// Def 的名称，用于继承。
        /// </summary>
        public readonly string name;

        /// <summary>
        /// The name of the base def, used to inherit.
        /// 基 Def 的名称，用于继承。
        /// </summary>
        public readonly string parentName;

        /// <summary>
        /// If the def is abstract or not. A abstract Def means that it won't be instantiated in the game.
        /// Def 是否为抽象。抽象 Def 意味着其在游戏中不会被实例化。
        /// </summary>
        public readonly bool isAbstract;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">
        /// The path to the document of the def.
        /// Def 文档的路径。
        /// </param>
        /// <param name="element">
        /// The xml element that defined the def in the document.
        /// 文档中定义 Def 的 xml 元素。
        /// </param>
        /// <param name="comment">
        /// The valid comment content before the element.
        /// 位于元素之前的有效的注释内容。
        /// </param>
        public Def(string filename, XElement element, XComment comment = null) {
            this.filename = filename;
            this.element = element;
            this.defType = element.Name.ToString();
            this.defName = element.Element("defName")?.Value;
            this.name = element.Attribute("Name")?.Value;
            this.parentName = element.Attribute("ParentName")?.Value;
            this.isAbstract = element.Attribute("Abstract")?.Value == "True";
            
            // process comment
            if (comment != null)
            {
                // All punctuation mark by in ASCII English part:
                // !"#$%&'()*+,-./:;<=>?@[\]^_`{|}~
                // use some of these marks and whitespace
                var content = comment.Value.Trim(new char[] { '#', '$', '%', '&', '*', '+', '-', '/', '=', '@', '\\', '^', '_', '|', '~', '\t', '\n', '\v', '\f', '\r', ' ' });
                if (content.Length > 0 && !content.Contains("\n") && !content.Contains("\r")) {
                    this.comment = content;
                }
            }
        }
    }
}
