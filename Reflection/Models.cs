using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RimTrans.Reflection {
  class ClassInfo {
    public static readonly List<Type> classes = new List<Type>();
    public static readonly List<ClassInfo> classesOf = new List<ClassInfo>();
    public static readonly Type TYPE_DEF = typeof(Verse.Def);

    public static void Crawl() {
      var defs = new List<ClassInfo> { new ClassInfo(TYPE_DEF) };
      defs.AddRange(
        TYPE_DEF
          .Assembly
          .GetTypes()
          .Where(t => t.IsSubclassOf(TYPE_DEF))
          .Select(t => new ClassInfo(t))
      );
      classes.AddRange(classesOf.Select(ci => ci.type));
      classesOf.AddRange(defs);
      foreach (var ci in defs) {
        foreach (var fi in ci.fields) {
          CrawlFieldType(fi.type);
        }
      }
    }

    public static void CrawlFieldType(TypeInfo ti) {
      if (ti.category == TypeInfo.CATEGORY_CLASS) {
        if (!classes.Contains(ti.type) && ti.type.Assembly == TYPE_DEF.Assembly) {
          classes.Add(ti.type);
          classesOf.Add(new ClassInfo(ti.type));
          foreach (var subClass in TYPE_DEF.Assembly.GetTypes().Where(t => t.IsSubclassOf(ti.type))) {
            if (!classes.Contains(subClass)) {
              var subClassInfo = new ClassInfo(subClass);
              classes.Add(subClass);
              classesOf.Add(subClassInfo);
              foreach (var fi in subClassInfo.fields) {
                CrawlFieldType(fi.type);
              }
            }
          }
        }
      } else if (ti.category == TypeInfo.CATEGORY_LIST || ti.category == TypeInfo.CATEGORY_DICT) {
        CrawlFieldType(ti.of);
      } else if (ti.category == TypeInfo.CATEGORY_ENUM) {
        if (!EnumInfo.enums.Contains(ti.type)) {
          EnumInfo.enums.Add(ti.type);
          EnumInfo.enumsOf.Add(new EnumInfo(ti.type));
        }
      }
    }

    [NonSerialized]
    public Type type;

    public bool isAbstract;
    public string baseClass;
    public string name;
    public List<FieldInfo> fields;

    public ClassInfo(Type type) {
      this.isAbstract = type.IsAbstract;
      this.baseClass = type.BaseType == typeof(Object)
          ? null
          : type.BaseType.Name;
      this.type = type;
      this.name = type.Name;
      this.fields = type
        .GetFields(
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly
        )
        .Where(fi => {
          if (fi.GetCustomAttribute<Verse.UnsavedAttribute>() != null) {
            return false;
          }
          if (fi.GetCustomAttribute<NonSerializedAttribute>() != null) {
            return false;
          }

          return true;
        }).Select(fi => new FieldInfo(fi)).ToList();
    }
  }

  class EnumInfo {
    public static readonly List<Type> enums = new List<Type>();
    public static readonly List<EnumInfo> enumsOf = new List<EnumInfo>();

    public class EnumValue {
      public string name;
      public object value;
    }

    [NonSerialized]
    public Type type;

    public string name;
    public List<EnumValue> values = new List<EnumValue>();

    public EnumInfo(Type type) {
      if (!type.IsEnum) {
        throw new Exception($"Type '{type.FullName}' is not a enum type.");
      }
      this.type = type;
      var underlyingType = Enum.GetUnderlyingType(type);
      this.name = type.Name;
      var names = Enum.GetNames(type);
      var values = Enum.GetValues(type);
      for (int i = 0; i < name.Length; i++) {
        this.values.Add(new EnumValue {
          name = names[i],
          value = Convert.ChangeType(values.GetValue(i), underlyingType)
        });
      }
    }
  }

  class FieldInfo {
    public string name;
    public bool mustTranslate;
    public TypeInfo type;


    public FieldInfo(System.Reflection.FieldInfo fi) {
      this.name = fi.Name;
      this.mustTranslate = fi.FieldType.GetCustomAttribute<Verse.MustTranslateAttribute>() != null;
      this.type = new TypeInfo(fi.FieldType);
    }
  }

  class TypeInfo {
    public static string CATEGORY_DEF = "DEF";
    public static string CATEGORY_TYPE = "TYPE";
    public static string CATEGORY_VALUE = "VALUE";
    public static string CATEGORY_CLASS = "CLASS";
    public static string CATEGORY_ENUM = "ENUM";
    public static string CATEGORY_LIST = "LIST";
    public static string CATEGORY_DICT = "DICT";
    public static string CATEGORY_UNKNOWN = "UNKNOWN";

    [NonSerialized]
    public Type type;

    public string category;
    public string name;
    public TypeInfo of;

    public TypeInfo(Type type) {

      this.type = type;
      if (type.IsSubclassOf(typeof(Verse.Def))) {
        this.category = CATEGORY_DEF;
        this.name = type.Name;
      } else if (type == typeof(Type)) {
        this.category = CATEGORY_TYPE;
        this.name = "Type";
      } else if (type.IsValueType || type == typeof(string)) {
        this.category = CATEGORY_VALUE;
        this.name = type.Name;
      } else if (type.IsEnum) {
        this.category = CATEGORY_ENUM;
        this.name = type.Name;
      } else if (type.IsArray) {
        this.category = CATEGORY_LIST;
        this.name = "Array";
        this.of = new TypeInfo(type.GetElementType());
      } else if (type.IsGenericType) {
        var genericTypeDefinition = type.GetGenericTypeDefinition();
        var genericArguments = type.GetGenericArguments();
        if (genericTypeDefinition == typeof(List<>)) {
          this.category = CATEGORY_LIST;
          this.name = "List";
          this.of = new TypeInfo(genericArguments[0]);
        } else if (genericTypeDefinition == typeof(Dictionary<,>)) {
          this.category = CATEGORY_DICT;
          this.name = "Dictionary";
          this.of = new TypeInfo(genericArguments[1]);
        } else {
          this.category = CATEGORY_UNKNOWN;
          this.name = type.FullName;
        }
      } else if (type.IsClass) {
        this.category = CATEGORY_CLASS;
        this.name = type.Name;
      }
    }
  }
}
