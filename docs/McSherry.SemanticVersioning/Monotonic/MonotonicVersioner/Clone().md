# `MonotonicVersioner.Clone()` method

```c#
public MonotonicVersioner Clone()
```

**Namespace:** [`McSherry.SemanticVersioning.Monotonic`][1]  
**Minimum version:** 1.2.0

Returns a [MonotonicVersioner][2] with an identical chronology,
but which can advance its versions separately.

[1]: ../
[2]: ./

### Return Value

A [MonotonicVersioner][2] with an identical chronology up to the
moment this method was called, but which is able to separately
advance its version numbers.