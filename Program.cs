using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScottPlot;
using SkiaSharp;

namespace SqlToGraph
{
    #region Data Models
    
    /// <summary>
    /// Represents a single data point with string X coordinate and numeric Y coordinate for charting.
    /// </summary>
    public sealed record DataPoint
    {
        public required string X { get; init; }
        public required double Y { get; init; }
        
        public override string ToString() => $"({X}, {Y:F2})";
    }

    /// <summary>
    /// Represents a SQL query and its associated title extracted from comments.
    /// </summary>
    public sealed record SqlQueryInfo
    {
        public required string Title { get; init; }
        public required string Query { get; init; }
    }

    /// <summary>
    /// Represents the results of executing a SQL query, including the title and data points.
    /// </summary>
    public sealed record QueryResult
    {
        public required string Title { get; init; }
        public required List<DataPoint> Data { get; init; }
        
        public bool HasData => Data.Count > 0;
        public bool IsTimeSeries => Data.All(dp => DateTime.TryParse(dp.X, out _));
    }
    
    #endregion

    #region Business Logic Services
    
    /// <summary>
    /// Service for parsing SQL queries and extracting metadata from comments.
    /// </summary>
    public static class SqlQueryParser
    {
        /// <summary>
        /// Parses the SQL file to extract comments as titles and the subsequent SQL queries.
        /// Assumes comments start with '--' and precede a single query.
        /// Queries can span multiple lines until a semicolon is encountered.
        /// </summary>
        public static List<SqlQueryInfo> ParseQueriesFromFile(string filePath)
        {
            var queries = new List<SqlQueryInfo>();
            var lines = File.ReadAllLines(filePath);
            string currentTitle = "Untitled Chart";
            var currentQueryBuilder = new StringBuilder();

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                if (IsCommentLine(trimmedLine))
                {
                    currentTitle = ExtractTitleFromComment(trimmedLine);
                    currentQueryBuilder.Clear();
                }
                else if (IsContentLine(trimmedLine))
                {
                    currentQueryBuilder.Append(trimmedLine).Append(" ");

                    if (IsQueryComplete(trimmedLine))
                    {
                        string query = CleanQuery(currentQueryBuilder.ToString());
                        queries.Add(new SqlQueryInfo { Title = currentTitle, Query = query });
                        ResetForNextQuery(ref currentTitle, currentQueryBuilder);
                    }
                }
            }

            // Handle case where file ends without a semicolon for the last query
            if (currentQueryBuilder.Length > 0)
            {
                string query = CleanQuery(currentQueryBuilder.ToString());
                queries.Add(new SqlQueryInfo { Title = currentTitle, Query = query });
            }

            return queries;
        }

        /// <summary>
        /// Checks if a line is a comment line starting with '--'.
        /// </summary>
        private static bool IsCommentLine(string line) => line.StartsWith("--");

        /// <summary>
        /// Checks if a line contains meaningful content (not empty or whitespace).
        /// </summary>
        private static bool IsContentLine(string line) => !string.IsNullOrWhiteSpace(line);

        /// <summary>
        /// Checks if a query is complete (ends with semicolon).
        /// </summary>
        private static bool IsQueryComplete(string line) => line.EndsWith(";");

        /// <summary>
        /// Extracts the title from a comment line by removing the '--' prefix.
        /// </summary>
        private static string ExtractTitleFromComment(string commentLine)
        {
            return commentLine.Substring(2).Trim();
        }

        /// <summary>
        /// Cleans a query string by trimming whitespace and removing trailing semicolon.
        /// </summary>
        private static string CleanQuery(string query)
        {
            query = query.Trim();
            return query.EndsWith(";") ? query.Substring(0, query.Length - 1) : query;
        }

        /// <summary>
        /// Resets variables for parsing the next query.
        /// </summary>
        private static void ResetForNextQuery(ref string currentTitle, StringBuilder queryBuilder)
        {
            currentTitle = "Untitled Chart";
            queryBuilder.Clear();
        }
    }
    
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
    
    /// <summary>
    /// Service for creating charts using ScottPlot.
    /// </summary>
    public static class ChartService
    {
        /// <summary>
        /// Creates a graphical line chart using ScottPlot and returns it as a byte array.
        /// </summary>
        public static byte[] CreateChart(List<DataPoint> sortedData)
        {
            if (!sortedData.Any()) 
            {
                // Create empty chart with message
                var emptyPlot = new Plot();
                emptyPlot.Add.Text("No data available", 0.5, 0.5);
                emptyPlot.Axes.SetLimits(0, 1, 0, 1);
                emptyPlot.Grid.MajorLineColor = ScottPlot.Colors.Transparent;
                return emptyPlot.GetImageBytes(1200, 800);
            }

            // Create a ScottPlot chart
            var plt = new Plot();
            
            // Prepare data for plotting
            var xValues = new double[sortedData.Count];
            var yValues = new double[sortedData.Count];
            var xLabels = new string[sortedData.Count];
            
            // Check if X values are dates for proper spacing
            bool isTimeSeries = sortedData.All(dp => DateTime.TryParse(dp.X, out _));
            
            for (int i = 0; i < sortedData.Count; i++)
            {
                if (isTimeSeries && DateTime.TryParse(sortedData[i].X, out DateTime date))
                {
                    xValues[i] = date.ToOADate(); // Convert to OLE Automation date for proper spacing
                }
                else
                {
                    xValues[i] = i; // Use index for categorical data
                }
                yValues[i] = sortedData[i].Y;
                xLabels[i] = sortedData[i].X;
            }
            
            // Add line plot with enhanced quality
            var scatter = plt.Add.Scatter(xValues, yValues);
            scatter.LineWidth = 3;  // Increased from 2 for better visibility at high resolution
            scatter.MarkerSize = 8; // Increased from 6 for better visibility
            scatter.Color = ScottPlot.Colors.Blue;
            
            // Add trend line if we have enough data points
            if (sortedData.Count >= 2)
            {
                var trendLine = plt.Add.ScatterLine(xValues, yValues);
                trendLine.LinePattern = ScottPlot.LinePattern.Dashed;
                trendLine.LineWidth = 2; // Increased from 1
                trendLine.Color = ScottPlot.Colors.Red.WithAlpha(0.7);
                trendLine.MarkerStyle = ScottPlot.MarkerStyle.None;
                
                // Calculate linear regression for trend line
                double sumX = xValues.Sum();
                double sumY = yValues.Sum();
                double sumXY = xValues.Zip(yValues, (x, y) => x * y).Sum();
                double sumXX = xValues.Sum(x => x * x);
                int n = xValues.Length;
                
                double slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
                double intercept = (sumY - slope * sumX) / n;
                
                // Create trend line points
                double minX = xValues.Min();
                double maxX = xValues.Max();
                double[] trendX = { minX, maxX };
                double[] trendY = { slope * minX + intercept, slope * maxX + intercept };
                
                var trend = plt.Add.ScatterLine(trendX, trendY);
                trend.LineWidth = 3; // Increased from 2 for better high-res visibility
                trend.Color = ScottPlot.Colors.Red.WithAlpha(0.8);
                trend.MarkerStyle = ScottPlot.MarkerStyle.None;
                trend.LinePattern = ScottPlot.LinePattern.Dashed;
            }
            
            // Configure chart appearance
            plt.YLabel("Count");
            plt.XLabel("X Values");
            
            // Handle X-axis labels
            if (isTimeSeries)
            {
                plt.Axes.DateTimeTicksBottom();
            }
            else
            {
                // For categorical data, set custom tick positions and labels
                var tickPositions = xValues.Take(Math.Min(10, xValues.Length)).ToArray(); // Limit to 10 labels for readability
                var tickLabels = xLabels.Take(Math.Min(10, xLabels.Length)).ToArray();
                plt.Axes.Bottom.SetTicks(tickPositions, tickLabels);
                plt.Axes.Bottom.TickLabelStyle.Rotation = -45; // Rotate labels for better readability
            }
            
            // Style the plot with higher quality settings
            plt.Grid.MajorLineColor = ScottPlot.Colors.Gray.WithAlpha(.3);
            plt.Grid.MinorLineColor = ScottPlot.Colors.Gray.WithAlpha(.1);
            plt.FigureBackground.Color = ScottPlot.Colors.White;
            plt.DataBackground.Color = ScottPlot.Colors.White;
            
            // Enhanced styling for higher resolution
            plt.Axes.Title.Label.FontSize = 16;
            plt.Axes.Left.Label.FontSize = 14;
            plt.Axes.Bottom.Label.FontSize = 14;
            plt.Axes.Left.TickLabelStyle.FontSize = 12;
            plt.Axes.Bottom.TickLabelStyle.FontSize = 12;
            
            // Anti-aliasing and quality settings
            plt.ScaleFactor = 2.0f; // Double resolution for crisp rendering
            
            // Set high resolution for the chart
            int chartWidth = 1200;  // Doubled from 600
            int chartHeight = 800;  // Doubled from 400
            
            // Render chart to PNG bytes with high quality
            return plt.GetImageBytes(chartWidth, chartHeight);
        }
        
        /// <summary>
        /// Attempts to sort data by date if X values are valid dates, otherwise sorts alphabetically.
        /// </summary>
        public static IEnumerable<DataPoint> TrySortByDate(List<DataPoint> data)
        {
            // Check if all X values can be parsed as dates
            var dateParseResults = data.Select(dp => new { 
                DataPoint = dp, 
                IsDate = DateTime.TryParse(dp.X, out DateTime parsedDate),
                ParsedDate = DateTime.TryParse(dp.X, out parsedDate) ? parsedDate : DateTime.MinValue
            }).ToList();

            // If all X values are valid dates, sort by date (newest first for time series)
            if (dateParseResults.All(x => x.IsDate))
            {
                return dateParseResults
                    .OrderByDescending(x => x.ParsedDate)
                    .Select(x => x.DataPoint);
            }
            
            // Otherwise, sort alphabetically
            return data.OrderBy(dp => dp.X);
        }
    }
    
    /// <summary>
    /// Service for PDF generation and report creation.
    /// </summary>
    public static class PdfService
    {
        private const string DateTimeFormat = "yyyy-MM-dd";
        
        /// <summary>
        /// Generates a PDF report with a text-based chart representation for each query result.
        /// Each chart is displayed on a separate A4 page with string X values (dates/categories) and numeric Y values.
        /// Automatically detects time series data and provides appropriate sorting and statistics.
        /// </summary>
        public static void GeneratePdfReport(List<QueryResult> results)
        {
            string filename = $"SqlToGraph-{DateTime.Now:yyyy-MM-dd}.pdf";

            Document.Create(container =>
            {
                foreach (var result in results)
                {
                    container.Page(page =>
                    {
                        ConfigurePage(page);
                        CreatePageContent(page, result);
                    });
                }
            })
            .GeneratePdf(filename);

            Console.WriteLine($"PDF saved to: {Path.GetFullPath(filename)}");
        }

        /// <summary>
        /// Configures the basic page settings for the PDF.
        /// </summary>
        private static void ConfigurePage(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.PageColor(QuestPDF.Helpers.Colors.White);
            page.DefaultTextStyle(x => x.FontSize(12));
        }

        /// <summary>
        /// Creates the content for a single page containing chart data.
        /// </summary>
        private static void CreatePageContent(PageDescriptor page, QueryResult result)
        {
            page.Content().Column(column =>
            {
                column.Spacing(10);
                AddTitle(column, result.Title);

                if (!result.Data.Any())
                {
                    AddNoDataMessage(column);
                    return;
                }

                AddChartRepresentation(column, result.Data);
            });
        }

        /// <summary>
        /// Adds the chart title to the page.
        /// </summary>
        private static void AddTitle(ColumnDescriptor column, string title)
        {
            column.Item().AlignCenter().Text(title).FontSize(18).Bold();
        }

        /// <summary>
        /// Adds a message when no data is available for charting.
        /// </summary>
        private static void AddNoDataMessage(ColumnDescriptor column)
        {
            column.Item().AlignCenter().Text("No data to display for this chart.").Italic();
        }

        /// <summary>
        /// Creates a chart representation with data points.
        /// </summary>
        private static void AddChartRepresentation(ColumnDescriptor column, List<DataPoint> data)
        {
            column.Item().Border(1).Padding(10).Column(chartColumn =>
            {
                AddDataPointsList(chartColumn, data);
            });
        }

        /// <summary>
        /// Adds a list of data points to the chart representation, with smart sorting for dates.
        /// </summary>
        private static void AddDataPointsList(ColumnDescriptor chartColumn, List<DataPoint> data)
        {
            // Try to sort by date if X values are dates, otherwise sort alphabetically
            var sortedData = ChartService.TrySortByDate(data).ToList();
            
            // Create a visual chart representation
            AddVisualChart(chartColumn, sortedData);
        }

        /// <summary>
        /// Creates a graphical line chart using ScottPlot and embeds it in the PDF.
        /// </summary>
        private static void AddVisualChart(ColumnDescriptor chartColumn, List<DataPoint> sortedData)
        {
            if (!sortedData.Any()) return;

            try
            {
                // Generate chart using ChartService
                byte[] chartBytes = ChartService.CreateChart(sortedData);
                
                // Add the chart image to the PDF
                chartColumn.Item().Image(chartBytes).FitWidth();
            }
            catch (Exception ex)
            {
                // Fallback to text description if chart generation fails
                chartColumn.Item().Text($"Chart generation failed: {ex.Message}")
                    .FontSize(10)
                    .FontColor("#FF0000");
                
                // Show basic data summary as fallback
                chartColumn.Item().PaddingTop(5);
                chartColumn.Item().Text("Data Summary:");
                
                foreach (var point in sortedData.Take(10))
                {
                    chartColumn.Item().Text($"{point.X}: {point.Y:F2}")
                        .FontSize(9)
                        .FontFamily("Courier New");
                }
                
                if (sortedData.Count > 10)
                {
                    chartColumn.Item().Text($"... and {sortedData.Count - 10} more points")
                        .FontSize(9)
                        .Italic();
                }
            }
        }
    }
    
    /// <summary>
    /// Service for data processing and transformation.
    /// </summary>
    public static class DataProcessor
    {
        /// <summary>
        /// Fills in missing days with 0 values for time series data.
        /// Only applies to data where all X values are valid dates.
        /// </summary>
        public static List<DataPoint> FillMissingDays(List<DataPoint> data)
        {
            if (!data.Any()) return data;

            // Check if all X values are valid dates
            var dateParseResults = data.Select(dp => new { 
                DataPoint = dp, 
                IsDate = DateTime.TryParse(dp.X, out DateTime parsedDate),
                ParsedDate = DateTime.TryParse(dp.X, out parsedDate) ? parsedDate : DateTime.MinValue
            }).ToList();

            // Only fill missing days for time series data (all dates)
            if (!dateParseResults.All(x => x.IsDate))
            {
                Console.WriteLine("  Skipping missing day fill - data is not time series (contains non-date X values).");
                return data;
            }

            var validDateResults = dateParseResults.Where(x => x.IsDate).ToList();
            if (!validDateResults.Any()) return data;

            // Get date range
            var minDate = validDateResults.Min(x => x.ParsedDate).Date;
            var maxDate = validDateResults.Max(x => x.ParsedDate).Date;
            
            Console.WriteLine($"  Filling missing days from {minDate:yyyy-MM-dd} to {maxDate:yyyy-MM-dd}");

            // Create a dictionary of existing data points by date
            var existingData = validDateResults.ToDictionary(
                x => x.ParsedDate.Date,
                x => x.DataPoint.Y
            );

            // Generate all dates in the range
            var filledData = new List<DataPoint>();
            for (var date = minDate; date <= maxDate; date = date.AddDays(1))
            {
                if (existingData.ContainsKey(date))
                {
                    // Use existing data
                    filledData.Add(new DataPoint 
                    { 
                        X = date.ToString("yyyy-MM-dd"), 
                        Y = existingData[date] 
                    });
                }
                else
                {
                    // Fill missing day with 0
                    filledData.Add(new DataPoint 
                    { 
                        X = date.ToString("yyyy-MM-dd"), 
                        Y = 0 
                    });
                }
            }

            int originalCount = data.Count;
            int filledCount = filledData.Count;
            int addedCount = filledCount - originalCount;
            
            if (addedCount > 0)
            {
                Console.WriteLine($"  Added {addedCount} missing days with 0 values (total: {filledCount} points)");
            }
            else
            {
                Console.WriteLine($"  No missing days found - data is already complete");
            }

            return filledData;
        }
    }
    
    #endregion
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