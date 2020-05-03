// Non-scalar base types
//
using System;
using System.Collections.Generic;
using System.Linq;
using Andl.Common;

namespace AndlEra {
  /// <summary>
  /// Base type for relations
  /// </summary>
  public abstract class RelationBase {
    protected string[] _heading;
    protected string[] _key;
    protected HashSet<object> _body;
    protected int _hashcode;
    public string[] Heading { get { return _heading; } }

    public override int GetHashCode() {
      return _hashcode;
    }

    public override string ToString() {
      return _body.Take(5).Join(";");
    }

    public override bool Equals(object obj) {
      if (!(obj is RelationBase)) return false;
      var other = ((RelationBase)obj);
      if (other._body.Count != _body.Count) return false;
      foreach (var b in _body)
        if (!other._body.Contains(b)) return false;
      return true;
    }

    public RelationBase(string[] heading, string[] key, HashSet<object> body) {
      _heading = heading;
      _key = key;
      _body = body;
      _hashcode = CalcHashCode(body);
    }

    internal static int CalcHashCode(HashSet<object> body) {
      int code = 1;
      foreach (object b in body)
        code = (code << 1) ^ b.GetHashCode();
      return code;
    }
  }

  /// <summary>
  /// Base type for tuples
  /// </summary>
  public abstract class TupleBase {
    protected string[] _heading;
    protected object[] _values;
    protected int _hashcode;
    public string[] Heading { get { return _heading; } }

    public override int GetHashCode() {
      return _hashcode;
    }

    public override bool Equals(object obj) {
      if (!(obj is TupleBase)) return false;
      var other = (TupleBase)obj;
      if (other._values.Length != _values.Length) return false;
      for (int x = 0; x < _values.Length; ++x)
        if (!_values[x].Equals(other._values[x])) return false;
      return true;
    }
    public override string ToString() {
      return _values.Join(",");
    }

    public TupleBase(string[] heading, object[] values) {
      _heading = heading;
      _values = values;
      _hashcode = CalcHashCode(values);
    }

    internal static int CalcHashCode(object[] values) {
      int code = 1;
      foreach (object value in values)
        code = (code << 1) ^ value.GetHashCode();
      return code;
    }

  }

  /// <summary>
  /// Generated relation type for S (suppliers)
  /// </summary>
  class RelS : RelationBase {

    RelS(IList<TupS> tuples) : base(
      new string[] { "SNo", "Sname", "Status", "City" }, 
      new string[] { "SNo" },
      new HashSet<object>(tuples.Cast<object>())) {
    }

  }
  /// <summary>
  /// Generated tuple type for S (suppliers)
  /// </summary>
  public class TupS : TupleBase {

    public string Sno { get { return (string)_values[0]; } }
    public string Sname { get { return (string)_values[1]; } }
    public int Status { get { return (int)_values[2]; } }
    public string City { get { return (string)_values[3]; } }

    public TupS(string Sno, string Sname, int Status, string City) : base(
      new string[] { "SNo", "Sname", "Status", "City" },
      new object[] { Sno, Sname, Status, City }) { 
    }
   }

}
