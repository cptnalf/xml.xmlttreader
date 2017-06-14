using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace xmlttreader
{
  /// <summary>
  /// Class to read xml files encoded as:
  /// &lt;tt&gt;
  ///   &lt;ttRow&gt;...&lt;ttRow&gt;
  ///   ...
  /// &lt;/tt&gt;
  /// 
  /// where 'tt' is the default literal.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <remarks>
  /// Progress 10.2b currently generates these files 
  /// when a Write-Xml call is made for a (temp-)table handle.
  /// </remarks>
  public class Reader<T> : IEnumerable<T> where T : class, new()
  {
    private System.Xml.XmlReader _rdr;
    private string _base = "tt";
    private string _rowPostfix = "Row";

    /// <summary>the name of the root element (defaults to 'tt')</summary>
    public string baseName {get { return _base; } set { _base = value; } }
    /// <summary>the postfix of the row elements (defaults to 'Row', resulting in default of 'ttRow')</summary>
    public string rowPostfix {get { return _rowPostfix; } set { _rowPostfix = value; } }

    /// <summary>
    /// map the keys in the dictionary to actual fields on the object.
    /// </summary>
    public FieldMapping.FieldMapper<T> mapper {get;set; }

    /// <summary>
    /// start the reader by pointing it to a file.
    /// </summary>
    /// <param name="file"></param>
    public void init(string file)
    {
      _rdr = System.Xml.XmlReader.Create(new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite));

      _rdr.MoveToContent();
      _rdr.Read();
    }

    /// <summary>
    /// get an enumerator of objects generated from the contents of the XML file.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator()
    {
      T obj = null;
      Dictionary<string,string> values = null;
      string rootname = _base;
      string rowName = _base + _rowPostfix;

      /* open file.
       * look for ttRow
       *  all the other elements in here are fields.
       *  find a good mapping?
       * 
       */
      
      while(!_rdr.EOF && _rdr.ReadState == System.Xml.ReadState.Interactive)
        {
          if (_rdr.NodeType == System.Xml.XmlNodeType.Element)
            {
              if (_rdr.Name == rootname) {  }
              else
                {
                  if (_rdr.Name == rowName)
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
                    } /* end start of a new record. */
                  else
                    {
                      /* probably a field in the row. */
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
                    } /* end field in row. */
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
