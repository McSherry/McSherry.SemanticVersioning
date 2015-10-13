# `SemanticVersion.ToString(String, IFormatProvider)` method

```c#
string IFormattable.ToString(
    string format,
    IFormatProvider provider
)
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0  
**Implements:** `System.IFormattable.ToString`

[1]: ../

Formats the value of the current [SemanticVersion][2] as
specified.

[2]: ./


### Parameters

- **`format`**  
  **Type:** `System.String`  
  The format to use, or null for the default format.
- **`provider`**  
  **Type:** `System.IFormatProvider`  
  The format provider to use, or null for the default
  provider. This parameter is ignored.
  
  
### Return Value

**Type:** `System.String`

A string representation of the current [SemanticVersion][2],
formatted as specified.


## Exceptions

- **`System.FormatException`**  
  Thrown when the format specifier given in _`format`_ is not
  recognised or is invalid.
  
  
## Remarks

The format of a Semantic Version is not dependent on culture
information, and so the value of _`provider`_ is ignored.

The value of _`format`_ should contain one of the below-listed
format specifiers. Custom format patterns are not supported. If
_`format`_ is null, the default format specifier, `G`, is used
in its place.

The list of recognised format specifiers is given in the below.

- **`c`**  
  **Description:** Prefixed concise format. Identical to the
  concise format (`C`), except prefixed with a lowercase `v`.  
  **Examples:** `v1.8`, `v1.15.1`, `v2.1-beta.3`
  
- **`C`**  
  **Description:** Concise format. Omits metadata items, and only
  includes the [Patch][3] version if it is non-zero.
  **Examples:** `1.8`, `1.15.1`, `2.1-beta.3`
  
- **`g`**  
  **Description:** Prefixed default format. Identical to the
  default format (`G`), except prefixed with a lowercase `v`.
  **Examples:** `v1.7.0-alpha.2+20150925.f8f2cb1a`, `v1.2.5`,
  `v2.0.1-rc.1`
  
- **`G`**  
  **Description:** The default format, as given by the Semantic
  Versioning 2.0.0 specification.
  **Example:** `1.7.0-alpha.2+20150925.f8f2cb1a`, `1.2.5`,
  `2.0.1-rc.1`