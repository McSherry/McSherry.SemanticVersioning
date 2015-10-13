# `SemanticVersion.CompatibleWith(SemanticVersion)` method

```c#
public bool CompatibleWith(
    SemanticVersion semver
)
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ../

Determines whether the specified [SemanticVersion][2] is
backwards-compatible with the current version.

[2]: ./


### Parameters

- **`semver`**  
  **Type:** `McSherry.SemanticVersioning.SemanticVersion`  
  The [SemanticVersion][2] to test for backwards compatibility.
  
  
### Return Value

**True** if _`semver`_ is backwards-compatible with the current
version. **False** if otherwise.


## Remarks

The following situations will always produce a false result:

- The [Major][3] versions of the compared [SemanticVersion][2]s
  differ.
- The [Major][3] versions of either of the compared versions
  are equal to zero (unless the two versions are equivalent).
- The parameter _`semver`_ is null.

[3]: ./Major.md

If none of the above conditions are met, compatibility is
determined through simple precedence comparison, where a
version will only be considered compatible if it is of equal
or greater precedence.

It should be noted that a _`semver`_ value with pre-release
identifiers will be considered backwards-compatible provided
its [Major][3]-[Minor][4]-[Patch][5] trio is greater than the
trio of this version and the [Major][3] versions are equal.
This is because, even though it is a pre-release version, it
is within the same major version, and so should, if the
Semantic Versioning specification is being properly adhered
to, be backwards-compatible.

[4]: ./Minor.md
[5]: ./Patch.md