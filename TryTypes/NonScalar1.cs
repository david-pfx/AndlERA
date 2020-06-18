using System;
using System.Collections.Generic;
using System.Linq;
using Andl.Common;

namespace AndlEra {
  public static class TupleTypeHelper {
    public static bool AreEqual(object[] values, object[] valuesx) {
      if (values.Length != valuesx.Length) return false;
      for (int x = 0; x < values.Length; ++x)
        if (values[x] != valuesx[x]) return false;
      return true;
    }

    internal static int CalcHashCode(object[] values) {
      int code = 1;
      foreach (object value in values)
        code = (code << 1) ^ value.GetHashCode();
      return code;
    }

    internal static string Format(object[] values) {
      return values.Join(",");
    }
  }
  /// <summary>
  /// 
  /// </summary>
  struct S_TupleType {
    string[] _names;
    object[] _values;
    int _hashcode;
    public string[] _Names { get { return _names; } }

    public override bool Equals(object obj) {
      return (obj is S_TupleType
        && TupleTypeHelper.AreEqual(((S_TupleType)obj)._values, _values));
    }

    public override int GetHashCode() {
      return _hashcode;
    }

    public override string ToString() {
      return TupleTypeHelper.Format(_values);
    }

    public string Sno { get { return (string)_values[0]; } }
    public string Sname { get { return (string)_values[1]; } }
    public int Status { get { return (int)_values[2]; } }
    public string City { get { return (string)_values[3]; } }

    public static S_TupleType Create(string[] names, object[] values) {
      return new S_TupleType() {
        _names = names,
        _values = values,
        _hashcode = TupleTypeHelper.CalcHashCode(values),
      };
    }

    public static S_TupleType Create(string Sno, string Sname, int Status, string City) {
      return Create(new string[] { "SNo", "Sname", "Status", "City" },
        new object[] { Sno, Sname, Status, City });
    }

  }

  public static class RelationTypeHelper {
    public static bool AreEqual(HashSet<object> body, HashSet<object> other) {
      if (body.Count != other.Count) return false;
      foreach (var b in body)
        if (!other.Contains(b)) return false;
      return true;
    }

    internal static int CalcHashCode(HashSet<object> body) {
      int code = 1;
      foreach (object b in body)
        code = (code << 1) ^ b.GetHashCode();
      return code;
    }

    internal static string Format(HashSet<object> body) {
      return body.Take(5).Join(";");
    }
  }

  /// <summary>
  /// Generated relation type for S (suppliers)
  /// </summary>
  struct S_RelationType {
    string[] _names;
    HashSet<object> _tuples;
    int _hashcode;
    public string[] _Names { get { return _names; } }

    public override bool Equals(object obj) {
      return obj is S_RelationType
        && RelationTypeHelper.AreEqual(_tuples, ((S_RelationType)obj)._tuples);
    }

    public override int GetHashCode() {
      return _hashcode;
    }

    public override string ToString() {
      return RelationTypeHelper.Format(_tuples);
    }

    public static S_RelationType Create(string[] names, HashSet<object> body) {
      return new S_RelationType {
        _names = names,
        _tuples = body,
        _hashcode = RelationTypeHelper.CalcHashCode(body),
      };
    }

    public static S_RelationType Create(IList<S_TupleType> body) {
      var names = new string[] { "SNo", "Sname", "Status", "City" };
      var xbody = new HashSet<object>(body.Cast<object>());
      return new S_RelationType {
        _names = new string[] { "SNo", "Sname", "Status", "City" },
        _tuples = new HashSet<object>(body.Cast<object>()),
      };
    }

  }
}
