# `MonotonicComparer` class

```c#
[CLSCompliant(true)]
public sealed class MonotonicComparer : IComparer<SemanticVersion>
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

Represents a semantic version comparison operation using monotonic
versioning comparison rules.

[1]: ../


### Constructors

None public.

### Methods

- **[Compare(SemanticVersion, SemanticVersion)][2]**  
  Compares two monotonic versions and returns an indication of
  their relative order.
  
### Static Properties

- **[Standard][3]**  
  A `MonotonicComparer` which compares using the standard rules
  for monotonic versions.

[2]: ./Compare(SemanticVersion,SemanticVersion).md
[3]: ./Standard.md

## Remarks

This class compares monotonic versions as set out by the Monotonic
Versioning Manifesto 1.2.