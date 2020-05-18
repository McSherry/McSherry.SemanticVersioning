# `MonotonicVersioner` constructor

```c#
public MonotonicVersioner(
    bool startAtOne,
    IEnumerable<string> metadata
    )
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

[1]: ../

Creates a new [`MonotonicVersioner`][2] instance with the
specified initial compatibility line and metadata.

### Parameters

- **`startAtOne`**  
  **Type:** `System.Bool`  
  If true, the produced [`Compatibility`][3] number sequence
  starts at one. If false, zero.
- **`metadata`**  
  **Type:** `System.Collections.Generic.IEnumerable<string>`  
  Any metadata items to be included as part of the initial
  version number.

## Exceptions

- **`System.ArgumentNullException`**  
  _`metadata`_ or any item thereof is null.

- **`System.ArgumentException`**  
  One or more of the items in _`metadata`_ is not a valid
  metadata item.

## Remarks

The Monotonic Versioning Manifesto 1.2 does not specify whether the
[`Compatiblity`][3] component of versions are to start at one or zero.
It is assumed that either is valid as neither is specifically
recommended nor prohibited.

[2]: ./
[3]: ./Compatibility.md