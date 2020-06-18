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
  /// Param by tuple type
  /// Provides library of <T> functions
  /// </summary>
  public class RelValueST<Ttup> : RelBaseST<Ttup>
  where Ttup : TupBase, new() {

    static RelValueST() {
      Heading = GetHeading(typeof(Ttup));
    }

    public RelValueST() : base() { }

    // create new relation value from body as set
    public static RelValueST<Ttup> Create(ISet<Ttup> tuples) {

      return RelBaseST<Ttup>.Create<RelValueST<Ttup>>(tuples);
    }

    // create new relation value from body as enumerable
    public static RelValueST<Ttup> Create(IEnumerable<Ttup> tuples) {

      return RelBaseST<Ttup>.Create<RelValueST<Ttup>>(new HashSet<Ttup>(tuples));
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
    public bool IsSubset(RelValueST<Ttup> other) {
      return this.All(b => other.Contains(b));
    }

    // this relation is a superset of other
    public bool IsSuperset(RelValueST<Ttup> other) {
      return other.All(b => this.Contains(b));
    }

    // this relation has no tuples in common with other
    public bool IsDisjoint(RelValueST<Ttup> other) {
      return !this.Any(b => other.Contains(b));
    }

    ///-------------------------------------------------------------------------
    /// functions that return a relation 
    /// Note that functions return the new value, so thay can
    /// be used as a fluent interface (left to right)
    /// 

    // generate a new relation with a selection of tuples
    public RelValueST<Ttup> Restrict(Func<Ttup, bool> predicate) {
      return Create<RelValueST<Ttup>>(this.Where(t => predicate(t)));
    }

    // generate a new relation with one attribute renamed
    public RelValueST<T> Rename<T>()
    where T : TupBase, new() {

      var newbody = RelOpsST.Rename<Ttup, T>(this);
      return RelValueST<T>.Create<RelValueST<T>>(newbody);
    }

    // generate a new relation that is a projection
    public RelValueST<T> Project<T>()
    where T : TupBase, new() {

      var newbody = RelOpsST.Project<Ttup, T>(this);
      return RelValueST<T>.Create<RelValueST<T>>(newbody);
    }

    public RelValueST<T> Extend<T>(Func<Ttup, object> func)
    where T : TupBase, new() {

      var newbody = RelOpsST.Extend<Ttup, T>(this, func);
      return RelValueST<T>.Create<RelValueST<T>>(newbody);
    }

    public RelValueST<T> Transform<T>(Func<Ttup, T> func)
    where T : TupBase, new() {

      var newbody = RelOpsST.Transform<Ttup, T>(this, func);
      return RelValueST<T>.Create<RelValueST<T>>(newbody);
    }

    public RelValueST<T> Aggregate<T, T1>(Func<Ttup, T1, T1> func)
    where T : TupBase, new()
    where T1 : new() {

      var newbody = RelOpsST.Aggregate<Ttup, T, T1>(this, func);
      return RelValueST<T>.Create<RelValueST<T>>(newbody);
    }

    // generate a new relation that is a set union
    public RelValueST<Ttup> Union(RelValueST<Ttup> other) {

      var newbody = RelOpsST.Union<Ttup>(this, other);
      return Create<RelValueST<Ttup>>(newbody);
    }

    // generate a new relation that is a set minus
    public RelValueST<Ttup> Minus(RelValueST<Ttup> other) {

      var newbody = RelOpsST.Minus<Ttup>(this, other);
      return Create<RelValueST<Ttup>>(newbody);
    }

    // generate a new relation that is a set intersection
    public RelValueST<Ttup> Intersect(RelValueST<Ttup> other) {

      var newbody = RelOpsST.Intersect<Ttup>(this, other);
      return Create<RelValueST<Ttup>>(newbody);
    }

    // generate a new relation that is a natural join
    public RelValueST<T> Join<T1, T>(RelValueST<T1> other)
    where T : TupBase, new()
    where T1 : TupBase, new() {

      var newbody = RelOpsST.Join<T, Ttup, T1>(this, other);
      return RelValueST<T>.Create<RelValueST<T>>(newbody);
    }

    public RelValueST<T> AntiJoin<T1, T>(RelValueST<T1> other)
    where T : TupBase, new()
    where T1 : TupBase, new() {

      var newbody = RelOpsST.AntiJoin<T, Ttup, T1>(this, other);
      return RelValueST<T>.Create<RelValueST<T>>(newbody);
    }

    public RelValueST<Ttup> While<Trel>(Func<Trel, RelValueST<Ttup>> func)
    where Trel : RelValueST<Ttup>, new() {

      var newbody = RelOpsST.While<Ttup, Trel>(this, func);
      return RelValueST<Ttup>.Create<RelValueST<Ttup>>(newbody);
    }
  }
}
