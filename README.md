# Hash Modulus Finder

This is a small utility, meant for Stationeers IC10 programmers, in some unique situations.

## Use Case

Say you have a list of PrefabHashes, that you want to be able to use as entries in a dictionary.  A Hash Map, if you will.
Normally, being that these are CRC32 hashes IC10 doesn't have a great way of converting these into something you can use as a table index.
However, there is a way: If you know ahead of time what hashes you need to sort, you can use a simple modulus operation to get unique values to use as table indices, however the table is often very sparse.
Taking this a step further, if you do this *twice*, you can oftentimes shrink the table to a very, very low degree of sparseness.

### Example

How is this useful?  Perhaps you want to sum up the quantity of all the smeltable ingots in a vending machine.  You *could* do a search loop..  or you can use a hash map in your IC10 stack.
That's where this tool comes in.  Consider the following code:

```mips
  move SlotIdx 102

stock_sum_loop:
  sub SlotIdx SlotIdx 1
  ls IngotPrefabHash VendingMachine SlotIdx PrefabHash # Load 
  breqz IngotPrefabHash _skip_sum_loop # If no prefab hash, skip empty slot

  mod IngotTableIndex IngotPrefabHash INGOT_MODULUS_1
  mod IngotTableIndex IngotTableIndex INGOT_MODULUS_2
  add IngotTableAddress IngotTableIndex SP_STOCK_TABLE # SP_STOCK_TABLE is where in the stack you want to put this data
  get StockAmount db IngotTableAddress
  ls IngotQuantity VendingMachine Scratch Quantity # Load 

  add StockAmount StockAmount IngotQuantity
  poke IngotTableAddress StockAmount

_skip_sum_loop:
  bgt SlotIdx 2 stock_sum_loop # Loop through all vending machine item slots, except import/export (0 and 1)
```

This code would almost be ideal - it's fast, accurate, and small...  except, you need to know what `INGOT_MODULUS_1` and `INGOT_MODULUS_2` are.  So let's have a look at this tool's output.

```
cat ingot_prefab_hashes | ./HashModulusFinder
Searching for double-modulus pair for 19-hash table index conversion
>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
No modulus found for table size 19, adding sparsity.
>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
No modulus found for table size 20, adding sparsity.
>>>>>
Smallest double-modulus to ensure unique table indices for 19 hashes: 199424
Fit into data table length set by 2nd modulus 21
Took 10060 ms

ItemSilverIngot = 17
ItemSiliconIngot = 18
ItemNickelIngot = 7
ItemLeadIngot = 19
ItemIronIngot = 14
ItemGoldIngot = 16
ItemCopperIngot = 13
ItemSteelIngot = 3
ItemSolderIngot = 11
ItemInvarIngot = 20
ItemElectrumIngot = 12
ItemConstantanIngot = 15
ItemWaspaloyIngot = 5
ItemStelliteIngot = 4
ItemInconelIngot = 8
ItemHastelloyIngot = 6
ItemAstroloyIngot = 9
ItemCoalOre = 1
ItemCobaltOre = 2
```

Just like that, we have our two magic constants - `INGOT_MODULUS_1` is 199424, and `INGOT_MODULUS_2` is 21.  And, as a bonus, you'll know exactly which offsets inside the table correspond to the stock of each ingot!

That's what this tool does - finds the most optimal modulus operators to allow you to convert a set of known PrefabHashes into memory table addresses with only a handful of lines of code.
