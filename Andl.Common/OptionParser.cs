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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Andl.Common {
  /// <summary>
  /// Parse some options and filenames
  /// </summary>
  public class OptionParser {
    public int Noisy { get { return Logger.Level; } }
    public int PathsCount { get { return _paths.Count; } }
    public string GetPath(int n) {
      return (n < _paths.Count) ? _paths[n] : null;
    }

    List<string> _paths = new List<string>();
    Dictionary<string, Action<string>> _options;
    string _help;

    public static OptionParser Create(Dictionary<string, Action<string>> options, string help) {
      return new OptionParser { _options = options, _help = help };
    }

    public bool Parse(string[] args) {
      for (var i = 0; i < args.Length; ++i) {
        if (args[i].StartsWith("/") || args[i].StartsWith("-")) {
          if (!Option(args[i].Substring(1), args[i].Substring(2, args[i].Length - 2)))
            return false;
        } else _paths.Add(args[i]);
      }
      return true;
    }

    // Capture the options
    bool Option(string arg, string rest) {
      if (arg == "?") {
        Logger.WriteLine(_help);
        return false;
      } else if (Regex.IsMatch(arg, "[0-9]+")) {
        Logger.Level = int.Parse(arg);
      } else if (_options.ContainsKey(arg)) {
        _options[arg](rest);
      } else {
        Logger.WriteLine("*** Bad option: {0}", arg);
        return false;
      }
      return true;
    }
  }

}
