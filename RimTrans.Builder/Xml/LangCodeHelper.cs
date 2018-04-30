using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimTrans.Builder.Xml {
    public static class LangCodeHelper {
        private static readonly Dictionary<string, string> langCodesDict = new Dictionary<string, string> {
            { "ChineseSimplified", "zh-cn" },
            { "ChineseTraditional", "zh-tw" },
            { "Czech", "cs" },
            { "Danish", "da" },
            { "Dutch", "nl" },
            { "English", "en" },
            { "Estonian", "et" },
            { "Finnish", "fi" },
            { "French", "fr" },
            { "German", "de" },
            { "Hungarian", "hu" },
            { "Italian", "it" },
            { "Japanese", "ja" },
            { "Korean", "ko" },
            { "Norwegian", "no" },
            { "Polish", "pl" },
            { "Portuguese", "pt" },
            { "PortugueseBrazilian", "pt-br" },
            { "Romanian", "ro" },
            { "Russian", "ru" },
            { "Slovak", "sk" },
            { "Spanish", "es" },
            { "SpanishLatin", "es-la" },
            { "Swedish", "sv" },
            { "Turkish", "tr" },
            { "Ukrainian", "uk" },
        };

        public static string GetCode(string name) {
            string code;
            if (langCodesDict.TryGetValue(name, out code)) {
                return code;
            } else {
                return name;
            }
        }

        public static string GetCodeShort(string name) {
            string code;
            if (langCodesDict.TryGetValue(name, out code)) {
                return code.Replace("-", "");
            } else {
                return name;
            }
        }
    }
}
