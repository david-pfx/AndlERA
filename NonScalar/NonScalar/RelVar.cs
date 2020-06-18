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
/// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Andl.Common;

namespace AndlEra {
  ///===========================================================================
  /// <summary>
  /// Untyped relvar, with update operators
  /// </summary>
  public class RelVar : IEnumerable<Tup> {

    public RelValue Value { get; protected set; }
    public CommonHeading Heading { get { return Value.Heading; } }

    public static implicit operator RelValue(RelVar v) => v.Value;

    public override bool Equals(object obj) {
      var other = obj as RelVar;
      if (other == null) return false;
      return other.Heading.IsEqual(Heading) && other.Value.Equals(Value);
    }

    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    // format using relvar heading, not inner class
    public string Format() {
      return Value.Select(t => t.Format(Heading)).Join("\n");
    }


    public RelVar() {
      Value = new RelValue();
    }

    public RelVar(RelNode node) {
      Value = new RelValue(node);
    }

    public RelNode ToRelNode() {
      return new WrapperNode(Heading, Value);
    }

    public void Insert(RelNode node) {

      Value = new RelValue(Heading, Value.Body.Union(node));
    }

    // update tuples that satisfy predicate
    public void Update(string heading, Func<Tup, bool> selfunc, object newvalue) {

      // map for selection tuple
      var head1 = CommonHeading.Create(heading);
      var map1 = head1.CreateMap(Heading);
      // map for replace tuple
      var head2 = Heading.Remove(head1.Fields.Last()).Append(CommonField.Empty);
      var map2 = head2.CreateMap(Heading);
      Value = new RelValue(Heading, Value.Select(t => selfunc(RelStatic.CreateByMap<Tup>(t, map1))
        ? RelStatic.CreateByMap<Tup>(t, map2, newvalue) : t));
    }

    // remove tuples that satisfy predicate
    public void Delete(string heading, Func<Tup, bool> selfunc) {

      var head = CommonHeading.Create(heading);
      var map = head.CreateMap(Heading);
      Value = new RelValue(Heading, Value.Where(t => !selfunc(RelStatic.CreateByMap<Tup>(t, map))));
    }

    public IEnumerator<Tup> GetEnumerator() => ((IEnumerable<Tup>)Value).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
  }

  ///===========================================================================
  /// <summary>
  /// Untyped relvale
  /// </summary>
  public class RelValue : RelBase<Tup> {

    public HashSet<Tup> Body { get { return _body; } }

    public RelValue() : base() {
      Heading = CommonHeading.Empty;
      _body = new HashSet<Tup>();
    }

    // create new relation value from body as set
    public RelValue(CommonHeading heading, HashSet<Tup> body) {
      Heading = heading;
      _body = body;      
    }

    // create new relation value from body as enumerable
    public RelValue(CommonHeading heading, IEnumerable<Tup> body) {
      Heading = heading;
      _body = new HashSet<Tup>(body);
    }

    // create new relation value from relnode
    public RelValue(RelNode node) 
      : this(node.Heading, node.AsEnumerable<Tup>()) { }

    //--------------------------------------------------------------------------
    // RA Operations

    // return singleton tuple: error if none, random if more than one
    public Tup Single() {
      return Body.First();
    }

    public bool Contains(Tup tuple) {
      return Body.Contains(tuple);
    }

    // this relation is a subset of other stream
    public bool IsSubset(RelNode other) {
      int count = 0;
      foreach (var t in other) {
        if (Body.Contains(t) && ++count == Count)
          return true;
      }
      return false;
    }

    // this relation is a superset of other stream
    public bool IsSuperset(RelNode other) {
      return other.All(b => Body.Contains(b));
    }

    // this relation has no tuples in common with other
    public bool IsDisjoint(RelNode other) {
      return !other.Any(b => Body.Contains(b));
    }

  }
}
