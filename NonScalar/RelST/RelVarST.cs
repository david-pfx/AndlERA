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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Andl.Common;

namespace AndlEra {
  ///===========================================================================
  /// relational stores
  /// 

  // relvar based on tuple type
  public class RelVarST<T> : IEnumerable<T>
  where T : TupBase, new() {

    // internal class to hold value as relation
    public class Rel : RelValueST<T> {
      public static new Rel Create(IEnumerable<T> tuples) {
        return RelValueST<T>.Create<Rel>(tuples);
      }
    }

    public Rel Value { get; protected set; }
    public CommonHeading Heading { get; protected set; }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Value).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
    public static implicit operator RelValueST<T>(RelVarST<T> v) => v.Value;

    public override bool Equals(object obj) {
      var other = obj as RelVarST<T>;
      if (other == null) return false;
      return other.Heading.IsEqual(Heading) && other.Value.Equals(Value);
    }

    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    // format using relvar heading, not inner class
    public string Format() {
      return Value.Select(t => t.Format(Heading)).Join("\n");
    }

    public RelVarST() {
      Value = Rel.Create(new T[0]);
      Heading = RelBaseST<T>.Heading;
    }

    public RelVarST(RelValueST<T> tuples) {
      Value = Rel.Create(tuples);
      Heading = RelBaseST<T>.Heading;
    }

    public RelVarST(RelNode value) {
      Heading = RelBaseST<T>.Heading;
      Init(value);
    }

    protected void Init(RelNode value) {
      var equal = value.Heading.IsEqual(Heading);
      var compat = equal || value.Heading.IsCompatible(Heading);
      if (!compat) throw Error.Fatal($"headings are not compatible: <{value.Heading}> and <{Heading}>");
      var map = Heading.CreateMap(value.Heading);
      Value = Rel.Create(value.Select(t => RelStatic.CreateByMap<T>(t, map)));
    }

    // relational assignment
    public void Assign(RelValueST<T> value) {
      Value = Rel.Create(value);
    }

    // insert tuples, discard duplicates
    public void Insert(RelValueST<T> value) {

      Value = Rel.Create(RelOpsST.Union(Value, value));
    }

    // update tuples that satisfy predicate
    public void Update(Func<T, bool> selfunc, Func<T, T> upfunc) {

      Value = Rel.Create(Value.Select(t => selfunc(t) ? upfunc(t) : t));
    }

    // remove tuples that satisfy predicate
    public void Delete(Func<T, bool> selfunc) {

      Value = Rel.Create(Value.Where(t => !selfunc(t)));
    }
  }

}
