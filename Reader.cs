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
    public static IEnumerable<T> Build<T>(FieldMapping.FieldMapper<T> mapper, string filename) where T : class, new()
    {
      var nr = new Reader<T>();
      nr.mapper = mapper;
      nr.init(filename);
      return nr;
    }
  }
  
  public class Reader<T> : IEnumerable<T> where T : class, new()
  {
    private System.Xml.XmlReader _rdr;

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
      Dictionary<string,string> values = null;

      while(!_rdr.EOF && _rdr.ReadState == System.Xml.ReadState.Interactive)
        {
          if (_rdr.NodeType == System.Xml.XmlNodeType.Element)
            {
              if (_rdr.Name == "tt") {  }
              else
               {
                  if (_rdr.Name == "ttRow")
                    {
                      if (obj != null || values != null) 
                        {
                          obj = _dovalues(obj, values);
                          if (obj != null) { yield return obj; }

                          obj = null;
                          values = null;
                        }
                      if (mapper != null && mapper.overwrite)
                        { obj = mapper.create(); }
                      else
                        { values = new Dictionary<string, string>(); }
                    }
                  else
                    {
                      if (values != null || obj != null)
                        {
                          var name = _rdr.Name;
                          var contents = _rdr.ReadElementContentAsString();
                          
                          if (mapper != null && this.mapper.overwrite)
                            { mapper.set(obj, name, contents); }
                          else
                            {
                              if (values.ContainsKey(name)) 
                                {
                                  if (mapper != null)
                                    {
                                      if (obj == null) { obj = mapper.create(); }
                                      mapper.set(obj, name, values[name]);
                                      mapper.set(obj, name, contents);
                                      /* nulls are ignored always.
                                       * if this converts to another thing (eg int/bool)
                                       * we'll get the default value or zero most likely
                                       * which should be ok.
                                       */
                                      values[name] = null; 
                                    }
                                  else { values[name] = values[name] + contents; }
                                }
                              else { values.Add(name, contents); }
                            }
                        }
                    }
                }
            }

          _rdr.Read();
        }

      if (obj != null || values != null) 
        { 
          obj = _dovalues(obj, values);
          if (obj != null) { yield return obj; }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

    private T _dovalues(T obj, Dictionary<string,string> dict)
    {
      if (dict != null) 
        {
          if (obj == null) { obj = mapper.create(); }

          mapper.convert(obj, dict);
        }
      
      return obj;
    }
  }
}
