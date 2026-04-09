using System.Diagnostics;
using System.Text;

// ReSharper disable AccessToModifiedClosure - Closure not modified during parallel execution

var Labels = args.Length == 0 ? Console.In.ReadToEnd().Split().Where(x => !string.IsNullOrWhiteSpace(x)).ToList() : args.ToList();
var Hashes = Labels.Select(ComputeHash).ToList();

var mod_1 = 0;
var mod_2 = Hashes.Count - 1;
var count = Hashes.Count;

Console.Out.WriteLine($"Searching for double-modulus pair for {Hashes.Count}-hash table index conversion");
var TimeTaken = Stopwatch.StartNew();
more_padding:
mod_2++;

Parallel.For(mod_2, int.MaxValue, (i, ct) =>
{
    Span<int> cache = stackalloc int[count];
    
    if ((i & 0xFFFFFF) == 0)
        Console.Out.Write('>');

    cache[0] = mod(Hashes[0], i) % mod_2;
    
    for (var x = 1; x < count; x++)
    {
        cache[x] = mod(Hashes[x], i) % mod_2;
        for (var y = x - 1; y >= 0; y--)
            if (cache[x] == cache[y])
                return;
    }

    mod_1 = i;
    ct.Break(); // We found a solution - end search after saving the answer
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
return;


// Actual modulus, not remainder, branchless
int mod(int k, int n) => k % n + (int)(((uint)k & 0x80000000U) >> 31) * n;

int ComputeHash(string Input) => unchecked((int)System.IO.Hashing.Crc32.HashToUInt32(Encoding.ASCII.GetBytes(Input)));
