# `GapBuffer<T>`

Implementation of a generic GapBuffer.

This is a collection type that maintains a gap within the backing storage. By keeping the gap located at the last point
edited, future localised changes require less moving around of existing data than with a simple linear store. In
essence, for ideal operations the gap allows for focused insertions/deletions that operate purely within the scope of
that gap, requiring no shuffling of the rest of the content. For non-ideal operations there is minimal extra overhead.

This is particularly relevant with large data files or text collections being repeatedly edited around the same focal
point. A good example would be a text editor or word processor with a `GapBuffer<char>`. If your collection operations
are largely either read-only or appends, then an alternative collection type would likely be more suitable.

## Contents

- [Status](#Status)
- [Example usage](#example-usage)
- [Performance](#performance)

## Status

- Working
- Tested
    - Unit tests
    - Performance tests

Consider this *beta* in that the tests pass and it is working in existing projects, but it has not been running long
enough to catch all the edge cases.

## Example usage

``` c#
using GapBuffer;

var buffer = new GapBuffer<string>();
buffer.Append("Hello");
buffer.Append("World");
buffer.Insert(1, "Cruel");

for (var i = 0; i < 3; i++)
{
    Console.WriteLine(buffer[i]);
}

// Hello
// Cruel
// World
```

## Performance

These are the results of the performance tests in the unit test project. The number of iterations were based on
a `PerfTestSize` of `500,000`. Timings are in seconds.

```
TEST                                    LIST        BUFFER GAP       DIFFERENCE
===============================================================================
FillWithAppend                          0.0019592   0.0108110       5.5x slower
FillWithInsertAtStart                   5.5352715   0.0337876     163.8x faster
LocalisedInserts                        4.7664272   0.0030817   1,546.7x faster
LocalisedRemoves                        3.4372988   0.0023032   1,492.4x faster
RandomisedLocalisedInsertsAndRemovals   7.6194859   0.6023950      12.7x faster
===============================================================================
```

- `FillWithAppend` does `PerfTestSize` inserts, one at each position from 0 to the end. This means the amount of
  shuffling needed decreases as the insert index increases.
- `FillWithInsertAtStart` does `PerfTestSize` inserts, all at position 0, meaning all other items are shuffled each time
  through.
- `LocalisedInserts` does `PerfTestSize / 2` inserts, one at each position from `PerfTestSize / 10` onwards, analogous
  to a large amount of continuous typing in a text editor.
- `LocalisedRemoves` does `PerfTestSize / 2` removals, all at position `PerfTestSize / 10`, analogous to holding down
  the delete key in a text editor.
- `RandomisedLocalisedInsertsAndRemovals` pre-prepares `PerfTestSize` actions (to ensure both timings do the exact same
  steps). Those actions are localised (clumped) random choices between insert and remove, where the position shifts
  randomly approximately 20% of the time. This is analogous to jumping around in a text editor and doing clumps of
  consecutive inserts and/or removals.

*These were done on a MacBook Air M1, 7 core, 8GB with DotNet 6. Not that it matters, as it is the comparative results
that count.*
