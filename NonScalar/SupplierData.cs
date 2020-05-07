/// Andl is A New Data Language. See andl.org.
///
/// Copyright © David M. Bennett 2015-16 as an unpublished work. All rights reserved.
///
/// This software is provided in the hope that it will be useful, but with 
/// absolutely no warranties. You assume all responsibility for its use.
/// 
/// This software is completely free to use for purposes of personal study. 
/// For distribution, modification, commercial use or other purposes you must 
/// comply with the terms of the licence originally supplied with it in 
/// the file Licence.txt or at http://andl.org/Licence/.
///
using System.Collections.Generic;
using System.Linq;
using AndlEra;

// Supplier data set in C# format
// This is the standard data set used in books by Date and Darwen
namespace SupplierData {
  ///===========================================================================
  /// <summary>
  /// Sample supplier data
  /// </summary>
  public static class Supplier {
    public static RelS S = RelS.Create(
      new List<TupS> {
        TupS.Create( "S1", "Smith", 20, "London" ),
        TupS.Create( "S2", "Jones", 10, "Paris" ),
        TupS.Create( "S3", "Blake", 30, "Paris" ),
        TupS.Create( "S4", "Clark", 20, "London" ),
        TupS.Create( "S5", "Adams", 30, "Athens" ),
      });

    public static RelP P = RelP.Create(
      new List<TupP> {
        TupP.Create( "P1", "Nut",   "Red",   12.0m,"London" ),
        TupP.Create( "P2", "Bolt",  "Green", 17.0m,"Paris"  ),
        TupP.Create( "P3", "Screw", "Blue",  17.0m,"Oslo"   ),
        TupP.Create( "P4", "Screw", "Red",   14.0m,"London" ),
        TupP.Create( "P5", "Cam",   "Blue",  12.0m,"Paris"  ),
        TupP.Create( "P6", "Cog",   "Red",   19.0m,"London" ),
      });

    public static RelSP SP = RelSP.Create(
      new List<TupSP> {
        TupSP.Create( "S1", "P1", 300 ),
        TupSP.Create( "S1", "P2", 200 ),
        TupSP.Create( "S1", "P3", 400 ),
        TupSP.Create( "S1", "P4", 200 ),
        TupSP.Create( "S1", "P5", 100 ),
        TupSP.Create( "S1", "P6", 100 ),
        TupSP.Create( "S2", "P1", 300 ),
        TupSP.Create( "S2", "P2", 400 ),
        TupSP.Create( "S3", "P2", 200 ),
        TupSP.Create( "S4", "P2", 200 ),
        TupSP.Create( "S4", "P4", 300 ),
        TupSP.Create( "S4", "P5", 400 ),
      });
    public static RelJ J = RelJ.Create(
      new List<TupJ> {
        TupJ.Create("J1","Sorter","Paris"),
        TupJ.Create("J2","Display","Rome"),
        TupJ.Create("J3","OCR","Athens"),
        TupJ.Create("J4","Console","Athens"),
        TupJ.Create("J5","RAID","London"),
        TupJ.Create("J6","EDS","Oslo"),
        TupJ.Create("J7","Tape","London"),
      });

    public static RelSPJ SPJ = RelSPJ.Create(
      new List<TupSPJ> {
        TupSPJ.Create( "S1", "P1", "J1", 200),
        TupSPJ.Create( "S1", "P1", "J4", 700),
        TupSPJ.Create( "S2", "P3", "J1", 400),
        TupSPJ.Create( "S2", "P3", "J2", 200),
        TupSPJ.Create( "S2", "P3", "J3", 200),
        TupSPJ.Create( "S2", "P3", "J4", 500),
        TupSPJ.Create( "S2", "P3", "J5", 600),
        TupSPJ.Create( "S2", "P3", "J6", 400),
        TupSPJ.Create( "S2", "P3", "J7", 800),
        TupSPJ.Create( "S2", "P5", "J2", 100),
        TupSPJ.Create( "S3", "P3", "J1", 200),
        TupSPJ.Create( "S3", "P4", "J2", 500),
        TupSPJ.Create( "S4", "P6", "J3", 300),
        TupSPJ.Create( "S4", "P6", "J7", 300),
        TupSPJ.Create( "S5", "P2", "J2", 200),
        TupSPJ.Create( "S5", "P2", "J4", 100),
        TupSPJ.Create( "S5", "P5", "J5", 500),
        TupSPJ.Create( "S5", "P5", "J7", 100),
        TupSPJ.Create( "S5", "P6", "J2", 200),
        TupSPJ.Create( "S5", "P1", "J4", 100),
        TupSPJ.Create( "S5", "P3", "J4", 200),
        TupSPJ.Create( "S5", "P4", "J4", 800),
        TupSPJ.Create( "S5", "P5", "J4", 400),
        TupSPJ.Create( "S5", "P6", "J4", 500),
      });
  }

  ///===========================================================================
  /// <summary>
  /// Generated relation type for S (suppliers)
  /// </summary>
  public class RelS : RelationBase<TupS> {
    public static RelS Create(IList<TupS> tuples) {
      return Create<RelS>(tuples);
    }
  }
  /// <summary>
  /// Generated tuple type for S (suppliers)
  /// </summary>
  public class TupS : TupleBase {
    public readonly static string[] Heading = { "SNo", "Sname", "Status", "City" };

    public string Sno { get { return (string)_values[0]; } }
    public string Sname { get { return (string)_values[1]; } }
    public int Status { get { return (int)_values[2]; } }
    public string City { get { return (string)_values[3]; } }

    public static TupS Create(string Sno, string Sname, int Status, string City) {
      return Create<TupS>(new object[] { Sno, Sname, Status, City });
    }
  }

  ///===========================================================================
  /// <summary>
  /// Generated relation type for P (products)
  /// </summary>
  public class RelP : RelationBase<TupP> {
    public static RelP Create(IList<TupP> tuples) {
      return Create<RelP>(tuples);
    }
  }

  /// <summary>
  /// Generated tuple type for P (products)
  /// </summary>
  public class TupP : TupleBase {
    public readonly static string[] Heading = { "Pno", "Pname", "Color", "Weight", "City" };

    public string Pno { get { return (string)_values[0]; } }
    public string Pname { get { return (string)_values[1]; } }
    public string Color { get { return (string)_values[2]; } }
    public decimal Weight { get { return (decimal)_values[3]; } }
    public string City { get { return (string)_values[4]; } }

    public static TupP Create(string pno, string pname, string color, decimal weight, string city) {
      return Create<TupP>(new object[] { pno, pname, color, weight, city });
    }
  }

  ///===========================================================================
  /// <summary>
  /// Generated relation type for SP (supplies)
  /// </summary>
  public class RelSP : RelationBase<TupSP> {
    public static RelSP Create(IList<TupSP> tuples) {
      return Create<RelSP>(tuples);
    }
  }

  /// <summary>
  /// Generated tuple type for SP (supplies)
  /// </summary>
  public class TupSP : TupleBase {
    public readonly static string[] Heading = { "Sno", "Pno", "Qty" };

    public string Sno { get { return (string)_values[0]; } }
    public string Pno { get { return (string)_values[1]; } }
    public int Qty { get { return (int)_values[2]; } }

    public static TupSP Create(string Sno, string Pno, int Qty) {
      return Create<TupSP>(new object[] { Sno, Pno, Qty });
    }
  }

  ///===========================================================================
  /// <summary>
  /// Generated relation type for J (jobs)
  /// </summary>
  public class RelJ : RelationBase<TupJ> {
    public static RelJ Create(IList<TupJ> tuples) {
      return Create<RelJ>(tuples);
    }
  }
  /// <summary>
  /// Generated tuple type for J (jobs)
  /// </summary>
  public class TupJ : TupleBase {
    public readonly static string[] Heading = { "Jno", "Jname", "City " };

    public string Jno { get { return (string)_values[0]; } }
    public string Jname { get { return (string)_values[1]; } }
    public string City { get { return (string)_values[2]; } }

    public static TupJ Create(string jno, string jname, string city) {
      return Create<TupJ>(new object[] { jno, jname, city });
    }
  }

  ///===========================================================================
  /// <summary>
  /// Generated relation type for SPJ (job supplies)
  /// </summary>
  public class RelSPJ : RelationBase<TupSPJ> {
    public static RelSPJ Create(IList<TupSPJ> tuples) {
      return Create<RelSPJ>(tuples);
    }
  }

  /// <summary>
  /// Generated tuple type for SPJ (job supplies)
  /// </summary>
  public class TupSPJ : TupleBase {
    public readonly static string[] Heading = { "Sno", "Pno", "Jno", "Qty" };
    
    public string Sno { get { return (string)_values[0]; } }
    public string Pno { get { return (string)_values[1]; } }
    public string Jno { get { return (string)_values[2]; } }
    public int Qty { get { return (int)_values[3]; } }

    public static TupSPJ Create(string Sno, string Pno, string Jno, int Qty) {
      return Create<TupSPJ>(new object[] { Sno, Pno, Jno, Qty });
    }
  }
}

