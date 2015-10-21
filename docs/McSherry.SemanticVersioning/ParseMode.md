# `ParseMode` enumeration

```c#
[Flags]
[CLSCompliant(true)]
public enum ParseMode : int
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0

[1]: ./

Represents the parsing modes that the [SemanticVersion][2] parser
may be configured to use.

[2]: ./SemanticVersion


### Members

- **`Strict`**  
  The default parser behaviour, with no set flags. This forces
  specification compliance.
  
- **`Lenient`**  
  The opposite of `Strict`, with all parser flags set.
  
- **`AllowPrefix`**  
  The parser will accept a version prefixed with `v` or `V`.
  
- **`OptionalPatch`**  
  The parser will accept versions with the [SemanticVersion.Patch][3]
  version component omitted.
  
[3]: ./SemanticVersion/Patch.md