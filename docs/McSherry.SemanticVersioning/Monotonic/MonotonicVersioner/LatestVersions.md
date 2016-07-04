# `MonotonicVersioner.LatestVersions` property

```c#
public IReadOnlyDictionary<int, SemanticVersion> LatestVersions { get; }
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

**Type:** `System.Collections.Generic.IReadOnlyDictionary<int, SemanticVersion>`  
The latest versions in each line of compatibility, where the key is the line
of compatibility.

[1]: ../