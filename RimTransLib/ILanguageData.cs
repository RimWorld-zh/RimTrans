using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RimTrans.Builder
{
    interface ILanguageData<T>
    {
        T BuildNew(T original, bool isFreshBuild, T core);
        void Save();
        void Save(string path);
    }
}
