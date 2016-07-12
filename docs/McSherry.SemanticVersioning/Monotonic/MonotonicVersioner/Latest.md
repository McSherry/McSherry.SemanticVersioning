# `MonotonicVersioner.Latest` property

```c#
public SemanticVersion Latest { get; }
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

**Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
The chronologically-latest verson number in this versioning sequence.

[1]: ../
[2]: ../../SemanticVersion

## Remarks

The chronologically-latest version is the version with the greatest value as
its release component, regardless of the line of compatibility.