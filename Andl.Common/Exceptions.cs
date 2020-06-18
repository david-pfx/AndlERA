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

namespace Andl.Common {
  [Serializable]
  public class AndlException : Exception {
    public AndlException(string msg) : base(msg) { }
  }

  /// <summary>
  /// Shortcut class to throw commonly used exceptions
  /// </summary>
  public class Error {
    public static Exception NullArg(string arg) {
      return new ArgumentNullException(arg);
    }
    public static Exception Invalid(string msg) {
      return new InvalidOperationException(msg);
    }
    public static Exception MustOverride(string arg) {
      return new NotImplementedException($"must override {arg}");
    }
    public static Exception NotImpl(string msg) {
      return new NotImplementedException(msg);
    }
    public static Exception Argument(string msg) {
      return new ArgumentException(msg);
    }
    public static Exception Fatal(string msg) {
      return new AndlException("Fatal error: " + msg);
    }
    public static Exception Fatal(string origin, string msg) {
      return new AndlException($"Fatal error ({origin}): {msg}");
    }
    public static Exception Assert(string msg) {
      return new AndlException("Assertion failure: " + msg);
    }
  }
}
