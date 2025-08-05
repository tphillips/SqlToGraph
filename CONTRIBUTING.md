# Contributing to SqlToGraph

Thank you for your interest in contributing to SqlToGraph! This document provides guidelines for contributing to the project.

## Code of Conduct

By participating in this project, you agree to abide by our Code of Conduct. Please be respectful and constructive in all interactions.

## How to Contribute

### Reporting Issues

1. **Search existing issues** first to avoid duplicates
2. **Use the issue template** when creating new issues
3. **Provide detailed information** including:
   - Operating system and .NET version
   - MySQL version and configuration
   - Steps to reproduce the issue
   - Expected vs actual behavior
   - Sample SQL queries (if applicable)

### Submitting Changes

1. **Fork the repository** and create a feature branch
2. **Follow coding standards** outlined below
3. **Write tests** for new functionality
4. **Update documentation** as needed
5. **Submit a pull request** with a clear description

### Development Setup

```bash
# Clone your fork
git clone https://github.com/your-username/SqlToGraph.git
cd SqlToGraph

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test
```

## Coding Standards

### C# Guidelines

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful names for variables, methods, and classes
- Add XML documentation comments for public APIs
- Prefer `async/await` over `Task.Result` or `.Wait()`
- Use `sealed` classes when inheritance is not intended
- Prefer records for immutable data models

### Code Structure

```
SqlToGraph/
├── Core/                 # Application orchestration
├── Models/              # Data models and configuration
├── Services/            # Business logic services
├── Program.cs           # Entry point
└── *.sql               # Sample queries
```

### Error Handling

- Use specific exception types when possible
- Provide helpful error messages with context
- Log errors appropriately
- Fail fast for configuration errors
- Gracefully handle data processing errors

### Testing

- Write unit tests for business logic
- Include integration tests for database operations
- Test error conditions and edge cases
- Use descriptive test method names

## Pull Request Process

1. **Update the README.md** if you change functionality
2. **Update CHANGELOG.md** with your changes
3. **Ensure all tests pass** and code builds successfully
4. **Request review** from maintainers
5. **Address feedback** promptly and professionally

### PR Title Format

Use one of these prefixes:
- `feat:` - New features
- `fix:` - Bug fixes
- `docs:` - Documentation changes
- `refactor:` - Code refactoring
- `test:` - Adding tests
- `chore:` - Maintenance tasks

### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Added tests for new functionality
- [ ] All existing tests pass
- [ ] Manual testing completed

## Checklist
- [ ] Code follows project conventions
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No breaking changes (or breaking changes documented)
```

## Feature Requests

We welcome feature requests! Please:

1. **Check existing issues** for similar requests
2. **Describe the use case** clearly
3. **Explain the expected behavior**
4. **Consider implementation complexity**
5. **Be willing to contribute** if possible

### Priority Features

Current focus areas:
- Chart type improvements (bar charts, scatter plots)
- Additional database support (PostgreSQL, SQL Server)
- Export format options (PNG, SVG)
- Interactive chart features
- Performance optimizations

## Documentation

### README Updates

- Keep examples current and working
- Update feature lists when adding functionality
- Maintain clear installation instructions
- Include troubleshooting information

### Code Documentation

- Use XML comments for public APIs
- Include usage examples in comments
- Document complex algorithms
- Explain non-obvious design decisions

## Release Process

1. Update version numbers in project files
2. Update CHANGELOG.md with release notes
3. Create release tag
4. Publish NuGet package (if applicable)
5. Update GitHub release notes

## Getting Help

- **Discord/Slack**: Join our community chat
- **GitHub Discussions**: For general questions
- **GitHub Issues**: For bugs and feature requests
- **Email**: maintainer@example.com for security issues

## Recognition

Contributors will be:
- Listed in CONTRIBUTORS.md
- Credited in release notes
- Given appropriate GitHub repository permissions

Thank you for contributing to SqlToGraph!
