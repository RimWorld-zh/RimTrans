using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace RimTrans.Core {
    public class Injection {
        /// <summary>
        /// The def to inject
        /// </summary>
        public readonly Def def;

        /// <summary>
        /// The path to the document of the def
        /// </summary>
        public readonly string filename;

        /// <summary>
        /// The valid comment content before the element.
        /// </summary>
        public readonly string comment;

        /// <summary>
        /// The type of the def, such as 'ThingDef', 'PawnKindDef'....
        /// </summary>
        public readonly string defType;

        /// <summary>
        /// The name (ID) of the def
        /// </summary>
        public readonly string defName;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="def">The def for translating.</param>
        public Injection(Def def) {
            this.def = def;
            this.filename = Path.GetFileName(def.filename);
            this.comment = def.comment;
            this.defType = def.defType;
            this.defName = def.defName;
        }

        private void ParseElement(Schema definition, XElement element) {

        }
    }
}
