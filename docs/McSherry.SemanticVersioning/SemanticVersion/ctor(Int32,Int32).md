# `SemanticVersion` constructor

    public SemanticVersion(
        int major,
        int minor
    )

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum Version:** 1.0.0

Creates a new [SemanticVersion][2] using the provided version
components, with [Patch][3] set to zero.

[1]: /docs/McSherry.SemanticVersioning
[2]: ../
[3]: ../Patch.md


### Parameters

- **`major`**  
  **Type:** `System.Int32`  
  The semantic version's major version.
- **`minor`**  
  **Type:** `System.Int32`  
  The semantic version's minor version.
  

## Exceptions

- **`System.ArgumentOutOfRangeException`**  
  Thrown when _`major`_ or _`minor`_ is negative.
