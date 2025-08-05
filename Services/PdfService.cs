/*
 * SqlToGraph - Professional SQL Data Visualization Tool
 * 
 * Copyright (c) 2025 SqlToGraph Contributors
 * Licensed under the MIT License (see LICENSE file for details)
 */

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SqlToGraph.Models;

namespace SqlToGraph.Services
{
    /// <summary>
    /// Service for PDF generation and report creation.
    /// </summary>
    public static class PdfService
    {
        /// <summary>
        /// Generates a PDF report with a text-based chart representation for each query result.
        /// Each chart is displayed on a separate A4 page with string X values (dates/categories) and numeric Y values.
        /// Automatically detects time series data and provides appropriate sorting and statistics.
        /// </summary>
        public static void GeneratePdfReport(List<QueryResult> results)
        {
            string filename = $"Report-{DateTime.Now:yyyy-MM-dd}.pdf";

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
}
