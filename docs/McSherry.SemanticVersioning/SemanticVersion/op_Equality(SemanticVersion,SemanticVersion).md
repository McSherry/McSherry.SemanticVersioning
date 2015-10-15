# SemanticVersion.Equality Operator(SemanticVersion, SemanticVersion)

```c#
public static bool operator ==(
    SemanticVersion l,
    SemanticVersion r
)
```

**Namespace:** [`McSherry.SemanticVersioning`][1]  
**Minimum version:** 1.0.0

[1]: ../

Determines whether the two specified [SemanticVersion][2]s are
equal in value.

[2]: ./


### Parameters

- **`l`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The first [SemanticVersion][2] to compare.
- **`r`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The second [SemanticVersion][2] to compare.
  

### Return Value

**Type:** `System.Boolean`

True if the provided [SemanticVersion][2]s are equal in value. False
if otherwise.