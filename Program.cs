/*
 * SqlToGraph - Professional SQL Data Visualization Tool
 * By Tristan Phillips
 * 
 * Licensed under the MIT License (see LICENSE file for details)
 * 
 * A high-performance C# console application that executes SQL queries against 
 * MySQL databases and generates professional PDF reports with high-resolution 
 * line charts and trend analysis.
 * 
 * Features:
 * - High-resolution chart rendering (1200x800, 2.0x scale factor)
 * - Professional PDF generation with QuestPDF
 * - Time series data processing with optional gap filling
 * - Automatic trend line analysis
 * - Service-oriented architecture for maintainability
 * 
 * Dependencies:
 * - .NET 8.0
 * - MySql.Data 9.0.0
 * - QuestPDF 2024.3.0
 * - ScottPlot 5.0.42
 * - SkiaSharp 2.88.8
 * 
 * For usage instructions and examples, see README.md
 */

using MySql.Data.MySqlClient;
using QuestPDF.Infrastructure;
using SqlToGraph.Services;

namespace SqlToGraph
{
    /// <summary>
    /// Main application class for converting SQL query results to PDF charts.
    /// Supports string-based X axis values (categories or dates) with numeric Y values.
    /// Automatically detects and handles date-based time series data with proper sorting.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            // Register QuestPDF license type (Community is free for non-commercial use)
            QuestPDF.Settings.License = LicenseType.Community;

            if (!ValidateCommandLineArguments(args))
            {
                return;
            }

            string connectionString = args[0];
            string sqlFilePath = args[1];
            bool fillMissingDays = args.Length == 3 && args[2].Equals("--fill-missing-days", StringComparison.OrdinalIgnoreCase);

            if (fillMissingDays)
            {
                Console.WriteLine("Fill missing days option enabled - will add missing dates with 0 values for time series data.");
            }

            if (!File.Exists(sqlFilePath))
            {
                Console.WriteLine($"Error: SQL file not found at '{sqlFilePath}'");
                return;
            }

            try
            {
                ProcessSqlFile(connectionString, sqlFilePath, fillMissingDays);
            }
            catch (MySqlException ex)
            {
                LogMySqlError(ex);
            }
            catch (Exception ex)
            {
                LogGeneralError(ex);
            }
        }

        /// <summary>
        /// Validates command line arguments and displays usage information if invalid.
        /// </summary>
        private static bool ValidateCommandLineArguments(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("Usage: dotnet run <mysql_connection_string> <sql_file_path> [--fill-missing-days]");
                Console.WriteLine("Example: dotnet run \"Server=localhost;Port=3306;Database=testdb;Uid=user;Pwd=password;\" \"./queries.sql\"");
                Console.WriteLine("Example with fill missing days: dotnet run \"Server=localhost;Port=3306;Database=testdb;Uid=user;Pwd=password;\" \"./queries.sql\" --fill-missing-days");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Main processing workflow for SQL file to PDF conversion.
        /// </summary>
        private static void ProcessSqlFile(string connectionString, string sqlFilePath, bool fillMissingDays)
        {
            var queries = SqlQueryParser.ParseQueriesFromFile(sqlFilePath);
            if (!queries.Any())
            {
                Console.WriteLine("No valid SQL queries found in the file.");
                return;
            }

            var results = DatabaseService.ExecuteQueries(connectionString, queries, fillMissingDays);

            if (results.Any())
            {
                PdfService.GeneratePdfReport(results);
                Console.WriteLine("PDF report generated successfully.");
            }
            else
            {
                Console.WriteLine("No data available to generate charts.");
            }
        }

        /// <summary>
        /// Logs MySQL-specific errors with detailed information.
        /// </summary>
        private static void LogMySqlError(MySqlException ex)
        {
            Console.WriteLine($"MySQL Error: {ex.Message}");
            Console.WriteLine($"Error Code: {ex.ErrorCode}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }

        /// <summary>
        /// Logs general application errors.
        /// </summary>
        private static void LogGeneralError(Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }
}