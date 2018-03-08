using System;

namespace duduluu.System.Linq {
    internal static class Errors {
        internal static Exception ArgumentNull(string s) => new ArgumentNullException(s);
    }
}
