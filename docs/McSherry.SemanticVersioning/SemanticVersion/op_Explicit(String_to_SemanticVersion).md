# SemanticVersion.Explicit operator(String to SemanticVersion)

```c#
public static explicit operator SemanticVersion(
    string version
)
```

**Namespace:** [`McSherry.SemanticVersioning`][1]  
**Minimum version:** 1.0.0

[1]: ../

Converts a version string to a [SemanticVersion][2], with
all [`ParseMode`][3] modifiers active.

[2]: ./
[3]: ../ParseMode.md


### Parameters

- **`version`**  
  **Type:** `System.String`  
  The version string to convert to a [SemanticVersion][2].
  

### Return Value

**Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]


## Exceptions

- **`System.InvalidCastException`**  
  Thrown when _`version`_ is invalid and cannot be cast to a
  [SemanticVersion][2]. This wraps the exceptions thrown by
  [Parse(String)][3] and [Parse(String, ParseMode)][4].

[3]: ./Parse(String).md
[4]: ./Parse(String,ParseMode).md