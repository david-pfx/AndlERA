/// Andl is A New Data Language. See http://andl.org.
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

// Non-scalar base types
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Andl.Common;

namespace AndlEra {
  ///===========================================================================
  /// <summary>
  /// Base type for relational tuples
  /// Values but no heading
  /// Gets heading from type, on request
  /// </summary>
  public abstract class TupBase : IEnumerable<object> {
    // Ordered set of values, accessible to create in relation
    protected internal object[] Values { get; internal set; }
    // Calculated hash code, never changes, accessible to create in relation
    internal int HashCode { get; set; }

    public object this[int i] { get { return Values[i]; } }
    public int Degree { get { return Values.Length; } }

    IEnumerator<object> IEnumerable<object>.GetEnumerator() => ((IEnumerable<object>)Values).GetEnumerator();
    public IEnumerator GetEnumerator() => Values.GetEnumerator();

    // override used by hash collections
    public override int GetHashCode() {
      return HashCode;
    }

    public override bool Equals(object obj) {
      if (!(obj is TupBase)) return false;
      var other = (TupBase)obj;
      if (other.Values.Length != Values.Length) return false;
      for (int x = 0; x < Values.Length; ++x)
        if (!Values[x].Equals(other.Values[x])) return false;
      return true;
    }
    public override string ToString() {
      return Values.Join(",");  // TODO:deal with TVA and RVA
    }

    public string Format(CommonHeading heading) {
      return Enumerable.Range(0, Values.Length)
        .Select(x => heading[x].Format(Values[x]))
        .Join(", ");
    }

    public TupBase() {
      Values = new object[0];
    }

    public static T Create<T>(params object[] values)
    where T : TupBase, new() {
      return new T() {
        Values = values,
        HashCode = CalcHashCode(values),
      };
    }

    internal T Init<T>(object[] values)
    where T : TupBase {

      Values = values;
      HashCode = CalcHashCode(values);
      return this as T;
    }

    //--- impl
    
    // hashcode is constant, independent of value order
    internal static int CalcHashCode(object[] values) {
      int code = 261;
      foreach (object value in values)
        code = code ^ value.GetHashCode();
      return code;
    }
  }
}