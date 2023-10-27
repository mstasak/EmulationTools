using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;

public enum OperandModel {
    None,
    R8Left,
    R8Right,
    R16WithSP,
    R16WithPSW,
    R16OnlyBD, //BC or DE only
    Imm8,
    Imm16,
    R8Imm8,
    R16WithSPImm16,
    RstNum,
    DBList,
    DWList,
    DSSize,
    BlockName,
}

public class Operand {
    //public string Name;
    //public bool IsResolved;
    public string Text;
    //public OperandModel OperandModel;
    public byte[]? Bytes; //for variable length operands (db, dw)
    public ushort? WordValue; //for all types except strings, e.g. DB "Hello, World!\n"
    public byte? OpcodeModifier;  //shifted and masked code, to be or'ed to opcode to insert a register, rp, or rst code
    public bool HasError;
    public string? ErrorDescription;

    public Operand(string text) {
        Text = text;
        //ErrorDescription = "";
    }

    public Operand(bool isResolved, string text, byte[]? bytes, ushort? wordValue, byte? opcodeModifier, bool hasError, string? errorDescription) {
        //IsResolved = isResolved;
        Text = text;
        Bytes = bytes;
        WordValue = wordValue;
        OpcodeModifier = opcodeModifier;
        HasError = hasError;
        ErrorDescription = errorDescription;
    }
}