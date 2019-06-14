# `VersionRange.MemoizationAgent` property

```c#
public IDictionary<SemanticVersion, bool> MemoizationAgent { get; set; }
```

**Namespace:** [McSherry.SemanticVersioning.Ranges][1]  
**Minimum version:** 1.3.0

[1]: ../

**Type:** `System.Collections.Generic<SemanticVersion, bool>`  
The cache used to memoize results of [VersionRange][2] satisfaction methods.

[2]: ./


## Remarks

Assign `null` to disable memoization. The value of this property is `null` by default.

Accesses by [VersionRange][2] are surrounded by `lock (SynchronizationObject)`.