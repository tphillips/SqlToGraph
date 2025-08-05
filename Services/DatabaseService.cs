/*
 * SqlToGraph - Professional SQL Data Visualization Tool
 * 
 * Copyright (c) 2025 SqlToGraph Contributors
 * Licensed under the MIT License (see LICENSE file for details)
 */

using MySql.Data.MySqlClient;
using SqlToGraph.Models;

namespace SqlToGraph.Services
{
    /// <summary>
    /// Service for database operations and data retrieval.
    /// </summary>
    public static class DatabaseService
    {
        /// <summary>
        /// Executes all SQL queries and returns the results.
        /// </summary>
        public static List<QueryResult> ExecuteQueries(string connectionString, List<SqlQueryInfo> queries, bool fillMissingDays)
        {
            var results = new List<QueryResult>();

            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            Console.WriteLine("Successfully connected to MySQL database.");

            foreach (var queryInfo in queries)
            {
                Console.WriteLine($"Executing query: {queryInfo.Title}");
                var data = ExecuteSqlQuery(connection, queryInfo.Query);
                
                if (data.Any())
                {
                    // Apply missing day filling if enabled and data is time series
                    if (fillMissingDays)
                    {
                        data = DataProcessor.FillMissingDays(data);
                    }
                    
                    results.Add(new QueryResult { Title = queryInfo.Title, Data = data });
                    Console.WriteLine($"  Found {data.Count} data points.");
                }
                else
                {
                    Console.WriteLine("  No data returned for this query.");
                }
            }

            return results;
        }

        /// <summary>
        /// Executes a single SQL query against the MySQL connection and returns X, Y data.
        /// </summary>
        private static List<DataPoint> ExecuteSqlQuery(MySqlConnection connection, string sqlQuery)
        {
            var data = new List<DataPoint>();
            
            using var command = new MySqlCommand(sqlQuery, connection);
            using var reader = command.ExecuteReader();
            
            if (!ValidateQueryColumns(reader, sqlQuery))
            {
                return data; // Return empty list if columns are invalid
            }

            while (reader.Read())
            {
                var dataPoint = TryParseDataPoint(reader, sqlQuery);
                if (dataPoint != null)
                {
                    data.Add(dataPoint);
                }
            }

            return data;
        }

        /// <summary>
        /// Validates that the query result has both X and Y columns.
        /// </summary>
        private static bool ValidateQueryColumns(MySqlDataReader reader, string sqlQuery)
        {
            bool hasX = false;
            bool hasY = false;
            
            Console.WriteLine($"Debug: Query '{sqlQuery}' returned {reader.FieldCount} columns:");
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                string columnType = reader.GetFieldType(i).Name;
                Console.WriteLine($"  Column {i}: {columnName} (Type: {columnType})");
                
                if (columnName.Equals("X", StringComparison.OrdinalIgnoreCase)) hasX = true;
                if (columnName.Equals("Y", StringComparison.OrdinalIgnoreCase)) hasY = true;
            }

            if (!hasX || !hasY)
            {
                Console.WriteLine($"Warning: Query '{sqlQuery}' did not return both 'X' and 'Y' columns. Skipping chart generation for this query.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse a data point from the current reader row.
        /// Handles mixed data types and column swapping issues.
        /// </summary>
        private static DataPoint? TryParseDataPoint(MySqlDataReader reader, string sqlQuery)
        {
            try
            {
                // Get raw values first
                var xValue = reader["X"];
                var yValue = reader["Y"];
                
                Console.WriteLine($"Debug: Raw values - X: {xValue} (Type: {xValue?.GetType()?.Name ?? "NULL"}), Y: {yValue} (Type: {yValue?.GetType()?.Name ?? "NULL"})");

                // Handle X column - should be string/date for our chart
                string x;
                if (xValue is DateTime xDateTime)
                {
                    x = xDateTime.ToString("yyyy-MM-dd");
                }
                else if (xValue is DBNull || xValue == null)
                {
                    x = "";
                }
                else
                {
                    x = xValue.ToString() ?? "";
                }

                // Handle Y column - should be numeric for our chart
                double y;
                if (yValue is DBNull || yValue == null)
                {
                    Console.WriteLine($"Warning: Y column contains NULL value for query '{sqlQuery}'. Skipping row.");
                    return null;
                }
                else if (yValue is DateTime yDateTime)
                {
                    // This might indicate columns are swapped or misinterpreted
                    Console.WriteLine($"Warning: Y column contains DateTime value ({yDateTime}) instead of numeric. This might indicate a data type issue in your query.");
                    Console.WriteLine($"Suggestion: Check if your SELECT statement has the columns in the correct order (Y should be numeric, X should be string/date).");
                    return null;
                }
                else
                {
                    // Try to convert to double
                    try
                    {
                        y = Convert.ToDouble(yValue);
                    }
                    catch
                    {
                        Console.WriteLine($"Warning: Cannot convert Y value '{yValue}' to numeric for query '{sqlQuery}'. Skipping row.");
                        return null;
                    }
                }
                
                Console.WriteLine($"Debug: Parsed successfully - X: '{x}', Y: {y}");
                return new DataPoint { X = x, Y = y };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: An error occurred while reading data for query '{sqlQuery}'. Skipping row. Error: {ex.Message}");
                Console.WriteLine($"Debug: Exception type: {ex.GetType().Name}");
            }

            return null;
        }
    }
}
