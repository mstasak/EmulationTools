using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
internal static class InstructionSet8080 {
    private static readonly InstructionDefinition[] instructionSet = {
        new InstructionDefinition {Mnemonic = "NOP", Opcode = 0x00, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "LXI", Opcode = 0x01, OperandModel = OperandModel.R16WithSP, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "STAX", Opcode = 0x02, OperandModel = OperandModel.R16OnlyBD, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "INX", Opcode = 0x03, OperandModel = OperandModel.R16WithSP, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "INR", Opcode = 0x04, OperandModel = OperandModel.R8Left, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "DCR", Opcode = 0x05, OperandModel = OperandModel.R8Left, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "MVI", Opcode = 0x06, OperandModel = OperandModel.R8Imm8, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "RLC", Opcode = 0x07, OperandModel = OperandModel.None, IsPseudoOp = false},
        // unused Opcode 0x08
        new InstructionDefinition {Mnemonic = "DAD", Opcode = 0x09, OperandModel = OperandModel.R16WithSP, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "LDAX", Opcode = 0x0A, OperandModel = OperandModel.R16OnlyBD, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "DCX", Opcode = 0x0B, OperandModel = OperandModel.R16WithSP, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "RRC", Opcode = 0x0F, OperandModel = OperandModel.None, IsPseudoOp = false},
        // unused Opcode 0x10
        new InstructionDefinition {Mnemonic = "RAL", Opcode = 0x17, OperandModel = OperandModel.None, IsPseudoOp = false},
        // unused Opcode 0x18
        new InstructionDefinition {Mnemonic = "RAR", Opcode = 0x1F, OperandModel = OperandModel.None, IsPseudoOp = false},
        // unused Opcode 0x20
        new InstructionDefinition {Mnemonic = "SHLD", Opcode = 0x22, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "DAA", Opcode = 0x27, OperandModel = OperandModel.None, IsPseudoOp = false},
        // unused Opcode 0x28
        new InstructionDefinition {Mnemonic = "LHLD", Opcode = 0x2A, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "CMA", Opcode = 0x2F, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "STA", Opcode = 0x32, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        // unused Opcode 0x30
        new InstructionDefinition {Mnemonic = "STC", Opcode = 0x37, OperandModel = OperandModel.None, IsPseudoOp = false},
        // unused Opcode 0x38
        new InstructionDefinition {Mnemonic = "LDA", Opcode = 0x3A, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "CMC", Opcode = 0x3F, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "MOV", Opcode = 0x40, OperandModel = OperandModel.R8Right, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "HLT", Opcode = 0x76, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "ADD", Opcode = 0x80, OperandModel = OperandModel.R8Left, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "ADC", Opcode = 0x88, OperandModel = OperandModel.R8Left, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "SUB", Opcode = 0x90, OperandModel = OperandModel.R8Left, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "SBB", Opcode = 0x98, OperandModel = OperandModel.R8Left, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "ANA", Opcode = 0xA0, OperandModel = OperandModel.R8Left, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "XRA", Opcode = 0xA8, OperandModel = OperandModel.R8Left, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "ORA", Opcode = 0xB0, OperandModel = OperandModel.R8Left, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "CMP", Opcode = 0xB8, OperandModel = OperandModel.R8Left, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "RNZ", Opcode = 0xC0, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "POP", Opcode = 0xC1, OperandModel = OperandModel.R16WithPSW, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "JNZ", Opcode = 0xC2, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "JMP", Opcode = 0xC3, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "CNZ", Opcode = 0xC4, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "PUSH", Opcode = 0xC5, OperandModel = OperandModel.R16WithPSW, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "ADI", Opcode = 0xC6, OperandModel = OperandModel.Imm8, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "RST", Opcode = 0xC7, OperandModel = OperandModel.RstNum, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "RZ", Opcode = 0xC8, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "RET", Opcode = 0xC9, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "JZ", Opcode = 0xCA, OperandModel = OperandModel.Imm16, IsPseudoOp = false},  
        // unused Opcode 0xCB
        new InstructionDefinition {Mnemonic = "CZ", Opcode = 0xCC, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "CALL", Opcode = 0xCD, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "ACI", Opcode = 0xCE, OperandModel = OperandModel.Imm8, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "RNC", Opcode = 0xD0, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "JNC", Opcode = 0xD2, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "OUT", Opcode = 0xD3, OperandModel = OperandModel.Imm8, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "CNC", Opcode = 0xD4, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "SUI", Opcode = 0xD6, OperandModel = OperandModel.Imm8, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "RC", Opcode = 0xD8, OperandModel = OperandModel.None, IsPseudoOp = false},
        // unused Opcode 0xD9
        new InstructionDefinition {Mnemonic = "JC", Opcode = 0xDA, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "IN", Opcode = 0xDB, OperandModel = OperandModel.Imm8, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "CC", Opcode = 0xDC, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        // unused Opcode 0xDD
        new InstructionDefinition {Mnemonic = "SBI", Opcode = 0xDE, OperandModel = OperandModel.Imm8, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "RPO", Opcode = 0xE0, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "JPO", Opcode = 0xE2, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "XTHL", Opcode = 0xE3, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "CPO", Opcode = 0xE4, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "ANI", Opcode = 0xE6, OperandModel = OperandModel.Imm8, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "RPE", Opcode = 0xE8, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "PCHL", Opcode = 0xE9, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "JPE", Opcode = 0xEA, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "XCHG", Opcode = 0xEB, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "CPE", Opcode = 0xEC, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        // unused Opcode 0xED
        new InstructionDefinition {Mnemonic = "XRI", Opcode = 0xEE, OperandModel = OperandModel.Imm8, IsPseudoOp = false},

        new InstructionDefinition {Mnemonic = "RPO", Opcode = 0xF0, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "JPO", Opcode = 0xF2, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "DI", Opcode = 0xF3, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "CP", Opcode = 0xF4, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "ORI", Opcode = 0xF6, OperandModel = OperandModel.Imm8, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "RM", Opcode = 0xF8, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "SPHL", Opcode = 0xF9, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "JM", Opcode = 0xFA, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "EI", Opcode = 0xFB, OperandModel = OperandModel.None, IsPseudoOp = false},
        new InstructionDefinition {Mnemonic = "CM", Opcode = 0xFC, OperandModel = OperandModel.Imm16, IsPseudoOp = false},
        // unused Opcode 0xFD
        new InstructionDefinition {Mnemonic = "CPI", Opcode = 0xFE, OperandModel = OperandModel.Imm8, IsPseudoOp = false},
        
        
        new InstructionDefinition {Mnemonic = "ORG", Opcode = null, OperandModel = OperandModel.Imm16, IsPseudoOp = true},
        new InstructionDefinition {Mnemonic = "DB", Opcode = null, OperandModel = OperandModel.DBList, IsPseudoOp = true},
        new InstructionDefinition {Mnemonic = "DW", Opcode = null, OperandModel = OperandModel.DWList, IsPseudoOp = true},
        new InstructionDefinition {Mnemonic = "DS", Opcode = null, OperandModel = OperandModel.DSSize, IsPseudoOp = true},
        new InstructionDefinition {Mnemonic = "END", Opcode = null, OperandModel = OperandModel.None, IsPseudoOp = true},
        new InstructionDefinition {Mnemonic = "BLOCK", Opcode = null, OperandModel = OperandModel.BlockName, IsPseudoOp = true},
        new InstructionDefinition {Mnemonic = "ENDBLOCK", Opcode = null, OperandModel = OperandModel.BlockName, IsPseudoOp = true},
        //DB
        //DW
        //DS
        //END
        //any others? extras like chain or include, output formatting options, error limit, 
    };

    internal static InstructionDefinition[] InstructionSet => instructionSet;
}
