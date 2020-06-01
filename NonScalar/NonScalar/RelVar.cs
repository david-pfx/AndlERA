using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Andl.Common;

namespace AndlEra {
  ///===========================================================================
  /// relational stores
  /// 

  // relvar based on tuple type
  public class RelVar<Ttup>
  where Ttup : TupleBase, new() {

    // internal class to hold value as relation
    public class Rel : RelationBase<Ttup> {
      public static Rel Create(IEnumerable<Ttup> tuples) {
        return RelationBase<Ttup>.Create<Rel>(tuples);
      }
    }

    public Rel Value { get; protected set; }
    public CommonHeading Heading { get; protected set; }

    public static implicit operator RelationBase<Ttup>(RelVar<Ttup> v) => v.Value;

    public override string ToString() => Value.ToString();

    public string Format() => Value.Format();

    public RelVar() {
      Value = Rel.Create(new Ttup[0]);
      Heading = RelationBase<Ttup>.Heading;
    }

    public RelVar(RelationBase<Ttup> tuples) {
      Value = Rel.Create(tuples);
      Heading = RelationBase<Ttup>.Heading;
    }

    public RelVar(RelationNode value) {
      Heading = RelationBase<Ttup>.Heading;
      Init(value);
    }

    protected void Init(RelationNode value) {
      var equal = value.Heading.IsEqual(Heading);
      var compat = equal || value.Heading.IsCompatible(Heading);
      if (!compat) throw Error.Fatal($"headings are not compatible: <{value.Heading}> and <{Heading}>");
      var map = Heading.CreateMap(value.Heading);
      Value = Rel.Create(value.Select(t => RelOps.CreateByMap<Ttup>(t, map)));
    }

    // relational assignment
    public void Assign(RelationBase<Ttup> value) {
      Value = Rel.Create(value);
    }

    // insert tuples, discard duplicates
    public void Insert(RelationBase<Ttup> value) {

      Value = Rel.Create(RelOps.Union(Value, value));
    }

    // update tuples that satisfy predicate
    public void Update(Func<Ttup, bool> selfunc, Func<Ttup, Ttup> upfunc) {

      Value = Rel.Create(Value.Select(t => selfunc(t) ? upfunc(t) : t));
    }

    // remove tuples that satisfy predicate
    public void Delete(Func<Ttup, bool> selfunc) {

      Value = Rel.Create(Value.Where(t => !selfunc(t)));
    }
  }

  ///===========================================================================
  /// <summary>
  /// Untyped relvar
  /// </summary>
  public class RelVar : RelVar<TupMin> {
    public RelVar() {
      Value = Rel.Create(new TupMin[0]);
      Heading = CommonHeading.Empty;
    }

    //public RelVar(RelationBase<T> tuples) {
    //  Value = Rel.Create(tuples);
    //  Heading = RelationBase<Ttup>.Heading;
    //}

    public RelVar(RelationNode value) {
      Heading = value.Heading;
      Init(value);
    }

  }

}
