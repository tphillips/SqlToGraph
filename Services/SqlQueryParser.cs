/*
 * SqlToGraph - Professional SQL Data Visualization Tool
 * 
 * Copyright (c) 2025 SqlToGraph Contributors
 * Licensed under the MIT License (see LICENSE file for details)
 */

using System.Text;
using SqlToGraph.Models;

namespace SqlToGraph.Services
{
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
}
