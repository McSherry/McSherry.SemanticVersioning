# `SemanticVersion.CompareTo(SemanticVersion)` method

```c#
public int CompareTo(
    SemanticVersion semver
)
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ../

Compares the current [SemanticVersion][2] with another version to
determine relative precedence.

### Parameters

- **`semver`**  
  **Type:** `McSherry.SemanticVersioning.SemanticVersion`  
  The [SemanticVersion][2] to compare to the current version.
  
[2]: ./


### Return Value

- **Less than zero**  
  The current [SemanticVersion][2] has lesser precedence than
  _`semver`_.
- **Zero**  
  The current [SemanticVersion][2] has equal precedence to _`semver`_.
- **Greater than zero**  
  The current [SemanticVersion][2] has greater precedence than
  _`semver`_.

