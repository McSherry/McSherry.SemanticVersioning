# `MonotonicVersioner.Next(MonotonicChange)` method

```c#
public SemanticVersion Next(
    MonotonicChange change
    )
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

[1]: ../

Returns the next version number when a specified change is made to
the latest version.

### Parameters

- **`change`**  
  **Type:** [`McSherry.SemanticVersioning.Monotonic.MonotonicChange`][2]  
  The type of change being made to the latest version.

[2]: ../MonotonicChange.md

### Return Value

**Type:** [`McSherry.SemanticVersioning.SemanticVersion`][3]

The next version number produced when the specified change is made to the
latest version.

If _`change`_ is equal to [`MonotonicChange.Compatible`][2], the release
number is incremented by the compatibility number remains the same.

If _`change`_ is equal to [`MonotonicChange.Breaking`][2], both the release
and compatibility numbers are incremented.

[3]: ../../SemanticVersion

## Exceptions

- **`System.ArgumentOutOfRangeException`**  
  _`change`_ is not a recognised type of change.