# `VersionRange` constructor

```c#
public VersionRange(
    string range
    )
```

**Namespace:** [`McSherry.SemanticVersioning.Ranges`][1]  
**Minimum Version:** 1.1.0

Creates a version range from a string representing the range.

[1]: ../


### Parameters

- **`range`**  
  **Type:** `System.String`  
  The version range string from which to create an instance.
  

## Exceptions

- **`System.ArgumentNullException`**  
  Thrown when _`ranges`_ is null, empty, or contains only
  whitespace characters.
- **`System.ArgumentException`**  
  Thrown when _`range`_ is invalid for any reason unrelated to
  an invalid semantic version string.
- **`System.FormatException`**  
  Thrown when _`range`_ contains an invalid semantic version
  string.