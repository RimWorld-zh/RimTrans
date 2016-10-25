using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using RimTrans.Xml;

namespace RimTrans.ModX
{
    internal static class GeneratorX
    {
        /// <summary>
        /// Clone
        /// </summary>
        public static Dictionary<string, XDocument> Clone(this Dictionary<string, XDocument> dict)
        {
            var result = new Dictionary<string, XDocument>();
            foreach (var kvp in dict)
            {
                result.Add(kvp.Key, new XDocument(kvp.Value));
            }
            return result;
        }

        /// <summary>
        /// Get the document by key, if not existing, create new one;
        /// </summary>
        public static XDocument Document(this Dictionary<string, XDocument> dict, string key)
        {
            XDocument doc = null;
            foreach (var kvp in dict)
            {
                if (string.Compare(key, kvp.Key, true) == 0)
                {
                    doc = kvp.Value;
                    break;
                }
            }
            if (doc == null)
            {
                doc = XDocument.Parse(Resources.EmptyDoc, LoadOptions.PreserveWhitespace);
                dict.Add(key, doc);
            }
            return doc;
        }

        /// <summary>
        /// How many fields in this collection
        /// </summary>
        public static int CountFields(this List<object> contents)
        {
            int result = 0;
            foreach (var content in contents)
            {
                if (content.GetType() == typeof(XElement)) result++;
            }
            return result;
        }

        /// <summary>
        /// Generate!!!!
        /// </summary>
        public static Dictionary<string, XDocument> Generate(this Dictionary<string, XDocument> defs)
        {
            Dictionary<string, XDocument> defInjected = new Dictionary<string,XDocument>();

            // Special Recipes_Add.xml
            XDocument recipes = XDocument.Parse(Resources.Recipes_Add, LoadOptions.PreserveWhitespace);

            foreach (var kvp in defs)
            {
                string fileName = kvp.Key.Substring(kvp.Key.LastIndexOf('\\') + 1);

                XDocument docDef = kvp.Value;

                XComment comm = null;

                // For files which have multiple DefType, like races of Core.
                List<DefType> commentSheet = null;
                int[] countSingleFieldsSheet = new int[100];
                // For typesetting of guns and projectiles
                bool isPrevFieldProjectile = false;

                foreach (var node in docDef.Root.Nodes())
                {
                    if (Config.IsCommentExtract && node.NodeType == XmlNodeType.Comment && (node as XComment).IsValid())
                    {
                        comm = node as XComment;
                        if (Config.IsCommentTypesetting) comm.Value = comm.Value.TrimComm();
                        commentSheet = new List<DefType>();
                    }

                    if (node.NodeType == XmlNodeType.Element && (node as XElement).HasDefName())
                    {
                        XElement def = node as XElement;
                        DefType defType = def.GetDefType();
                        ThingCategory category = def.Category();

                        if ((defType == DefType.ThingDef && category == ThingCategory.Mote) == false &&
                            defType != DefType.SongDef &&
                            defType != DefType.SoundDef)
                        {
                            string keyDefInjected;
                            if (defType == DefType.Unknown)
                            {
                                keyDefInjected = def.Name.ToString() + "\\" + fileName;
                            }
                            else
                            {
                                keyDefInjected = defType.ToString() + "\\" + fileName;
                            }
                            XDocument docDefInjected = defInjected.Document(keyDefInjected);

                            XElement defName = def.Field(FieldType.defName);
                            if (defName.IsValidDefName())
                            {
                                // Generate Field
                                var contents = new List<object>();
                                contents.AddRange(def.GenerateNormal(defName));
                                if (defType == DefType.ThingDef && category == ThingCategory.Pawn)
                                    contents.AddRange(def.GeneratePawn(defName));
                                if (Config.IsGenBuildingExtra &&
                                    (defType == DefType.ThingDef || defType == DefType.TerrainDef))
                                    contents.AddRange(def.GenerateBuildingExtra(defName));

                                // Special Recipes_Add.xml
                                if (defType == DefType.ThingDef) recipes.AddRecipe(def, defName);

                                int countMultiFields = contents.CountFields();

                                // Inset Comment
                                if (countMultiFields > 0)
                                {
                                    if (Config.IsCommentExtract && comm != null && commentSheet.IndexOf(defType) < 0)
                                    {
                                        commentSheet.Add(defType);
                                        if (docDefInjected.Root.HasElements) docDefInjected.Root.Add("\n");
                                        if (countSingleFieldsSheet[(int)defType] > 0)
                                        {
                                            docDefInjected.Root.Add("\n");
                                            countSingleFieldsSheet[(int)defType] = 0;
                                        }
                                        docDefInjected.Root.Add(Config.IndentSpaces, comm, "\n\n");
                                    }
                                }

                                // Inset Blank Line between fields groups
                                if (countMultiFields == 1)
                                {
                                    countSingleFieldsSheet[(int)defType]++;
                                    if (def.GetDefType() == DefType.ThingDef && def.Category() == ThingCategory.Projectile)
                                        isPrevFieldProjectile = true;
                                    else
                                        isPrevFieldProjectile = false;
                                }
                                else if (countMultiFields > 1)
                                {
                                    if (countSingleFieldsSheet[(int)defType] == 1)
                                    {
                                        if (isPrevFieldProjectile == false) docDefInjected.Root.Add("\n");
                                    }
                                    else if (countSingleFieldsSheet[(int)defType] > 1)
                                    {
                                        docDefInjected.Root.Add("\n");
                                    }
                                    countSingleFieldsSheet[(int)defType] = 0;
                                    contents.Add("\n");
                                }

                                // Inset Fields
                                docDefInjected.Root.Add(contents);
                            }
                            else
                            {
                                //TODO: Log
                            }

                        }

                    }
                }

                // Inset Blank Line at last
                for (int i = 0; i < countSingleFieldsSheet.Length; i++)
                {
                    if (countSingleFieldsSheet[i] > 0)
                    {
                        string key = Enum.GetName(typeof(DefType), i) + "\\" + fileName;
                        defInjected.Document(key).Root.Add("\n");
                    }
                }
            }

            // Special Recipes_Add.xml
            if (recipes.Root.HasElements) defInjected.Add(@"RecipeDef\Recipes_Add.xml", recipes);

            // Remove Empty Documents and Inset Blank line
            List<string> keysEmptyDoc = new List<string>();
            foreach (var kvp in defInjected)
            {
                if (kvp.Value.Root.HasElements)
                {
                    kvp.Value.Root.Add("\n");
                }
                else
                {
                    keysEmptyDoc.Add(kvp.Key);
                }
            }
            foreach (var key in keysEmptyDoc)
            {
                defInjected.Remove(key);
            }

            return defInjected;
        }

        /// <summary>
        /// Get the DefInjected fields collection of this Def
        /// </summary>
        private static IEnumerable<object> GenerateNormal(this XElement ele, XElement defName, int level = 0)
        {
            List<object> contents = new List<object>();
            foreach (var field in ele.Elements())
            {
                if (field.HasElements == false && field.IsInjectableField())
                {
                    string fieldFullName = field.FieldName();
                    XElement fieldParent = field.Parent;
                    int lv = level;
                    while (lv > 0)
                    {
                        fieldFullName = fieldParent.FieldName() + "." + fieldFullName;
                        fieldParent = fieldParent.Parent;
                        lv--;
                    }
                    fieldFullName = defName.Value + "." + fieldFullName;
                    contents.Add(Config.IndentSpaces);
                    contents.Add(new XElement(fieldFullName, field.Value));
                    contents.Add("\n");
                }
                else
                {
                    contents.AddRange(field.GenerateNormal(defName, level + 1));
                }
            }
            return contents;
        }

        /// <summary>
        /// Get the fields(meat, leather and corpse) of this races Def.
        /// </summary>
        private static IEnumerable<object> GeneratePawn(this XElement def, XElement defName)
        {
            List<object> contents = new List<object>();
            XElement label = def.Field(FieldType.label);
            if (label != null)
            {
                bool isNoLeather = false;
                bool isMachanoid = false;
                XElement useLeatherFrom = null;
                XElement useMeatFrom = null;
                XElement leatherLabel = null;
                XElement meatLabel = null;

                XElement statBases = def.Field(FieldType.statBases);
                if (statBases != null)
                {
                    XElement leatherAmount = statBases.Field(FieldType.LeatherAmount);
                    if (leatherAmount != null && leatherAmount.Value == "0")
                        isNoLeather = true;
                }
                XElement race = def.Field(FieldType.race);
                if (race != null)
                {
                    XElement fleshType = race.Field(FieldType.fleshType);
                    if (fleshType != null && fleshType.Value.ToLower() == "mechanoid")
                        isMachanoid = true;

                    useLeatherFrom = race.Field(FieldType.useLeatherFrom);
                    useMeatFrom = race.Field(FieldType.useMeatFrom);
                    leatherLabel = race.Field(FieldType.leatherLabel);
                    meatLabel = race.Field(FieldType.meatLabel);
                }

                if (isMachanoid)
                {
                    contents.Add(Config.IndentSpaces);
                    contents.Add(new XComment(" Flesh Type: Mechanoid "));
                    contents.Add("\n");
                }
                else
                {
                    // Leather
                    if (isNoLeather)
                    {
                        contents.Add(Config.IndentSpaces);
                        contents.Add(new XComment(" Leather Amount: 0 "));
                        contents.Add("\n");
                    }
                    else if (useLeatherFrom != null)
                    {
                        contents.Add(Config.IndentSpaces);
                        contents.Add(new XComment(string.Format(" Use Leather From: {0} ", useLeatherFrom.Value)));
                        contents.Add("\n");
                    }
                    else
                    {
                        string leather = string.Format("{0} leather", label.Value);
                        if (leatherLabel != null) leather = leatherLabel.Value;
                        contents.Add(Config.IndentSpaces);
                        contents.Add(new XElement(defName.Value + "_Leather.label", leather));
                        contents.Add("\n");
                        contents.Add(Config.IndentSpaces);
                        contents.Add(new XElement(defName.Value + "_Leather.description", leather));
                        contents.Add("\n");
                        contents.Add(Config.IndentSpaces);
                        contents.Add(new XElement(defName.Value + "_Leather.stuffProps.stuffAdjective", leather));
                        contents.Add("\n");
                    }

                    // Meat
                    if (useMeatFrom != null)
                    {
                        contents.Add(Config.IndentSpaces);
                        contents.Add(new XComment(string.Format(" Use Meat From: {0} ", useMeatFrom.Value)));
                        contents.Add("\n");
                    }
                    else
                    {
                        string meat = string.Format("{0} meat", label.Value);
                        if (meatLabel != null) meat = meatLabel.Value;
                        contents.Add(Config.IndentSpaces);
                        contents.Add(new XElement(defName.Value + "_Meat.label", meat));
                        contents.Add("\n");
                        contents.Add(Config.IndentSpaces);
                        contents.Add(new XElement(defName.Value + "_Meat.description", meat));
                        contents.Add("\n");
                    }
                }

                // Corpse
                string corpse = string.Format("{0} corpse", label.Value);
                contents.Add(Config.IndentSpaces);
                contents.Add(new XElement(defName.Value + "_Corpse.label", corpse));
                contents.Add("\n");
                contents.Add(Config.IndentSpaces);
                contents.Add(new XElement(defName.Value + "_Corpse.description", corpse));
                contents.Add("\n");
            }
            return contents;
        }

        /// <summary>
        /// Special generate building extra(blueprint and frame)
        /// </summary>
        private static IEnumerable<object> GenerateBuildingExtra(this XElement ele, XElement defName, int level = 0)
        {
            List<object> contents = new List<object>();
            //TODO: 
            return contents;
        }


        /// <summary>
        /// Special generate recipes
        /// </summary>
        private static void AddRecipe(this XDocument recipes, XElement def, XElement defName)
        {
            XElement label = def.Field(FieldType.label);
            XElement recipeMaker = def.Field(FieldType.recipeMaker);
            if (label != null && recipeMaker != null)
            {
                string users = " Recipe Users: ";
                XElement recipeUsers = recipeMaker.Field(FieldType.recipeUsers);
                if (recipeUsers != null)
                {
                    foreach (var li in recipeUsers.Elements())
                    {
                        users += li.Value + " ";
                    }
                }

                XComment lastComm = new XComment(" RimTrans ");
                foreach (var node in recipes.Root.Nodes())
                {
                    if (node.NodeType == XmlNodeType.Comment)
                        lastComm = node as XComment;
                }

                if (users != lastComm.Value)
                {
                    recipes.Root.Add("\n");
                    recipes.Root.Add(Config.IndentSpaces);
                    recipes.Root.Add(new XComment(users));
                    recipes.Root.Add("\n\n");
                }

                // RecipeMake
                recipes.Root.Add(Config.IndentSpaces);
                recipes.Root.Add(new XElement("Make_" + defName.Value + ".label", string.Format("Make {0}", label.Value)));
                recipes.Root.Add("\n");

                // RecipeMakeDesc
                recipes.Root.Add(Config.IndentSpaces);
                recipes.Root.Add(new XElement("Make_" + defName.Value + ".description", string.Format("Make {0}", label.Value)));
                recipes.Root.Add("\n");

                // RecipeMakeJobString
                recipes.Root.Add(Config.IndentSpaces);
                recipes.Root.Add(new XElement("Make_" + defName.Value + ".jobString", string.Format("Making {0}.", label.Value)));
                recipes.Root.Add("\n\n");
            }
        }

    }
}
