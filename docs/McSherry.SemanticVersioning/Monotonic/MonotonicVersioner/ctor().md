# `MonotonicVersioner` constructor

```c#
public MonotonicVersioner()
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

Creates a new [`MonotonicVersioner`][2] instance.

[1]: ../
[2]: ./

## Remarks

The [`Compatibility`][3] number sequence produced by an instance which
was created using this constructor starts at one. If a zero-based
sequence is required, use [`MonotonicVersioner(bool)`][4].

[3]: ./Compatibility.md
[4]: ./ctor(bool).md