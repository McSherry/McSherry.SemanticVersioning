# `SemanticVersion.Patch` property

```c#
public int Patch { get; }
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ../

**Type:** `System.In32`  
The semantic version's patch version.


## Remarks

This version component is incremented each time a version with a
backwards-compatible bug fix is released. This component is reset
to zero with every [Major][2] or [Minor][3] version increment.

[2]: ./Major.md
[3]: ./Minor.md