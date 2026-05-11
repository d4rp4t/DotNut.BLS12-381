# DotNut.BLS12-381
========== WORK IN PROGRESS ==========
This repository contains C# native implementation of BLS12-381 curve. 

**The author is not a cryptographer, and the work wasn't audited. No warranties are made.**

I simply made it for fun.

Every entity in this library is struct based, because:
a) struct have nice sequential memory alignment
b) these live on stack, not on a shared heap

I tried to make it as constant-time as possible, but C# JIT can be.. well too much optimizing.

Peace.