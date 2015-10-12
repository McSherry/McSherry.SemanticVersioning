# `SemanticVersion.Minor` property

```c#
public int Minor { get; }
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ../

**Type:** `System.In32`  
The semantic version's minor version.


## Remarks

This version component is incremented each time a version with at
least one new feature in it is released. This component is reset
to zero with every [Major][2] version increment.

[2]: ./Major.md