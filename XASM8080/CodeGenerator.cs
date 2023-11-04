using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
public class CodeGenerator {

    public ushort? MemoryAddress {
        get; set;
    }

    public byte[] CodeBuffer {
        get; set;
    }

    public ushort? BufferAddressMinUsed;
    public ushort? BufferAddressMaxUsed;
    

    private static readonly Lazy<CodeGenerator> lazy =
        new(() => new CodeGenerator());

    public static CodeGenerator Instance => lazy.Value;

    private CodeGenerator() {
        CodeBuffer = new byte[65536];
    }

    public void Reset(int Pass, int Address, bool FinalPass) {
        MemoryAddress = null;
        BufferAddressMinUsed = null;
        BufferAddressMaxUsed = null;
    }
    internal void WriteByte(byte? data, List<byte> outputBytes) {
        if (data == null) { 
            return; 
        }
        if (outputBytes != null) {
            outputBytes.Add(data.Value);
        }
        if (MemoryAddress == null) { 
            return; 
        }
        if (MemoryAddress < (BufferAddressMinUsed ?? ushort.MaxValue)) {
            BufferAddressMinUsed = MemoryAddress;
        }
        if (MemoryAddress > (BufferAddressMaxUsed ?? ushort.MinValue)) {
            BufferAddressMaxUsed = MemoryAddress;
        }
        CodeBuffer[MemoryAddress.Value] = data.Value;
        MemoryAddress++;
    }
    internal void WriteWord(ushort? data, List<byte> outputBytes) {
        if (data == null) {
            return;
        }
        WriteByte((byte)(data & 0xff), outputBytes);
        WriteByte((byte)(data >> 8), outputBytes);
    }
    internal void WriteBytes(byte[]? data, List<byte> outputBytes) {
        if (data == null) {
            return;
        }
        foreach (var b in data) {
            WriteByte(b, outputBytes);
        }
    }
    internal void WriteWords(ushort[]? data, List<byte> outputBytes) {
        if (data == null) {
            return;
        }
        foreach (var w in data) {
            WriteWord(w, outputBytes);
        }
    }

    internal byte[] GetOutput() {
        return CodeBuffer[(BufferAddressMinUsed ?? 0) .. (BufferAddressMaxUsed ?? 65536)];
    }

}


