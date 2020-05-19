# `SemanticVersion.ToString(String, IFormatProvider)` method

```c#
string IFormattable.ToString(
    string format,
    IFormatProvider provider
)
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum version:** 1.0.0  
**Implements:** `System.IFormattable.ToString`

[1]: ../

Formats the value of the current [SemanticVersion][2] as
specified.

[2]: ./


### Parameters

- **`format`**  
  **Type:** `System.String`  
  The format to use, or null for the default format.
- **`provider`**  
  **Type:** `System.IFormatProvider`  
  The format provider to use, or null for the default
  provider. This parameter is ignored.
  
  
### Return Value

**Type:** `System.String`

A string representation of the current [SemanticVersion][2],
formatted as specified.


## Exceptions

- **`System.FormatException`**  
  Thrown when the format specifier given in _`format`_ is not
  recognised or is invalid.
  
  
## Remarks

The format of a Semantic Version is not dependent on culture information, and so the value of _`provider`_ is ignored.

The parameter _`format`_ should be a string containing a custom format pattern as set out below. If it is `null` or an empty string, the general format specifier `G` is used. In _`format`_, any character which is not a format specifier (including whitespace) is included verbatim in the resulting formatted string.

The formatter will ignore any characters placed between double braces. For example, `G` will be interpreted as the general format specifier while `{{G}}` will be included as the literal character "G".

### Standard format specifiers

The following single-character standard format specifiers can be used as shorthand for longer custom format patterns. In formatting a Semantic Version, they are expanded in place to the custom format patterns given in the table.

| Specifier | Expansion    | Description                                                  | Example                            |
| --------- | ------------ | ------------------------------------------------------------ | ---------------------------------- |
| `G`       | `M.m.pRRDD`  | The general format specifier. Includes all version components and, if present, all pre-release identifiers and metadata items. | `1.7.0-alpha.2+20150925.f8f2cb1a`  |
| `g`       | `vM.m.pRRDD` | (Deprecated) The prefixed general format specifier. Identical to the general format specifier, except prefixed with `v`. | `v1.7.0-alpha.2+20150925.f8f2cb1a` |
| `C`       | `M.mppRR`    | The concise format specifier. Only includes the patch component if it is non-zero and never includes metadata items. | `1.7-alpha.2`, `2.0.3-rc.1`        |
| `c`       | `vM.mppRR`   | (Deprecated) The prefixed concise format specifier. Identical to the concise format specifier, except prefixed with `v`. | `v1.7-alpha.2`, `v2.0.3-rc.1`      |

### Custom format specifiers

The custom format specifiers shown in the table below allow inserting individual parts of a Semantic Version into the string.

| Format specifier | Description                                                  | Example                |
| ---------------- | ------------------------------------------------------------ | ---------------------- |
| `M`              | Major version. Includes the major version component.         | `1`                    |
| `m`              | Minor version. Includes the minor version component.         | `7`                    |
| `p`              | Patch version. Includes the patch version component.         | `0`                    |
| `pp`             | Optional patch version. Includes the patch version component with a separator character only if the component is non-zero. Otherwise includes nothing. | `.3`                   |
| `R`              | Standalone optional pre-release identifier group. If the version has pre-release identifiers, includes all identifiers with separator characters. Otherwise includes nothing. | `alpha.2`              |
| `RR`             | Prefixed optional pre-release identifier group. If the version has pre-release identifiers, includes all identifiers prefixed with a hyphen and with separator characters. Otherwise includes nothing. | `-alpha.2`             |
| `r0`, `r1`       | Indexed pre-release identifier. Includes the pre-release identifier with the specified zero-based index. An out-of-bounds index produces an error. | `alpha`, `2`           |
| `rr`             | Reserved. Use produces an error.                             | N/A                    |
| `D`              | Standalone optional metadata item group. If the version has metadata items, includes all items with separator characters. Otherwise includes nothing. | `20150925.f8f2cb1a`    |
| `DD`             | Prefixed optional metadata item group. If the version has metadata items, includes all items prefixed with a plus sign and with separator characters. Otherwise includes nothing. | `+20150925.f8f2cb1a`   |
| `d0`, `d1`       | Indexed metadata item. Includes the metadata item with the specified zero-based index. An out-of-bounds index produces an error. | `20150925`, `f8f2cb1a` |
| `dd`             | Reserved. Use produces an error.                             | N/A                    |

In future, new meaning may be given to repetitions or different cases of the specifiers given above (such as `r`,  `P`, or `PP`). These specifiers should not be used but will not produce an error.