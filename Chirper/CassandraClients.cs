namespace JavaGeneration.Chirper
{
  using HectorSharp;

  public static class CassandraClients
  {
    internal static KeyedCassandraClientFactory Factory { get; set; }

    internal static Endpoint Endpoint { get; set; }

    public static ICassandraClient Make()
    {
      return Factory.Make(Endpoint);
    }
  }
}