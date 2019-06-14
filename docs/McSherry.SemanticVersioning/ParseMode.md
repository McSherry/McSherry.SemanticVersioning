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
  The opposite of `Strict`, with all parser flags set but `Greedy`.
  
- **`AllowPrefix`**  
  The parser will accept a version prefixed with `v` or `V`.
  
- **`OptionalPatch`**  
  The parser will accept versions with the [SemanticVersion.Patch][3]
  version component omitted.
  
- **`Greedy`**  
  The parser will, if it encounters an error, attempt to return a
  valid [SemanticVersion][2] instance instead of an error.
  
  **Remarks:** The effect of other `ParseMode`s must be considered when
  specifying `Greedy`. For example, `1.2` will produce the expected result
  with both `Greedy` and `OptionalPatch`, but `v1.2` with `Greedy` will
  result in failure unless `AllowPrefix` is also specified.
[3]: ./SemanticVersion/Patch.md