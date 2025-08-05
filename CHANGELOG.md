# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-08-05

### Added
- **Service-Oriented Architecture**: Introduced dedicated service classes for better code organization
  - `SqlQueryParser`: SQL file parsing and comment extraction
  - `DatabaseService`: Database connection and query execution
  - `ChartService`: High-resolution chart generation with ScottPlot
  - `PdfService`: Professional PDF report generation
  - `DataProcessor`: Time series data processing and gap filling
- **Enhanced Data Models**: Converted to immutable `sealed record` types with computed properties
  - `DataPoint`: Immutable data point with string X and numeric Y coordinates
  - `SqlQueryInfo`: Immutable SQL query with title metadata
  - `QueryResult`: Enhanced with `HasData` and `IsTimeSeries` properties
- **Comprehensive Documentation**: Added XML documentation for all public APIs
- **Project Files**: Added professional project files for GitHub
  - README.md with complete usage instructions
  - LICENSE (MIT License)
  - CONTRIBUTING.md with contribution guidelines
  - .gitignore with appropriate exclusions

### Enhanced
- **High-Resolution Chart Rendering**: Upgraded to 1200x800 resolution with 2.0x scale factor for crisp output
- **Improved Chart Quality**: Enhanced line thickness, marker sizes, and font sizes for better visibility
- **Professional PDF Layout**: Clean, professional PDF output with embedded high-quality charts
- **Robust Error Handling**: Comprehensive error handling with detailed logging and fallback mechanisms
- **Type Safety**: Enhanced null safety and type conversion handling
- **Code Organization**: Structured code into logical regions with clear separation of concerns

### Changed
- **Breaking**: Refactored from mutable classes to immutable records
- **Breaking**: Consolidated modular files into organized single-file architecture
- **Architecture**: Moved from instance-based to static service classes for better performance
- **Data Handling**: Improved type conversion and null handling for database values
- **Chart Rendering**: Upgraded from basic to high-resolution chart output with enhanced styling

### Fixed
- **Build Errors**: Resolved compilation issues from modularization conflicts
- **Type Conversion**: Fixed issues with mixed data types in database columns
- **Chart Quality**: Resolved low-resolution chart rendering issues
- **Error Messages**: Improved error reporting with specific guidance for common issues

### Technical Details
- **Target Framework**: .NET 8.0
- **Dependencies**: 
  - MySql.Data 9.0.0
  - QuestPDF 2024.3.0
  - ScottPlot 5.0.42
  - SkiaSharp 2.88.8
- **Chart Resolution**: 1200x800 pixels with 2.0x scale factor
- **PDF Features**: A4 pages with embedded high-resolution charts and fallback text summaries

## [1.0.0] - 2025-08-04

### Added
- Initial release with basic SQL to chart functionality
- Support for MySQL database connections
- Basic chart generation with ScottPlot
- PDF report generation with QuestPDF
- String-based X-axis support for dates and categories
- Optional missing day filling for time series data
- Command-line interface with help text

### Features
- SQL file parsing with comment-based titles
- Automatic date detection and sorting
- Basic line charts with trend lines
- PDF output with charts and data summaries

