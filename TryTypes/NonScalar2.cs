// Non-scalar base types
//
using System;
using System.Collections.Generic;
using System.Linq;
using Andl.Common;

namespace AndlEra {
  /// <summary>
  /// Base type for tuples
  /// </summary>
  public abstract class TupleBase {
    public static string[] Heading { get; protected set; }
    protected object[] _values { get; private set; }
    private int _hashcode;

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

    public TupleBase(object[] values) {
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
  /// Base type for relations
  /// </summary>
  public abstract class RelationBase<Ttuple> where Ttuple:TupleBase {
    public static string[] Heading { get; protected set; }
    public static string[] Key { get; protected set; }
    protected HashSet<Ttuple> Body { get; private set; }
    private int _hashcode;

    public override int GetHashCode() {
      return _hashcode;
    }

    public override string ToString() {
      return Body.Take(5).Join(";");
    }

    public override bool Equals(object obj) {
      if (!(obj is RelationBase<Ttuple>)) return false;
      var other = ((RelationBase<Ttuple>)obj);
      if (other.Body.Count != Body.Count) return false;
      foreach (var b in Body)
        if (!other.Body.Contains(b)) return false;
      return true;
    }

    public RelationBase(IList<Ttuple> tuples) {
      Body = new HashSet<Ttuple>(tuples);
      _hashcode = CalcHashCode(Body);
    }

    internal static int CalcHashCode(HashSet<Ttuple> body) {
      int code = 1;
      foreach (Ttuple b in body)
        code = (code << 1) ^ b.GetHashCode();
      return code;
    }

    public IEnumerator<Ttuple> GetEnumerator() {
      foreach (var tuple in Body)
        yield return tuple;
    }

  }

}