﻿/// Andl is A New Data Language. See http://andl.org.
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
using System.Text;
using System.Threading.Tasks;
using Andl.Common;

namespace AndlEra {
  ///===========================================================================
  /// <summary>
  /// Base type for relations, parameterised on tuple type
  /// Uses settable heading
  /// </summary>
  public class RelBase<T> : IEnumerable<T>
  where T : TupBase, new() {
    public CommonHeading Heading { get; protected set; }
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
      var other = obj as RelBase<T>;
      if (obj == null) return false;
      if (!(other._body.Count == _body.Count && other.GetHashCode() == GetHashCode())) return false;
      foreach (var b in _body)
        if (!other._body.Contains(b)) return false;
      return true;
    }

    // initialiser for use by subclasses
    protected void Init<TRel>(HashSet<T> body) {
      _body = body;
      _hashcode = CalcHashCode(_body);
    }
  }
}
