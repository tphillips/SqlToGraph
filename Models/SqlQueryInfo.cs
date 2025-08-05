/*
 * SqlToGraph - Professional SQL Data Visualization Tool
 * 
 * Copyright (c) 2025 SqlToGraph Contributors
 * Licensed under the MIT License (see LICENSE file for details)
 */

namespace SqlToGraph.Models
{
    /// <summary>
    /// Represents a SQL query and its associated title extracted from comments.
    /// </summary>
    public sealed record SqlQueryInfo
    {
        public required string Title { get; init; }
        public required string Query { get; init; }
    }
}
