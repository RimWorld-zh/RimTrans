using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RimWorld;
using Verse;

namespace RimTrans.Builder.Crawler {
    /// <summary>
    /// Privode some methods.
    /// </summary>
    public static class Helper {
        #region Xml Helper

        /// <summary>
        /// Get class attribute value of this element.
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        public static string GetClassName(this XElement ele) {
            XAttribute className = ele.Attribute("Class");
            if (className != null) {
                return className.Value;
            }
            return null;
        }

        /// <summary>
        /// Set class attribute value of this element.
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="className"></param>
        public static void SetClassName(this XElement ele, string className) {
            ele.SetAttributeValue("Class", className);
        }

        /// <summary>
        /// Merge the same type of Defs or Tags.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        public static void MergeElement(XElement destination, XElement source) {
            if (!destination.HasElements && !source.HasElements) {
                if (destination.Value == string.Empty && source.Value != string.Empty) {
                    destination.Value = source.Value;
                }
            } else if (!destination.HasElements && source.HasElements) {
                if (destination.Value != string.Empty) {
                    destination.Value = string.Empty;
                }
                destination.Add(source.Elements());
            } else if (destination.HasElements && source.HasElements) {
                if (destination.Elements().First().Name.ToString() == "li" &&
                    source.Elements().First().Name.ToString() == "li") {
                    destination.AddFirst(source.Elements());
                } else {
                    foreach (XElement fieldSource in source.Elements()) {
                        bool isMatched = false;
                        foreach (XElement fieldDestination in destination.Elements()) {
                            if (string.Compare(fieldSource.Name.ToString(), fieldDestination.Name.ToString(), true) == 0 &&
                                fieldSource.GetClassName() == fieldDestination.GetClassName()) {
                                isMatched = true;
                                MergeElement(fieldDestination, fieldSource);
                                break;
                            }
                        }
                        if (!isMatched) {
                            destination.Add(fieldSource);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process the list in Defs or Tags.
        /// </summary>
        /// <param name="element"></param>
        public static void MergeListItems(XElement element) {
            if (element.HasElements) {
                if (element.Elements().First().Name.ToString() == "li") {
                    List<XElement> uniques = new List<XElement>();
                    foreach (XElement current in element.Elements()) {
                        bool isMatched = false;
                        foreach (XElement item in uniques) {
                            if (current.GetClassName() == item.GetClassName()) {
                                isMatched = true;
                                MergeElement(item, current);
                                break;
                            }
                        }
                        if (!isMatched) {
                            uniques.Add(current);
                        }
                    }
                    element.RemoveNodes();
                    element.Add(uniques);
                    foreach (XElement item in element.Elements()) {
                        MergeListItems(item);
                    }
                } else {
                    foreach (XElement child in element.Elements()) {
                        MergeListItems(child);
                    }
                }
            }
        }

        #endregion

        #region Reflection Helper

        /// <summary>
        /// For getting all fields
        /// </summary>
        public readonly static BindingFlags FieldBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Class Object is the base class of all class.
        /// </summary>
        public readonly static Type ClassObject = typeof(Object);

        /// <summary>
        /// Class Def is the base class of all DefType classes
        /// </summary>
        public readonly static Type ClassDef = typeof(Def);

        /// <summary>
        /// All DefType classes
        /// </summary>
        public readonly static IEnumerable<Type> AllClassesDef = typeof(Def).AllSubclasses();

        /// <summary>
        /// Decompiled source code directory of Assembly-CSharp.dll
        /// </summary>
        public static string sourceCodePath;

        /// <summary>
        /// Get field default value from source file.
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static string GetDefaultValueFromSource(FieldInfo fieldInfo) {
            if (fieldInfo == null)
                return null;

            string result = null;
            Regex regex = new Regex($"(public|private|internal|protected)\\ [A-Za-z_][A-Za-z_0-9<>]*\\ {fieldInfo.Name} \\=\\ .*\\;");

            if (sourceCodePath != null && Directory.Exists(sourceCodePath)) {
                string[] sections = fieldInfo.DeclaringType.FullName.Split(new char[] { '.' });
                string sourceFilePath = Path.Combine(sourceCodePath, Path.Combine(sections)) + ".cs";
                if (File.Exists(sourceFilePath)) {
                    using (StreamReader sr = new StreamReader(sourceFilePath)) {
                        while (sr.Peek() >= 0) {
                            string s = sr.ReadLine();
                            if (regex.IsMatch(s)) {
                                result = s.Substring(s.IndexOf('=') + 2).TrimEnd(new char[] { ';' });
                                //Log.Info();
                                //Log.WriteLine(sourceFilePath);
                                //Log.Indent();
                                //Log.Write(s);
                                //Log.Indent();
                                //Log.WriteLine(result);
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get name of this type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTypeName(Type type) {
            if (type.IsGenericType) {
                string genericName;
                if (type.GetGenericTypeDefinition() == typeof(List<>)) {
                    genericName = "List";
                } else if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
                    genericName = "Dictionary";
                } else {
                    genericName = type.FullName;
                }
                Type[] genericArguments = type.GetGenericArguments();
                StringBuilder genericArgumentsNames = new StringBuilder();
                foreach (Type curGenericArgument in genericArguments) {
                    genericArgumentsNames.Append(GetTypeName(curGenericArgument));
                    genericArgumentsNames.Append(", ");
                }
                genericArgumentsNames.Remove(genericArgumentsNames.Length - 2, 2);
                return $"{genericName}<{genericArgumentsNames.ToString()}>";
            }
            if (type == typeof(string))
                return "string";
            if (type == typeof(int))
                return "int";
            if (type == typeof(float))
                return "float";
            if (type == typeof(double))
                return "double";
            if (type == typeof(bool))
                return "bool";
            return type.FullName;
        }

        /// <summary>
        /// Get all value names of this enum type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetAvailableValues(Type type) {
            if (type.IsEnum) {
                StringBuilder availableValues = new StringBuilder();
                foreach (var value in Enum.GetNames(type)) {
                    availableValues.Append(value);
                    availableValues.Append(", ");
                }
                availableValues.Remove(availableValues.Length - 2, 2);
                return availableValues.ToString();
            }
            //else if (type == typeof(bool))
            //{
            //    return "true, false";
            //}
            return null;
        }

        /// <summary>
        /// Get field default value from attribute or source code file.
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static string GetDefaultValue(FieldInfo fieldInfo) {
            if (fieldInfo == null)
                return null;
            if (fieldInfo.FieldType.IsGenericType)
                return null;

            string result = null;
            DefaultValueAttribute defaultValueAttribute = fieldInfo.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttribute != null) {
                if (defaultValueAttribute.value == null) {
                    result = null;
                } else if (defaultValueAttribute.value is string) {
                    result = defaultValueAttribute.value as string;
                } else if (defaultValueAttribute.value is bool) {
                    result = defaultValueAttribute.value.ToString().ToLower();
                } else {
                    try {
                        result = defaultValueAttribute.value.ToString();
                        if (fieldInfo.FieldType == typeof(float) || fieldInfo.FieldType == typeof(double)) {
                            if (result.Last() == 'f' || result.Last() == 'd') {
                                result = result.Substring(0, result.Length - 1);
                            }
                            if (result.IndexOf('.') < 0) {
                                result += ".0";
                            }
                        }
                    } catch (Exception) {
                    }
                }
            } else {
                result = GetDefaultValueFromSource(fieldInfo);
                if (string.IsNullOrEmpty(result)) {
                    if (fieldInfo.FieldType == typeof(bool)) {
                        result = false.ToString().ToLower();
                    }
                } else {
                    if (fieldInfo.FieldType.IsEnum) {
                        result = result.Substring(result.IndexOf('.') + 1);
                    } else if (fieldInfo.FieldType == typeof(float) || fieldInfo.FieldType == typeof(double)) {
                        if (result.Last() == 'f' || result.Last() == 'd') {
                            result = result.Substring(0, result.Length - 1);
                        }
                        if (result.IndexOf('.') < 0) {
                            result += ".0";
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get field description from attribute.
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static string GetDescription(FieldInfo fieldInfo) {
            if (fieldInfo == null)
                return null;

            DescriptionAttribute descriptionAtrribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAtrribute != null)
                return descriptionAtrribute.description.Replace("\n", "\\n");
            return null;
        }

        /// <summary>
        /// Get field loading alias from attribute
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static string GetAlias(FieldInfo fieldInfo) {
            if (fieldInfo == null)
                return null;

            LoadAliasAttribute loadAliasAttribute = fieldInfo.GetCustomAttribute<LoadAliasAttribute>();
            if (loadAliasAttribute != null)
                return loadAliasAttribute.alias;
            return null;
        }

        #endregion




    }
}
