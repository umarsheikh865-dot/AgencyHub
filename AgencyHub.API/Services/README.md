# AgencyHub - Enterprise Multi-Tenant SaaS

## Testing

AgencyHub uses xUnit and Moq for automated testing.

Test coverage includes:
- Authentication & JWT validation
- Role-based authorization (401/403 handling)
- Tenant isolation data boundaries
- Client, Project, and Task CRUD operations
- Validation & Error handling