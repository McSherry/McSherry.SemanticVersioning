# `SemanticVersion.Major` property

```c#
public int Major { get; }
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ../

**Type:** `System.Int32`  
The semantic version's major version.


## Remarks

This version component is incremented each time a version with a
breaking change in it is released. If this is zero, then the version
number represents an unstable, pre-release version that may have
breaking changes made at any time without an increment.