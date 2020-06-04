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
  /// Base type for relations
  /// </summary>
  public class RelationBase<Ttup> : IEnumerable<Ttup>
  where Ttup : TupleBase, new() {
    public static CommonHeading Heading { get; protected set; }
    public int Count { get { return _body.Count; } }
    public bool IsEmpty { get { return _body.Count == 0; } }
    public bool Exists { get { return _body.Count > 0; } }

    protected HashSet<Ttup> _body { get; set; }
    int _hashcode;

    public override int GetHashCode() {
      return _hashcode;
    }

    public override string ToString() {
      return (Count > 5) ? _body.Take(5).Join(";") + "..."
        : _body.Join(";");
    }

    // interfaces
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Ttup>)_body).GetEnumerator();
    public IEnumerator<Ttup> GetEnumerator() => ((IEnumerable<Ttup>)_body).GetEnumerator();

    // --- impl

    // hashcode is constant, indepedent of tuple order
    internal static int CalcHashCode(HashSet<Ttup> body) {
      int code = 261;
      foreach (Ttup b in body)
        code = code ^ b.GetHashCode();
      return code;
    }

    public string Format() {
      return this.Select(t => t.Format(Heading)).Join("\n");
    }

    // override equals based on type and content 
    // same type means same heading
    public override bool Equals(object obj) {
      var other = obj as RelationBase<Ttup>;
      if (obj == null) return false;
      if (!(other._body.Count == _body.Count && other.GetHashCode() == GetHashCode())) return false;
      foreach (var b in _body)
        if (!other._body.Contains(b)) return false;
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

    // set up heading here in case relation is never instantiated
    static RelationBase() {
      Heading = TupleBase.GetHeading(typeof(Ttup));
    }

    public RelationBase() {
      Heading = TupleBase.GetHeading(typeof(Ttup));
      _body = new HashSet<Ttup>();
    }

    // create new relation value from body as set
    public static Trel Create<Trel>(ISet<Ttup> tuples)
    where Trel : RelationBase<Ttup>, new() {

      var body = new HashSet<Ttup>(tuples);
      return new Trel() {
        _body = body,
        _hashcode = CalcHashCode(body),
      };
    }

    // create new relation value from body as enumerable
    public static Trel Create<Trel>(IEnumerable<Ttup> tuples)
    where Trel : RelationBase<Ttup>, new() {

      return Create<Trel>(new HashSet<Ttup>(tuples));
    }

    ///-------------------------------------------------------------------------
    /// Functions that return a scalar value
    /// 
    // return singleton tuple: error if none, random if more than one
    public Ttup Single() {
      return this.First();
    }

    public bool Contains(Ttup tuple) {
      return this.Contains(tuple);
    }

    // this relation is a subset of other
    public bool IsSubset(RelationBase<Ttup> other) {
      return this.All(b => other.Contains(b));
    }

    // this relation is a superset of other
    public bool IsSuperset(RelationBase<Ttup> other) {
      return other.All(b => this.Contains(b));
    }

    // this relation has no tuples in common with other
    public bool IsDisjoint(RelationBase<Ttup> other) {
      return !this.Any(b => other.Contains(b));
    }

    ///-------------------------------------------------------------------------
    /// functions that return a relation 
    /// Note that functions return the new value, so thay can
    /// be used as a fluent interface (left to right)
    /// 

    // generate a new relation with a selection of tuples
    public RelationBase<Ttup> Restrict(Func<Ttup, bool> predicate) {
      return Create<RelationBase<Ttup>>(this.Where(t => predicate(t)));
    }

    // generate a new relation with one attribute renamed
    public RelationBase<T> Rename<T>()
    where T : TupleBase, new() {

      var newbody = RelOps.Rename<Ttup, T>(this);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    // generate a new relation that is a projection
    public RelationBase<T> Project<T>()
    where T : TupleBase, new() {

      var newbody = RelOps.Project<Ttup, T>(this);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    public RelationBase<T> Extend<T>(Func<Ttup, object> func)
    where T : TupleBase, new() {

      var newbody = RelOps.Extend<Ttup, T>(this, func);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    public RelationBase<T> Transform<T>(Func<Ttup, T> func)
    where T : TupleBase, new() {

      var newbody = RelOps.Transform<Ttup, T>(this, func);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    public RelationBase<T> Aggregate<T,T1>(Func<Ttup, T1, T1> func)
    where T : TupleBase, new()
    where T1 : new() {

      var newbody = RelOps.Aggregate<Ttup,T,T1>(this, func);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    // generate a new relation that is a set union
    public RelationBase<Ttup> Union(RelationBase<Ttup> other) {

      var newbody = RelOps.Union<Ttup>(this, other);
      return Create<RelationBase<Ttup>>(newbody);
    }

    // generate a new relation that is a set minus
    public RelationBase<Ttup> Minus(RelationBase<Ttup> other) {

      var newbody = RelOps.Minus<Ttup>(this, other);
      return Create<RelationBase<Ttup>>(newbody);
    }

    // generate a new relation that is a set intersection
    public RelationBase<Ttup> Intersect(RelationBase<Ttup> other) {

      var newbody = RelOps.Intersect<Ttup>(this, other);
      return Create<RelationBase<Ttup>>(newbody);
    }

    // generate a new relation that is a natural join
    public RelationBase<T> Join<T1, T>(RelationBase<T1> other)
    where T : TupleBase, new()
    where T1 : TupleBase, new() {

      var newbody = RelOps.Join<T, Ttup, T1>(this, other);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    public RelationBase<T> AntiJoin<T1, T>(RelationBase<T1> other)
    where T : TupleBase, new()
    where T1 : TupleBase, new() {

      var newbody = RelOps.AntiJoin<T, Ttup, T1>(this, other);
      return RelationBase<T>.Create<RelationBase<T>>(newbody);
    }

    public RelationBase<Ttup> While<Trel>(Func<Trel, RelationBase<Ttup>> func) 
    where Trel : RelationBase<Ttup>, new() {

      var newbody = RelOps.While<Ttup,Trel>(this, func);
      return RelationBase<Ttup>.Create<RelationBase<Ttup>>(newbody);
    }
  }

  ///===========================================================================
  /// <summary>
  /// Untyped relation base
  /// Probably buggy: trouble with initialising heading
  /// </summary>
  public class RelationBase : RelationBase<Tup> {
    public RelationBase() {
      Heading = CommonHeading.Empty;
      _body = new HashSet<Tup>();
    }

    //public RelationBase(RelationNode node) {
    //  Heading = node.Heading;
    //  _body = node.ToHashSet<TupleBase>();
    //}

  }

}