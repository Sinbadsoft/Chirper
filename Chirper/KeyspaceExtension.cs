using HectorSharp;

namespace JavaGeneration.Chirper
{
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
    }
}