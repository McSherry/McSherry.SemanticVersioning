# `MonotonicVersioner.Chronology` property

```c#
public IEnumerable<SemanticVersion> Chronology { get; }
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

[1]: ../

**Type:** `System.Collections.Generic.IEnumerable<SemanticVersion>`  
The monotonic versions this instance has produced, in chronological order.

## Remarks

For monotonic versions, chronological order means the versions are ordered
by ascending release number.