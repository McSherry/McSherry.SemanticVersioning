# `MonotonicVersioner` class

```c#
[CLSCompliant(true)]
public sealed class MonotonicVersioner
```

**Namespace:** [McSherry.SemanticVersioning.Monotonic][1]  
**Minimum Version:** 1.2.0

Provides a method of working with [SemanticVersion][2]s as monotonic versions.

[1]: ../
[2]: ../../SemanticVersion

## Constructors

- **[MonotonicVersioner()][3]**  
  Creates a new [MonotonicVersioner][4] instance.

- **[MonotonicVersioner(bool)][5]**  
  Creates a new [MonotonicVersioner][4] instance with the specified initial
  compatibility line.

- **[MonotonicVersioner(bool, IEnumerable(String))][6]**  
  Creates a new [MonotonicVersioner][4] instance with the specified initial
  compatibility line and metadata.

- **[MonotonicVersioner(IEnumerable(SemanticVersion))][7]**  
  Creates a new [MonotonicVersioner][4] with the specified version number
  history.

[3]: ./ctor().md
[4]: ./
[5]: ./ctor(bool).md
[6]: ./ctor(bool,IEnumerable(String)).md
[7]: ./ctor(IEnumerable(SemanticVersion)).md

## Properties

- **[Latest][8]**  
  The chronologically-latest version number in this versioning sequence.
- **[LatestVersions][9]**  
  The latest versions in each line of compatibility, where the key is the
  line of compatibility.
- **[Compatibility][10]**  
  The highest compatibility number. This component indicates which releases
  are compatible with each other.
- **[Release][11]**  
  The current release number. This component indicates when a release was
  made relative to other releases.
- **[Chronology][12]**  
  The monotonic versions this instance has produced, in chronological order.

[8]:  ./Latest.md
[9]:  ./LatestVersions.md
[10]: ./Compatibility.md
[11]: ./Release.md
[12]: ./Chronology.md

## Methods

- **[Next(MonotonicChange)][13]**  
  Returns the next version number when a specified change is made to the latest
  version.
- **[Next(MonotonicChange, IEnumerable(String))][14]**  
  Returns the next version number when a specified change is made to the latest
  version.
- **[Next(Int32, MonotonicChange)][15]**  
  Returns the next version number when a specified change is made to a given line
  of compatibility.
- **[Next(Int32, MonotonicChange, IEnumerable(String))][16]**  
  Returns the next version number when a specified change is made to a given line
  of compatibility.
- **[Clone()][17]**  
  Returns a [MonotonicVersioner][1] with an identical chronology, but which can
  advcance its versions separately.

[13]: ./Next(MonotonicChange).md
[14]: ./Next(MonotonicChange,IEnumerable(String)).md
[15]: ./Next(Int32,MonotonicChange).md
[16]: ./Next(Int32,MonotonicChange,IEnumerable(String)).md
[17]: ./Clone().md

## Remarks

Monotonic Versioning is a simplified versioning scheme that is compatible with
Semantic Versioning 2.0.0. The scheme uses two components: compatibility, indicating
a "line of compatibility" where all versions with the same component are compatible;
and release, which is incremented with every release, regardless of the line of
compatibility.

For example, a first release would be "1.0". A backwards-compatible update to this
would be "1.1". If a breaking change was made, that release would be "2.2". However,
if the first line of compatibility was updated again, it would be "1.3".

The full manifesto is available from the Applied Computer Science Lab website. This
class is based on the 1.2 manifesto.