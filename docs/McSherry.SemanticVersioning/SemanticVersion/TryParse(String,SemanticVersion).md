# `SemanticVersion.TryParse(String, out SemanticVersion)` method

```c#
public static bool TryParse(
    string version,
    out SemanticVersion semver
)
```

**Namespace:** [`McSherry.SemanticVersioning`][1]  
**Minimum version:** 1.0.0

[1]: ../

Attempts to convert a version string to a [SemanticVersion][2].

[2]: ./


### Parameters

- **`version`**  
  **Type:** `System.String`  
  The version string to be converted to a [SemanticVersion][2].
- **`semver`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  When the method returns, this parameter is either set to the created
  [SemanticVersion][2] (if parsing was successful), or is given an
  undefined value (if parsing was unsuccessful).
  
[3]: ../ParseMode.md


### Return Value

**Type:** `System.Boolean`

True if parsing succeeded, false if otherwise.


## Remarks

This method is equivalent to calling [TryParse(String, ParseMode, SemanticVersion)][4]
and passing the value [`ParseMode.Strict`][3].

[4]: ./TryParse(String,ParseMode,SemanticVersion).md