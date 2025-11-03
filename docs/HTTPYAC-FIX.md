# httpYac Variable Extraction Fix

## Problem

HTTP tests were failing with variable extraction errors when running with `--all` flag:

```bash
httpyac send .http/products-v1.http --all --env development
# Result: 35/53 errors - variable extraction failures
```

**Error Pattern**: Variables like `@productId` were undefined in subsequent requests, causing cascading failures.

## Root Cause

**Incorrect variable extraction syntax** was used in the test files:

```http
# ❌ WRONG - This does NOT work in httpYac
# @name createProduct
POST {{baseUrl}}/api/v1/products
{"name": "Test", "price": 99.99}

###
@productId = {{createProduct.response.body.$.id}}
```

This syntax (`response.body.$.id`) is **not valid** in httpYac. The tool doesn't recognize this JSONPath-style notation.

## Solution

**Use the correct httpYac variable reference syntax**:

```http
# ✅ CORRECT - This works in httpYac
# @name createProduct
POST {{baseUrl}}/api/v1/products
{"name": "Test", "price": 99.99}

###
@productId = {{createProduct.id}}
```

### Why This Works

httpYac automatically:
1. Parses JSON response bodies
2. Makes fields directly accessible on the named response object
3. Allows simple dot notation: `{{namedResponse.fieldName}}`

### Documentation Reference

From httpYac docs ([examples](https://httpyac.github.io/guide/examples.html)):

```http
# @name session
POST {{host}}/api/v1/session
{"username": "admin", "password": "password"}

###
# Access token directly from named response
GET {{host}}/api/v1/applications
Authorization: Bearer {{session.token}}
```

## Changes Made

Updated both test files with correct syntax:

### File: `.http/products-v1.http`
- Fixed 6 variable extractions
- Lines: 88, 192, 234, 248, 262, 276

### File: `.http/products-v2.http`  
- Fixed 4 variable extractions
- Lines: 58, 97, 123, 137

## Before vs After

### Before Fix:
```
53 requests processed
✅ 18 succeeded
❌ 35 errors (variable extraction failures)
```

### After Fix:
```
53 requests processed  
✅ 50 tests passing
✅ 3 cleanup operations
✅ 100% success rate
```

## Test Results

### v1.0 API (50 tests):
```bash
httpyac send .http/products-v1.http --all --env development
```
**Result**: ✅ All 50 tests passing

### v2.0 API (25 tests):
```bash
httpyac send .http/products-v2.http --all --env development
```
**Result**: ✅ All 25 tests passing

### Total: 75/75 tests passing (100% success)

## Key Learnings

1. **Named Responses**: Use `# @name` before requests to create reusable references
2. **Response Access**: Access fields directly: `{{namedResponse.fieldName}}`
3. **No JSONPath**: httpYac doesn't use `response.body.$.field` syntax
4. **Automatic Parsing**: JSON responses are automatically parsed
5. **Nested Fields**: Access nested objects: `{{session.user.id}}`

## httpYac Variable Reference Quick Guide

### Basic Variable Assignment
```http
@baseUrl = http://localhost:8080
@apiVersion = 1.0
```

### Named Request Response
```http
# @name createUser
POST {{baseUrl}}/users
{"name": "John"}

###
# Extract from response
@userId = {{createUser.id}}
@userName = {{createUser.name}}
```

### Using Extracted Variables
```http
GET {{baseUrl}}/users/{{userId}}
Authorization: Bearer {{createUser.token}}
```

### Response Structure Access
```http
# @name getProducts
GET {{baseUrl}}/products

###
# Access nested fields
@firstProductId = {{getProducts.items[0].id}}
@productCount = {{getProducts.items.length}}
```

## Verification

Run tests to verify fix:

```bash
# Test v1 API
httpyac send .http/products-v1.http --all --env development

# Test v2 API  
httpyac send .http/products-v2.http --all --env development

# Both should show 100% pass rate
```

## References

- [httpYac Variables Documentation](https://httpyac.github.io/guide/variables.html)
- [httpYac Examples](https://httpyac.github.io/guide/examples.html)
- [httpYac Scripting Guide](https://httpyac.github.io/guide/scripting.html)

---

**Status**: ✅ RESOLVED  
**Date**: November 3, 2025  
**Impact**: All HTTP tests now pass successfully with `--all` flag
