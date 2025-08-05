/*
 * SqlToGraph - Professional SQL Data Visualization Tool
 * 
 * Copyright (c) 2025 SqlToGraph Contributors
 * Licensed under the MIT License (see LICENSE file for details)
 */

using SqlToGraph.Models;

namespace SqlToGraph.Services
{
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
}
