# SemanticVersion.GreaterThanOrEqual Operator(SemanticVersion, SemanticVersion)

```c#
public static bool operator >=(
    SemanticVersion l,
    SemanticVersion r
)
```

**Namespace:** [`McSherry.SemanticVersioning`][1]  
**Minimum version:** 1.0.0

[1]: ../

Determines wheter the precedence of one [SemanticVersion][2] is
equal to or greater than the precedence of another.

[2]: ./


### Parameters

- **`l`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The [SemanticVersion][2] to check for equal or greater precedence.
- **`r`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The [SemanticVersion][2] to compare against.
  
  
### Return Value

**Type:** `System.Boolean`

True if the precedence of _`l`_ is equal to or greater than the
precedence of _`r`_. False if otherwise.