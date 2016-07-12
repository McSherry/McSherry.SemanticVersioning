# `MonotonicVersioner` constructor

```c#
public MonotonicVersioner(
    bool startAtOne
    )
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

[1]: ../

Creates a new [`MonotonicVersioner`][2] instance with the specified
initial compatibility line.

[2]: ./

### Parameters

- **`startAtOne`**  
  **Type:** `System.Bool`  
  If true, the produced [`Compatibility`][3] number sequence starts
  at one. If false, zero.

[3]: ./Compatibility.md

## Remarks

The Monotonic Versioning Manifesto 1.2 does not specify whether the
[`Compatiblity`][3] component of versions are to start at one or zero.
It is assumed that either is valid as neither is specifically
recommended nor prohibited.

If the [`Compatibility`][3] components are to start at one,
[`MonotonicVersioner()`][4] may be used.

[4]: ./ctor().md