# HTTP API Testing with httpYac

This directory contains HTTP test files for the Products API using [httpYac](https://httpyac.github.io/), a powerful HTTP client extension for VS Code.

## Prerequisites

1. **Install httpYac Extension**
   - Open VS Code
   - Go to Extensions (Cmd+Shift+X on Mac, Ctrl+Shift+X on Windows)
   - Search for "httpYac"
   - Install the extension by anweber

2. **Start the API**
   ```bash
   cd src/Products.API
   dotnet run
   ```
   
   The API should start on `http://localhost:5000` (or the port specified in launchSettings.json)

## File Structure

```
.http/
├── http-client.env.json          # Environment variables (dev, prod)
├── products-v1.http               # Complete test suite for API v1.0
├── products-v2.http               # Complete test suite for API v2.0
└── README.md                      # This file
```

## Environment Configuration

The `http-client.env.json` file contains environment-specific variables:

- **development**: Local development settings
  - `baseUrl`: http://localhost:5000
  - `apiVersion`: 1.0
  - `apiVersion2`: 2.0

- **production**: Production settings
  - `baseUrl`: https://api.products.com
  - Update these when deploying

To switch environments in httpYac:
1. Click the environment selector in the bottom status bar
2. Choose "development" or "production"

## Test Files

### products-v1.http

Comprehensive test suite for Products API v1.0 containing 50+ test cases:

**Product Tests:**
- Get all products
- Search products by name
- Get product by ID
- Create product
- Update product
- Delete product

**Product Options Tests:**
- Get all options for a product
- Get specific option
- Create option
- Update option
- Delete option

**Error Scenarios:**
- 404 Not Found cases
- 400 Bad Request cases
- Validation errors
- ID mismatches
- Invalid data formats

### products-v2.http

Test suite for Products API v2.0:
- All v1.0 tests adapted for v2.0
- Version compatibility tests
- Placeholder for v2.0-specific features

## How to Use

### Running Individual Requests

1. Open a `.http` file in VS Code
2. Hover over any request
3. Click "Send Request" or press `Ctrl+Alt+R` (Windows/Linux) or `Cmd+Alt+R` (Mac)
4. View the response in the panel

### Running All Requests Sequentially

1. Open a `.http` file
2. Click "Send All Requests" button in the top right
3. Or use the command palette: `httpYac: Send All`
4. Watch all tests execute in order with variable extraction

### Variable Extraction

The test files use httpYac's variable extraction feature:

```http
### Create Product
# @name createProduct
POST {{baseUrl}}/api/v1.0/products
Content-Type: application/json

{
  "name": "Test Product",
  "price": 99.99
}

### Extract the ID
@productId = {{createProduct.response.body.$.id}}

### Use the ID in subsequent request
GET {{baseUrl}}/api/v1.0/products/{{productId}}
```

Variables are automatically extracted and available for subsequent requests.

## Test Coverage

### products-v1.http (50 test cases)

1. **CRUD Operations** (Tests 1-14)
   - Basic product operations
   - Search and filtering
   - Error handling

2. **Product Options** (Tests 15-33)
   - Option CRUD operations
   - Relationship validation
   - Error scenarios

3. **Delete Operations** (Tests 34-44)
   - Cascading deletes
   - Cleanup verification
   - Not found scenarios

4. **Edge Cases** (Tests 45-50)
   - Invalid content types
   - Malformed JSON
   - Validation boundaries
   - Large numbers
   - Empty/null values

### Expected Response Codes

- `200 OK` - Successful GET request
- `201 Created` - Successful POST request with resource creation
- `204 No Content` - Successful PUT/DELETE request
- `400 Bad Request` - Validation errors, malformed data
- `404 Not Found` - Resource doesn't exist
- `415 Unsupported Media Type` - Wrong content type

## Running Tests in CI/CD

httpYac can be run from the command line for automated testing:

```bash
# Install httpYac CLI
npm install -g httpyac

# Run all tests in a file
httpyac .http/products-v1.http --all

# Run with specific environment
httpyac .http/products-v1.http --all --env development

# Generate JUnit report
httpyac .http/products-v1.http --all --junit --output test-results.xml
```

## Best Practices

1. **Sequential Testing**: Run tests in order as later tests depend on earlier ones
2. **Variable Extraction**: Use `@name` and variable extraction for reusable IDs
3. **Cleanup**: Delete operations are at the end to clean up test data
4. **Comments**: Each test includes expected response codes and descriptions
5. **Separation of Concerns**: Separate files for different API versions

## Troubleshooting

### Request Fails with Connection Error
- Ensure the API is running: `dotnet run --project src/Products.API`
- Check the `baseUrl` in `http-client.env.json`
- Verify the port matches your API configuration

### Variables Not Working
- Ensure the `@name` directive is set on the source request
- Check that the extraction path is correct: `$.id` for JSON
- Variables are scoped to the file and persist during the session

### 404 Errors
- Run tests sequentially from the beginning
- Extracted IDs from created resources are needed for subsequent tests
- Don't run delete tests before other tests

### 400 Bad Request
- Check request body formatting
- Ensure required fields are present
- Validate data types match API expectations

## Additional Resources

- [httpYac Documentation](https://httpyac.github.io/)
- [HTTP Request Syntax](https://httpyac.github.io/guide/request.html)
- [Variable Support](https://httpyac.github.io/guide/variables.html)
- [Environment Variables](https://httpyac.github.io/guide/environments.html)

## Maintenance

When the API changes:

1. Update test files to reflect new endpoints
2. Modify expected response codes if behavior changes
3. Add new test cases for new features
4. Update environment variables if needed
5. Keep variable extraction paths in sync with response structure

## Contact

For issues or questions about these tests, please refer to the main project documentation or open an issue in the repository.
