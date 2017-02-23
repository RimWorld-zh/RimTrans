using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RimTrans.Builder
{
    public class DefinitionData
    {
        public string Path { get; private set; }

        private SortedDictionary<string, XDocument> _data;

        private SortedDictionary<string, XDocument> _dataAbstract;


        private DefinitionData()
        {

        }

        #region Loader

        public static DefinitionData Load(string path, DefinitionData definitionDataCore = null)
        {
            DefinitionData definitionData = new DefinitionData();

            return definitionData;
        }

        #endregion

        #region Abstraction and Inheritance

        public static SortedDictionary<string, XDocument> GetAbstraction(DefinitionData definitionData, DefinitionData definitionDataCore = null)
        {
            SortedDictionary<string, XDocument> dataAbstract = new SortedDictionary<string, XDocument>();

            return dataAbstract;
        }

        #endregion
    }
}
