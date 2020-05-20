# `SemanticVersionFormat.PrefixedDefault` property

```c#
[Obsolete]
public static string PrefixedDefault => "g"
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ../

**Type:** `System.String`  
The default way to format a semantic version, prefixed with
a letter `v`.


## Remarks

For details on how this option formats a semantic version, see [Default][2]. Standard format specifiers that include a prefix have been deprecated in favour of using custom format patterns, which can include any prefix desired.

See remarks for [SemanticVersion][1a]'s implementation of [IFormattable.ToString][3] for further information.

[1a]: ../
[2]: ./Default.md
[3]: ../SemanticVersion/IFormattable.ToString(String,IFormatProvider).md