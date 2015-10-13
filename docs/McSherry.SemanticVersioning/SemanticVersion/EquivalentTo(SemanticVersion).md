# `SemanticVersion.EquivalentTo(SemanticVersion)` method

```c#
public bool EquivalentTo(
    SemanticVersion semver
)
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ../

Determines whether the specified [SemanticVersion][2] is
equivalent to the current version.

[2]: ./


### Parameters

- **`semver`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The [SemanticVersion][2] to compare against.
  
  
### Return Value

**Type:** `System.Boolean`

True if the current [SemanticVersion][2] is equivalent to _`semver`_.


## Remarks

This differs from [Equals(SemanticVersion)][3] in that the value of
[Metadata][4] is ignored.

[3]: ./Equals(SemanticVersion).md
[4]: ./Metadata.md