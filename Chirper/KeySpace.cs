using System;
using System.Text;
using Apache.Cassandra;
using Aquiles.Cassandra10;
using Aquiles.Core.Cluster;
using Aquiles.Helpers;
using Aquiles.Helpers.Encoders;
using CassandraClient = Apache.Cassandra.Cassandra.Client;
using System.Collections.Generic;

namespace JavaGeneration.Chirper
{
    public class KeySpace
    {
        private const string keyspaceName = "Chirper";

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
            // Encoding.ASCII.GetString(column.Value)
            var column = GetColumn(family, key, superColumnName, columnName);
            return column == null ? null : Encoding.ASCII.GetString(column.Column.Value);
        }

        public List<string> GetValues(string family, string key, int count)
        {
            return GetValues(family, key, null, count);
        }

        public List<string> GetValues(string family, string key, string superColumnName, int count)
        {
            return GetSlice(family, key, superColumnName, count).ConvertAll(c => Encoding.ASCII.GetString(c.Column.Value));
        }

        public ColumnOrSuperColumn GetColumn(string family, string key, string columnName)
        {
            return GetColumn(family, key, null, columnName);
        }

        public ColumnOrSuperColumn GetColumn(string family, string key, string superColumnName, string columnName)
        {
            //var getCommand = new GetCommand
            //                     {
            //                         KeySpace = Name,
            //                         Key = key,
            //                         ColumnFamily = family,
            //                         ColumnName = columnName,
            //                         SuperColumnName = superColumnName
            //                     };
            //using (var connection = AquilesHelper.RetrieveConnection(Cluster))
            //{
            //    connection.Execute(getCommand);
            //    return getCommand.Output == null ? null : getCommand.Output.Column;
            //}

            
            byte[] keyAsByteArray = Encoding.ASCII.GetBytes(key);
            byte[] columnNameAsByteArray = Encoding.ASCII.GetBytes(columnName);
            var columnPath = new ColumnPath
                                        {
                Column = columnNameAsByteArray,
                Column_family = family,
            };

            ICluster cluster = AquilesHelper.RetrieveCluster(Cluster);

            return (ColumnOrSuperColumn) cluster.Execute(new ExecutionBlock(client =>
                                                                                {
                                                                                    try
                                                                                    {
                                                                                        return client.get(keyAsByteArray, columnPath,
                                                                                                          ConsistencyLevel.ONE);
                                                                                    }
                                                                                    catch (Exception e)
                                                                                    {

                                                                                        return null;
                                                                                    }
                                                                                }), keyspaceName);
        }

        public List<ColumnOrSuperColumn> GetSlice(string family, string key, int count)
        {
            return GetSlice(family, key, null, count);
        }

        public List<ColumnOrSuperColumn> GetSlice(string family, string key, string superColumnName, int count)
        {
            //var getSliceCommand = new GetSliceCommand
            //                          {
            //                              KeySpace = Name,
            //                              Key = key,
            //                              ColumnFamily = family,
            //                              SuperColumn = superColumnName,
            //                              Predicate =
            //                                  new SlicePredicate
            //                                      {
            //                                          Slice_range = new SliceRange {Count = count}
            //                                      }
            //                          
            //using (var connection = AquilesHelper.RetrieveConnection(Cluster))
            //{
            //    connection.Execute(getSliceCommand);
            //    return getSliceCommand.Output == null
            //               ? new List<AquilesColumn>()
            //               : getSliceCommand.Output.Results.Select(o => o.Column).ToList();
            //}

            byte[] keyAsByteArray = Encoding.ASCII.GetBytes(key);
            var columnParent = new ColumnParent
                                   {
                                       Column_family = family,
                                   };
            var predicate = new SlicePredicate
                                {
                                    Slice_range = new SliceRange
                                                      {
                                                          Count = 1000,
                                                          Reversed = false,
                                                          Start = new byte[0],
                                                          Finish = new byte[0],
                                                      },
                                };

            ICluster cluster = AquilesHelper.RetrieveCluster(Cluster);
            object rtnValue = cluster.Execute(new ExecutionBlock(
                                                  client =>
                                                  client.get_slice(keyAsByteArray, columnParent, predicate,
                                                                   ConsistencyLevel.ONE)), keyspaceName);
            return rtnValue as List<ColumnOrSuperColumn>;
        }

        public void Insert(string family, string key, string columnName, string value)
        {
            Insert(family, key, null, columnName, value);
        }

        public void Insert(string family, string key, string superColumnName, string columnName, string value)
        {
            ICluster cluster = AquilesHelper.RetrieveCluster(Cluster);
            byte[] keyAsByteArray = Encoding.ASCII.GetBytes(key);
            byte[] valueAsByteArray = Encoding.ASCII.GetBytes(value);
            var columnParent = new ColumnParent
                                   {
                                       Column_family = family,
                                   };
            var column = new Column
                             {
                                 Name = ByteEncoderHelper.UTF8Encoder.ToByteArray(columnName),
                                 Timestamp = UnixHelper.UnixTimestamp,
                                 Value = valueAsByteArray
                             };

            
            cluster.Execute(new ExecutionBlock(client =>
                                                   {
                                                       // http://www.dotnetperls.com/convert-string-byte-array

                                                       client.insert(keyAsByteArray, columnParent, column,
                                                                     ConsistencyLevel.ONE);
                                                       return null;
                                                   }), keyspaceName);
        }
    }
}