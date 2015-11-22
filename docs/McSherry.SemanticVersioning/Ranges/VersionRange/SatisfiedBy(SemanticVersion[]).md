# `VersionRange.SatisfiedBy(SemanticVersion[])` method

```c#
public bool SatisfiedBy(
    params SemanticVersion[] semvers
    )
```

**Namespace:** [McSherry.SemanticVersioning.Ranges][1]  
**Minimum version:** 1.1.0

[1]: ../

Determines whether the current version range is satisfied
by all specified [SemanticVersion][2] instances.

[2]: ../SemanticVersion


### Parameters

- **`semver`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion[]`][2]  
  The [SemanticVersion][2] instances to check against the current
  version range.
  

### Return Value

**Type:** `System.Boolean`

True if all [SemanticVersion][2] instances in _`semvers`_ satisfy the
current range, false if otherwise.