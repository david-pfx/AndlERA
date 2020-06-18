using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Andl.Common;

namespace TryTypes {
  class Program {
    static void Main(string[] args) {
      Console.WriteLine("Andl Era");
      Exec();
    }

    private static void Exec() {
      var t = new Three();
      t.Main();
    }
  }

  public class One<T> where T : Two {
    public static int Y;  
    static One() {
      var t = typeof(T);
      var p1 = t.GetProperty("ZZ", BindingFlags.Public | BindingFlags.Static);
      var v1 = p1.GetValue(null);
      var p3 = t.GetField("Z", BindingFlags.Public | BindingFlags.Static);
      var v3 = p3.GetValue(null);
      var prop = typeof(T).GetProperty("Z", BindingFlags.Public | BindingFlags.Static);
      // Y is to be initialised with the Z value from T
      // this code does not compile
      // Y = T.Z;
    }
    public int X {  get { return Y; } }
  }

  public class Two {
    public static int Z = 42;
    public static int ZZ { get; } = 43;
    public int xxx { get; }
  }

  public class Three {
    public void Main() {
      One<Two> a = new One<Two>();
      Console.WriteLine("X = {0}", a.X);
    }
  }
}
