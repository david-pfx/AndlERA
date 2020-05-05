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

    public string Format(string[] heading) {
      return Enumerable.Range(0, _values.Length)
        .Select(x => heading[x] + ": " + _values[x].ToString())
        .Join(", ");
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

  ///===========================================================================
  /// <summary>
  /// Base type for relations
  /// </summary>
  public class RelationBase<Ttuple> where Ttuple:TupleBase {
    public static string[] Heading { get; protected set; }
    public int Count { get { return Body.Count; } }
    public bool IsEmpty { get { return Body.Count == 0; } }
    public bool Exists { get { return Body.Count > 0; } }

    protected HashSet<Ttuple> Body { get; private set; }
    private int _hashcode;

    public override int GetHashCode() {
      return _hashcode;
    }

    public override string ToString() {
      //return Body.Take(5).Join(";");
      return Body.Take(5).Select(t => t.Format(Heading)).Join("\n");
    }

    public override bool Equals(object obj) {
      if (!(obj is RelationBase<Ttuple>)) return false;
      var other = ((RelationBase<Ttuple>)obj);
      if (other.Body.Count != Body.Count) return false;
      foreach (var b in Body)
        if (!other.Body.Contains(b)) return false;
      return true;
    }

    //--- ctor
    static RelationBase() {
      // hack to get heading value from tuple
      var prop = typeof(Ttuple).GetField("Heading");
      Heading = (string[])prop.GetValue(null);
    }

    public RelationBase(IEnumerable<Ttuple> tuples) {
      Body = new HashSet<Ttuple>(tuples);
      _hashcode = CalcHashCode(Body);
    }

    // return singleton tuple: error if none, random if more than one
    public Ttuple Single() {
      return Body.First();
    }

    public bool Contains(Ttuple tuple) {
      return Body.Contains(tuple);
    }

    public RelationBase<Ttuple> Select(Func<Ttuple, bool> predicate) {
      return new RelationBase<Ttuple>(Body.Where(t => predicate(t)));
    }

    // --- impl
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

  ///===========================================================================
  /// <summary>
  /// 
  /// </summary>
  public class TupSequence : TupleBase {
    public readonly static string[] Heading = { "N" };
    public int N { get { return (int)_values[0]; } }

    //static TupSequence() {
    //  Heading = new string[] { "N" };
    //}

    public TupSequence(int N) : base(
      new object[] { N }) {
    }
  }

  public class RelSequence : RelationBase<TupSequence> {
    public RelSequence(int count) 
      : base(Enumerable.Range(0, count).Select(n => new TupSequence(n))) {
    }

  }

  ///===========================================================================
  /// <summary>
  /// 
  /// </summary>
  public class TupText : TupleBase {
    public readonly static string[] Heading = { "Seq", "Line" };

    public int Seq { get { return (int)_values[0]; } }
    public string Line { get { return (string)_values[1]; } }

    public TupText(int Seq, string Line) : base(
      new object[] { Seq, Line }) {
    }
  }

  public class RelText : RelationBase<TupText> {
    public RelText(IList<string> text)
      : base(Enumerable.Range(0, text.Count).Select(n => new TupText(n, text[n]))) {
    }

  }
}