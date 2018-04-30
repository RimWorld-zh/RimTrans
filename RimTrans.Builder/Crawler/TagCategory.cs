using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimTrans.Builder.Crawler {
    /// <summary>
    /// Category of TagInfo
    /// </summary>
    public enum TagCategory {
        Unkown,

        Standard,
        Def,
        Enum,

        Flag,
        FlagItem,

        ListSimple,
        ListSimpleItem,

        ListComplex,
        ListComplexItem,

        ListDef,
        ListDefItem,

        ListRef,
        ListRefItem,

        Dict,
        DictItem,
    }
}
