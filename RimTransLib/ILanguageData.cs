using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RimTransLib
{
    interface ILanguageData<T>
    {
        T BuildNew(T original, bool isRebuild, T core);
        void Save();
        void Save(string path);
    }
}
