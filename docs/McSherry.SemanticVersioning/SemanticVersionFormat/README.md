# `SemanticVersionFormat` class

```c#
[CLSCompliant(true)]
public static class SemanticVersionFormat
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum Version:** 1.0.0

[1]: ../

Lists the format identifiers accepted by the [SemanticVersion][2]
class's implementation of `IFormattable`.

[2]: ../SemanticVersion


## Properties

- **[Default][3]**  
  The default way to format a semantic version.
- (Deprecated) **[PrefixedDefault][4]**  
  The default way to format a semantic version, prefixed with a
  letter `v`.
- **[Concise][5]**  
  A way to concisely format a semantic version. Omits metadata and
  only includes the [Patch][6] version if it is non-zero.
- (Deprecated) **[PrefixedConcise][7]**  
  A concise way to format a semantic version, prefixed with a
  letter `v`.
  
[3]: ./Default.md
[4]: ./PrefixedDefault.md
[5]: ./Concise.md
[6]: ../SemanticVersion/Patch.md
[7]: ./PrefixedConcise.md