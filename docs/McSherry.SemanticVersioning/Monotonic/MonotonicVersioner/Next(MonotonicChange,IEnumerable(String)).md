# `MonotonicVersioner.Next(MonotonicChange, IEnumerable<string>)` method

```c#
public SemanticVersion Next(
    MonotonicChange change,
    IEnumerable<string> metadata
    )
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

[1]: ../

Returns the next version number when a specified change is made to the
latest version.

### Parameters

- **`change`**  
  **Type:** [`McSherry.SemanticVersioning.Monotonic.MonotonicChange`][2]  
  The type of change being made to the latest version.
  
- **`metadata`**  
  **Type:** `System.Collections.Generic.IEnumerable<string>`  
  The metadata to be included with the new version.

[2]: ../MonotonicChange.md

### Return Value

The next version number produced when the specified change is made to the
latest version, with the specified metadata.

If _`change`_ is equal to [`MonotonicChange.Compatible`][2], the release
number is incremented by the compatibility number remains the same.

If _`change`_ is equal to [`MonotonicChange.Breaking`][2], both the release
and compatibility numbers are incremented.

## Exceptions

- **`System.ArgumentOutOfRangeException`**  
  _`change`_ is not a recognised type of change.
- **`System.ArgumentNullException`**  
  _`metadata`_ or an item therein is null.
- **`System.ArgumentException`**  
  One or more items within _`metadata`_ is not a valid metadata string.