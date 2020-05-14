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
  public class RelationBase<Ttup>
  where Ttup : TupleBase, new() {
    public static string[] Heading { get; protected set; }
    public int Count { get { return Body.Count; } }
    public bool IsEmpty { get { return Body.Count == 0; } }
    public bool Exists { get { return Body.Count > 0; } }

    internal HashSet<Ttup> Body { get; private set; }
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
      if (!(obj is RelationBase<Ttup>)) return false;
      var other = ((RelationBase<Ttup>)obj);
      if (other.Body.Count != Body.Count) return false;
      foreach (var b in Body)
        if (!other.Body.Contains(b)) return false;
      return true;
    }

    // create a new tuple of matching type generically
    public static Ttup NewTuple(object[] values) {
      return new Ttup() {
        Values = values,
        HashCode = TupleBase.CalcHashCode(values),
      };
    }

    //--- ctor
    static RelationBase() {
      // reflection hack to get heading value from tuple
      var prop = typeof(Ttup).GetField("Heading");
      Heading = (string[])prop.GetValue(null);
      Logger.Assert(Heading != null, "Heading must not be null");
    }

    public RelationBase() {
      Body = new HashSet<Ttup>();
    }

    // create new relation value from body as set
    public static Trel Create<Trel>(ISet<Ttup> tuples)
    where Trel : RelationBase<Ttup>, new() {

      var body = new HashSet<Ttup>(tuples);
      return new Trel() {
        Body = body,
        _hashcode = CalcHashCode(body),
      };
    }

    // create new relation value from body as enumerable
    public static Trel Create<Trel>(IEnumerable<Ttup> tuples)
    where Trel : RelationBase<Ttup>, new() {

      return Create<Trel>(new HashSet<Ttup>(tuples));
    }

    ///===========================================================================
    /// Functions that return a scalar value
    /// 

    // return singleton tuple: error if none, random if more than one
    public Ttup Single() {
      return Body.First();
    }

    public bool Contains(Ttup tuple) {
      return Body.Contains(tuple);
    }

    // this relation is a subset of other
    public bool IsSubset(RelationBase<Ttup> other) {
      return Body.All(b => other.Body.Contains(b));
    }

    // this relation is a superset of other
    public bool IsSuperset(RelationBase<Ttup> other) {
      return other.Body.All(b => Body.Contains(b));
    }

    // this relation has no tuples in common with other
    public bool IsDisjoint(RelationBase<Ttup> other) {
      return !Body.Any(b => other.Body.Contains(b));
    }

    ///===========================================================================
    /// functions that return a relation 
    /// Note that functions return the new value, so thay can
    /// be used as a fluent interface (left to right)
    /// 

    // generate a new relation with a selection of tuples
    public RelationBase<Ttup> Select(Func<Ttup, bool> predicate) {
      return Create<RelationBase<Ttup>>(Body.Where(t => predicate(t)));
    }

    // generate a new relation with one attribute renamed
    public RelationBase<T> Rename<T>()
    where T : TupleBase, new() {

      var newbody = RelOps.Rename<Ttup, T>(Body);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    // generate a new relation that is a projection
    public RelationBase<T> Project<T>()
    where T : TupleBase, new() {

      var newbody = RelOps.Project<Ttup, T>(Body);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    public RelationBase<T> Extend<T>(Func<Ttup, object> func)
    where T : TupleBase, new() {

      var newbody = RelOps.Extend<Ttup, T>(Body, func);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    public RelationBase<T> Transform<T>(Func<Ttup, T> func)
    where T : TupleBase, new() {

      var newbody = RelOps.Transform<Ttup, T>(Body, func);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    public RelationBase<T> Aggregate<T,T1>(Func<Ttup, T1, T1> func)
    where T : TupleBase, new()
    where T1 : new() {

      var newbody = RelOps.Aggregate<Ttup,T,T1>(Body, func);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    // generate a new relation that is a set union
    public RelationBase<Ttup> Union(RelationBase<Ttup> other) {

      var newbody = RelOps.Union<Ttup>(Body, other.Body);
      return Create<RelationBase<Ttup>>(newbody);
    }

    // generate a new relation that is a set minus
    public RelationBase<Ttup> Minus(RelationBase<Ttup> other) {

      var newbody = RelOps.Minus<Ttup>(Body, other.Body);
      return Create<RelationBase<Ttup>>(newbody);
    }

    // generate a new relation that is a set intersection
    public RelationBase<Ttup> Intersect(RelationBase<Ttup> other) {

      var newbody = RelOps.Intersect<Ttup>(Body, other.Body);
      return Create<RelationBase<Ttup>>(newbody);
    }

    // generate a new relation that is a natural join
    public RelationBase<T> Join<T1, T>(RelationBase<T1> other)
    where T : TupleBase, new()
    where T1 : TupleBase, new() {

      var newbody = RelOps.Join<T, Ttup, T1>(Body, other.Body);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    public RelationBase<T> AntiJoin<T1, T>(RelationBase<T1> other)
    where T : TupleBase, new()
    where T1 : TupleBase, new() {

      var newbody = RelOps.AntiJoin<T, Ttup, T1>(Body, other.Body);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    public RelationBase<Ttup> While(Func<RelationBase<Ttup>, RelationBase<Ttup>> func) {

      var newbody = RelOps.While<Ttup>(Body, func);
      return RelationBase<Ttup>.Create<RelationBase<Ttup>>(newbody);
    }

    // --- impl
    internal static int CalcHashCode(HashSet<Ttup> body) {
      int code = 1;
      foreach (Ttup b in body)
        code = (code << 1) ^ b.GetHashCode();
      return code;
    }

    public IEnumerator<Ttup> GetEnumerator() {
      foreach (var tuple in Body)
        yield return tuple;
    }
  }


  ///===========================================================================
  /// relational stores
  /// 

  public class RelVar<Ttup>
  where Ttup : TupleBase, new() {

    public RelationBase<Ttup> Value { get; private set; }

    public static implicit operator RelationBase<Ttup>(RelVar<Ttup> v) => v.Value;

    public override string ToString() => Value.ToString();

    public string Format() => Value.Format();

    public RelVar() {
      Value = new RelationBase<Ttup>();
    }

    public static RelVar<Ttup> Create(RelationBase<Ttup> value) {
      return new RelVar<Ttup> {
        Value = value,
      };
    }

    public void Assign(RelationBase<Ttup> value) {
      Value = value;
    }

    public void Insert(RelationBase<Ttup> value) {

      var newbody = RelOps.Union(Value.Body, value.Body);
      Value = RelationBase<Ttup>.Create<RelationBase<Ttup>>(newbody);
    }

    public void Update(Func<Ttup,bool> selfunc, Func<Ttup,Ttup> upfunc) {

      var newbody = Value.Body.Select(t => selfunc(t) ? upfunc(t) : t);
      Value = RelationBase<Ttup>.Create<RelationBase<Ttup>>(newbody);
    }

    public void Delete(Func<Ttup,bool> selfunc) {

      var newbody = Value.Body.Where(t => !selfunc(t));
      Value = RelationBase<Ttup>.Create<RelationBase<Ttup>>(newbody);
    }



  }

}