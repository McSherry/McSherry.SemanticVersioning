# `MonotonicComparer.Compare(SemanticVersion, SemanticVersion)` method

```c#
public int Compare(
    SemanticVersion x,
    SemanticVersion y
)
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0  
**Implements:** `System.Collections.Generic.IComparer<SemanticVersion>.Compare`

[1]: ../

Compares two monotonic versions and returns an indication of
their relative order.

### Parameters

- **`x`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The monotonic version to compare to _`y`_.
- **`y`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The monotonic version to compare to _`x`_.

[2]: ../../SemanticVersion

### Return Value

**Type:** `System.Int32`

A value less than zero if _`x`_ precedes _`y`_ in sort order, or if
_`x`_ is null and _`y`_ is not null.

Zero if _`x`_ is equal to _`y`_, including if both are null.

A value greater than zero if _`x`_ follows _`y`_ in sort order, or if
_`y`_ is null and _`x`_ is not null.

## Exceptions

- **`System.ArgumentException`**  
  _`x`_ or _`y`_ is not a valid monotonic version.