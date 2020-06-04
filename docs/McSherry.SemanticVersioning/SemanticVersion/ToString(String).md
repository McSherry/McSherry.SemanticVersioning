# `SemanticVersion.ToString(String)` method

```c#
public string ToString(
    string format
)
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ../

Formats the value of the current [SemanticVersion][2] as
specified.

[2]: ./


### Parameters

- **`format`**  
  **Type:** `System.String`  
  The format to use, or `null` for the default format.
  
  
### Return Value

A string representation of the current [SemanticVersion][2],
formatted as specified.


## Exceptions

- **`System.FormatException`**  
  Thrown when the format specifier given in _`format`_ is
  not recognised or is invalid.
  
  
## Remarks

For information on the acceptable format specifier, see the
[Remarks][3] section of [`IFormattable` ToString(string, IFormatProvider)][4]'s
documentation.

[3]: ./IFormattable.ToString(String,IFormatProvider).md#remarks
[4]: ./IFormattable.ToString(String,IFormatProvider).md