using System;
using System.Collections;
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

  ///===========================================================================
  /// <summary>
  /// Untyped relvar
  /// </summary>
  public class RelVar : RelVarST<Tup> {
    public RelVar() {
      Value = Rel.Create(new Tup[0]);
      Heading = CommonHeading.Empty;
    }

    //public RelVar(RelationBase<T> tuples) {
    //  Value = Rel.Create(tuples);
    //  Heading = RelationBase<Ttup>.Heading;
    //}

    public RelVar(RelNode value) {
      Heading = value.Heading;
      Init(value);
    }

    public RelNode ToRelNode() {
      return new WrapperNode(Heading, Value);
    }

    public void Insert(RelNode node) {

      Value = Rel.Create(RelOpsST.Union<Tup>(Value, node.Cast<Tup>()));
    }

    // update tuples that satisfy predicate
    public void Update(string heading, Func<Tup, bool> selfunc, object newvalue) {

      // map for selection tuple
      var head1 = CommonHeading.Create(heading);
      var map1 = head1.CreateMap(Heading);
      // map for replace tuple
      var head2 = Heading.Remove(head1.Fields.Last()).Append(CommonField.Empty);
      var map2 =  head2.CreateMap(Heading);
      Value = Rel.Create(Value.Select(t => selfunc(RelStatic.CreateByMap<Tup>(t, map1)) 
        ? RelStatic.CreateByMap<Tup>(t, map2, newvalue) : t));
    }

    // remove tuples that satisfy predicate
    public void Delete(string heading, Func<Tup, bool> selfunc) {

      var head = CommonHeading.Create(heading);
      var map = head.CreateMap(Heading);
      Value = Rel.Create(Value.Where(t => !selfunc(RelStatic.CreateByMap<Tup>(t, map))));
    }

  }

}
