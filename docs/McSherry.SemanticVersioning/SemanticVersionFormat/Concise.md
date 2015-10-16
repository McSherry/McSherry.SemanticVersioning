# `SemanticVersionFormat.Concise` property

```c#
public static string Concise => "C"
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ../

**Type:** `System.String`  
A way to concisely format a semantic version. Omits metadata
and only includes the [Patch][2] version if it is non-zero.

[2]: ../SemanticVersion/Patch.md