/// Andl is A New Data Language. See andl.org.
///
/// Copyright © David M. Bennett 2015-16 as an unpublished work. All rights reserved.
///
/// This software is provided in the hope that it will be useful, but with 
/// absolutely no warranties. You assume all responsibility for its use.
/// 
/// This software is completely free to use for purposes of personal study. 
/// For distribution, modification, commercial use or other purposes you must 
/// comply with the terms of the licence originally supplied with it in 
/// the file Licence.txt or at http://andl.org/Licence/.
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

  namespace Andl.Common {
  // common types used for conversions
  public enum CommonType {
    None, Binary, Bool, Integer, Double, Number, Text, Time,
    Table, Row, User,
  }

  /// <summary>
  /// Field (aka column/attribute) based on common types 
  /// </summary>
  public struct CommonField {
    public string Name;
    public CommonType CType;

    public CommonField(string name, CommonType ctype) {
      Name = name;
      CType = ctype;
    }

    public CommonField(string name, Type type) {
      Name = name;
      CType = CommonConverter.TypeToCommon(type);
    }

    public override string ToString() {
      return $"{Name}:{CType}";
    }

    // parse a heading to extract the fields
    public static CommonField[] ToFields(string heading) {
      var items = heading.Split(',');
      return items.Select(i => {
        var subitems = i.Split(':');
        return new CommonField(subitems[0], (CommonType)Enum.Parse(typeof(CommonType), subitems[1], true));
      }).ToArray();
    }
  }

  /// <summary>
  /// Heading represents a set of common fields
  /// </summary>
  public class CommonHeading {
    public CommonField[] Fields;
    public int Degree { get { return Fields.Length; } }
    public static CommonHeading Empty = new CommonHeading();
    public CommonField this[int index] {
      get { return Fields[index]; }
      set { Fields[index] = value; }
    }

    public static CommonHeading Create(CommonField[] fields) {
      return new CommonHeading {
        Fields = fields,
      };
    }

    public static CommonHeading Create(string heading) {
      return Create(CommonField.ToFields(heading));
    }

    public string[] ToNames() {
      return Fields.Select(f => f.Name).ToArray();
    }

    public CommonType[] ToTypes() {
      return Fields.Select(f => f.CType).ToArray();
    }

    public override string ToString() {
      return $"{Fields.Join(",")}";
    }

    public CommonHeading Adapt(string newheading) {
      return Adapt(newheading.Split(','));
    }

    public CommonHeading Adapt(string[] newheading) {
      var fields = newheading.Select(name => {
        var field = Fields.FirstOrDefault(f => f.Name == name);
        return (field.CType == CommonType.None) ? new CommonField(name, CommonType.None) : field;
      });
      return CommonHeading.Create(fields.ToArray());
    }

    // Create a map from other to this
    public int[] CreateMap(CommonHeading other) {
      return Enumerable.Range(0, Fields.Length)
        .Select(x => Array.FindIndex(other.Fields, f => f.Name == Fields[x].Name))  // equals?
        .ToArray();
    }
  }

  /// <summary>
  /// Table is name plus heading
  /// </summary>
  public class CommonTable {
    public string TableName;
    public CommonHeading Heading;
    public CommonField[] Fields { get { return Heading.Fields; } }
    public int Degree { get { return Fields.Length; } }
    public static CommonTable Empty = new CommonTable();

    public static CommonTable Create(string name, CommonHeading heading) {
      return new CommonTable {
        TableName = name,
        Heading = heading
      };
    }
    public static CommonTable Create(string name, CommonField[] fields) {
      return Create(name, CommonHeading.Create(fields));
    }

    public override string ToString() {
      return $"{TableName}:{Heading}";
    }
  }

  /// <summary>
  /// Carrier for data values
  /// </summary>
  public struct CommonRow {
    public object[] Values;
    public int Length { get { return Values.Length; } }

    public object this[int index] {
      get { return Values[index]; }
      set { Values[index] = value; }
    }

    public override string ToString() {
      return Values.Join(",");
    }

    public CommonRow(int length) {
      Values = new object[length];
    }

    public CommonRow(params object[] values) {
      Values = values;
    }
  }

  /// <summary>
  /// Conversion tables
  /// </summary>
  public class CommonConverter {
    static readonly Dictionary<CommonType, Func<string, object>> ToObjectDict = new Dictionary<CommonType, Func<string, object>> {
      { CommonType.Binary, s => null },
      { CommonType.Bool, s => s.SafeBoolParse() },
      { CommonType.Double, s => s.SafeBoolParse() },
      { CommonType.Integer, s => s.SafeIntParse() },
      { CommonType.None, s => null },
      { CommonType.Number, s => s.SafeDecimalParse() },
      { CommonType.Text, s => s },
      { CommonType.Time, s => s.SafeDatetimeParse() },
    };

    static readonly Dictionary<CommonType, Func<object, string>> ToStringDict = new Dictionary<CommonType, Func<object, string>> {
      { CommonType.Binary, s => null },
      { CommonType.Bool, s => s.ToString() },
      { CommonType.Double, s => s.ToString() },
      { CommonType.Integer, s => s.ToString() },
      { CommonType.None, s => null },
      { CommonType.Number, s => s.ToString() },
      { CommonType.Text, s => s as string },
      { CommonType.Time, s => s.ToString() },
    };

    public static readonly Dictionary<CommonType, Type> ToTypeDict = new Dictionary<CommonType, Type> {
      { CommonType.Binary, typeof(byte[]) },
      { CommonType.Bool, typeof(bool) },
      { CommonType.Double, typeof(double) },
      { CommonType.Integer, typeof(int) },
      { CommonType.Number, typeof(decimal) },
      { CommonType.Text, typeof(string) },
      { CommonType.Time, typeof(DateTime) },
    };

    public static readonly Dictionary<Type, CommonType> ToCommonDict = new Dictionary<Type, CommonType> {
      { typeof(byte[])  , CommonType.Binary },
      { typeof(bool)    , CommonType.Bool   },
      { typeof(double)  , CommonType.Double },
      { typeof(int)     , CommonType.Integer},
      { typeof(decimal) , CommonType.Number },
      { typeof(string)  , CommonType.Text   },
      { typeof(DateTime), CommonType.Time   },
    };

    public static readonly Dictionary<CommonType, object> ToDefaultDict = new Dictionary<CommonType, object> {
      { CommonType.Binary, new byte[0] },
      { CommonType.Bool, false },
      { CommonType.Double, 0.0 },
      { CommonType.Integer, 0 },
      { CommonType.Number, 0.0m },
      { CommonType.Text, "" },
      { CommonType.Time, DateTime.MinValue },
    };

    public static CommonRow ToObject(string[] row, CommonField[] heading) {
      var values = Enumerable.Range(0, row.Length).Select(x => {
        var rawvalue = row[x] as string;
        if (rawvalue == null) throw Error.Fatal($"bad raw value {heading[x].Name} = {row[x]}");
        var value = ToObjectDict[heading[x].CType](rawvalue);
        if (value == null) throw Error.Fatal($"bad value {heading[x].Name} = {row[x]}");
        return value;
      }).ToArray();
      return new CommonRow { Values = values };
    }

    public static object StringToCommon(string rawvalue, CommonType ctype, string name) {
      if (rawvalue == null) throw Error.NullArg("rawvalue");
      var value = ToObjectDict[ctype](rawvalue);
      if (value == null) throw Error.Fatal($"bad value {name} = {value}");
      return value;
    }

    public static CommonType TypeToCommon(Type type) {
      if (ToCommonDict.ContainsKey(type)) return ToCommonDict[type];
      return CommonType.None;
    }

    public static Type CommontoType(CommonType ctype) {
      if (ToTypeDict.ContainsKey(ctype)) return ToTypeDict[ctype];
      return null;
    }

    public static object GetDefault(CommonType ctype) {
      if (ToDefaultDict.ContainsKey(ctype)) return ToDefaultDict[ctype];
      return null;
    }

  }
}
