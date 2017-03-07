using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace xmlttreader
{
  /* open file.
   * look for ttRow
   *  all the other elements in here are fields.
   *  find a good mapping?
   * 
   */
  public static class ReaderT
  {
    public static Reader<T> Build<T>(FieldMapping.FieldMapper<T> mapper) where T : class, new()
    {
      var nr = new Reader<T>();
      nr.mapper = mapper;
      return nr;
    }
  }


  public class Reader<T> : IEnumerable<T> where T : class, new()
  {
    private System.Xml.XmlReader _rdr;
    private T _obj;

    public FieldMapping.FieldMapper<T> mapper {get;set; }

    public void init(string file)
    {
      _rdr = System.Xml.XmlReader.Create(new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite));

      _rdr.MoveToContent();
      _rdr.Read();
    }

    public IEnumerator<T> GetEnumerator()
    {
      T obj = null;

      while(!_rdr.EOF && _rdr.ReadState == System.Xml.ReadState.Interactive)
        {
          if (_rdr.NodeType == System.Xml.XmlNodeType.Element)
            {
              if (_rdr.Name == "tt") {  }
              else
               {
                  if (_rdr.Name == "ttRow")
                    {
                      if (obj != null) { yield return obj; }
                      obj = mapper.create();
                    }
                  else
                    {
                      if (obj != null)
                        {
                          var name = _rdr.Name;
                          var contents = _rdr.ReadElementContentAsString();
              
                          mapper.set(obj, name, contents);
                        }
                    }
                }
            }

          _rdr.Read();
        }

      if (obj != null) { yield return obj; }
    }

    IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
  }
}
