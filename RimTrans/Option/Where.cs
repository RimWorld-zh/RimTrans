using System;

namespace RimTrans.Option
{
    /// <summary>
    /// Where is the Mod, direct mod or workshop mod
    /// </summary>
    public enum Where
    {
        None = 0,
        Direct = 0x01,
        Workshop = 0x02,
        Custom = 0x04,
    }
}
