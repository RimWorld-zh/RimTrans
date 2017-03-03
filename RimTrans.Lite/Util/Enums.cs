using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimTrans.Lite.Util
{
    /// <summary>
    /// For indicating where the mod is.
    /// </summary>
    public enum ModCategory
    {
        /// <summary>
        /// The mod in game's "Mods" directory.
        /// </summary>
        Internal,

        /// <summary>
        /// The mod from steam workshop, in the "294100" directory
        /// </summary>
        Workshop,

        /// <summary>
        /// The mod from custom path.
        /// </summary>
        Custom,
    }

    public enum GenerateMode
    {
        /// <summary>
        /// For Core
        /// </summary>
        Core,

        /// <summary>
        /// Mod base Core
        /// </summary>
        Standard,

        /// <summary>
        /// Mod without Core
        /// </summary>
        Special
    }
}
