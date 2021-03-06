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
  public class RelBaseST<T> : RelBase<T>
  where T : TupBase, new() {

    // heading local to this class 'instance' for use by RelopsST
    public static new CommonHeading Heading { get; protected set; }

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

      var ret = new Trel();
      ret.Init<T>(new HashSet<T>(tuples));
      return ret;
    }

    // create new relation value from body as enumerable
    public static Trel Create<Trel>(IEnumerable<T> tuples)
    where Trel : RelBaseST<T>, new() {

      return Create<Trel>(new HashSet<T>(tuples));
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
  }
}