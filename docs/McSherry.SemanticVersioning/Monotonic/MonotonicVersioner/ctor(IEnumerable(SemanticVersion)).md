# `MonotonicVersioner` constructor

```c#
public MonotonicVersioner(
    IEnumerable<SemanticVersion> chronology
    )
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

[1]: ../

Creates a new [`MonotonicVersioner`][2] with the specified
version number history.

### Parameters

- **`chronology`**  
  **Type:** `System.Collections.Generic.IEnumerable<SemanticVersion>`  
  A collection of version numbers providing the version history to
  use for this instance.

## Exceptions

- **`System.ArgumentNullException`**  
  _`chronology`_ or an item thereof is null.

- **`System.ArgumentOutOfRangeException`**  
  _`chronology`_ contains a version which is not a
  valid monotonic version.

- **`System.ArgumentException`**  
  _`chronology`_ provides an incomplete version history.
  The chronology may:  
    - Not provide a contiguous sequence of [`Compatibility`][3] numbers;
    - Not provide a contiguous sequence of [`Release`][4] numbers;
    - Not contain a [`Compatibility`][3] starting at either zero or one;
    - Be empty.