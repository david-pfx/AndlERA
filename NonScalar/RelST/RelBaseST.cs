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
  /// Base type for relations, parameterised on tuple type
  /// Gets heading from tuple type parameter
  /// </summary>
  public class RelBaseST<T> : IEnumerable<T>
  where T : TupBase, new() {
    public static CommonHeading Heading { get; protected set; }
    public int Count { get { return _body.Count; } }
    public bool IsEmpty { get { return _body.Count == 0; } }
    public bool Exists { get { return _body.Count > 0; } }

    protected HashSet<T> _body { get; set; }
    int _hashcode;

    public override int GetHashCode() {
      return _hashcode;
    }

    public override string ToString() {
      return (Count > 5) ? _body.Take(5).Join(";") + "..."
        : _body.Join(";");
    }

    // interfaces
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)_body).GetEnumerator();
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_body).GetEnumerator();

    // --- impl

    // hashcode is constant, indepedent of tuple order
    internal static int CalcHashCode(HashSet<T> body) {
      int code = 261;
      foreach (T b in body)
        code = code ^ b.GetHashCode();
      return code;
    }

    public string Format() {
      return this.Select(t => t.Format(Heading)).Join("\n");
    }

    // override equals based on type and content 
    // same type means same heading
    public override bool Equals(object obj) {
      var other = obj as RelBaseST<T>;
      if (obj == null) return false;
      if (!(other._body.Count == _body.Count && other.GetHashCode() == GetHashCode())) return false;
      foreach (var b in _body)
        if (!other._body.Contains(b)) return false;
      return true;
    }

    // create a new tuple of matching type generically
    public static T NewTuple(object[] values) {
      return new T() {
        Values = values,
        HashCode = TupBase.CalcHashCode(values),
      };
    }

    //--- ctor

    // set up heading here when relation not instantiated but used to get heading
    static RelBaseST() {
      Heading = GetHeading(typeof(T));
    }

    public RelBaseST() {
      Heading = GetHeading(typeof(T));
      _body = new HashSet<T>();
    }

    // create new relation value from body as set
    public static Trel Create<Trel>(ISet<T> tuples)
    where Trel : RelBaseST<T>, new() {

      var body = new HashSet<T>(tuples);
      return new Trel() {
        _body = body,
        _hashcode = CalcHashCode(body),
      };
    }

    // create new relation value from body as enumerable
    public static Trel Create<Trel>(IEnumerable<T> tuples)
    where Trel : RelBaseST<T>, new() {

      return Create<Trel>(new HashSet<T>(tuples));
    }

    protected void Init<TRel>(HashSet<T> body) {
      _body = new HashSet<T>();
      _hashcode = CalcHashCode(_body);
    }

    //--- impl

    // reflection hack to get heading value from tuple
    // TODO: is this the best way to handle heading not found?
    internal static CommonHeading GetHeading(Type ttype) {
      var prop = ttype.GetField("Heading");
      if (prop == null) return CommonHeading.Empty;
      var heading = (string)prop.GetValue(null);
      if (heading == null) return CommonHeading.Empty;
      return CommonHeading.Create(heading);   // TODO: add types
    }

    //protected RelationBase(HashSet<Ttup> body) {
    //  Heading = TupleBase.GetHeading(typeof(Ttup));
    //  _body = body;
    //  _hashcode = CalcHashCode(_body);
  }
}