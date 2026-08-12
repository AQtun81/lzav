# Cross-Platform C# Bindings for LZAV

| Platform | x86 | x64 | arm | arm64 |
|----------|-----|-----|-----|-------|
| Windows  | ✓   | ✓   | ✗   | ✓     |
| Linux    | ✗   | ✓   | ✓   | ✓     |
| MacOS    | ✗   | ✓   | ✗   | ✓     |
| Android  | ✗   | ✗   | ✗   | ✗     |
| iOS      | ✗   | ✗   | ✗   | ✗     |

ℹ️ This package bundles both dynamic and static libraries, in AoT builds the library will be statically linked.

### Usage
To compress data:
```cs
using AQtun.LZAV;

byte[] src = File.ReadAllBytes(FILENAME);
byte[] dst = new byte[Lzav.CompressBound(src.Length)];
int length = Lzav.CompressDefault(src, dst);
if (length < 0) {
  // Error handling.
}
```
To decompress data:
```cs
using AQtun.LZAV;

byte[] decompressed = new byte[decompressed_size];
int length = Lzav.Decompress(compressed, decompressed);
if (length < 0) {
  // Error handling.
}
```
Method signatures:
```cs
// compression
int CompressBound(int sourceLength);
int CompressDefault(IntPtr source, IntPtr destination, int sourceLength, int destinationLength);
int CompressDefault(ReadOnlySpan<byte> source, Span<byte> destination);
int CompressDefault(ReadOnlySpan<byte> source, out Span<byte> output);

// high ratio compression
int CompressBoundHi(int sourceLength);
int CompressHi(IntPtr source, IntPtr destination, int sourceLength, int destinationLength);
int CompressHi(ReadOnlySpan<byte> source, Span<byte> destination);
int CompressHi(ReadOnlySpan<byte> source, out Span<byte> output);

// decompression
int Decompress(IntPtr source, IntPtr destination, int sourceLength, int destinationLength);
int Decompress(ReadOnlySpan<byte> source, Span<byte> destination);
int Decompress(ReadOnlySpan<byte> source, int decompressedSize, out Span<byte> output)
```