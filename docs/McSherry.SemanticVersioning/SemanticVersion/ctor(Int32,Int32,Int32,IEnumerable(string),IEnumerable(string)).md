# `SemanticVersion` constructor

```c#
public SemanticVersion(
    int major,
    int minor,
    int patch,
    IEnumerable<string> identifiers,
    IEnumerable<string> metadata
)
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum Version:** 1.0.0

Creates a new [SemanticVersion][2] using the provided version
components, pre-release identifiers, and metadata items.

[1]: ../
[2]: ./


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
- **`identifiers`**  
  **Type:** `System.Collections.Generic.IEnumerable<string>`  
  The semantic version's pre-release identifiers.
- **`metadata`**  
  **Type:** `System.Collections.Generic.IEnumerable<string>`  
  The semantic version's build metadata items.
  
  
## Exceptions

- **`System.ArgumentOutOfRangeException`**  
  Thrown when any of _`major`_, _`minor`_, and _`patch`_
  is negative.
- **`System.ArgumentNullException`**  
  Thrown when _`identifiers`_ or any of its items are null.
- **`System.ArgumentException`**  
  Thrown when any of the items in _`identifiers`_ are not
  valid pre-release identifiers, or when any of the items
  in _`metadata`_ are not valid build metadata items.