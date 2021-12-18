using System;
using System.Collections.Generic;
using System.Diagnostics;
using GapBuffer;
using NUnit.Framework;

namespace GapBufferTests;

public class GapBufferPerformanceTests
{
    private const int PerfTestSize = 500_000;

    // Fill 50%, starting 10% in.
    private const int StartOffset = PerfTestSize / 10;
    private const int EndOffset = StartOffset + PerfTestSize / 2;
    private Stopwatch _bufferGapStopwatch = new();
    private Stopwatch _listStopwatch = new();

    [TearDown]
    public void TearDown()
    {
        Console.WriteLine();
        Console.WriteLine($"LIST        Duration: {_listStopwatch.Elapsed}");
        Console.WriteLine($"BUFFER GAP  Duration: {_bufferGapStopwatch.Elapsed}");
    }

    [Test]
    public void FillWithAppend()
    {
        var gb = new GapBuffer<char>();
        var l = new List<char>();

        StartBufferGapStopwatch();
        for (var i = 0; i < PerfTestSize; i++) gb.Add('*');
        StopBufferGapStopwatch();

        StartListStopwatch();
        for (var i = 0; i < PerfTestSize; i++) l.Add('*');
        StopListStopwatch();
    }

    [Test]
    public void FillWithInsertAtStart()
    {
        StartBufferGapStopwatch();
        var gb = new GapBuffer<char>();
        for (var i = 0; i < PerfTestSize; i++) gb.Insert(0, '*');
        StopBufferGapStopwatch();

        StartListStopwatch();
        var l = new List<char>();
        for (var i = 0; i < PerfTestSize; i++) l.Insert(0, '*');
        StopListStopwatch();
    }

    [Test]
    public void LocalisedInserts()
    {
        var gb = StartBufferGapStopwatch();
        for (var i = StartOffset; i < EndOffset; i++) gb.Insert(i, '*');
        StopBufferGapStopwatch();

        var l = StartListStopwatch();
        for (var i = StartOffset; i < EndOffset; i++) l.Insert(i, '*');
        StopListStopwatch();
    }

    [Test]
    public void LocalisedRemoves()
    {
        var gb = StartBufferGapStopwatch();
        for (var i = StartOffset; i < EndOffset; i++) gb.RemoveAt(StartOffset);
        StopBufferGapStopwatch();

        var l = StartListStopwatch();
        for (var i = StartOffset; i < EndOffset; i++) l.RemoveAt(StartOffset);
        StopListStopwatch();
    }

    [Test]
    public void RandomisedLocalisedInsertsAndRemovals()
    {
        // Ensure both list types use the same set of actions.
        var actions = GetActions();

        var gb = StartBufferGapStopwatch();
        foreach (var action in actions)
            if (action.IsInsert) gb.Insert(action.Index, '*');
            else gb.RemoveAt(action.Index);
        StopBufferGapStopwatch();

        var l = StartListStopwatch();
        foreach (var action in actions)
            if (action.IsInsert) l.Insert(action.Index, '*');
            else l.RemoveAt(action.Index);
        StopListStopwatch();
    }

    private static IList<BufferAction> GetActions()
    {
        var r = new Random();
        var actions = new List<BufferAction>();
        var offset = 0;

        for (var i = 0; i < PerfTestSize; i++)
        {
            // Around 80% of the time stay in the same area of the buffer.
            if (r.Next(100) > 80) offset = r.Next(0, PerfTestSize / 2);
            actions.Add(new BufferAction
            {
                Index = offset++,
                IsInsert = r.Next(100) > 50
            });
        }

        return actions;
    }

    private GapBuffer<char> StartBufferGapStopwatch()
    {
        var gb = new GapBuffer<char>();
        for (var i = 0; i < PerfTestSize; i++) gb.Add('*');

        _bufferGapStopwatch = new Stopwatch();
        _bufferGapStopwatch.Start();
        return gb;
    }

    private void StopBufferGapStopwatch()
    {
        _bufferGapStopwatch.Stop();
    }

    private List<char> StartListStopwatch()
    {
        var l = new List<char>();
        for (var i = 0; i < PerfTestSize; i++) l.Add('*');

        _listStopwatch = new Stopwatch();
        _listStopwatch.Start();
        return l;
    }

    private void StopListStopwatch()
    {
        _listStopwatch.Stop();
    }

    private struct BufferAction
    {
        public int Index;
        public bool IsInsert = true;

        public override string ToString()
        {
            return $"{Index}, insert = {IsInsert}, remove = {!IsInsert}";
        }
    }
}