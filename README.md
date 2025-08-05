# SqlToGraph

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](#)
[![QuestPDF](https://img.shields.io/badge/QuestPDF-2024.3.0-green.svg)](https://github.com/QuestPDF/QuestPDF)
[![ScottPlot](https://img.shields.io/badge/ScottPlot-5.0.42-orange.svg)](https://github.com/ScottPlot/ScottPlot)

A high-performance C# console application that executes SQL queries against a MySQL database and generates professional PDF reports with high-resolution line charts and trend analysis.

Simply add one or more queries terminated with ; to a SQL file that return Just 2 cols. X and Y. Add a chart title with a -- comment above the query.  

Perfect for automated management information visualization.

## 📋 Table of Contents

- [Features](#-features)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Usage](#-usage)
- [SQL File Format](#-sql-file-format)
- [Chart Features](#-chart-features)
- [Output](#-output)
- [Error Handling](#️-error-handling)
- [Technical Details](#️-technical-details)
- [Contributing](#-contributing)
- [License](#-license)
- [Troubleshooting](#-troubleshooting)

## 🚀 Features

### 📊 Professional Charts
- **High-Resolution Rendering**: 1200x800 pixel charts with 2.0x scale factor for crisp output
- **Line Charts**: Professional quality charts using ScottPlot library
- **Trend Lines**: Automatic linear regression trend analysis with dashed red lines
- **Time Series Support**: Smart date detection and chronological ordering
- **Categorical Data**: Support for non-date string X-axis values

### 🗓️ Time Series Intelligence
- **Date Detection**: Automatically identifies date-based data patterns
- **Missing Days Fill**: Optional feature to fill gaps with zero values (`--fill-missing-days`)
- **Smart Sorting**: Chronological ordering for dates, alphabetical for categories
- **Date Formatting**: Consistent yyyy-MM-dd format handling

### 📄 PDF Generation
- **Clean Layout**: Minimalist design focused on data visualization
- **A4 Pages**: Professional page formatting with proper margins
- **Multi-Query Support**: Each query generates a separate page
- **Error Handling**: Graceful fallback for chart generation failures
- **High Quality**: Embedded high-resolution charts with professional styling

## 🔧 Installation

### Prerequisites
- .NET 8.0 SDK
- MySQL Server (accessible database)

### 📦 Dependencies
The project automatically installs these NuGet packages:
- `MySql.Data` (9.0.0) - MySQL database connectivity
- `QuestPDF` (2024.3.0) - Professional PDF generation
- `ScottPlot` (5.0.42) - High-quality chart rendering
- `SkiaSharp` (2.88.8) - Graphics backend for chart rendering

### 🛠️ Setup
1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-username/SqlToGraph.git
   cd SqlToGraph
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

## 🚀 Quick Start

```bash
# Run with sample data
dotnet run "Server=localhost;Port=3306;Database=testdb;Uid=user;Pwd=password;" "./sample_queries.sql"

# With missing days fill for time series
dotnet run "Server=localhost;Port=3306;Database=testdb;Uid=user;Pwd=password;" "./sample_queries.sql" --fill-missing-days
```

## 📖 Usage

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

## 📝 SQL File Format

### Query Structure
- **Comments**: Use `--` to define chart titles
- **Query Requirements**: Must return columns named `X` and `Y`
- **Data Types**: X should be string/date, Y should be numeric
- **Termination**: End queries with semicolon (`;`)

### Example SQL File
```sql
-- Daily Activity Data
SELECT 7 as Y, '2025-08-05' as X
UNION ALL SELECT 13, 2025-08-04
UNION ALL SELECT 5, 2025-08-01;

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
- **Y Values**: Must be numeric (INTEGER, DECIMAL, FLOAT, etc.)
- **X Values**: Can be dates (YYYY-MM-DD format) or categorical strings

## 📊 Chart Features

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

## 📄 Output

### PDF Structure
Each query generates one page containing:
1. **Page Title**: From SQL comment (e.g., "Daily Activity Data")
2. **Line Chart**: Professional chart with trend line
3. **Data Points**: Listed below chart (if ≤20 points)

### File Naming
Generated PDFs are named: `Report-YYYY-MM-DD.pdf`

## ⚠️ Error Handling

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

## ⚙️ Technical Details

### 🏗️ Architecture
- **Platform**: .NET 8.0 cross-platform
- **Database**: MySQL with MySql.Data connector
- **Charts**: ScottPlot with SkiaSharp backend
- **PDF**: QuestPDF for document generation
- **Code Organization**: Service-oriented architecture with static classes

### 🚀 Performance
- **Memory Efficient**: Streams data processing
- **Chart Resolution**: 1200x800 pixels with 2.0x scale factor for optimal quality
- **Concurrent Safe**: Single-threaded execution for database safety
- **High-Quality Output**: Professional-grade charts and PDF generation

### 📁 File I/O
- **SQL Parsing**: Multi-line query support with comment extraction
- **PDF Generation**: Direct file output with absolute path reporting
- **Error Logging**: Detailed console output for debugging

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Test with sample data
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

**Note**: This project uses QuestPDF Community License (free for non-commercial use).

## 🔧 Troubleshooting

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

## 📊 Sample Data

The project includes `sample_queries.sql` with example datasets:
- Daily activity data with missing days
- Weekly sales performance  
- Monthly revenue trends

Run these examples to see the full functionality in action:

```bash
dotnet run "your_connection_string" "./sample_queries.sql" --fill-missing-days
```

---

<div align="center">

**⭐ If you find this project helpful, please give it a star! ⭐**

[Report Bug](https://github.com/your-username/SqlToGraph/issues) • [Request Feature](https://github.com/your-username/SqlToGraph/issues) • [Documentation](https://github.com/your-username/SqlToGraph/wiki)

</div>
