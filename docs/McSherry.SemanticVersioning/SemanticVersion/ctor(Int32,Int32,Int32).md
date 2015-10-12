# `SemanticVersion` constructor

```c#
public SemanticVersion(
    int major,
    int minor,
    int patch
)
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum Version:** 1.0.0

Creates a new [SemanticVersion][2] using the provided
version components.

[1]: /docs/McSherry.SemanticVersioning
[2]: ./
[3]: ./Patch.md


### Parameters

- **`major`**  
  **Type:** `System.Int32`  
  The semantic version's major version.
- **`minor`**  
  **Type:** `System.Int32`  
  The semantic version's minor version.
- **`patch`**  
  **Type:** `System.Int32`  
  The semantic version's patch version.
  
  
## Exceptions

- **`System.ArgumentOutOfRangeException`**  
  Thrown when any of _`major`_, _`minor`_, and _`patch`_ is
  negative.