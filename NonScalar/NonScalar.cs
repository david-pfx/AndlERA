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

  ///===========================================================================
  /// <summary>
  /// Base type for relations
  /// </summary>
  public class RelationBase<Ttuple> where Ttuple:TupleBase,new() {
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

    // create a new tuple of matching type generically
    public static Ttuple NewTuple(object[] values) {
      return new Ttuple() { 
        _values = values,
        _hashcode = TupleBase.CalcHashCode(values),
      };
    }

    //--- ctor
    static RelationBase() {
      // hack to get heading value from tuple
      var prop = typeof(Ttuple).GetField("Heading");
      Heading = (string[])prop.GetValue(null);
    }

    // create new relation value from body
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

    public RelationBase<Ttuple> Rename<T>() 
    where T:TupleBase,new() {
      var map = MapRename(Heading, RelationBase<T>.Heading);
      var body = Body.Select(t => NewTuple(t.MapValues(map)));
      return new RelationBase<Ttuple>(body);
    }

    // construct renaming map from two headings
    private static int[] MapRename(string[] head, string[] ohead) {
      var map = new int[head.Length];
      var odd = -1;
      for (int hx = 0; hx < head.Length; ++hx) {
        map[hx] = Array.FindIndex(ohead, s => s == Heading[hx]);  // equals?
        if (map[hx] == -1) {
          Logger.Assert(odd == -1);
          odd = hx;
        }
      }
      for (int ox = 0; ox < ohead.Length; ++ox) {
        if (Array.FindIndex(map, i => i == map[ox]) == -1) {
          map[odd] = ox;
          break;
        }
      }
      return map;
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

  
}