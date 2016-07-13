# `SemanticVersion.MemoizationAgent` property

```c#
public static IDictionary<string, SemanticVersion> MemoizationAgent { get; set; }
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum Version:** 1.2.0

[1]: ../

**Type:** `System.Collections.Generic.IDictionary<string, SemanticVersion>`  
The cache to use to memoize the results of the [`SemanticVersion`][2]
parsing methods.

[2]: ./

## Remarks

Assign `null` to disable memoization. The value of this property is
`null` by default.

Internal accesses by the parser to the memoization agent are
surrounded by `lock (MemoizationAgent)`.