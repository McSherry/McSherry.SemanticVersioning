# `SemanticVersion.TryParse(String, ParseMode, out SemanticVersion, IEnumerator(char))` method

```c#
public static bool TryParse(
    string version,
    ParseMode mode,
    out SemanticVersion semver,
    out IEnumerator<char> enumerator
)
```

**Namespace:** [`McSherry.SemanticVersioning`][1]  
**Minimum version:** 1.3.0

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
- **`enumerator`**  
  **Type:** `System.Collections.Generic.IEnumerator<char>`  
  On success, either null (if the parser reached the end of `version`) or
  an IEnumerator\<char\> positioned after the last character of the
  [SemanticVersion][2] parsed from `version`. On failure, undefined.
[3]: ../ParseMode.md


### Return Value

**Type:** `System.Boolean`

True if parsing succeeded, false if otherwise.

## Remarks

For information about `enumerator`, see the remarks for [Parse(String, ParseMode, out IEnumerator(char))][4].

[4]: ./Parse(String,ParseMode,IEnumerator(T)).md