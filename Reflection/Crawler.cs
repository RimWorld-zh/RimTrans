using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RimTrans.Reflection {
  class DistinctAttributeComparer : IEqualityComparer<Attribute> {
    public bool Equals(Attribute x, Attribute y) {
      return x.GetType() == y.GetType();
    }

    public int GetHashCode(Attribute obj) {
      return obj.GetHashCode();
    }
  }

  /// <summary>
  /// The crawler for assemblies of RimWorld game or mods.
  /// </summary>
  public static class Crawler {
    /// <summary>
    /// Gets type packages from assemblies.
    /// </summary>
    /// <param name="mapInputOutput">The intput/output paths map, input is the path to a assembly, output is the path to output type package</param>
    public static void Crawl(Dictionary<string, string> mapInputOutput) {
      var mapAsmOutput = new Dictionary<Assembly, string>();
      foreach (var (input, output) in mapInputOutput) {
        if (input.ToLower() == "core") {
          mapAsmOutput.Add(AssemblyMeta.assemblyCore, output);
        } else {
          var asm = Assembly.LoadFrom(input);
          mapAsmOutput.Add(asm, output);
        }
      }

      foreach (var (asm, output) in mapAsmOutput) {
        var asmMeta = new AssemblyMeta(asm);
        var jsonText = Serialize(asmMeta);
        File.WriteAllText(output, jsonText);
      }
    }

    public static string Serialize(AssemblyMeta assmeblyMeta) {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new JsonStringEnumConverter());

      options.WriteIndented = true;
      
      var jsonText = JsonSerializer.Serialize(assmeblyMeta, options);
      return jsonText;
    }
  }

  /// <summary>
  /// The meta info of the assembly. Includes enums, classes and defs.
  /// </summary>
  public class AssemblyMeta {
    public static readonly Assembly assemblyCore = typeof(Verse.Def).Assembly;

    /// <summary>
    /// The name of the assembly.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// All enum types.
    /// </summary>
    public SortedDictionary<string, EnumMeta> enums { get; set; }

    /// <summary>
    /// All class types.
    /// </summary>
    public SortedDictionary<string, ClassMeta> classes { get; set; }

    /// <summary>
    /// The map for defs name to full name.
    /// </summary>
    public SortedDictionary<string, string> defs { get; set; }

    public AssemblyMeta(Assembly assembly) {
      name = assembly.FullName;
      enums = new SortedDictionary<string, EnumMeta>();
      classes = new SortedDictionary<string, ClassMeta>();
      defs = new SortedDictionary<string, string>();

      foreach (var type in assembly.GetTypes()) {
        var typeMeta = new TypeMeta(type);
        if (typeMeta.category == TypeCategory.Enum) {
          var enumMeta = new EnumMeta(type);
          enums.Add(enumMeta.name, enumMeta);
        }
        if (typeMeta.category == TypeCategory.Class || typeMeta.category == TypeCategory.Def) {
          var classMeta = new ClassMeta(type);
          classes.Add(classMeta.name, classMeta);
          if (typeMeta.category == TypeCategory.Def) {
            defs.Add(type.Name, type.FullName);
          }
        }
      }
    }
  }

  /// <summary>
  /// The meta info of the enum.
  /// </summary>
  public class EnumMeta {
    public string name { get; set; }
    // [string, number]
    public Dictionary<string, object> values { get; set; }

    public EnumMeta(Type type) {
      if (!type.IsEnum) {
        throw new Exception($"Type '{type.Name}({type.FullName})' is not a enum type.");
      }

      name = TypeMeta.GetName(type);
      values = new Dictionary<string, object>();

      var underlyingType = Enum.GetUnderlyingType(type);
      var rawNames = Enum.GetNames(type);
      var rawValues = Enum.GetValues(type);
      for (int i = 0; i < rawValues.Length; i++) {
        values.Add(
          rawNames[i],
          Convert.ChangeType(rawValues.GetValue(i), underlyingType)
        );
      }
    }
  }

  /// <summary>
  /// The meta info of the class
  /// </summary>
  public class ClassMeta {
    public string baseClass { get; set; } = null;
    public string name { get; set; }
    public List<FieldMeta> fields { get; set; }
    public Dictionary<string, Dictionary<string, object>> attrs { get; set; }

    private static IEnumerable<FieldMeta> GetFields(Type type) {
      var fields = type
        .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Select(fi => new FieldMeta(fi));
      if (type.BaseType != typeof(object)) {
        return GetFields(type.BaseType).Concat(fields);
      }
      return fields;
    }

    public ClassMeta(Type type) {
      if (!(type.IsClass || type.IsValueType && !type.IsPrimitive)) {
        throw new Exception($"Type '{type.Name}({type.FullName})' is not a class or struct type.");
      }

      if (type.BaseType != typeof(object)) {
        baseClass = TypeMeta.GetName(type.BaseType);
      }
      name = TypeMeta.GetName(type);

      fields = GetFields(type).ToList();

      attrs = type
        .GetCustomAttributes()
        .GroupBy(a => a.GetType())
        .Select(g => g.First())
        .ToDictionary(
          a => TypeMeta.GetName(a.GetType()),
          a => a.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(
              p => p.Name,
              p => p.GetValue(a)
            )
        );
    }
  }

  /// <summary>
  /// Meta info of the field.
  /// </summary>
  public class FieldMeta {
    public string name { get; set; }
    public TypeMeta type { get; set; }
    public Dictionary<string, Dictionary<string, object>> attrs { get; set; }

    public FieldMeta(FieldInfo fieldInfo) {
      name = fieldInfo.Name;
      type = new TypeMeta(fieldInfo.FieldType);
      attrs = fieldInfo
        .GetCustomAttributes()
        .GroupBy(a => a.GetType())
        .Select(g => g.First())
        .ToDictionary(
          a => TypeMeta.GetName(a.GetType()),
          a => a.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(
              p => p.Name,
              p => p.GetValue(a)
            )
        );
    }
  }

  public enum TypeCategory {
    Type,     // Type type in reflection
    Value,    // The value types (in javascript) like 'string', 'int', 'float' or 'bool'
    Enum,     // Enum type
    List,     // Generic type array or List<>
    Dict,     // Generic type `Dictionary<string, object>`
    Generic,  // Other generic types
    Class,    // Normal class or struct type
    Def,      // Special class for RimWorld Defs
    Unknown,  // Unknown
  }

  /// <summary>
  /// The meta info of the type.
  /// </summary>
  public class TypeMeta {
    public static readonly Type typeType = typeof(Type);
    public static readonly Type typeBoolean = typeof(bool);
    public static readonly List<Type> typesNumber = new List<Type> {
      typeof(byte), typeof(sbyte),
      typeof(decimal), typeof(double), typeof(float),
      typeof(int), typeof(uint),
      typeof(long), typeof(ulong),
      typeof(short), typeof(ushort),
    };
    public static readonly Type typeString = typeof(string);

    public static readonly Type typeDef = typeof(Verse.Def);

    public static string GetName(Type type) {
      return string.IsNullOrEmpty(type.FullName) ? type.Name : type.FullName;
    }


    /// <summary>
    /// The name of the type.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Tye category of the type.
    /// </summary>
    public TypeCategory category { get; set; }

    /// <summary>
    /// The arguments types of list or dict, for other categories this is null.
    /// </summary>
    public TypeMeta of { get; set; } = null;

    /// <summary>
    /// The type is nullable or not, only valid for value types.
    /// </summary>
    public bool nullable { get; set; } = false;

    public TypeMeta(Type type) {
      name = TypeMeta.GetName(type);
      category = TypeCategory.Unknown;

      var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
      if (nullableUnderlyingType != null) {
        nullable = true;
        type = nullableUnderlyingType;
      }

      // type
      if (type == typeof(Type)) {
        name = "Type";
        category = TypeCategory.Type;
        return;
      }

      // boolean
      if (type == typeBoolean) {
        name = "boolean";
        category = TypeCategory.Value;
        return;
      }

      // number
      if (typesNumber.Contains(type)) {
        name = "number";
        category = TypeCategory.Value;
        return;
      }

      // string
      if (type == typeString) {
        name = "string";
        category = TypeCategory.Value;
        return;
      }

      // enum
      if (type.IsEnum) {
        category = TypeCategory.Enum;
        return;
      }

      // list(array)
      if (type.IsArray) {
        name = "Array";
        category = TypeCategory.List;
        of = new TypeMeta(type.GetElementType());
        return;
      }

      if (type.IsGenericType) {
        var definition = type.GetGenericTypeDefinition();
        var arguments = type.GetGenericArguments();
        var interfaces = definition.GetInterfaces();

        // list
        if (interfaces.Any(i => i.Name == "IList")) {
          name = "List";
          category = TypeCategory.List;
          of = new TypeMeta(arguments[0]);
          return;
        }

        // dict
        if (interfaces.Any(i => i.Name == "IDictionary")) {
          if (arguments[0] == typeString || arguments[0].IsClass && arguments[0].IsSubclassOf(typeDef)) {
            name = "Dict";
            category = TypeCategory.Dict;
            of = new TypeMeta(arguments[1]);
            return;
          }
        }

        // generic
        category = TypeCategory.Generic;
        return;
      }

      // struct
      if (type.IsValueType && !type.IsPrimitive) {
        category = TypeCategory.Class;
        return;
      }

      if (type.IsClass) {
        // def
        if (type == typeDef || type.IsSubclassOf(typeDef)) {
          category = TypeCategory.Def;
          return;
        }

        // class
        category = TypeCategory.Class;
        return;
      }
    }
  }
}
