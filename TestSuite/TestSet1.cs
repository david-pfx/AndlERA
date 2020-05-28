using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AndlEra;
using Andl.Common;
using System.Linq;

namespace TestSuite {
  [TestClass]
  public class TestSet1 {
    const string Testdata = @".\Data";

    [TestMethod]
    public void Import() {
      var si = RelationNode.Import(SourceKind.Csv, Testdata, "S");
      Assert.AreEqual("S#,SNAME,STATUS,CITY", si.Heading.ToNames().Join(","));

      var s8i = RelationNode.Import(SourceKind.Csv, Testdata, "S8", "SNo:text,SName:text,Status:integer,City:text");
      Assert.AreEqual("SNo,SName,Status,City", s8i.Heading.ToNames().Join(","));

      var pi = RelationNode.Import(SourceKind.Csv, Testdata, "P", "PNo:text,PName:text,Color:text,Weight:number,City:text");
      Assert.AreEqual(2, pi.Count(t => t.ToString().Contains("Blue")));

      var spi = RelationNode.Import(SourceKind.Csv, Testdata, "SP", "SNo:text,PNo:text,Qty:integer");
      Assert.AreEqual(12, spi.Cardinality);

      var mmqi = RelationNode.Import(SourceKind.Csv, Testdata, "MMQ", "MajorPNo:text,MinorPno:text,Qty:number");
      Assert.AreEqual(3, mmqi.Degree);
    }

    [TestMethod]
    public void Monadic() {
      var si = RelationNode.Import(SourceKind.Csv, Testdata, "S");

      var siren = si.Rename("CITY,XXXCITY");
      Assert.AreEqual("S#,SNAME,STATUS,XXXCITY", siren.Heading.ToNames().Join(","));
      Assert.AreEqual(4, siren.Degree);
      Assert.AreEqual(5, siren.Cardinality);

      var siext = si.Extend("STATUS,PlusA", new TupExtend(t => (string)t[0] + "A"));
      Assert.AreEqual("S#,SNAME,STATUS,CITY,PlusA", siext.Heading.ToNames().Join(","));
      Assert.AreEqual(5, siext.Degree);
      Assert.AreEqual(5, siext.Cardinality);
      Assert.AreEqual(2, siext.Count(t => t.ToString().Contains("30A")));
      Assert.AreEqual(1, siext.Count(t => t.ToString().Contains("S3,Blake,30,Paris,30A")));
      Assert.AreEqual("S3,Blake,30,Paris,30A", siext.Where(t => t.ToString().Contains("S3")).Join(";"));

      var sirep = si.Extend("STATUS,STATUS", new TupExtend(t => (string)t[0] + "B"));
      Assert.AreEqual("S#,SNAME,STATUS,CITY", sirep.Heading.ToNames().Join(","));
      Assert.AreEqual(4, sirep.Degree);
      Assert.AreEqual(5, sirep.Cardinality);
      Assert.AreEqual(2, sirep.Count(t => t.ToString().Contains("30B")));
      Assert.AreEqual("S3,Blake,30B,Paris", sirep.Where(t => t.ToString().Contains("S3")).Join(";"));
      Assert.AreEqual(1, sirep.Count(t => t.ToString().Contains("S3,Blake,30B,Paris")));
    }
  }
}
