﻿/// Andl is A New Data Language. See http://andl.org.
///
/// Copyright © David M. Bennett 2015-20 as an unpublished work. All rights reserved.
/// 
/// This work is licensed under the Creative Commons Attribution-NonCommercial 4.0 International License. 
/// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc/4.0/.
/// 
/// In summary, you are free to share and adapt this software freely for any non-commercial purpose provided 
/// you give due attribution and do not impose additional restrictions.
/// 
/// This software is provided in the hope that it will be useful, but with 
/// absolutely no warranties. You assume all responsibility for its use.
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

  ///===========================================================================
  /// <summary>
  /// Field (aka column/attribute) based on common types 
  /// </summary>
  public struct CommonField {
    public string Name;
    public CommonType CType;
    public CommonField[] Fields;

    public bool HasHeading { get { return CType == CommonType.Row || CType == CommonType.Table || CType == CommonType.User; } }
    public static CommonField Empty = new CommonField();

    //-- overrides
    public override string ToString() {
      return (Fields == null) ? $"{Name}:{CType}"
        : $"{Name}:{CType}{{{Fields.Join(",")}}}";
    }

    public override bool Equals(object obj) {
      if (!(obj is CommonField)) return false;
      CommonField other = (CommonField)obj;
      if (!(Name == other.Name && CType == other.CType && HasHeading == other.HasHeading)) return false;
      return !HasHeading || Fields.SequenceEqual(other.Fields);
    }

    public override int GetHashCode() => Name.GetHashCode();

    //-- ctors
    public CommonField(string name, CommonType ctype, CommonField[] fields = null) {
      Name = name;
      CType = ctype;
      Fields = fields;
      Logger.Assert(HasHeading == (Fields != null));
    }

    public CommonField(string name, Type type) {
      Name = name;
      CType = CommonConverter.TypeToCommon(type);
      Fields = null;
    }

    public string Format(object v) {
      if (CType == CommonType.Row || CType == CommonType.User) {
        return Name + ":{" + FormatRow((CommonRow)v, Fields) + "}";
      }
      if (CType == CommonType.Table) {
        var rows = (CommonRow[])v;
        var f = Fields;
        return Name + ":{{" + rows.Select(r => FormatRow(r, f)).Join("},{") + "}}";
      }
      return Name + ":" + v.ToString();
    }

    static string FormatRow(CommonRow row, CommonField[] fields) {
      return Enumerable.Range(0, fields.Length)
        .Select(x => fields[x].Format(row[x]))
        .Join(",");
    }

    // parse a heading to extract the fields
    public static CommonField[] ToFields(string heading) {
      Logger.Assert(heading != null);
      var items = heading.Split(',');
      return items.Select(i => {
        var subitems = i.Split(':');
        var ctype = (subitems.Length < 2) ? CommonType.Text
          : (CommonType)Enum.Parse(typeof(CommonType), subitems[1], true);
      return new CommonField(subitems[0], ctype);
      }).ToArray();
    }
  }

  ///===========================================================================
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

    public override string ToString() {
      return $"{Fields.Join(",")}";
    }

    // Compares equality but does not implement Equals()
    public bool IsEqual(CommonHeading other) {
      return other != null && Fields.SequenceEqual(other.Fields);
      //return other != null && Degree == other.Degree && Fields.SequenceEqual(other.Fields);
    }

    // Compares structural equality
    public bool IsCompatible(CommonHeading other) {
      return other != null && Degree == other.Degree && Fields.All(f => other.Fields.Contains(f));
    }

    public static CommonHeading Create(IEnumerable<CommonField> fields) {
      return new CommonHeading {
        Fields = fields.ToArray(),
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

    // create a new heading but using types from current where matching
    public CommonHeading Adapt(string newheading) {
      return Adapt(newheading.Split(','));
    }

    public CommonHeading Adapt(string[] newheading) {
      var fields = newheading.Select(name => {
        var field = Fields.FirstOrDefault(f => f.Name == name);
        return (field.CType == CommonType.None) ? new CommonField(name, CommonType.None) : field;
      });
      return CommonHeading.Create(fields);
    }

    // rename a field from old to new
    public CommonHeading Rename(CommonField field1, CommonField field2) {
      var fields = Fields.Select(f => (f.Name == field1.Name) ? new CommonField(field2.Name, f.CType) : f);
      return CommonHeading.Create(fields);
    }

    // create a heading as the union of this and other
    public CommonHeading Union(CommonHeading other) {
      var fields = Fields.Concat(other.Fields.Where(f => !Fields.Any(o => f.Name == o.Name)));
      return CommonHeading.Create(fields);
    }

    // create a heading as the intersection of this and other
    public CommonHeading Intersect(CommonHeading other) {
      var fields = Fields.Where(f => other.Fields.Any(o => f.Name == o.Name));
      return CommonHeading.Create(fields);
    }

    // create a heading as this minus other
    public CommonHeading Minus(CommonHeading other) {
      var fields = Fields.Where(f => !other.Fields.Any(o => f.Name == o.Name));
      return CommonHeading.Create(fields);
    }
    // create a heading plus one field
    public CommonHeading Append(CommonField field) {
      return CommonHeading.Create(Fields.Append(field));
    }
    public CommonHeading Append(IEnumerable<CommonField> fields) {
      return CommonHeading.Create(Fields.Concat(fields));
    }
    // create a heading minus one field
    public CommonHeading Remove(CommonField field) {
      return CommonHeading.Create(Fields.Where(f => f.Name != field.Name));
    }
    // Create a map from other to this
    public int[] CreateMap(CommonHeading other) {
      return Enumerable.Range(0, Fields.Length)
        .Select(x => Array.FindIndex(other.Fields, f => f.Name == Fields[x].Name))  // equals?
        .ToArray();
    }

    // Create map using old rename method
    public int[] CreateMapRename(CommonHeading other, bool dorename = false) {
      // a queue of indexes to new fields in other
      var q = new Queue<int>(Enumerable.Range(0, other.Degree)
        .Where(x => !Fields.Any(f => f.Name == other.Fields[x].Name)));

      return Fields.Select(f => {
          var x = Array.FindIndex(other.Fields, o => f.Name == o.Name);  // equals?
          return (dorename && x == -1 && q.Count > 0) ? q.Dequeue() : x;
        }).ToArray();

    }
  }

  ///===========================================================================
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

  ///===========================================================================
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
      return CommonConverter.ValueToString(this);
    }

    public CommonRow(int length) {
      Values = new object[length];
    }

    public CommonRow(IEnumerable<object> values) {
      Values = values.ToArray();
    }
  }

  ///===========================================================================
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

    public static string ValueToString(object value) {
      if (value is object[]) 
        return ((object[])value)
          .Select(v => ValueToString(v))
          .Join(",");
      if (value is CommonRow[]) 
        return "{" + ((CommonRow[])value)
          .Select(v => ValueToString(v))
          .Join(";") + "}";
      if (value is CommonRow) 
        return "{" + ValueToString(((CommonRow)value).Values) + "}";
      return value.ToString();
    }
  }
}
