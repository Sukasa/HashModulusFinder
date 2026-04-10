using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable AccessToModifiedClosure - Closure not modified during parallel execution

var Labels = args.Length == 0 ? Console.In.ReadToEnd().Split().Where(x => !string.IsNullOrWhiteSpace(x)).ToList() : args.ToList();
var Hashes = Labels.Select(ComputeHash).ToList();


var mod_1 = 0;
var mod_2 = Hashes.Count - 1;
var count = Hashes.Count;

// Do some basic input validation; first check that we have at least two hashes and then make sure none of the hashes match
if (count < 2)
{
    Console.Error.WriteLine("Must specify at least two hash values");
    return -1;
}

for (var x = 0; x < count - 1; x++)
for (var y = x + 1; y < count; y++)
    if (Hashes[y] == Hashes[x])
    {
        Console.Error.WriteLine("Two or more of the supplied hashes are identical and cannot be disambiguated");
        return -1;
    }

Console.Out.WriteLine($"Searching for double-modulus pair for {Hashes.Count}-hash table index conversion");
var TimeTaken = Stopwatch.StartNew();
more_padding:
mod_2++;


Parallel.ForEach(Partitioner.Create(mod_2, int.MaxValue), range =>
{
    Span<int> cache = stackalloc int[count];
    var j = range.Item2;
    
    for (var i = range.Item1; mod_1 == 0 && i < j; i++)
    {
        if ((i & 0xFFFFFF) == 0)
            Console.Out.Write('>');

        cache[0] = mod(Hashes[0], i) % mod_2;

        for (var x = 1; x < count; x++)
        {
            cache[x] = mod(Hashes[x], i) % mod_2;
            for (var y = x - 1; y >= 0; y--)
                if (cache[x] == cache[y])
                    goto next_iter;
        }

        mod_1 = i;
        next_iter: ;
    }
});

if (mod_1 == 0)
{
    Console.Out.WriteLine();
    Console.Out.WriteLine($"No modulus found for table size {mod_2}, adding sparsity.");
    goto more_padding;
}

TimeTaken.Stop();

Console.Out.WriteLine();
Console.Out.WriteLine($"Smallest double-modulus to ensure unique table indices for {Hashes.Count} hashes: {mod_1}");
Console.Out.WriteLine($"Fit into data table length set by 2nd modulus {mod_2}");
Console.Out.WriteLine($"Took {TimeTaken.ElapsedMilliseconds} ms");
Console.Out.WriteLine();

for (var i = 0; i < Hashes.Count; i++)
    Console.Out.WriteLine($"{Labels[i]} = {mod(Hashes[i], mod_1) % mod_2}");

return 0;


// Actual modulus, not remainder, branchless

[MethodImpl(MethodImplOptions.AggressiveInlining)]
int mod(int k, int n) => k % n + (int)(((uint)k & 0x80000000U) >> 31) * n;

int ComputeHash(string Input) => int.TryParse(Input, out var val) ? val : unchecked((int)System.IO.Hashing.Crc32.HashToUInt32(Encoding.ASCII.GetBytes(Input)));