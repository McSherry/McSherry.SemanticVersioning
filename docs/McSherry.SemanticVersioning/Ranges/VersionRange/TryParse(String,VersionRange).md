# `VersionRange.TryParse(String, out VersionRange)` method

```c#
public static bool TryParse(
    string range,
    out VersionRange result
    )
```

**Namespace:** [`McSherry.SemanticVersioning.Ranges`][1]  
**Minimum Version:** 1.1.0

[1]: ../

Attempts to parse a version range from a string.


### Parameters

- **`range`**  
  **Type:** `System.String`  
  The string representing the version range.
- **`result`**  
  **Type:** [`McSherry.SemanticVersioning.Ranges.VersionRange`][2]  
  The [VersionRange][2] that, on success, is given a value
  equivalent to _`range`_.
  
[2]: ./


### Return Value

**Type:** `System.Boolean`

True on success, false on failure.