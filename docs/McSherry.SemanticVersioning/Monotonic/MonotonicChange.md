# `MonotonicChange` enumeration

```c#
[CLSCompliant(true)]
public enum MonotonicChange
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

[1]: ./

The types of change which may be made to a monotonic-versioned
software package.

### Members

- **`Compatible`**  
  A backwards-compatible change, where the change would not break
  existing uses of the software package's API.

- **`Breaking`**  
  A breaking change, where the change will break existing uses of
  the software package's API.