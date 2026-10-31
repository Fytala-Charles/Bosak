// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 28 October 2026
// PURPOSE              : Perform FULLY-NORMALIZED conversion using a coherent pinned Unicode 17 backend.
// SPECIAL NOTES        : Part of the standard XPath / XQuery function library.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 28-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Buffers;
using System.Text;

namespace Bosak.XPath.Standard.Functions;

/// <summary>Performs FULLY-NORMALIZED conversion with pinned Unicode 17 canonical data.</summary>
internal static class FullNormalizationEngine
{
    private const int SBase = 0xAC00, LBase = 0x1100, VBase = 0x1161, TBase = 0x11A7;
    private const int LCount = 19, VCount = 21, TCount = 28, NCount = VCount * TCount, SCount = LCount * NCount;
    private static readonly Data Tables = new();

    internal static string Normalize(string input)
    {
        if (input.Length == 0) return string.Empty;
        if (Rune.DecodeFromUtf16(input.AsSpan(), out var first, out _) != OperationStatus.Done)
            throw new ArgumentException("Invalid Unicode scalar sequence.", nameof(input));
        return NormalizeNfc(IsComposing(first.Value) ? " " + input : input);
    }

    internal static bool IsComposing(int scalar) =>
        Tables.CombiningClass(scalar) != 0 || Tables.CompositionSeconds.Contains(scalar) ||
        (uint)(scalar - VBase) < VCount || (uint)(scalar - TBase - 1) < TCount - 1;

    internal static string NormalizeNfc(string input)
    {
        if (input.Length == 0) return string.Empty;
        var values = new List<Scalar>(input.Length);
        var remaining = input.AsSpan();
        while (!remaining.IsEmpty)
        {
            if (Rune.DecodeFromUtf16(remaining, out var rune, out int consumed) != OperationStatus.Done)
                throw new ArgumentException("Invalid Unicode scalar sequence.", nameof(input));
            Decompose(rune.Value, values);
            remaining = remaining[consumed..];
        }

        // Stable ordering within each non-starter run. The ordinal tie-break avoids
        // changing equal-class order and bounds pathological mark runs to O(n log n).
        int start = 0;
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i].Class != 0) continue;
            Order(values, start, i - start);
            start = i + 1;
        }
        Order(values, start, values.Count - start);

        int write = 0, starter = -1;
        byte previousClass = 0;
        for (int i = 0; i < values.Count; i++)
        {
            var current = values[i];
            if (starter >= 0 && (previousClass == 0 || previousClass < current.Class) &&
                TryCompose(values[starter].Value, current.Value, out int composite))
            {
                values[starter] = new Scalar(composite, 0, values[starter].Ordinal);
                continue;
            }
            if (current.Class == 0) starter = write;
            values[write++] = current;
            previousClass = current.Class;
        }

        var output = new StringBuilder(input.Length);
        Span<char> buffer = stackalloc char[2];
        for (int i = 0; i < write; i++)
        {
            int length = new Rune(values[i].Value).EncodeToUtf16(buffer);
            output.Append(buffer[..length]);
        }
        return output.ToString();
    }

    private static void Order(List<Scalar> values, int start, int length)
    {
        if (length > 1) values.Sort(start, length, ScalarOrder.Instance);
    }

    private static void Decompose(int scalar, List<Scalar> output)
    {
        int index = scalar - SBase;
        if ((uint)index < SCount)
        {
            Add(LBase + index / NCount, output);
            Add(VBase + index % NCount / TCount, output);
            if (index % TCount != 0) Add(TBase + index % TCount, output);
        }
        else if (Tables.Decompositions.TryGetValue(scalar, out var mapping))
        {
            foreach (int item in mapping) Decompose(item, output);
        }
        else Add(scalar, output);
    }

    private static void Add(int scalar, List<Scalar> output) =>
        output.Add(new Scalar(scalar, Tables.CombiningClass(scalar), output.Count));

    private static bool TryCompose(int first, int second, out int composite)
    {
        int l = first - LBase, v = second - VBase;
        if ((uint)l < LCount && (uint)v < VCount)
        {
            composite = SBase + (l * VCount + v) * TCount;
            return true;
        }
        int s = first - SBase, t = second - TBase;
        if ((uint)s < SCount && s % TCount == 0 && t > 0 && t < TCount)
        {
            composite = first + t;
            return true;
        }
        return Tables.Compositions.TryGetValue(Key(first, second), out composite);
    }

    private static ulong Key(int first, int second) => ((ulong)(uint)first << 21) | (uint)second;
    private readonly record struct Scalar(int Value, byte Class, int Ordinal);

    private sealed class ScalarOrder : IComparer<Scalar>
    {
        internal static readonly ScalarOrder Instance = new();
        /// <summary>Orders canonical classes while preserving equal-class source order.</summary>
        /// <param name="x">The first scalar.</param>
        /// <param name="y">The second scalar.</param>
        /// <returns>The canonical ordering comparison.</returns>
        public int Compare(Scalar x, Scalar y)
        {
            int order = x.Class.CompareTo(y.Class);
            return order != 0 ? order : x.Ordinal.CompareTo(y.Ordinal);
        }
    }

    private sealed class Data
    {
        private readonly Dictionary<int, byte> _classes = new();
        internal readonly Dictionary<int, int[]> Decompositions = new();
        internal readonly Dictionary<ulong, int> Compositions = new();
        internal readonly HashSet<int> CompositionSeconds = new();

        internal Data()
        {
            using var stream = typeof(FullNormalizationEngine).Assembly.GetManifestResourceStream(
                "Bosak.XPath.Standard.Resources.UnicodeNormalization17.bin")
                ?? throw new InvalidOperationException("Missing Unicode 17 normalization data.");
            using var reader = new BinaryReader(stream);
            if (!reader.ReadBytes(8).AsSpan().SequenceEqual("BUN17\0\0\0"u8))
                throw new InvalidOperationException("Invalid Unicode 17 normalization data.");
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++) _classes.Add(reader.ReadInt32(), reader.ReadByte());
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int scalar = reader.ReadInt32(), length = reader.ReadByte();
                var mapping = new int[length];
                for (int j = 0; j < length; j++) mapping[j] = reader.ReadInt32();
                Decompositions.Add(scalar, mapping);
            }
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int first = reader.ReadInt32(), second = reader.ReadInt32(), composite = reader.ReadInt32();
                Compositions.Add(Key(first, second), composite);
                CompositionSeconds.Add(second);
            }
            if (stream.Position != stream.Length)
                throw new InvalidOperationException("Trailing Unicode 17 normalization data.");
        }

        internal byte CombiningClass(int scalar) => _classes.GetValueOrDefault(scalar);
    }
}
