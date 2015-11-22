# `VersionRange.SatisfiedBy(SemanticVersion)` method

```c#
public bool SatisfiedBy(
    SemanticVersion semver
    )
```

**Namespace:** [McSherry.SemanticVersioning.Ranges][1]  
**Minimum version:** 1.1.0

[1]: ../

Determines whether the current version range is satisfied
by a specified [SemanticVersion][2].

[2]: ../SemanticVersion


### Parameters

- **`semver`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The [SemanticVersion][2] to check against the current version
  range.
  

### Return Value

**Type:** `System.Boolean`

True if the current version range is satisfied by _`semver`_, 
false if otherwise.