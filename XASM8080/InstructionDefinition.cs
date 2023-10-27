using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
public struct InstructionDefinition {
    public string Mnemonic;
    public byte? Opcode;
    public OperandModel OperandModel;
    public bool IsPseudoOp;
}
