# `MonotonicVersioner.Next(Int32, MonotonicChange)` method

```c#
public SemanticVersion Next(
    int line,
    MonotonicChange change
    )
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

[1]: ../

Returns the next version number when a specified change is made to a given
line of compatibility.

### Parameters

- **`line`**  
  **Type:** `System.Int32`  
  The line of compatibility to which the change is being made.
- **`change`**  
  **Type:** [`McSherry.SemanticVersioning.Monotonic.MonotonicChange`][2]  
  The type of change being made to _`line`_.

[2]: ../MonotonicChange.md

### Return Value

**Type:** [`McSherry.SemanticVersioning.SemanticVersion`][3]

The next version number produced when the specified change is made to the
specified line of compatibility.

If _`change`_ is equal to [`MonotonicChange.Compatible`][2], the release
number is incremented but the compatibility number remains the same.

If _`change`_ is equal to [`MonotonicChange.Breaking`][2], both the release
and compatibility numbers are incremented.

## Exceptions

- **`System.ArgumentOutOfRangeException`**  
  _`line`_ is negative.  
  _`change`_ is not a recognised type of change.
  
- **`System.ArgumentException`**  
  _`line`_ is not a current line of compatibility.