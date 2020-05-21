# `SemanticVersion.Deconstruct(Int32, Int32, Int32)` method

```c#
public void Deconstruct(
    out int major,
    out int minor,
    out int patch
)
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.4.0  

[1]: ../

Deconstructs the current version.

### Parameters

- **`major`**
  **Type:** `System.Int32`
  The [Major][1] component of the current version.
- **`minor`**
  **Type:** `System.Int32`
  The [Minor][2] version component of the current version.
- **`patch`**
  **Type:** `System.Int32`
  The [Patch][3] version component of the current version

[1]: ./Major.md
[2]: ./Minor.md
[3]: ./Patch.md