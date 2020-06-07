using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Andl.Common;

namespace AndlEra {
  ///===========================================================================
  /// <summary>
  /// Base type for relation values
  /// </summary>
  public class RelValue<Ttup> : RelBase<Ttup>
  where Ttup : TupBase, new() {

    public RelValue() : base() { }

    // create new relation value from body as set
    public static RelValue<Ttup> Create(ISet<Ttup> tuples) {

      return RelBase<Ttup>.Create<RelValue<Ttup>>(tuples);
    }

    // create new relation value from body as enumerable
    public static RelValue<Ttup> Create(IEnumerable<Ttup> tuples) {

      return RelBase<Ttup>.Create<RelValue<Ttup>>(new HashSet<Ttup>(tuples));
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
    public bool IsSubset(RelValue<Ttup> other) {
      return this.All(b => other.Contains(b));
    }

    // this relation is a superset of other
    public bool IsSuperset(RelValue<Ttup> other) {
      return other.All(b => this.Contains(b));
    }

    // this relation has no tuples in common with other
    public bool IsDisjoint(RelValue<Ttup> other) {
      return !this.Any(b => other.Contains(b));
    }

    ///-------------------------------------------------------------------------
    /// functions that return a relation 
    /// Note that functions return the new value, so thay can
    /// be used as a fluent interface (left to right)
    /// 

    // generate a new relation with a selection of tuples
    public RelValue<Ttup> Restrict(Func<Ttup, bool> predicate) {
      return Create<RelValue<Ttup>>(this.Where(t => predicate(t)));
    }

    // generate a new relation with one attribute renamed
    public RelValue<T> Rename<T>()
    where T : TupBase, new() {

      var newbody = RelOps.Rename<Ttup, T>(this);
      return RelValue<T>.Create<RelValue<T>>(newbody);
    }

    // generate a new relation that is a projection
    public RelValue<T> Project<T>()
    where T : TupBase, new() {

      var newbody = RelOps.Project<Ttup, T>(this);
      return RelValue<T>.Create<RelValue<T>>(newbody);
    }

    public RelValue<T> Extend<T>(Func<Ttup, object> func)
    where T : TupBase, new() {

      var newbody = RelOps.Extend<Ttup, T>(this, func);
      return RelValue<T>.Create<RelValue<T>>(newbody);
    }

    public RelValue<T> Transform<T>(Func<Ttup, T> func)
    where T : TupBase, new() {

      var newbody = RelOps.Transform<Ttup, T>(this, func);
      return RelValue<T>.Create<RelValue<T>>(newbody);
    }

    public RelValue<T> Aggregate<T, T1>(Func<Ttup, T1, T1> func)
    where T : TupBase, new()
    where T1 : new() {

      var newbody = RelOps.Aggregate<Ttup, T, T1>(this, func);
      return RelValue<T>.Create<RelValue<T>>(newbody);
    }

    // generate a new relation that is a set union
    public RelValue<Ttup> Union(RelValue<Ttup> other) {

      var newbody = RelOps.Union<Ttup>(this, other);
      return Create<RelValue<Ttup>>(newbody);
    }

    // generate a new relation that is a set minus
    public RelValue<Ttup> Minus(RelValue<Ttup> other) {

      var newbody = RelOps.Minus<Ttup>(this, other);
      return Create<RelValue<Ttup>>(newbody);
    }

    // generate a new relation that is a set intersection
    public RelValue<Ttup> Intersect(RelValue<Ttup> other) {

      var newbody = RelOps.Intersect<Ttup>(this, other);
      return Create<RelValue<Ttup>>(newbody);
    }

    // generate a new relation that is a natural join
    public RelValue<T> Join<T1, T>(RelValue<T1> other)
    where T : TupBase, new()
    where T1 : TupBase, new() {

      var newbody = RelOps.Join<T, Ttup, T1>(this, other);
      return RelValue<T>.Create<RelValue<T>>(newbody);
    }

    public RelValue<T> AntiJoin<T1, T>(RelValue<T1> other)
    where T : TupBase, new()
    where T1 : TupBase, new() {

      var newbody = RelOps.AntiJoin<T, Ttup, T1>(this, other);
      return RelValue<T>.Create<RelValue<T>>(newbody);
    }

    public RelValue<Ttup> While<Trel>(Func<Trel, RelValue<Ttup>> func)
    where Trel : RelValue<Ttup>, new() {

      var newbody = RelOps.While<Ttup, Trel>(this, func);
      return RelValue<Ttup>.Create<RelValue<Ttup>>(newbody);
    }
  }
}
