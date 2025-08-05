/*
 * SqlToGraph - Professional SQL Data Visualization Tool
 * 
 * Copyright (c) 2025 SqlToGraph Contributors
 * Licensed under the MIT License (see LICENSE file for details)
 */

namespace SqlToGraph.Models
{
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
}
