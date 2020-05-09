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
      return (Count > 5) ? Body.Take(5).Join(";") + "..."
        : Body.Join(";");
    }

    public string Format() {
      return Body.Select(t => t.Format(Heading)).Join("\n");
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
        Values = values,
        HashCode = TupleBase.CalcHashCode(values),
      };
    }

    //--- ctor
    static RelationBase() {
      // hack to get heading value from tuple
      var prop = typeof(Ttuple).GetField("Heading");
      Heading = (string[])prop.GetValue(null);
    }

    // create new relation value from body
    public static T Create<T>(ISet<Ttuple> tuples) where T:RelationBase<Ttuple>,new() {
      var body = new HashSet<Ttuple>(tuples);
      return new T() {
        Body = body,
        _hashcode = CalcHashCode(body),
      };
    }

    public static T Create<T>(IEnumerable<Ttuple> tuples) where T : RelationBase<Ttuple>, new() {
      return Create<T>(new HashSet<Ttuple>(tuples));
    }

    // return singleton tuple: error if none, random if more than one
    public Ttuple Single() {
      return Body.First();
    }

    public bool Contains(Ttuple tuple) {
      return Body.Contains(tuple);
    }

    // generate a new relation with a selection of tuples
    public RelationBase<Ttuple> Select(Func<Ttuple, bool> predicate) {
      return Create<RelationBase<Ttuple>>(Body.Where(t => predicate(t)));
    }

    // generate a new relation with one attribute renamed
    public static RelationBase<Ttuple> Rename<T>(RelationBase<T> relation)
    where T : TupleBase, new() {

      var map = RelOps.MakeRenameMap(Heading, RelationBase<T>.Heading);
      var body = relation.Body.Select(t => NewTuple(RelOps.MapValues(t.Values, map)));
      return Create<RelationBase<Ttuple>>(body);
    }

    // generate a new relation that is a projection
    public static RelationBase<Ttuple> Project<T>(RelationBase<T> relation)
    where T : TupleBase, new() {

      var newbody = RelOps.Project<Ttuple,T>(relation.Body);
      return Create<RelationBase<Ttuple>>(newbody);
    }

    // generate a new relation that is a set union
    public static RelationBase<Ttuple> Union<T>(RelationBase<Ttuple> relation1, RelationBase<Ttuple> relation2)
    where T : TupleBase, new() {

      var newbody = RelOps.Minus<Ttuple>(relation1.Body, relation2.Body);
      return Create<RelationBase<Ttuple>>(newbody);
    }

    // generate a new relation that is a set minus
    public static RelationBase<Ttuple> Minus<T>(RelationBase<Ttuple> relation1, RelationBase<Ttuple> relation2)
    where T : TupleBase, new() {

      var newbody = RelOps.Minus<Ttuple>(relation1.Body, relation2.Body);
      return Create<RelationBase<Ttuple>>(newbody);
    }

    // generate a new relation that is a set intersection
    public static RelationBase<Ttuple> Intersect<T>(RelationBase<Ttuple> relation1, RelationBase<Ttuple> relation2)
    where T : TupleBase, new() {

      var newbody = RelOps.Intersect<Ttuple>(relation1.Body, relation2.Body);
      return Create<RelationBase<Ttuple>>(newbody);
    }

    // generate a new relation that is a natural join
    public static RelationBase<Ttuple> Join<T,T1,T2>(RelationBase<T1> relation1, RelationBase<T2> relation2)
    where T : TupleBase, new()
    where T1 : TupleBase, new()
    where T2 : TupleBase, new() {
      
      var newbody = RelOps.Join<Ttuple,T1,T2>(relation1.Body, relation2.Body);
      return Create<RelationBase<Ttuple>>(newbody);
    }

    class JK : TupleBase { }

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