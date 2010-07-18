namespace JavaGeneration.Chirper
{
  using System.Collections.Generic;
  using System.Linq;
  using Aquiles;
  using Aquiles.Command;
  using Aquiles.Model;

  public class KeySpace
  {
    public KeySpace(string cluster, string name)
    {
      Cluster = cluster;
      Name = name;
    }

    public string Cluster { get; private set; }
    
    public string Name { get; private set; }

    public bool Exists(string family, string key, string columnName)
    {
      return Exists(family, key, null, columnName);
    }

    public bool Exists(string family, string key, string superColumnName, string columnName)
    {
      return GetColumn(family, key, superColumnName, columnName) != null;
    }

    public string GetValue(string family, string key, string columnName)
    {
      return GetValue(family, key, null, columnName);
    }

    public string GetValue(string family, string key, string superColumnName, string columnName)
    {
      var column = GetColumn(family, key, superColumnName, columnName);
      return column == null ? null : column.Value;
    }

    public List<string> GetValues(string family, string key, int count)
    {
      return GetValues(family, key, null, count);
    }
    
    public List<string> GetValues(string family, string key, string superColumnName, int count)
    {
      return GetSlice(family, key, superColumnName, count).ConvertAll(c => c.Value);
    }

    public AquilesColumn GetColumn(string family, string key, string columnName)
    {
      return GetColumn(family, key, null, columnName);
    }

    public AquilesColumn GetColumn(string family, string key, string superColumnName, string columnName)
    {
      var getCommand = new GetCommand
        {
          KeySpace = Name,
          Key = key,
          ColumnFamily = family,
          ColumnName = columnName,
          SuperColumnName = superColumnName
        };

      using (var connection = AquilesHelper.RetrieveConnection(Cluster))
      {
        connection.Execute(getCommand);
        return getCommand.Output == null ? null : getCommand.Output.Column;
      }
    }

    public List<AquilesColumn> GetSlice(string family, string key, int count)
    {
      return GetSlice(family, key, null, count);
    }

    public List<AquilesColumn> GetSlice(string family, string key, string superColumnName, int count)
    {
      var getSliceCommand = new GetSliceCommand
      {
        KeySpace = Name,
        Key = key,
        ColumnFamily = family,
        SuperColumn = superColumnName,
        Predicate = new AquilesSlicePredicate { SliceRange = new AquilesSliceRange { Count = count } }
      };

      using (var connection = AquilesHelper.RetrieveConnection(Cluster))
      {
        connection.Execute(getSliceCommand);
        return getSliceCommand.Output == null 
          ? new List<AquilesColumn>()
          : getSliceCommand.Output.Results.Select(o => o.Column).ToList();
      }
    }

    public void Insert(string family, string key, string columnName, string value)
    {
      Insert(family, key, null, columnName, value);
    }

    public void Insert(string family, string key, string superColumnName, string columnName, string value)
    {
      InsertCommand insertCommand = new InsertCommand
        {
          ColumnFamily = family,
          Key = key,
          KeySpace = Name,
          Column = new AquilesColumn { ColumnName = columnName, Value = value },
          SuperColumn = superColumnName
        };
      
      using (var connection = AquilesHelper.RetrieveConnection(Cluster))
      {
        connection.Execute(insertCommand);
      }
    }
  }
}