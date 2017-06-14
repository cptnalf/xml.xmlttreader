using System.Collections.Generic;

namespace xmlttreader
{
  /// <summary>
  /// static class to make type-inference easier.
  /// </summary>
  public static class ReaderT
  {
    /// <summary>
    /// Build a new Xml file reader based on the provided FieldMapper.
    /// this initializes and returns the ready-to-use reader.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mapper"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static IEnumerable<T> Build<T>(FieldMapping.FieldMapper<T> mapper, string filename) where T : class, new()
    {
      var nr = new Reader<T>();
      nr.mapper = mapper;
      nr.init(filename);
      return nr;
    }
  }
}
