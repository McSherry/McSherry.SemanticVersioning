# `MonotonicExtensions.IsMonotonic(SemanticVersion)`method

```c#
public static bool IsMonotonic(
    this SemanticVersion version
    )
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

Determines whether a [SemanticVersion][2] is a valid monotonic version.

[1]: ../
[2]: ../../SemanticVersion

### Parameters

- **`version`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The [SemanticVersion][2] to be checked.

### Return Value

**Type:** `System.Bool`

True if _`version`_ is a valid monotonic version, false if otherwise.

## Exceptions

- **`System.ArgumentNullException`**  
  _`version`_ is null.

## Remarks

A [SemanticVersion][2] is considered a valid version if it: has no
[SemanticVersion.Patch][3] component; and has no [SemanticVersion.Identifiers][4].

[3]: ../../SemanticVersion/Patch.md
[4]: ../../SemanticVersion/Identifiers.md