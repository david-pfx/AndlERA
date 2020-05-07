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
    public static T Create<T>(IEnumerable<Ttuple> tuples) where T:RelationBase<Ttuple>,new() {
      var body = new HashSet<Ttuple>(tuples);
      return new T() {
        Body = body,
        _hashcode = CalcHashCode(body),
      };
    }

    // return singleton tuple: error if none, random if more than one
    public Ttuple Single() {
      return Body.First();
    }

    public bool Contains(Ttuple tuple) {
      return Body.Contains(tuple);
    }

    public RelationBase<Ttuple> Select(Func<Ttuple, bool> predicate) {
      return Create<RelationBase<Ttuple>>(Body.Where(t => predicate(t)));
    }

    public static RelationBase<Ttuple> Rename<T>(RelationBase<T> relation)
    where T : TupleBase, new() {
      var map = MapRename(Heading, RelationBase<T>.Heading);
      var body = relation.Body.Select(t => NewTuple(t.MapValues(map)));
      return Create<RelationBase<Ttuple>>(body);
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
        if (Array.FindIndex(map, i => i == ox) == -1) {
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