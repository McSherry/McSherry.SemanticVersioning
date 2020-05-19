# `SemanticVersionFormat.PrefixedConcise` property

```c#
[Obsolete]
public static string PrefixedConcise => "c"
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ../

**Type:** `System.String`  
A concise way to format a semantic version, prefixed with a
letter `v`.


## Remarks

For details on how this option formats a semantic version, see
[Concise][2].

This format specifier is deprecated in favour of custom format strings. See [SemanticVersion.ToString][3] for further information.

[2]: ./Concise.md
[3]: ../SemanticVersion/IFormattable.ToString(String,IFormatProvider).md