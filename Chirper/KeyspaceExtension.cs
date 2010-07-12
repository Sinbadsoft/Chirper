namespace JavaGeneration.Chirper
{
  using HectorSharp;

  public static class KeyspaceExtension
  {
    public static bool TryGetColumn(this IKeyspace keyspace, string key, ColumnPath path, out Column column)
    {
      try
      {
        column = keyspace.GetColumn(key, path);
      }
      catch
      {
        column = null;
      }

      return column != null;
    }

    public static bool Exists(this IKeyspace keyspace, string key, ColumnPath path)
    {
      Column column;
      return keyspace.TryGetColumn(key, path, out column);
    }

    public static string GetValueOrEmpty(this IKeyspace keyspace, string key, ColumnPath path)
    {
      Column column;
      return keyspace.TryGetColumn(key, path, out column) ? column.Value : string.Empty;
    }

    public static bool InsertIfNotNullOrEmpty(this IKeyspace keyspace, string key, ColumnPath path, string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return false;
      }

      keyspace.Insert(key, path, value);
      return true;
    }

    public static bool InsertIfNotNullOrEmpty(this IKeyspace keyspace, string key, ColumnPath path, byte[] value)
    {
      if (value != null && value.Length != 0)
      {
        return false;
      }

      keyspace.Insert(key, path, value);
      return true;
    }

  }
}