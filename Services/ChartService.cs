/*
 * SqlToGraph - Professional SQL Data Visualization Tool
 * 
 * Copyright (c) 2025 SqlToGraph Contributors
 * Licensed under the MIT License (see LICENSE file for details)
 */

using ScottPlot;

namespace SqlToGraph.Services
{
    /// <summary>
    /// Service for creating charts using ScottPlot.
    /// </summary>
    public static class ChartService
    {
        /// <summary>
        /// Creates a graphical line chart using ScottPlot and returns it as a byte array.
        /// </summary>
        public static byte[] CreateChart(List<Models.DataPoint> sortedData)
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
            plt.Axes.Title.Label.FontName = "Nimbus Sans";
            plt.Axes.Title.Label.FontSize = 16;
            plt.Axes.Left.Label.FontName = "Nimbus Sans";
            plt.Axes.Left.Label.FontSize = 14;
            plt.Axes.Bottom.Label.FontName = "Nimbus Sans";
            plt.Axes.Bottom.Label.FontSize = 14;
            plt.Axes.Left.TickLabelStyle.FontName = "Nimbus Sans";
            plt.Axes.Left.TickLabelStyle.FontSize = 12;
            plt.Axes.Bottom.TickLabelStyle.FontName = "Nimbus Sans";
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
        public static IEnumerable<Models.DataPoint> TrySortByDate(List<Models.DataPoint> data)
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
}
