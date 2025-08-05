# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 2.0.x   | :white_check_mark: |
| 1.0.x   | :x:                |

## Reporting a Vulnerability

If you discover a security vulnerability in SqlToGraph, please follow these steps:

1. **Do NOT** create a public GitHub issue for security vulnerabilities
2. Email the security concern to [your-email@domain.com] with:
   - Description of the vulnerability
   - Steps to reproduce the issue
   - Potential impact assessment
   - Any suggested fixes or mitigations

3. You should receive a response within 48 hours acknowledging receipt
4. We will investigate and provide updates on the status within 5 business days
5. Once the vulnerability is confirmed and fixed, we will:
   - Release a patch version
   - Publish a security advisory
   - Credit you for the responsible disclosure (if desired)

## Security Considerations

### Database Connections
- Always use secure connection strings
- Never commit connection strings with credentials to version control
- Use environment variables or secure configuration management
- Consider using SSL/TLS for database connections

### SQL Injection
- This application executes raw SQL queries from files
- **Never** execute SQL files from untrusted sources
- Validate and sanitize SQL files before execution
- Consider using parameterized queries for user input

### File System Access
- The application reads SQL files and writes PDF files
- Ensure appropriate file system permissions
- Validate file paths to prevent directory traversal attacks
- Run with minimal required privileges

## Best Practices

1. **Environment Isolation**: Use separate databases for development and production
2. **Access Control**: Limit database user permissions to minimum required
3. **Audit Trail**: Monitor and log application usage in production environments
4. **Regular Updates**: Keep dependencies updated to latest secure versions

For questions about security practices, please refer to the documentation or contact the maintainers.
