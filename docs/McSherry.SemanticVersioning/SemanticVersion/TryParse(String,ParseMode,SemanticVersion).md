# `SemanticVersion.TryParse(String, ParseMode, out SemanticVersion)` method

```c#
public static bool TryParse(
    string version,
    ParseMode mode,
    out SemanticVersion semver
)
```

**Namespace:** [`McSherry.SemanticVersioning`][1]  
**Minimum version:** 1.0.0

[1]: ../

Attempts to convert a version string to a [SemanticVersion][2], taking
into account a set of flags.

[2]: ./


### Parameters

- **`version`**  
  **Type:** `System.String`  
  The version string to be converted to a [SemanticVersion][2].
- **`mode`**  
  **Type:** [`McSherry.SemanticVersioning.ParseMode`][3]  
  A set of flags that augment how the version string is parsed.
- **`semver`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  When the method returns, this parameter is either set to the created
  [SemanticVersion][2] (if parsing was successful), or is given an
  undefined value (if parsing was unsuccessful).
  
[3]: ../ParseMode.md


### Return Value

**Type:** `System.Boolean`

True if parsing succeeded, false if otherwise.