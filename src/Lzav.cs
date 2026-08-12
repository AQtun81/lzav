using System;
using System.Runtime.InteropServices;

namespace AQtun.LZAV
{
    /// <summary>
    /// LZAV is a fast general-purpose in-memory data compression algorithm based on now-classic LZ77 lossless data compression method.
    /// </summary>
    public static partial class Lzav
    {
        #if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
        private const string LZAV_DLL = "liblzav";
        #else // NETFRAMEWORK
        private const string LZAV_DLL = "liblzav.dll";
        #endif
        
        /* COMPRESS
         -------------------------------------------------------------------------------------------------*/
        
        /// <summary>
        /// LZAV compression function with an external buffer option.<para/>
        /// 
        /// The function performs in-memory data compression using the LZAV compression
        /// algorithm and data format. The function produces "raw" compressed data
        /// without a header containing the data length, identifier, or checksum.<para/>
        /// 
        /// The function relies on forced code inlining meaning its multiple calls
        /// throughout the code may increase the code size considerably. It is
        /// suggested to wrap the call to this function in a non-inlined function.<para/>
        /// 
        /// Note that the compression algorithm and its output on the same source data
        /// may differ between LZAV versions, and may differ between big- and
        /// little-endian systems. However, decompression of compressed data produced
        /// by any prior compressor version will remain possible.
        /// </summary>
        /// <param name="source">Source (uncompressed) data pointer, can be 0 if <paramref name="sourceLength"/> equals 0. Address alignment is unimportant.</param>
        /// <param name="destination">Destination (compressed data) buffer pointer. The allocated size should be at least CompressBound() bytes. Address alignment is unimportant. Should be different from `src`.</param>
        /// <seealso cref="CompressBound"/>
        /// <param name="sourceLength">Source data length, in bytes, can be 0: in this case, the compressed length is assumed to be 0 as well.</param>
        /// <param name="destinationLength">Destination buffer's capacity, in bytes.</param>
        /// <param name="externalBuffer">External buffer to use for the hash-table; set to null for the function to manage memory itself (via the standard `malloc`). Supplying a pre-allocated buffer is useful if compression is performed often during an application's operation: this reduces memory allocation overhead and fragmentation. Note that the access to the supplied buffer is not implicitly thread-safe. Buffer's address must be aligned to 4 bytes.</param>
        /// <param name="externalBufferLength">The capacity of the <paramref name="externalBuffer"/>, in bytes; should be a power-of-2 value. Used as the hash-table size if <paramref name="externalBuffer"/> is null. The capacity should not be less than `4*sourceLength`, and for the default compression ratio should not be greater than 1 MiB. The same <paramref name="externalBufferLength"/> value can be used for any smaller source data. Using smaller <paramref name="externalBufferLength"/> values reduces the compression ratio and, at the same time, increases the compression speed. This aspect can be utilized on memory-constrained and low-performance processors.</param>
        /// <param name="mref">The minimal back-reference length, in bytes. Only 5 and 6 values are supported.</param>
        /// <returns>Length of the compressed data, in bytes. Returns 0 if <paramref name="sourceLength"/> is less than or equal to 0, if <paramref name="destinationLength"/> is too small, if buffer pointers are invalid, or if there is not enough memory.</returns>
        #if NET7_0_OR_GREATER
        [LibraryImport(LZAV_DLL, EntryPoint = "lzav_compress")]
        public static partial int Compress(IntPtr source, IntPtr destination, int sourceLength, int destinationLength, IntPtr externalBuffer, int externalBufferLength, UIntPtr mref);
        #else
        [DllImport(LZAV_DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzav_compress")]
        public static extern int Compress(IntPtr source, IntPtr destination, int sourceLength, int destinationLength, IntPtr externalBuffer, int externalBufferLength, UIntPtr mref);
        #endif
        
        /// <summary>
        /// LZAV compression function with an external buffer option.<para/>
        /// 
        /// The function performs in-memory data compression using the LZAV compression
        /// algorithm and data format. The function produces "raw" compressed data
        /// without a header containing the data length, identifier, or checksum.<para/>
        /// 
        /// The function relies on forced code inlining meaning its multiple calls
        /// throughout the code may increase the code size considerably. It is
        /// suggested to wrap the call to this function in a non-inlined function.<para/>
        /// 
        /// Note that the compression algorithm and its output on the same source data
        /// may differ between LZAV versions, and may differ between big- and
        /// little-endian systems. However, decompression of compressed data produced
        /// by any prior compressor version will remain possible.
        /// </summary>
        /// <param name="source">Source (uncompressed) Span&lt;byte&gt;.</param>
        /// <param name="destination">Destination (compressed data) Span&lt;byte&gt;. The span's `Length` should be at least CompressBound() bytes large.</param>
        /// <seealso cref="CompressBound"/>
        /// <param name="externalBuffer">External buffer to use for the hash-table; set to null for the function to manage memory itself (via the standard `malloc`). Supplying a pre-allocated buffer is useful if compression is performed often during an application's operation: this reduces memory allocation overhead and fragmentation. Note that the access to the supplied buffer is not implicitly thread-safe. Buffer's address must be aligned to 4 bytes.</param>
        /// <param name="mref">The minimal back-reference length, in bytes. Only 5 and 6 values are supported.</param>
        /// <returns>Length of the compressed data, in bytes. Returns 0 if `source.Length` is less than or equal to 0, if <paramref name="destination"/> is too small, or if there is not enough memory.</returns>
        public static unsafe int Compress(ReadOnlySpan<byte> source, Span<byte> destination, Span<byte> externalBuffer, UIntPtr mref)
        {
            fixed (byte* srcPtr = &MemoryMarshal.GetReference(source),
                         dstPtr = &MemoryMarshal.GetReference(destination),
                         extPtr = &MemoryMarshal.GetReference(externalBuffer))
            {
                return Compress((IntPtr)srcPtr, (IntPtr)dstPtr, source.Length, destination.Length, (IntPtr)extPtr, externalBuffer.Length, mref);
            }
        }
        
        /* COMPRESS DEFAULT
         -------------------------------------------------------------------------------------------------*/

        /// <summary>
        /// Function returns the buffer size required for the minimal reference length of 5.
        /// </summary>
        /// <param name="sourceLength">The length of the source data to be compressed.</param>
        /// <returns>The required allocation size for the destination compression buffer. Always a positive value.</returns>
        #if NET7_0_OR_GREATER
        [LibraryImport(LZAV_DLL, EntryPoint = "lzav_compress_bound")]
        public static partial int CompressBound(int sourceLength);
        #else
        [DllImport(LZAV_DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzav_compress_bound")]
        public static extern int CompressBound(int sourceLength);
        #endif
        
        /// <summary>
        /// Default LZAV compression function.<para/>
        ///
        /// The function performs in-memory data compression using the LZAV compression
        /// algorithm, with the default settings.<para/>
        ///
        /// See the Compress() method for a more detailed description.
        /// </summary>
        /// <seealso cref="Compress(IntPtr, IntPtr, int, int, IntPtr, int, UIntPtr)"/>
        /// <param name="source">Source (uncompressed) data pointer.</param>
        /// <param name="destination">Destination (compressed data) buffer pointer. The allocated size should be at least CompressBound() bytes large.</param>
        /// <seealso cref="CompressBound"/>
        /// <param name="sourceLength">Source data length, in bytes.</param>
        /// <param name="destinationLength">Destination buffer's capacity, in bytes.</param>
        /// <returns>Length of the compressed data, in bytes. Returns 0 if <paramref name="sourceLength"/> is less than or equal to 0, if <paramref name="destinationLength"/> is too small, or if there is not enough memory.</returns>
        #if NET7_0_OR_GREATER
        [LibraryImport(LZAV_DLL, EntryPoint = "lzav_compress_default")]
        public static partial int CompressDefault(IntPtr source, IntPtr destination, int sourceLength, int destinationLength);
        #else
        [DllImport(LZAV_DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzav_compress_default")]
        public static extern int CompressDefault(IntPtr source, IntPtr destination, int sourceLength, int destinationLength);
        #endif

        /// <summary>
        /// Default LZAV compression function.<para/>
        ///
        /// The function performs in-memory data compression using the LZAV compression
        /// algorithm, with the default settings.<para/>
        ///
        /// See the Compress() method for a more detailed description.
        /// </summary>
        /// <seealso cref="Compress(ReadOnlySpan&lt;byte&gt;, Span&lt;byte&gt;, Span&lt;byte&gt;, UIntPtr)"/>
        /// <param name="source">Source (uncompressed) Span&lt;byte&gt;.</param>
        /// <param name="destination">Destination (compressed data) Span&lt;byte&gt;. The span's `Length` should be at least CompressBound() bytes large.</param>
        /// <seealso cref="CompressBound"/>
        /// <returns>Length of the compressed data, in bytes. Returns 0 if `source.Length` is less than or equal to 0, if <paramref name="destination"/> is too small, or if there is not enough memory.</returns>
        public static unsafe int CompressDefault(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            fixed (byte* srcPtr = &MemoryMarshal.GetReference(source),
                         dstPtr = &MemoryMarshal.GetReference(destination))
            {
                return CompressDefault((IntPtr)srcPtr, (IntPtr)dstPtr, source.Length, destination.Length);
            }
        }

        /// <summary>
        /// Default LZAV compression function.<para/>
        ///
        /// The function performs in-memory data compression using the LZAV compression
        /// algorithm, with the default settings.<para/>
        ///
        /// See the Compress() method for a more detailed description.
        /// </summary>
        /// <seealso cref="Compress(ReadOnlySpan&lt;byte&gt;, Span&lt;byte&gt;, Span&lt;byte&gt;, UIntPtr)"/>
        /// <param name="source">Source (uncompressed) Span&lt;byte&gt;.</param>
        /// <param name="output">Output (compressed data) Span&lt;byte&gt;.</param>
        /// <returns>Length of the compressed data, in bytes. Returns 0 if `source.Length` is less than or equal to 0, if destination is too small, or if there is not enough memory.</returns>
        public static unsafe int CompressDefault(ReadOnlySpan<byte> source, out Span<byte> output)
        {
            output = new byte[CompressBound(source.Length)];
            fixed (byte* srcPtr = &MemoryMarshal.GetReference(source),
                         dstPtr = &MemoryMarshal.GetReference(output))
            {
                return CompressDefault((IntPtr)srcPtr, (IntPtr)dstPtr, source.Length, output.Length);
            }
        }

        /* COMPRESS HIGH
         -------------------------------------------------------------------------------------------------*/

        /// <summary>
        /// Function returns the buffer size required for the higher-ratio LZAV compression.
        /// </summary>
        /// <param name="sourceLength">The length of the source data to be compressed.</param>
        /// <returns>The required allocation size for the destination compression buffer. Always a positive value.</returns>
        #if NET7_0_OR_GREATER
        [LibraryImport(LZAV_DLL, EntryPoint = "lzav_compress_bound_hi")]
        public static partial int CompressBoundHi(int sourceLength);
        #else
        [DllImport(LZAV_DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzav_compress_bound_hi")]
        public static extern int CompressBoundHi(int sourceLength);
        #endif

        /// <summary>
        /// Higher-ratio LZAV compression function (much slower).<para/>
        ///
        /// The function performs in-memory data compression using the higher-ratio
        /// LZAV compression algorithm.
        /// </summary>
        /// <param name="source">Source (uncompressed) data pointer.</param>
        /// <param name="destination">Destination (compressed data) buffer pointer. The allocated size should be at least CompressBoundHi() bytes large.</param>
        /// <seealso cref="CompressBoundHi"/>
        /// <param name="sourceLength">Source data length, in bytes.</param>
        /// <param name="destinationLength">Destination buffer's capacity, in bytes.</param>
        /// <returns>Length of the compressed data, in bytes. Returns 0 if <paramref name="sourceLength"/> is less than or equal to 0, if <paramref name="destinationLength"/> is too small, or if there is not enough memory.</returns>
        #if NET7_0_OR_GREATER
        [LibraryImport(LZAV_DLL, EntryPoint = "lzav_compress_hi")]
        public static partial int CompressHi(IntPtr source, IntPtr destination, int sourceLength, int destinationLength);
        #else
        [DllImport(LZAV_DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzav_compress_hi")]
        public static extern int CompressHi(IntPtr source, IntPtr destination, int sourceLength, int destinationLength);
        #endif

        /// <summary>
        /// Higher-ratio LZAV compression function (much slower).<para/>
        ///
        /// The function performs in-memory data compression using the higher-ratio
        /// LZAV compression algorithm.
        /// </summary>
        /// <param name="source">Source (uncompressed) Span&lt;byte&gt;.</param>
        /// <param name="destination">Destination (compressed data) Span&lt;byte&gt;. The span's `Length` should be at least CompressBoundHi() bytes large.</param>
        /// <seealso cref="CompressBoundHi"/>
        /// <returns>Length of the compressed data, in bytes. Returns 0 if `source.Length` is less than or equal to 0, if <paramref name="destination"/> is too small, or if there is not enough memory.</returns>
        public static unsafe int CompressHi(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            fixed (byte* srcPtr = &MemoryMarshal.GetReference(source),
                         dstPtr = &MemoryMarshal.GetReference(destination))
            {
                return CompressHi((IntPtr)srcPtr, (IntPtr)dstPtr, source.Length, destination.Length);
            }
        }

        /// <summary>
        /// Higher-ratio LZAV compression function (much slower).<para/>
        ///
        /// The function performs in-memory data compression using the higher-ratio
        /// LZAV compression algorithm.
        /// </summary>
        /// <param name="source">Source (uncompressed) Span&lt;byte&gt;.</param>
        /// <param name="output">Output (compressed data) Span&lt;byte&gt;.</param>
        /// <returns>Length of the compressed data, in bytes. Returns 0 if `source.Length` is less than or equal to 0, if destination is too small, or if there is not enough memory.</returns>
        public static unsafe int CompressHi(ReadOnlySpan<byte> source, out Span<byte> output)
        {
            output = new byte[CompressBoundHi(source.Length)];
            fixed (byte* srcPtr = &MemoryMarshal.GetReference(source),
                         dstPtr = &MemoryMarshal.GetReference(output))
            {
                return CompressHi((IntPtr)srcPtr, (IntPtr)dstPtr, source.Length, output.Length);
            }
        }

        /* DECOMPRESS
         -------------------------------------------------------------------------------------------------*/

        /// <summary>
        /// LZAV decompression function.<para/>
        /// 
        /// The function decompresses "raw" data previously compressed into the LZAV
        /// data format.<para/>
        /// 
        /// Note that while the function does perform checks to avoid OOB memory
        /// accesses, and checks for decompressed data length equality, this is not a
        /// strict guarantee of valid decompression. In cases where the compressed data
        /// is stored in long-term storage without embedded data integrity mechanisms
        /// (e.g., a database without RAID 1 guarantee, a binary container without
        /// a digital signature or CRC), then a checksum (hash) of the original
        /// uncompressed data should be stored, and then evaluated against that of
        /// the decompressed data. Also, a separate checksum (hash) of
        /// an application-defined header, which contains uncompressed and compressed
        /// data lengths, should be checked before decompression. A high-performance
        /// "komihash" hash function can be used to obtain a hash value of the data.
        /// </summary>
        /// <param name="source">Source (compressed) data pointer; can be 0 if <paramref name="sourceLength"/> is 0. Address alignment is unimportant.</param>
        /// <param name="destination">Destination (decompressed data) buffer pointer. Address alignment is unimportant. Should be different from <paramref name="source"/>.</param>
        /// <param name="sourceLength">Source data length, in bytes; can be 0.</param>
        /// <param name="destinationLength">Expected destination data length, in bytes; can be 0. Should not be confused with the actual size of the destination buffer (which may be larger).</param>
        /// <returns>Length of the decompressed data, in bytes, or any negative value if an error occurred. Always returns a negative value if the resulting decompressed data length differs from <paramref name="destinationLength"/>. This means that error  result handling requires just a check for a negative return value (see the Lzav.Error enum for possible values).</returns>
        #if NET7_0_OR_GREATER
        [LibraryImport(LZAV_DLL, EntryPoint = "lzav_decompress")]
        public static partial int Decompress(IntPtr source, IntPtr destination, int sourceLength, int destinationLength);
        #else
        [DllImport(LZAV_DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzav_decompress")]
        public static extern int Decompress(IntPtr source, IntPtr destination, int sourceLength, int destinationLength);
        #endif

        /// <summary>
        /// LZAV decompression function.<para/>
        /// 
        /// The function decompresses "raw" data previously compressed into the LZAV
        /// data format.<para/>
        /// 
        /// Note that while the function does perform checks to avoid OOB memory
        /// accesses, and checks for decompressed data length equality, this is not a
        /// strict guarantee of valid decompression. In cases where the compressed data
        /// is stored in long-term storage without embedded data integrity mechanisms
        /// (e.g., a database without RAID 1 guarantee, a binary container without
        /// a digital signature or CRC), then a checksum (hash) of the original
        /// uncompressed data should be stored, and then evaluated against that of
        /// the decompressed data. Also, a separate checksum (hash) of
        /// an application-defined header, which contains uncompressed and compressed
        /// data lengths, should be checked before decompression. A high-performance
        /// "komihash" hash function can be used to obtain a hash value of the data.
        /// </summary>
        /// <param name="source">Source (compressed) Span&lt;byte&gt;; can be 0 if source length is 0. Address alignment is unimportant.</param>
        /// <param name="destination">Destination (decompressed data) Span&lt;byte&gt;. Address alignment is unimportant. Should be different from <paramref name="source"/>.</param>
        /// <returns>Length of the decompressed data, in bytes, or any negative value if an error occurred. Always returns a negative value if the resulting decompressed data length differs from destination length. This means that error  result handling requires just a check for a negative return value (see the Lzav.Error enum for possible values).</returns>
        public static unsafe int Decompress(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            fixed (byte* srcPtr = &MemoryMarshal.GetReference(source),
                         dstPtr = &MemoryMarshal.GetReference(destination))
            {
                return Decompress((IntPtr)srcPtr, (IntPtr)dstPtr, source.Length, destination.Length);
            }
        }

        /// <summary>
        /// LZAV decompression function.<para/>
        /// 
        /// The function decompresses "raw" data previously compressed into the LZAV
        /// data format.<para/>
        /// 
        /// Note that while the function does perform checks to avoid OOB memory
        /// accesses, and checks for decompressed data length equality, this is not a
        /// strict guarantee of valid decompression. In cases where the compressed data
        /// is stored in long-term storage without embedded data integrity mechanisms
        /// (e.g., a database without RAID 1 guarantee, a binary container without
        /// a digital signature or CRC), then a checksum (hash) of the original
        /// uncompressed data should be stored, and then evaluated against that of
        /// the decompressed data. Also, a separate checksum (hash) of
        /// an application-defined header, which contains uncompressed and compressed
        /// data lengths, should be checked before decompression. A high-performance
        /// "komihash" hash function can be used to obtain a hash value of the data.
        /// </summary>
        /// <param name="source">Source (compressed) Span&lt;byte&gt;; can be 0 if source length is 0. Address alignment is unimportant.</param>
        /// <param name="output">Destination (decompressed data) Span&lt;byte&gt;.</param>
        /// <param name="decompressedSize">Size in bytes of decompressed data.</param>
        /// <returns>Length of the decompressed data, in bytes, or any negative value if an error occurred. Always returns a negative value if the resulting decompressed data length differs from destination length. This means that error  result handling requires just a check for a negative return value (see the Lzav.Error enum for possible values).</returns>
        public static unsafe int Decompress(ReadOnlySpan<byte> source, int decompressedSize, out Span<byte> output)
        {
            output = new byte[decompressedSize];
            fixed (byte* srcPtr = &MemoryMarshal.GetReference(source),
                         dstPtr = &MemoryMarshal.GetReference(output))
            {
                return Decompress((IntPtr)srcPtr, (IntPtr)dstPtr, source.Length, output.Length);
            }
        }
        
        /// <summary>
        /// Decompression error codes.
        /// </summary>
        public enum Error
        {
            /// <summary>
            /// Incorrect function parameters.
            /// </summary>
            Params = -1,
            /// <summary>
            /// Source buffer out-of-bounds error.
            /// </summary>
            SrcOob = -2,
            /// <summary>
            /// Destination buffer out-of-bounds error.
            /// </summary>
            DstOob = -3,
            /// <summary>
            /// Back-reference out-of-bounds error.
            /// </summary>
            RefOob = -4,
            /// <summary>
            /// Decompressed length mismatch error.
            /// </summary>
            DstLen = -5,
            /// <summary>
            /// Unknown data format or mref error.
            /// </summary>
            UnkFmt = -6,
            /// <summary>
            /// Pointer overflow error.
            /// </summary>
            PtrOvr = -7
        }
    }
}