using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlOnlineMigration.Internals
{
    public class SqlScripts
    {
        public static string Copy(TableName source, TableName destination, string idColumnName, string[] columnNames, int batchSize, TimeSpan batchDelay)
        {
            return
                $@"SET IDENTITY_INSERT {destination} ON
                   
                   DECLARE @StartId AS BIGINT
                   DECLARE @LastId  AS BIGINT
                   DECLARE @EndId   AS BIGINT
                    
                   SET @StartId = 0

                   SELECT @LastID = MAX({idColumnName})
                   FROM {source}

                   WHILE @StartID < @LastID
                   BEGIN
                    SET @EndId = @StartId + {batchSize}

                    MERGE INTO {destination} destination
                    USING (
                        SELECT {ToCsv(columnNames)} FROM {source}
                        WHERE {idColumnName} BETWEEN @StartID AND @EndId
                    ) AS source ON (destination.{idColumnName} = source.{idColumnName})
                    WHEN NOT MATCHED BY TARGET THEN
                        INSERT ({ToCsv(columnNames)})
                        VALUES ({ToCsv(columnNames.Select(x => $"source.{x}").ToArray())})
                    ;

                    SET @StartID = @EndId + 1

                    WAITFOR DELAY '{batchDelay:hh':'mm':'ss'.'fff}'

                    END

                   SET IDENTITY_INSERT {destination} OFF";
        }
        
        public static string InsertIntoTriggerBody(TableName destination, string[] columnNames)
        {
            return $@"
                SET IDENTITY_INSERT {destination} ON                

                INSERT INTO {destination} ({ToCsv(columnNames)}) SELECT {ToCsv(columnNames)} FROM Inserted

                SET IDENTITY_INSERT {destination} OFF";
        }

        public static string DeleteFromTriggerBody(TableName destination, string idColumnName)
        {
            return $@"DELETE FROM {destination} WHERE {idColumnName} IN (SELECT {idColumnName} FROM Deleted)";
        }

        public static string UpdateTriggerBody(TableName destination, string idColumnName, string[] columnNames)
        {
            return $@"
                UPDATE {destination} 
                SET {ToSetList(columnNames.ToList().Except(new List<string> { idColumnName }).ToArray())} FROM Inserted 
                WHERE {destination}.{idColumnName} = Inserted.{idColumnName} ";
        }

        public static string DropTrigger(MultiPartIdentifier identifier)
        {
            return $"DROP TRIGGER {identifier}";
        }

        private static string ToSetList(string[] values)
        {
            return string.Join(",", values.Select(x => $" [{x}] = Inserted.[{x}] "));
        }

        private static string ToCsv(string[] values)
        {
            return string.Join(", ", values);
        }
    }
}
