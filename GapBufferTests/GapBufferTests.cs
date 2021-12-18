using GapBuffer;
using NUnit.Framework;

namespace GapBufferTests;

public class GapBufferTests
{
    private int _defaultCapacity;

    [SetUp]
    public void Setup()
    {
        var gb = new GapBuffer<char>();
        _defaultCapacity = gb.Capacity;
    }

    [Test]
    public void New_CreatesWithZeroCount()
    {
        var gb = new GapBuffer<char>();

        Assert.AreEqual(0, gb.Count);
    }

    [Test]
    public void IndexedAccess_FetchesExpectedItems()
    {
        var gb = new GapBuffer<char>();

        gb.Add('A');
        gb.Add('B');
        gb.Add('C');

        Assert.AreEqual('A', gb[0]);
        Assert.AreEqual('B', gb[1]);
        Assert.AreEqual('C', gb[2]);
    }

    [Test]
    public void IndexedAccess_GetBeforeStart_ThrowsBufferAccessException()
    {
        var gb = new GapBuffer<char>();
        gb.Add('A');

        Assert.Throws<BufferAccessException>(() =>
        {
            var result = gb[-1];
        });
    }

    [Test]
    public void IndexedAccess_SetBeforeStart_ThrowsBufferAccessException()
    {
        var gb = new GapBuffer<char>();
        gb.Add('A');

        Assert.Throws<BufferAccessException>(() => { gb[-1] = 'B'; });
    }

    [Test]
    public void IndexedAccess_GetPastEnd_ThrowsBufferAccessException()
    {
        var gb = new GapBuffer<char>();
        gb.Add('A');

        Assert.Throws<BufferAccessException>(() =>
        {
            var result = gb[gb.Count];
        });
    }

    [Test]
    public void IndexedAccess_SetPastEnd_ThrowsBufferAccessException()
    {
        var gb = new GapBuffer<char>();
        gb.Add('A');

        Assert.Throws<BufferAccessException>(() => { gb[gb.Count] = 'B'; });
    }

    [Test]
    public void Append_AddsToTheEnd()
    {
        var gb = new GapBuffer<char>();

        gb.Add('A');
        gb.Add('B');

        Assert.AreEqual(2, gb.Count);
        Assert.AreEqual('A', gb[0]);
        Assert.AreEqual('B', gb[1]);
    }

    [Test]
    public void AppendRange_AddsAllToTheEnd()
    {
        var gb = new GapBuffer<char>();

        gb.AddRange(new[] {'A', 'B'});

        Assert.AreEqual(2, gb.Count);
        Assert.AreEqual('A', gb[0]);
        Assert.AreEqual('B', gb[1]);
    }

    [Test]
    public void Insert_AddsWhereSpecified()
    {
        var gb = new GapBuffer<char>();
        gb.Add('A');
        gb.Add('C');

        gb.Insert(1, 'B');

        Assert.AreEqual(3, gb.Count);
        Assert.AreEqual('A', gb[0]);
        Assert.AreEqual('B', gb[1]);
        Assert.AreEqual('C', gb[2]);
    }

    [Test]
    public void InsertRange_AddsWhereSpecified()
    {
        var gb = new GapBuffer<char>();
        gb.Add('A');
        gb.Add('D');

        gb.InsertRange(1, new[] {'B', 'C'});

        Assert.AreEqual(4, gb.Count);
        Assert.AreEqual('A', gb[0]);
        Assert.AreEqual('B', gb[1]);
        Assert.AreEqual('C', gb[2]);
        Assert.AreEqual('D', gb[3]);
    }

    [Test]
    public void RemoveAt_RemovesWhereSpecified()
    {
        var gb = new GapBuffer<char>();
        gb.Add('A');
        gb.Add('B');
        gb.Add('C');

        gb.RemoveAt(1);

        Assert.AreEqual(2, gb.Count);
        Assert.AreEqual('A', gb[0]);
        Assert.AreEqual('C', gb[1]);
    }

    [Test]
    public void RemoveRange_RemovingMultiple_RemovesWhereSpecified()
    {
        var gb = new GapBuffer<char>();
        gb.Add('A');
        gb.Add('B');
        gb.Add('C');
        gb.Add('D');

        gb.RemoveRange(1, 2);

        Assert.AreEqual(2, gb.Count);
        Assert.AreEqual('A', gb[0]);
        Assert.AreEqual('D', gb[1]);
    }

    [TestCase(-1)]
    [TestCase(0)]
    public void RemoveRange_RemovingLessThanOne_DoesNothing(int removalCount)
    {
        var gb = new GapBuffer<char>();
        gb.Add('A');

        gb.RemoveRange(0, removalCount);

        Assert.AreEqual(1, gb.Count);
        Assert.AreEqual('A', gb[0]);
    }

    [Test]
    public void RemoveRange_RemovingMoreThanAvailable_RemovesWhatItCan()
    {
        var gb = new GapBuffer<char>();
        gb.Add('A');
        gb.Add('B');
        gb.Add('C');
        gb.Add('D');

        gb.RemoveRange(2, 9);

        Assert.AreEqual(2, gb.Count);
        Assert.AreEqual('A', gb[0]);
        Assert.AreEqual('B', gb[1]);
    }

    [Test]
    public void Clear_RemovesEntriesAndResets()
    {
        var gb = new GapBuffer<char>();
        gb.Add('A');
        gb.Add('B');
        gb.Add('C');

        gb.Clear();

        Assert.AreEqual(0, gb.Count);
        Assert.AreEqual(_defaultCapacity, gb.Capacity);
        Assert.AreEqual(0, gb.Count);
    }

    [Test]
    public void SetCapacity_AdjustsAccordingly()
    {
        var gb = new GapBuffer<char>();
        var newCapacity = _defaultCapacity * 2;

        gb.Add('A');
        gb.SetCapacity(newCapacity);
        gb.Add('B');

        Assert.AreEqual(newCapacity, gb.Capacity);
        Assert.AreEqual(2, gb.Count);
    }

    [Test]
    public void SetCapacity_SetTooLowForCurrentContent_ThrowsBufferCapacityException()
    {
        var gb = new GapBuffer<char>();
        for (var i = 0; i < 10; i++) gb.Add((char) (65 + i));

        Assert.Throws<BufferCapacityException>(() => gb.SetCapacity(1));
    }

    [Test]
    public void Overfilling_AutoExpandsCapacity_ToDoubleCurrentCount()
    {
        var gb = new GapBuffer<char>();
        for (var i = 0; i < _defaultCapacity; i++) gb.Add((char) (65 + i));

        // One over the capacity, so should auto-expand to double the current count.
        // Then one more to show that it doesn't need to expand a second time.
        gb.Add('A');
        gb.Add('B');

        Assert.AreEqual(_defaultCapacity + 2, gb.Count);
        Assert.AreEqual((_defaultCapacity + 1) * 2, gb.Capacity);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public void Count_ReturnsCorrectLength(int quantity)
    {
        var gb = new GapBuffer<char>();

        for (var i = 0; i < quantity; i++) gb.Add('*');

        Assert.AreEqual(quantity, gb.Count);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public void Length_ReturnsSameValueAsCount(int quantity)
    {
        var gb = new GapBuffer<char>();

        for (var i = 0; i < quantity; i++) gb.Add('*');

        Assert.AreEqual(gb.Count, gb.Length);
    }

    [Test]
    public void IndexOf_WithEmptyCollection_ReturnsMinusOne()
    {
        var gb = new GapBuffer<char>();

        var foundAt = gb.IndexOf('B');

        Assert.AreEqual(-1, foundAt);
    }

    [Test]
    public void IndexOf_WithNoMatch_ReturnsMinusOne()
    {
        var gb = new GapBuffer<char>();
        gb.AddRange(new[] {'A', 'B', 'C'});

        var foundAt = gb.IndexOf('*');

        Assert.AreEqual(-1, foundAt);
    }

    [Test]
    public void IndexOf_WhenMatched_ReturnsCorrectPosition()
    {
        var gb = new GapBuffer<char>();
        gb.AddRange(new[] {'A', 'B', 'C', 'D'});

        var foundAt = gb.IndexOf('C');

        Assert.AreEqual(2, foundAt);
    }

    [Test]
    public void IndexOf_WithMultipleMatches_ReturnsFirstMatch()
    {
        var gb = new GapBuffer<char>();
        gb.AddRange(new[] {'A', 'B', 'B', 'C'});

        var foundAt = gb.IndexOf('B');

        Assert.AreEqual(1, foundAt);
    }
}