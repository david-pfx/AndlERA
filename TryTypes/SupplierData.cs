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
  public class Supplier {
    public static RelS S = new RelS(
      new List<TupS> {
        new TupS( "S1", "Smith", 20, "London" ),
        new TupS( "S2", "Jones", 10, "Paris" ),
        new TupS( "S3", "Blake", 30, "Paris" ),
        new TupS( "S4", "Clark", 20, "London" ),
        new TupS( "S5", "Adams", 30, "Athens" ),
      });

    public static RelP P = new RelP(
      new List<TupP> {
        new TupP( "P1", "Nut",   "Red",   12.0m,"London" ),
        new TupP( "P2", "Bolt",  "Green", 17.0m,"Paris"  ),
        new TupP( "P3", "Screw", "Blue",  17.0m,"Oslo"   ),
        new TupP( "P4", "Screw", "Red",   14.0m,"London" ),
        new TupP( "P5", "Cam",   "Blue",  12.0m,"Paris"  ),
        new TupP( "P6", "Cog",   "Red",   19.0m,"London" ),
      });

    public static RelSP SP = new RelSP(
      new List<TupSP> {
        new TupSP( "S1", "P1", 300 ),
        new TupSP( "S1", "P2", 200 ),
        new TupSP( "S1", "P3", 400 ),
        new TupSP( "S1", "P4", 200 ),
        new TupSP( "S1", "P5", 100 ),
        new TupSP( "S1", "P6", 100 ),
        new TupSP( "S2", "P1", 300 ),
        new TupSP( "S2", "P2", 400 ),
        new TupSP( "S3", "P2", 200 ),
        new TupSP( "S4", "P2", 200 ),
        new TupSP( "S4", "P4", 300 ),
        new TupSP( "S4", "P5", 400 ),
      });
    public static RelJ J = new RelJ(
      new List<TupJ> {
        new TupJ("J1","Sorter","Paris"),
        new TupJ("J2","Display","Rome"),
        new TupJ("J3","OCR","Athens"),
        new TupJ("J4","Console","Athens"),
        new TupJ("J5","RAID","London"),
        new TupJ("J6","EDS","Oslo"),
        new TupJ("J7","Tape","London"),
      });

    public static RelSPJ SPJ = new RelSPJ(
      new List<TupSPJ> {
        new TupSPJ( "S1", "P1", "J1", 200),
        new TupSPJ( "S1", "P1", "J4", 700),
        new TupSPJ( "S2", "P3", "J1", 400),
        new TupSPJ( "S2", "P3", "J2", 200),
        new TupSPJ( "S2", "P3", "J3", 200),
        new TupSPJ( "S2", "P3", "J4", 500),
        new TupSPJ( "S2", "P3", "J5", 600),
        new TupSPJ( "S2", "P3", "J6", 400),
        new TupSPJ( "S2", "P3", "J7", 800),
        new TupSPJ( "S2", "P5", "J2", 100),
        new TupSPJ( "S3", "P3", "J1", 200),
        new TupSPJ( "S3", "P4", "J2", 500),
        new TupSPJ( "S4", "P6", "J3", 300),
        new TupSPJ( "S4", "P6", "J7", 300),
        new TupSPJ( "S5", "P2", "J2", 200),
        new TupSPJ( "S5", "P2", "J4", 100),
        new TupSPJ( "S5", "P5", "J5", 500),
        new TupSPJ( "S5", "P5", "J7", 100),
        new TupSPJ( "S5", "P6", "J2", 200),
        new TupSPJ( "S5", "P1", "J4", 100),
        new TupSPJ( "S5", "P3", "J4", 200),
        new TupSPJ( "S5", "P4", "J4", 800),
        new TupSPJ( "S5", "P5", "J4", 400),
        new TupSPJ( "S5", "P6", "J4", 500),
      });
  }

  ///===========================================================================
  /// <summary>
  /// Generated relation type for S (suppliers)
  /// </summary>
  public class RelS : RelationBase<TupS> {
    static RelS() {
      Heading = new string[] { "SNo", "Sname", "Status", "City" };
      Key = new string[] { "SNo" };
    }

    public RelS(IList<TupS> tuples) : base(tuples) {
    }
  }
  /// <summary>
  /// Generated tuple type for S (suppliers)
  /// </summary>
  public class TupS : TupleBase {
    public string Sno { get { return (string)_values[0]; } }
    public string Sname { get { return (string)_values[1]; } }
    public int Status { get { return (int)_values[2]; } }
    public string City { get { return (string)_values[3]; } }

    static TupS() {
      Heading = new string[] { "SNo", "Sname", "Status", "City" };
    }

    public TupS(string Sno, string Sname, int Status, string City) : base(
      new object[] { Sno, Sname, Status, City }) {
    }
  }

  ///===========================================================================
  /// <summary>
  /// Generated relation type for P (products)
  /// </summary>
  public class RelP : RelationBase<TupP> {
    static RelP() {
      Heading = new string[] { "Pno", "Pname", "Color", "Weight", "City" };
      Key = new string[] { "PNo" };
    }

    public RelP(IList<TupP> tuples) : base(tuples) {
    }

  }
  /// <summary>
  /// Generated tuple type for P (products)
  /// </summary>
  public class TupP : TupleBase {

    public string Pno { get { return (string)_values[0]; } }
    public string Pname { get { return (string)_values[1]; } }
    public string Color { get { return (string)_values[2]; } }
    public decimal Weight { get { return (decimal)_values[3]; } }
    public string City { get { return (string)_values[4]; } }

    static TupP() {
      Heading = new string[] { "Pno", "Pname", "Color", "Weight", "City" };
    }
    public TupP(string pno, string pname, string color, decimal weight, string city) :
      base(new object[] { pno, pname, color, weight, city }) {
    }
  }

  ///===========================================================================
  /// <summary>
  /// Generated relation type for SP (supplies)
  /// </summary>
  public class RelSP : RelationBase<TupSP> {
    static RelSP() {
      Heading = new string[] { "Sno", "Pno", "Qty" };
      Key = new string[] { "Sno", "PNo" };
    }

    public RelSP(IList<TupSP> tuples) : base(tuples) {
    }
  }

  /// <summary>
  /// Generated tuple type for SP (supplies)
  /// </summary>
  public class TupSP : TupleBase {
    public string Sno { get { return (string)_values[0]; } }
    public string Pno { get { return (string)_values[1]; } }
    public int Qty { get { return (int)_values[2]; } }

    static TupSP() {
      Heading = new string[] { "Sno", "Pno", "Qty" };
    }

    public TupSP(string Sno, string Pno, int Qty) : base(
      new object[] { Sno, Pno, Qty }) {
    }
  }

  ///===========================================================================
  /// <summary>
  /// Generated relation type for J (jobs)
  /// </summary>
  public class RelJ : RelationBase<TupJ> {
    static RelJ() {
      Heading = new string[] { "Jno", "Jname", "City " };
      Key = new string[] { "JNo" };
    }

    public RelJ(IList<TupJ> tuples) : base(tuples) {
    }

  }
  /// <summary>
  /// Generated tuple type for J (jobs)
  /// </summary>
  public class TupJ : TupleBase {
    public string Jno { get { return (string)_values[0]; } }
    public string Jname { get { return (string)_values[1]; } }
    public string City { get { return (string)_values[2]; } }

    static TupJ() {
      Heading = new string[] { "Jno", "Jname", "City " };
    }

    public TupJ(string jno, string jname, string city) : base(
      new object[] { jno, jname, city }) {
    }
  }

  ///===========================================================================
  /// <summary>
  /// Generated relation type for SPJ (job supplies)
  /// </summary>
  public class RelSPJ : RelationBase<TupSPJ> {
    static RelSPJ() {
      Heading = new string[] { "Sno", "Pno", "Jno", "Qty" };
      Key = new string[] { "Sno", "PNo", "Jno" };
    }

    public RelSPJ(IList<TupSPJ> tuples) : base(tuples) {
    }

  }

  /// <summary>
  /// Generated tuple type for SPJ (job supplies)
  /// </summary>
  public class TupSPJ : TupleBase {
    public string Sno { get { return (string)_values[0]; } }
    public string Pno { get { return (string)_values[1]; } }
    public string Jno { get { return (string)_values[2]; } }
    public int Qty { get { return (int)_values[3]; } }

    static TupSPJ() {
      Heading = new string[] { "Sno", "Pno", "Jno", "Qty" };
    }

    public TupSPJ(string Sno, string Pno, string Jno, int Qty) : base(
      new object[] { Sno, Pno, Jno, Qty }) {
    }
  }
}

