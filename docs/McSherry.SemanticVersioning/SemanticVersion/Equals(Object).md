# `SemanticVersion.Equals(Object)` method

```c#
public bool Equals(
    object obj
)
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0  
**Implements:** `System.Object.Equals(Object)`

[1]: ../

Determines whether the specified object is equal to the current
object.


### Parameters

- **`obj`**  
  **Type:** `System.Object`  
  The object to compare with the current object.
  
  
### Return Value

**Type:** `System.Boolean`

True if the specified and current objects are equal, False
if otherwise.


## Remarks

This method takes [build metadata items][2] into account when
comparing, and so may return false for equivalent versions with
differing build metadata.

[2]: ./Metadata.md