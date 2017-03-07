using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TTReaderTest
{
  public class RTBPMod
  {
    public string pmod {get;set; }
    public int nextObjNum {get;set; }
    public string desc { get;set; }
    public string prodID {get;set; }
    public string module {get;set; }
    public int verCounter { get;set; }
  }

  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TTReader()
    {
      var rdr = xmlttreader.ReaderT.Build(FieldMapping.FieldMapper<RTBPMod>.Create()
        .field(x => x.pmod, "pmod")
        .field(x => x.nextObjNum, "nxt-obj-num")
        .field(x => x.desc, "description")
        .field(x => x.prodID, "product-id")
        .field(x => x.module, "Module")
        .field(x => x.verCounter, "ver-counter")
        );

      rdr.init("c:\\tmp\\rtb.rtb_pmod.xml");

      var lst = rdr.ToList();
      Assert.IsNotNull(lst);
      Assert.IsTrue(lst.Count > 0);
    }
  }
}
