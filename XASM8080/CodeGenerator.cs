using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
/// <summary>
/// Manage generation of binary code - provides memory address and memory contents buffer.
/// </summary>
public class CodeGenerator {

    /// <summary>
    /// Address of next byte to be written
    /// </summary>
    public ushort? MemoryAddress {
        get; set;
    }

    /// <summary>
    /// Buffer for output code
    /// </summary>
    public byte[] CodeBuffer {
        get; set;
    }

    /// <summary>
    /// Lowest address used in CodeBuffer
    /// </summary>
    public ushort? BufferAddressMinUsed;

    /// <summary>
    /// Highest address used in CodeBuffer
    /// </summary>
    public ushort? BufferAddressMaxUsed;
    
    /// <summary>
    /// Lazy constructor for singleton Instance.
    /// </summary>
    private static readonly Lazy<CodeGenerator> lazy =
        new(() => new CodeGenerator());

    /// <summary>
    /// public singleton Instance.
    /// </summary>
    public static CodeGenerator Instance => lazy.Value;

    /// <summary>
    /// private constructor with field initializer
    /// </summary>
    private CodeGenerator() {
        CodeBuffer = new byte[65536];
    }

    /// <summary>
    /// Called between assembly passes to reset address pointer.
    /// </summary>
    /// <deleted-param name="Pass"></param>
    /// <deleted-param name="Address"></param>
    /// <deleted-param name="FinalPass"></param>
    public void Reset() { //(int Pass, ushort Address = 0, bool FinalPass = false) {
        MemoryAddress = null; //can't store anythhing before first ORG pseudo-op
        BufferAddressMinUsed = null;
        BufferAddressMaxUsed = null;
    }
    
    /// <summary>
    /// Put a byte into the output buffer; track min, max address used.
    /// Sends a copy of added data back to caller's outputBytes list for use in listings.
    /// Increments address pointer by 1.
    /// </summary>
    /// <param name="data">Byte to store.   If null, nothing is done.</param>
    /// <param name="outputBytes">Caller's list of bytes written, for listing generation.</param>
    internal void WriteByte(byte? data, List<byte> outputBytes) {
        if (data == null) { 
            return; 
        }
        if (MemoryAddress == null) { 
            return; 
        }
        if (outputBytes != null) {
            outputBytes.Add(data.Value);
        }
        MarkUsed(MemoryAddress.Value);
        CodeBuffer[MemoryAddress.Value] = data.Value;
        MemoryAddress++;
    }

    /// <summary>
    /// Put a word into the output buffer; track min, max address used.
    /// Sends a copy of added data back to caller's outputBytes list for use in listings.
    /// Increments address pointer by 2.
    /// </summary>
    /// <param name="data">Word to store.   If null, nothing is done.</param>
    /// <param name="outputBytes">Caller's list of bytes written, for listing generation.</param>
    internal void WriteWord(ushort? data, List<byte> outputBytes) {
        if (data == null) {
            return;
        }
        WriteByte((byte)(data & 0xff), outputBytes);
        WriteByte((byte)(data >> 8), outputBytes);
    }

    /// <summary>
    /// Put an array of bytes into the output buffer; track min, max address used.
    /// Sends a copy of added data back to caller's outputBytes list for use in listings.
    /// Increments address pointer by data.Count.
    /// </summary>
    /// <param name="data">Array of bytes to store.</param>
    /// <param name="outputBytes">Caller's list of bytes written, for listing generation.</param>
    internal void WriteBytes(byte[]? data, List<byte> outputBytes) {
        if (data == null) {
            return;
        }
        foreach (var b in data) {
            WriteByte(b, outputBytes);
        }
    }

    /// <summary>
    /// Put an array of words into the output buffer; track min, max address used.
    /// Sends a copy of added data back to caller's outputBytes list for use in listings.
    /// Increments address pointer by data.Count * 2.
    /// </summary>
    /// <param name="data">Array of words to store</param>
    /// <param name="outputBytes">Caller's list of bytes written, for listing generation.</param>
    internal void WriteWords(ushort[]? data, List<byte> outputBytes) {
        if (data == null) {
            return;
        }
        foreach (var w in data) {
            WriteWord(w, outputBytes);
        }
    }

    /// <summary>
    /// Return the used portion of the output buffer to the caller (presumably to write to a file).
    /// </summary>
    /// <returns>Range of bytes from min-used to max-used, from output buffer</returns>
    internal byte[] GetOutput() {
        return CodeBuffer[(BufferAddressMinUsed ?? 0) .. (BufferAddressMaxUsed ?? 65536)];
    }

    /// <summary>
    /// Track range of bytes used in output buffer.
    /// A call to MarkUsed(0) may be used to ensure output will start at location zero, even if first
    /// instruction is, say, ORG 0100H.
    /// </summary>
    /// <param name="MemoryAddress">An address to be considered used.</param>
    internal void MarkUsed(ushort MemoryAddress) {
        if (MemoryAddress < (BufferAddressMinUsed ?? ushort.MaxValue)) {
            BufferAddressMinUsed = MemoryAddress;
        }
        if (MemoryAddress > (BufferAddressMaxUsed ?? ushort.MinValue)) {
            BufferAddressMaxUsed = MemoryAddress;
        }
    }
}


