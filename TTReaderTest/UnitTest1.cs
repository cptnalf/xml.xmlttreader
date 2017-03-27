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

  public class Task1
  {
    public int id {get;set;}
    public List<string> description {get;set;}
    public string summary {get;set;}
    public DateTime completed {get;set;}

    public Task1() { this.description = new List<string>(); }
  }

  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TTReader()
    {
      var mapper = FieldMapping.FieldMapper<RTBPMod>.Create(false)
        .field(x => x.pmod, "pmod")
        .field(x => x.nextObjNum, "nxt-obj-num")
        .field(x => x.desc, "description")
        .field(x => x.prodID, "product-id")
        .field(x => x.module, "Module")
        .field(x => x.verCounter, "ver-counter")
        ;

      /* construct test to test all values.
       * int, string, long, ulong, uint, bool, list,
       * objects, datetime, nullable<*>
       */
      var rdr = xmlttreader.ReaderT.Build(mapper, "c:\\tmp\\rtb.rtb_pmod.xml");

      var lst = rdr.ToList();
      Assert.IsNotNull(lst);
      Assert.IsTrue(lst.Count > 0);
    }

    [TestMethod]
    public void TTReader_ArrayTest()
    {
      var mapper = FieldMapping.FieldMapper<Task1>.Create(false)
        .field(x => x.id, "task-num")
        .field(x => x.summary, "summary")
        .list(x => x.description,  true, (r,k) => string.IsNullOrWhiteSpace(k) ? null : k, "description")
        .field(x => x.completed, "entered-when")
        ;

      var rdr = xmlttreader.ReaderT.Build(mapper, "c:\\tmp\\rtb.rtb_task.xml");

      var lst = rdr.ToList();

      bool haveCompletedNotMin = false;
      CollectionAssert.AllItemsAreNotNull(lst);
      foreach(var x in lst)
        {
          Assert.AreNotEqual(0, x.id);
          Assert.AreNotEqual(DateTime.MinValue, x.completed);
          CollectionAssert.AllItemsAreNotNull(x.description);
          Assert.IsNotNull(x.summary);
        }
    }
  }
}
