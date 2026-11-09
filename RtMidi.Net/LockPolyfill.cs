// System.Threading.Lock (.NET 9+) is faster than locking on an object and makes the intent clear.
// On .NET 8 the same code locks on a plain object.
#if !NET9_0_OR_GREATER
global using Lock = object;
#endif
