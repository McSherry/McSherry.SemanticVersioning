# `SemanticVersion.Equals(SemanticVersion)` method

```c#
public bool Equals(
    SemanticVersion semver
)
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0  
**Implements:** `System.IEquatable<SemanticVersion>.Equals`

[1]: ../

Determines whether the specified [SemanticVersion][2] is equal
to the current version.

[2]: ./


### Parameters

- **`semver`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The [SemanticVersion][2] to compare with the current version.


### Return Value

**Type:** `System.Boolean`

True if the specified and current [SemanticVersion][2]s are equal,
false if otherwise.


## Remarks

This method takes [build metadata items][3] into account when
comparing, and so may return false for equivalent versions with
differing build metadata.

[3]: ./Metadata.md