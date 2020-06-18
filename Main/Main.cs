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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Andl.Common;
using SupplierData;
using static System.Console;

namespace AndlEra {
  class Program {
    static string DataPath = @"..\TestSuite\Data";

    static void Main(string[] args) {
      WriteLine($"Andl Era in {Directory.GetCurrentDirectory()}");
      SampleWithHeading();
      SampleSPP();
      SampleWithTypes();
    }

    static void Show(string msg, params RelNode[] node) {
      Show(msg, node.Select(n => n.Format()));
    }

    static void Show(string msg, params RelVar[] rvar) {
      Show(msg, rvar.Select(n => n.Format()));
    }

    static void Show(string msg, params object[] value) {
      Show(msg, value.Select(n => n.ToString()));
    }
    static void Show(string msg, IEnumerable<string> values) {
      WriteLine(msg);
      foreach (var value in values) {
        WriteLine(value);
        WriteLine("----------");
      }
    }

    // basic samples, operators return relations
    static void SampleWithHeading() {

      var si = RelNode.Import(SourceKind.Csv, DataPath, "S", "SNo:text,SName:text,Status:integer,City:text");
      var s8i = RelNode.Import(SourceKind.Csv, DataPath, "S8", "SNo:text,SName:text,Status:integer,City:text");
      var pi = RelNode.Import(SourceKind.Csv, DataPath, "P", "PNo:text,PName:text,Color:text,Weight:number,City:text");
      var spi = RelNode.Import(SourceKind.Csv, DataPath, "SP", "SNo:text,PNo:text,Qty:integer");

      Show("Restrict", si
        .Restrict("City", Where.Text(v => v == "Paris")));

      Show("Project", si
        .Project("City"));

      Show("Rename", si
        .Rename("SNo,S#","SName,NAME","City,SCITY"));

      Show("Extend",
        pi.Extend("Weight,WeightKg", Eval.Number(a => a * 0.454m)));

      Show("Replace",
        pi.Extend("Weight,Weight", Eval.Number(a => a * 0.454m)));

      Show("Joins", 
        si.Join(spi),
        si.Compose(spi),
        si.Semijoin(spi),
        si.Antijoin(spi));

      Show("Union", 
        si.Union(s8i),
        si.Minus(s8i),
        si.Intersect(s8i)
        );

      Show("Group",
        spi.Group("PNo,Qty,PQ"),
        spi.Group("PNo,Qty,PQ").Ungroup("PQ"));

      Show("Wrap", 
        spi.Wrap("PNo,Qty,PQ"),
        spi.Wrap("PNo,Qty,PQ").Unwrap("PQ"));

      Show("Agg", pi
        .Project("Color,Weight")
        .Aggregate("Weight,TotWeight", Eval.Number((v, a) => v + a))
        );

      var orgi = RelNode.Import(SourceKind.Csv, DataPath, "orgchart");
      Show("TClose", orgi
        .Restrict("boss", Where.Text(b => b != ""))
        .TranClose());

      var mmi = RelNode.Import(SourceKind.Csv, DataPath, "MMQ")
        .Remove("QTY");
      Show("TClose", mmi
        .TranClose());

      var mmqi = RelNode.Import(SourceKind.Csv, DataPath, "MMQ", "MajorPNo:text,MinorPNo:text,Qty:number");

      var mmexp = mmqi
        .Rename("Qty,ExpQty")
        .While(Eval.While(tw => tw
          .Rename("MinorPNo,zmatch")
          .Compose(mmqi.Rename("MajorPNo,zmatch"))
          .Extend("Qty,ExpQty,ExpQty", Eval.Number((v,w) => v * w))
          .Remove("Qty")));
      Show("While", mmexp);
      Show("Aggregated", mmexp
        .Aggregate("ExpQty,TotQty", Eval.Number((v, a) => v + a))
        );

      var v1 = new RelVar(si);
      Show("RelVar", v1);
      v1.Insert(RelNode.Data("SNo:text,SName:text,Status:integer,City:text",
        Tup.Data("S6", "White", 25, "Paris"),
        Tup.Data("S7", "Black", 15, "London")));

      Show("Insert P6 P7", v1);

      v1.Update("Status,City", v => (int)v[0] >= 25, "Sydney");
      Show("Move to Sydney", v1);

      v1.Delete("City", v => (string)v[0] == "Sydney");
      Show("Delete Sydneysiders", v1);
    }

    static void SampleSPP() {
      // Some sample queries from https://web.njit.edu/~hassadi/Dbase_Courses/CIS631/Ex_03.html

      var S = RelNode.Import(SourceKind.Csv, DataPath, "S");
      var P = RelNode.Import(SourceKind.Csv, DataPath, "P");
      var SP = RelNode.Import(SourceKind.Csv, DataPath, "SP");
      var J = RelNode.Import(SourceKind.Csv, DataPath, "J");
      var SPJ = RelNode.Import(SourceKind.Csv, DataPath, "SPJ", "S#,P#,J#,QTY:number");

      Show("Q1. Get suppliers names who supply part 'P2'",
        S.Join(SP)
          .Restrict("P#", Where.Text(v => v == "P2"))
          .Project("SNAME"),
        S.Join(SP.Restrict("P#", Where.Text(v => v == "P2")))
        .Project("SNAME"));

      Show("Q2. Get suppliers names who supply at least one red part.",
        S.Project("S#,SNAME")
        .Join(SP.Project("S#,P#"))
        .Join(P.Project("P#,COLOR"))
        .Restrict("COLOR", Where.Text(t => t == "Red"))
        .Project("SNAME"));

      Show("Q3. Get the supplier names for suppliers who do not supply part 'P2'",
        S .Antijoin(SP.Restrict("P#", Where.Text(v => v == "P2")))
          .Project("SNAME"));

      Show("Q4. Get the supplier names for suppliers who supply all parts.",
        S .Antijoin(
          S .Project("S#")
            .Join(P.Project("P#"))
            .Minus(SP.Project("S#,P#")))
          .Join(S)
          .Project("SNAME"));

      Show("Q5. Get supplier numbers who supply at lease one of the parts supplied by supplier 'S2'.",
        S .Restrict("S#", Where.Text(v => v == "S2"))
          .Join(SP)
          .Project("P#")
          .Join(SP)
          .Project("S#"));

      var Sq6 = S.Project("S#,CITY");
      Show("Q6. Get all pairs of supplier numbers such that two suppliers are 'colocated' (located in the same city).",
        Sq6.Rename("S#,Sa")
          .Join(Sq6.Rename("S#,Sb"))
          .Restrict("Sa,Sb", Where.Text((a, b) => a.CompareTo(b) < 0))
          .Remove("CITY"));

      Show("Q7. Join the three tables and find the result of natural join with selected attributes.",
        SP .Join(S)
           .Join(P.Project("P#,PNAME,CITY")));

      Show("Q8. Get all shipments where the quantity is in the range 300 to 750 inclusive.",
        SPJ.Restrict("QTY", Where.Number(v => v >= 300 && v <= 750)));

      Show("Q9. Get all supplier-number/part-number/project-number triples such that the indicated supplier, part, and project are all colocated (i.e., all in the same city).",
        S .Join(J.Join(P))
          .Project("S#,P#,J#"));

      Show("Q10. Get all pairs of city names such that a supplier in the first city supplies a project in the second city.",
        S.Rename("CITY,SCITY")
          .Join(SPJ)
          .Join(J.Rename("CITY,JCITY"))
          .Project("SCITY,JCITY"));

      Show("Q11. Get all cities in which at least one supplier, part, or project is located.",
        S .Project("CITY")
          .Union(P.Project("CITY"))
          .Union(J.Project("CITY")));

      Show("Q12. Get supplier-number/part-number pairs such that the indicated supplier does not supply the indicated part.",
        S .Project("S#")
          .Join(P.Project("P#"))
          .Minus(SPJ.Project("S#,P#")));

      Show("Q13. Get all pairs of part numbers and supplier numbers such that some supplier supplies both indicated parts.",
        SPJ.Project("S#,P#")
        .Rename("P#,PA")
        .Join(SPJ.Project("S#,P#")
          .Rename("P#,PB"))
        .Restrict("PA,PB", Where.Text((a, b) => a.CompareTo(b) < 0)));
    }

    // basic samples, operators return relations
    static void SampleWithTypes() {
      Show("Seq", RelSequenceST.Create(5));
      Show("SP", Supplier.SP);

      var v1 = new RelVarST<TupS>(Supplier.S);
      var v2 = new RelVarST<Tup1>(v1.Value
        .Restrict(t => t.Status == 30)
        .Rename<TupSX>()    // "SNo", "SName", "Status", "Supplier City"
        .Project<Tup1>());    // "Supplier City"
      Show("v2", v2.Value);

      Show("Extend", Supplier.S
        .Extend<TupSX>(t => "XXX")    // "SNo", "SName", "Status", "Supplier City"
        );

      Show("Join", Supplier.P
        .Rename<TupPcolour>()                 //  "PNo", "PName", "Colour", "Weight", "City"
        .Restrict(t => t.Colour == "Red")
        .Project<TupPPno>()                   // "PNo", "PName"
        .Join<TupSP, TupPjoin>(Supplier.SP)   // "PNo", "PName", "SNo", "Qty"
        );

      Show("Aggregation", Supplier.SP
        .Aggregate<TupAgg,int>((t,a) => a + t.Qty)  //  "PNo", "TotQty"
        );

      var seed = MMQData.MMQ.Extend<TupMMQA>(t => t.Qty);    // "Major", "Minor", "Qty", "AggQty"
      var zmq = MMQData.MMQ.Rename<TupzMQ>();    // "zmatch", "Minor", "Qty"
      var exp = seed.While<RelMMQA>(t => t
        .Rename<TupMzA>()             // "Major", "zmatch", "ExpQty"
        .Join<TupzMQ, TupMMQA>(zmq)
        .Transform<TupMMQA>(tt => TupMMQA.Create(tt.Major, tt.Minor, 0, tt.AggQty * tt.Qty)));
      Show("While", exp);
      Show("P1 -> P5", exp
        .Restrict(t => t.Major == "P1" && t.Minor == "P5")
        .Aggregate<TupMMT, int>((t, a) => a + t.AggQty));

      v1.Insert(
        RelS.Create<RelS>(
          new List<TupS> {
            TupS.Create( "S6", "White", 25, "Paris" ),
            TupS.Create( "S7", "Black", 15, "London" ),
          })
        );
      Show("Insert P6 P7", v1);

      v1.Update(t => t.City == "Paris", t => TupS.Create(t.SNo, t.SName, t.Status, "Sydney"));
      Show("Move to Sydney", v1);

      v1.Delete(t => t.City == "Sydney");
      Show("Delete Sydneysiders", v1);
    }
  }


  public class TupSX : TupBase {
    public readonly static string Heading = "SNo,SName,Status,Supplier City";
  }
  public class Tup1 : TupBase {
    public readonly static string Heading = "Supplier City";
  }
  public class TupSSP : TupBase {
    public readonly static string Heading = "SNo,SName,Status,City,PNo,Qty";
  }
  public class TupPcolour : TupBase {
    public readonly static string Heading = "PNo,PName,Colour,Weight,City";
    public string Colour { get { return (string)Values[2]; } }
  }

  public class TupPPno : TupBase {
    public readonly static string Heading = "PNo,PName";
  }

  public class TupPjoin : TupBase {
    public readonly static string Heading = "PNo,PName,SNo,Qty";
  }
  public class TupAgg : TupBase {
    public readonly static string Heading = "PNo,TotQty";
  }

  public class RelMMQA : RelValueST<TupMMQA> { }
  public class TupMMQA : TupMMQ {
    new public readonly static string Heading = "Major,Minor,Qty,AggQty";
    public int AggQty { get { return (int)Values[3]; } }
    public static TupMMQA Create(string major, string minor, int qty, int aggqty) {
      return Create<TupMMQA>(new object[] { major, minor, qty, aggqty });
    }
  }
  public class TupMzA : TupBase {
    public readonly static string Heading = "Major,zmatch,AggQty";
  }
  public class TupzMQ : TupBase {
    public readonly static string Heading = "zmatch,Minor,Qty";
  }
  public class TupMMT : TupBase {
    public readonly static string Heading = "Major,Minor,TotQty";
  }


}
