# SemanticVersion.GreaterThan Operator(SemanticVersion, SemanticVersion)

```c#
public static bool operator >(
    SemanticVersion l,
    SemanticVersion r
)
```

**Namespace:** [`McSherry.SemanticVersioning`][1]  
**Minimum version:** 1.0.0

[1]: ../

Determines whether one [SemanticVersion][2] has greater
precedence than another.

[2]: ./


### Parameters

- **`l`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The [SemanticVersion][2] to check for greater precedence.
- **`r`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The [SemanticVersion][2] to compare against.
  
  
### Return Value

**Type:** `System.Boolean`

True if _`l`_ has greater precedence than _`r`_. False if otherwise.