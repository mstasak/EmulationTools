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

    public int? BufferAddressMinUsed;
    public int? BufferAddressMaxUsed;
    

    private static readonly Lazy<CodeGenerator> lazy =
        new(() => new CodeGenerator());

    public static CodeGenerator Instance => lazy.Value;

    private CodeGenerator() {
        CodeBuffer = new byte[65536];
    }

    public void Reset(int Pass, int Address, bool FinalPass) => throw new NotImplementedException();
}


