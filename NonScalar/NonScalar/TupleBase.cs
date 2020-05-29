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
  /// Base type for tuples
  /// </summary>
  public abstract class TupleBase {
    // Ordered set of values, accessible to create in relation
    protected internal object[] Values { get; internal set; }
    // Calculated hash code, never changes, accessible to create in relation
    internal int HashCode { get; set; }

    public object this[int i] { get { return Values[i]; } }
    public int Degree { get { return Values.Length; } }

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
      return Values.Join(",");
    }

    public string Format(CommonHeading heading) {
      return Enumerable.Range(0, Values.Length)
        .Select(x => heading[x].Format(Values[x]))
        .Join(", ");
    }

    public TupleBase() {
      Values = new object[0];
    }

    public static T Create<T>(object[] values)
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
    internal static CommonHeading GetHeading(Type ttype) {
      var prop = ttype.GetField("Heading");
      var heading = (string)prop.GetValue(null);
      if (heading == null) throw Error.NullArg("Heading must not be null");
      return CommonHeading.Create(heading);   // TODO: add types
    }

    //--- impl
    internal static int CalcHashCode(object[] values) {
      int code = 1;
      foreach (object value in values)
        code = (code << 1) ^ value.GetHashCode();
      return code;
    }
  }
}