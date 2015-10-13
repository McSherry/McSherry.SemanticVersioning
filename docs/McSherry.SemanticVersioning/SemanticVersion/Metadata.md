# `SemanticVersion.Metadata` property

```c#
public IReadOnlyList<string> Metadata { get; }
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ../

**Type:** `System.Collections.Generic.IReadOnlyList<string>`  
The build metadata components of the semantic version.


## Remarks

Build metadata components provide additional information about
a release, such as the time and date it was built and the commit
identifier of the commit the release was built from.