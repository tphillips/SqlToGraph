/*
 * SqlToGraph - Professional SQL Data Visualization Tool
 * 
 * Copyright (c) 2025 SqlToGraph Contributors
 * Licensed under the MIT License (see LICENSE file for details)
 */

namespace SqlToGraph.Models
{
    /// <summary>
    /// Represents a single data point with string X coordinate and numeric Y coordinate for charting.
    /// </summary>
    public sealed record DataPoint
    {
        public required string X { get; init; }
        public required double Y { get; init; }
        
        public override string ToString() => $"({X}, {Y:F2})";
    }
}
