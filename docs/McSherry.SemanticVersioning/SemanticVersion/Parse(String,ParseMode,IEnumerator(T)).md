# `SemanticVersion.Parse(String, ParseMode, IEnumerator(char))` method

```c#
public static SemanticVersion Parse(
    string version,
    ParseMode mode,
    out IEnumerator<char> enumerator
)
```

**Namespace:** [`McSherry.SemanticVersioning`][1]  
**Minimum version:** 1.3.0

[1]: ../

Converts a version string to a [SemanticVersion][2], taking
into account a set of flags.

[2]: ./


### Parameters

- **`version`**  
  **Type:** `System.String`  
  The version string to be converted to a [SemanticVersion][2].
- **`mode`**  
  **Type:** [`McSherry.SemanticVersioning.ParseMode`][3]  
  A set of flags to augment how the version string is parsed.
- **`enumerator`**  
  **Type:** `System.Collections.Generic.IEnumerator<char>`  
  An enumerator over `version`, positioned after the last character
  of the [SemanticVersion][2] parsed from `version`, or null if the parser
  reached the end of the string.



[3]: ../ParseMode.md


### Return Value

**Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]

A [SemanticVersion][2] equivalent to the provided version string.


## Exceptions

- **`System.ArgumentNullException`**  
  Thrown when _`version`_ is null or empty.
- **`System.ArgumentException`**  
  Thrown when a component in the version string was expected but
  not found (for example, a missing minor or patch version).
- **`System.FormatException`**  
  Thrown when an invalid character or character sequence is
  encountered.
- **`System.OverflowException`**  
  Thrown when an attempt to convert the major, minor, or patch
  version into a `System.Int32` resulted in an overflow.

## Remarks

The parameter `enumerator` exposes the enumerator used internally
by the parser to walk `version`. Its intended use is where
[ParseMode.Greedy][3] is specified in `mode`, where it enables the caller to
implement further parsing (for example, if the caller has a meaningful
way to convert a `System.Version` to a [SemanticVersion][2]).

On success, the value of `enumerator` depends on whether the end of
`version` was reached. If it was, `enumerator` will be null. Otherwise, it
will have a value and its `IEnumerator<T>.Current` property will be
positioned after the last character of `version` that the parser processed.
If [ParseMode.Greedy][3] is specified and `1.0.0.0` is provided as an input,
the returned enumerator will be on the third `.`.

As part of the processing before parsing, leading and trailing whitespace
is stripped. Anything returned in `enumerator` will, accordingly, not include
leading or trailing whitespace.

On failure, the value of `enumerator` is undefined.