# SqlToGraph

A C# console application that executes SQL queries against a MySQL database and generates professional PDF reports with line charts and trend analysis.

## Features

### 📊 Professional Charts
- **Line Charts**: High-quality charts using ScottPlot library
- **Trend Lines**: Automatic linear regression trend analysis
- **Time Series Support**: Smart date detection and chronological ordering
- **Categorical Data**: Support for non-date string X-axis values

### 🗓️ Time Series Intelligence
- **Date Detection**: Automatically identifies date-based data
- **Missing Days Fill**: Optional feature to fill gaps with zero values
- **Smart Sorting**: Chronological ordering for dates, alphabetical for categories
- **Date Formatting**: Consistent yyyy-MM-dd format handling

### 📄 PDF Generation
- **Clean Layout**: Minimalist design focused on data visualization
- **A4 Pages**: Professional page formatting with proper margins
- **Multi-Query Support**: Each query generates a separate page
- **Error Handling**: Graceful fallback for chart generation failures

## Installation

### Prerequisites
- .NET 8.0 SDK
- MySQL Server (accessible database)

### Dependencies
The project automatically installs these NuGet packages:
- `MySql.Data` (8.4.0) - MySQL database connectivity
- `QuestPDF` (2024.3.0) - PDF generation
- `ScottPlot` (5.0.42) - Chart rendering
- `SkiaSharp` (2.88.8) - Graphics backend

### Setup
1. Clone or download the project
2. Navigate to the project directory
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Build the project:
   ```bash
   dotnet build
   ```

## Usage

### Basic Command
```bash
dotnet run "<connection_string>" "<sql_file_path>"
```

### With Missing Days Fill
```bash
dotnet run "<connection_string>" "<sql_file_path>" --fill-missing-days
```

### Examples
```bash
# Basic usage
dotnet run "Server=localhost;Port=3306;Database=analytics;Uid=user;Pwd=password;" "./sample_queries.sql"

# With gap filling for time series
dotnet run "Server=localhost;Port=3306;Database=analytics;Uid=user;Pwd=password;" "./sample_queries.sql" --fill-missing-days
```

## SQL File Format

### Query Structure
- **Comments**: Use `--` to define chart titles
- **Query Requirements**: Must return columns named `X` and `Y`
- **Data Types**: X should be string/date, Y should be numeric
- **Termination**: End queries with semicolon (`;`)

### Example SQL File
```sql
-- Daily Activity Data
SELECT 7 as Y, CAST('2025-08-05' AS CHAR) as X
UNION ALL SELECT 13, CAST('2025-08-04' AS CHAR)
UNION ALL SELECT 5, CAST('2025-08-01' AS CHAR);

-- Weekly Sales Performance
SELECT sales as Y, CAST(week_ending AS CHAR) as X
FROM (
    SELECT 15000 as sales, '2025-08-04' as week_ending
    UNION ALL SELECT 18000, '2025-07-28'
    UNION ALL SELECT 12000, '2025-07-21'
) t;
```

### Important Notes
- **Column Names**: Must be exactly `X` and `Y` (case-insensitive)
- **Date Casting**: Use `CAST(date_column AS CHAR)` for date columns to ensure proper string handling
- **Y Values**: Must be numeric (INTEGER, DECIMAL, FLOAT, etc.)
- **X Values**: Can be dates (YYYY-MM-DD format) or categorical strings

## Chart Features

### Automatic Chart Types
- **Time Series**: When all X values are valid dates
  - Chronological ordering (newest to oldest)
  - Date-formatted X-axis with proper spacing
  - Trend line analysis
- **Categorical**: When X values are non-date strings
  - Alphabetical ordering
  - Custom tick positioning
  - Rotated labels for readability

### Visual Elements
- **Line Chart**: Blue line with markers showing data progression
- **Trend Line**: Red dashed line showing linear regression trend
- **Grid Lines**: Subtle gray grid for easier value reading
- **Axis Labels**: "Count" for Y-axis, "X Values" for X-axis

### Missing Days Fill Feature
When `--fill-missing-days` is specified:
- **Automatic Detection**: Only applies to time series data
- **Complete Range**: Fills all missing days between min and max dates
- **Zero Values**: Missing days are assigned Y=0
- **Console Feedback**: Reports how many days were added

## Output

### PDF Structure
Each query generates one page containing:
1. **Page Title**: From SQL comment (e.g., "Daily Activity Data")
2. **Line Chart**: Professional chart with trend line
3. **Data Points**: Listed below chart (if ≤20 points)

### File Naming
Generated PDFs are named: `SqlToGraph-YYYY-MM-DD.pdf`

### Console Output
```
Fill missing days option enabled - will add missing dates with 0 values for time series data.
Successfully connected to MySQL database.
Executing query: Daily Activity Data
  Filling missing days from 2025-07-28 to 2025-08-05
  Added 4 missing days with 0 values (total: 9 points)
  Found 9 data points.
PDF report generated successfully.
PDF saved to: /path/to/SqlToGraph-2025-08-05.pdf
```

## Error Handling

### Database Errors
- Connection failures with detailed MySQL error codes
- Query validation (checks for X/Y columns)
- Type conversion warnings with suggestions

### Data Processing
- Graceful handling of NULL values
- Type mismatch detection and reporting
- Robust date parsing with fallbacks

### Chart Generation
- Fallback to text summary if chart rendering fails
- Error messages displayed in PDF
- Continuation with remaining queries on individual failures

## Technical Details

### Architecture
- **Platform**: .NET 8.0 cross-platform
- **Database**: MySQL with MySql.Data connector
- **Charts**: ScottPlot with SkiaSharp backend
- **PDF**: QuestPDF for document generation

### Performance
- **Memory Efficient**: Streams data processing
- **Chart Size**: 600x400 pixels for optimal quality/size balance
- **Concurrent Safe**: Single-threaded execution for database safety

### File I/O
- **SQL Parsing**: Multi-line query support with comment extraction
- **PDF Generation**: Direct file output with absolute path reporting
- **Error Logging**: Detailed console output for debugging

## License

This project uses QuestPDF Community License (free for non-commercial use).

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test with sample data
5. Submit a pull request

## Sample Data

The project includes `sample_queries.sql` with example datasets:
- Daily activity data with missing days
- Weekly sales performance
- Monthly revenue trends

Run these examples to see the full functionality in action.

## Troubleshooting

### Common Issues
1. **"No X/Y columns"**: Ensure your query returns columns named exactly `X` and `Y`
2. **Date parsing errors**: Use `CAST(date_column AS CHAR)` in your SQL
3. **Connection failures**: Verify MySQL server is running and credentials are correct
4. **Empty charts**: Check that your query returns data with valid Y numeric values

### Debug Output
The application provides detailed console output including:
- Column types and names from queries
- Data parsing results
- Chart generation status
- Missing days fill operations

For additional help, check the console output for specific error messages and suggestions.
