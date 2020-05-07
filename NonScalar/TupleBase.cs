// Non-scalar base types
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Andl.Common;

namespace AndlEra {
  ///===========================================================================
  /// <summary>
  /// Base type for tuples
  /// </summary>
  public abstract class TupleBase {
    protected internal object[] _values { get; internal set; }
    internal int _hashcode;

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

    public string Format(string[] heading) {
      return Enumerable.Range(0, _values.Length)
        .Select(x => heading[x] + ": " + _values[x].ToString())
        .Join(", ");
    }

    protected void Init(object[] values) {
      _values = values;
      _hashcode = CalcHashCode(values);
    }

      //--- impl
      internal static int CalcHashCode(object[] values) {
      int code = 1;
      foreach (object value in values)
        code = (code << 1) ^ value.GetHashCode();
      return code;
    }

    // build new values array using map of indexes
    internal object[] MapValues(IList<int> map) {
      return Enumerable.Range(0, map.Count)
        .Select(x => _values[map[x]])
        .ToArray();
    }
  }
}