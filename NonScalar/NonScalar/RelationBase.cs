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

    // set up heading here when relation not instantiated but used as class
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

    protected void Init<TRel>(HashSet<Ttup> body) {
      _body = new HashSet<Ttup>();
      _hashcode = CalcHashCode(_body);
    }

    //protected RelationBase(HashSet<Ttup> body) {
    //  Heading = TupleBase.GetHeading(typeof(Ttup));
    //  _body = body;
    //  _hashcode = CalcHashCode(_body);
  }
}