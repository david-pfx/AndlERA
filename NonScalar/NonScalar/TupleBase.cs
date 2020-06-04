﻿// Non-scalar base types
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
  /// Base type for tuples
  /// </summary>
  public abstract class TupleBase : IEnumerable<object> {
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
      if (!(obj is TupleBase)) return false;
      var other = (TupleBase)obj;
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

    public TupleBase() {
      Values = new object[0];
    }

    public static T Create<T>(params object[] values)
    where T : TupleBase, new() {
      return new T() {
        Values = values,
        HashCode = CalcHashCode(values),
      };
    }

    internal T Init<T>(object[] values)
    where T : TupleBase {

      Values = values;
      HashCode = CalcHashCode(values);
      return this as T;
    }

    // reflection hack to get heading value from tuple
    // TODO: is this the best way to handle heading not found?
    internal static CommonHeading GetHeading(Type ttype) {
      var prop = ttype.GetField("Heading");
      if (prop == null) return CommonHeading.Empty;
      var heading = (string)prop.GetValue(null);
      if (heading == null) return CommonHeading.Empty;
      return CommonHeading.Create(heading);   // TODO: add types
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