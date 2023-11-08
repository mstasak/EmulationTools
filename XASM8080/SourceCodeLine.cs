using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;

namespace XASM8080;
public partial class SourceCodeLine {
    /// <summary>
    /// This is the complete source code line; it will include line continuations (using \\\n)
    /// </summary>
    public string Source;
    public int LinePosition;
    public string SourceFileName;
    public int SourceLineNumber;
    public int? ErrorPosition;
    public string? ErrorMessage;
    public ushort? startAddr;
    /// <summary>
    /// Label defined at start of line:
    /// abc: instruction... - a "normal" symbol defined uniquely within this file and accessable from only this file
    /// $abc: instruction... - a global symbol which can be referenced from other files; unique to file
    /// .abc: instruction... - a "local" nonunique symbol local to a block which is either the span from 
    /// .blockname.abc: ...    one "normal" label to the next, or a containing block/endblock pair
    /// 
    /// </summary>
    public LabelDeclaration? Label;
    public InstructionDefinition? Instruction;
    public byte? Opcode;
    public List<Operand> Operands;
    public List<byte> OutputBytes = new();
    //public string AssemblerDirective; //future? listing options, strict switch, pass limits, forward ref disallowed flag, etc.
    // perhaps also import/link commands or standard include packages such as bios and cruntime-like libs or graphics output
    public string? Comment;

    //operand parse lookup tables:
    internal static string[] regs8 = { "^B", "^C", "^D", "^E", "^H", "^L", "^M", "^A" };
    internal static string[] regs16SP = { "^B", "^D", "^H", "^SP" };
    internal static string[] regs16PSW = { "^B", "^D", "^H", "^PSW" };
    internal static string[] regs16BD = { "^B", "^D" };
    internal static string[] rstNums = { "^0", "^1", "^2", "^3", "^4", "^5", "^6", "^7" };

    /// <summary>
    /// Construct a source code line for parsing and code generation.
    /// </summary>
    /// <param name="source">The line of source code to assemble: [label] [operation [operand...]] [comment]</param>
    public SourceCodeLine(string source, string fileName, int lineNumber) {
        Source = source;
        LinePosition = 0;
        Label = null;
        Instruction = null;
        Opcode = null;
        Operands = new();
        Comment = null;
        SourceFileName = fileName;
        SourceLineNumber = lineNumber;
    }
    public bool Parse(bool finalPass) {
        startAddr = CodeGenerator.Instance.MemoryAddress;
        ParseLabelDeclaration();
        ParseInstruction(); //includes operands
        ParseComment();
        GenerateCode();
        if (finalPass) {
            OutputGenerator.OutputLine(this);
        }
        return ErrorMessage != null;
    }

    public bool GenerateCode() {
        //if label, figure out value and add/update symbol table
        //  special handling for org and equ

        if (Label != null) {
            var sym = SymbolTable.Instance.Lookup(Label);
            ushort? newLabelValue = null; // = CodeGenerator.Instance.MemoryAddress;
            if (Instruction != null && ((Instruction.Value.Mnemonic == "ORG") || (Instruction.Value.Mnemonic == "EQU"))) {
                if (Operands.Count == 1 && Operands[0].WordValue.HasValue) {
                    newLabelValue = Operands[0].WordValue;
                    if (Instruction.Value.Mnemonic == "ORG") {
                        CodeGenerator.Instance.MemoryAddress = newLabelValue;
                        startAddr = newLabelValue;
                    }
                    if (sym != null) {
                        if (sym.WordValue.HasValue && !newLabelValue.HasValue) {
                            Console.WriteLine($"Internal error? Replacing defined symbol value with null: {sym.LabelText} = NULL");
                        }
                        sym.WordValue = newLabelValue;
                    }
                } else {
                    ReportError("ORG and EQU require one resolved word operand.");
                }
            } else {
                newLabelValue = CodeGenerator.Instance.MemoryAddress;
            }
            if (sym == null) {
                sym = new SymbolDefinition() {
                    DeclarationFileName = Label.FileName,
                    DeclarationLineNumber = Label.LineNumber,
                    Kind = Label.Kind switch {
                        LabelDeclarationKind.None => SymbolKind.None,
                        LabelDeclarationKind.Global => SymbolKind.Global,
                        LabelDeclarationKind.Static => SymbolKind.Static,
                        LabelDeclarationKind.Local => SymbolKind.Local,
                        _ => SymbolKind.None
                    },
                    LabelText = Label.LabelText,
                    ParentText = Label.ParentText,
                    WordValue = newLabelValue
                };
                SymbolTable.Instance.SymbolTab[sym.SymbolTableKey()] = sym;
            } else {
                sym.WordValue = newLabelValue;
            }

        }

        if (Instruction.HasValue && Instruction.Value.Mnemonic == "END") {
            Assembler.Instance.EndEncountered = true; //breaks assembly pass file and line loops
        }
        if (!CodeGenerator.Instance.MemoryAddress.HasValue && Instruction.HasValue && Instruction.Value.Mnemonic != "ORG"&& Instruction.HasValue && Instruction.Value.Mnemonic != "EQU") {
            return false; //current address undefined
        }
        //emit opcode and operands
        //  before final pass, write nothing, just advance address pointer
        if (Instruction.HasValue) {
            var instr = Instruction.Value;
            if (instr.IsPseudoOp == false) {
                //handle opcode modifiers
                var opc = instr.Opcode;
                foreach (var oprmod in Operands) {
                    opc = (byte)((opc ?? 0) | (oprmod.OpcodeModifier ?? 0));
                }
                CodeGenerator.Instance.WriteByte(opc, OutputBytes);
                foreach (var oprval in Operands) {
                    //switch (oprval.Kind) {
                    //    case OperandKind.Imm8:
                    //        CodeGenerator.Instance.WriteByte((byte)(oprval.WordValue ?? 0), OutputBytes);
                    //        break;
                    //    case OperandKind.Imm16:
                    //        CodeGenerator.Instance.WriteWord(oprval.WordValue ?? 0, OutputBytes);
                    //        break;
                    //    default:
                    //        break;
                    //}
                    if (oprval.Bytes != null) {
                        CodeGenerator.Instance.WriteBytes(oprval.Bytes.ToArray(), OutputBytes);
                    }
                }
                //write operand bytes where applicable
            } 
            if (Instruction.Value.Mnemonic == "DB") {
                foreach (var oprval in Operands) {
                    if (oprval.Kind == OperandKind.DBList) {
                        CodeGenerator.Instance.WriteBytes(oprval.Bytes!.ToArray(), OutputBytes!);
                    }
                }
            } else if (Instruction.Value.Mnemonic == "DW") {
                foreach (var oprval in Operands) {
                    if (oprval.Kind == OperandKind.DWList) {
                        CodeGenerator.Instance.WriteBytes(oprval.Bytes!.ToArray(), OutputBytes!);
                    }
                }
            } else if (Instruction.Value.Mnemonic == "DS") {
                var oprDSSize = Operands[0];
                if (CodeGenerator.Instance.MemoryAddress != null &&
                    oprDSSize.WordValue != null) {
                    CodeGenerator.Instance.MemoryAddress = (ushort)(CodeGenerator.Instance.MemoryAddress + oprDSSize.WordValue);
                }
            } else if (Instruction.Value.Mnemonic == "END") {
                Assembler.Instance.EndEncountered = true; //breaks assembly pass file and line loops
            } else if (Instruction != null && ((Instruction.Value.Mnemonic == "ORG") || (Instruction.Value.Mnemonic == "EQU"))) {
                if (Operands.Count == 1 && Operands[0].WordValue.HasValue) {
                    var newPCValue = Operands[0].WordValue;
                    if (Instruction.Value.Mnemonic == "ORG") {
                        CodeGenerator.Instance.MemoryAddress = newPCValue;
                    }
                } else {
                    ReportError("ORG requires one resolved word operand.");
                }
            }        }
        // write bin

        return true;
    }

    private void ReportError(string v) {

    }

    public bool OutputListings() {
        // write lst
        // write err
        // write sym
        // write xrf?
        // write prettyprint?
        return true;
    }

    /// <summary>
    /// Skip forward to next non-whitespace character (intended to skip spaces and tabs).
    /// </summary>
    public void SkipSpace() {
        while (LinePosition < Source.Length) {
            if (char.IsWhiteSpace(Source[LinePosition])) {
                LinePosition++;
            } else {
                break;
            }
        }
    }

    /// <summary>
    /// Try to skip a literal string, returning true if found.
    /// If found, LinePosition is advanced past the target string.
    /// </summary>
    /// <param name="literal">Target string.</param>
    /// <param name="ignoreCase">Iff true, case insensitive search used.</param>
    /// <returns></returns>
    public bool SkipLiteral(string literal, bool ignoreCase) {
        if (LinePosition + literal.Length - 1 < Source.Length) {
            if (string.Compare(literal, Source[LinePosition..(LinePosition + literal.Length)], ignoreCase) == 0) {
                LinePosition += literal.Length;
                return true;
            }
        }
        return false;
    }

    public void ParseLabelDeclaration() {
        SkipSpace();

        //parse and categorize a label
        var decl = new LabelDeclaration();
        decl.ParseLabelDeclarationName(Source, ref LinePosition, SourceFileName, SourceLineNumber);
        if (decl.Kind != LabelDeclarationKind.None) {
            Label = decl;
        }
    }

    /// <summary>
    /// Test current input position against a regular expression (typically begins with ^).
    /// If matched, return length of match.
    /// LinePosition is not changed.
    /// </summary>
    /// <param name="pattern">pattern to match</param>
    /// <param name="options">options, such as RegexOptions.IgnoreCase</param>
    /// <returns></returns>
    public int MatchRegExp(string pattern, RegexOptions options = RegexOptions.IgnoreCase) {
        Debug.Assert(pattern.StartsWith("^")); //only interested in matches at position 0
        var match = Regex.Match(Source[LinePosition..], pattern, options, matchTimeout: TimeSpan.FromMilliseconds(500));
        if (match != null) {
            return match.Length;
        }
        return 0;
    }

    /// <summary>
    /// Take numCharacters characters from Source beginning at LinePosition; advance LinePosition
    /// </summary>
    /// <param name="numCharacters">Length of string to return and skip over</param>
    /// <returns></returns>
    public string Munch(int numCharacters) {
        var rslt = Source[LinePosition..(LinePosition + numCharacters)];
        LinePosition += numCharacters;
        return rslt ?? "";
    }

    /// <summary>
    /// Parse an instruction (opcode) and its arguments (operands)
    /// </summary>
    public void ParseInstruction() {
        var opcodeLength = MatchRegExp("^\\s*[A-Z]+");
        if (opcodeLength > 0) {
            //instruction found (maybe not valid)
            var opcodeString = Munch(opcodeLength);
            //LinePosition += opcodeLength;
            opcodeString = opcodeString.TrimStart();
            opcodeLength = opcodeString.Length;
            var opcodeIx = InstructionSet8080.InstructionSet.ToList().FindIndex((instruction) => instruction.Mnemonic == opcodeString);

            if (opcodeIx > 0) {
                Instruction = InstructionSet8080.InstructionSet[opcodeIx];
            }
            if (Instruction.HasValue) {
                Opcode = Instruction.Value.Opcode;
                //parse operand(s)
                switch (Instruction.Value.OperandModel) {
                    case OperandModel.R8Left:
                        Operands.Add(ParseOperandReg8());
                        //if (!oper.HasError && Opcode.HasValue) { // <-- process in code emit later
                        //    Opcode = (byte)(Opcode | oper.OpcodeModifier);
                        //}
                        break;
                    case OperandModel.R8Right:
                        Operands.Add(ParseOperandReg8(false));
                        //if (!oper2.HasError && Opcode.HasValue) {
                        //    Opcode = (byte)(Opcode | oper2.OpcodeModifier);
                        //}
                        break;
                    case OperandModel.Imm8:
                        Operands.Add(ParseOperandImmByte());
                        break;
                    case OperandModel.RstNum:
                        Operands.Add(ParseOperandRst());
                        break;
                    case OperandModel.R16WithSP:
                        Operands.Add(ParseOperandReg16WithSP());
                        //if (!oper5.HasError && Opcode.HasValue) {
                        //    Opcode = (byte)(Opcode | oper5.OpcodeModifier);
                        //}
                        break;
                    case OperandModel.R16WithPSW:
                        Operands.Add(ParseOperandReg16WithPSW());
                        //if (!oper6.HasError && Opcode.HasValue) {
                        //    Opcode = (byte)(Opcode | oper6.OpcodeModifier);
                        //}
                        break;
                    case OperandModel.R16OnlyBD:
                        Operands.Add(ParseOperandReg16OnlyBD());
                        //if (!oper7.HasError && Opcode.HasValue) {
                        //    Opcode = (byte)(Opcode | oper7.OpcodeModifier);
                        //}
                        break;
                    case OperandModel.Imm16:
                        Operands.Add(ParseOperandImmWord());
                        break;
                    case OperandModel.DBList:
                        Operands.Add(ParseOperandListDB());
                        break;
                    case OperandModel.DWList:
                        Operands.Add(ParseOperandListDW());
                        break;
                    case OperandModel.DSSize:
                        Operands.Add(ParseOperandImmWord());
                        Operands.Last().Kind = OperandKind.DSSize;
                        break;
                    case OperandModel.None:
                        break;
                    case OperandModel.R8Imm8:
                        Operands.Add(ParseOperandReg8());
                        SkipSpace();
                        if (MatchString(",")) {
                            Munch(1);
                        }
                        Operands.Add(ParseOperandImmByte());
                        break;
                    case OperandModel.R16WithSPImm16:
                        Operands.Add(ParseOperandReg16WithSP());
                        SkipSpace();
                        if (MatchString(",")) {
                            Munch(1);
                        }
                        Operands.Add(ParseOperandImmWord());
                        break;
                        //default:
                        //    break;
                }
            }
        }
    }

    private Operand? ParseOperandListDB() {
        var oldPos = LinePosition;
        Operand? rslt = ParseOperandDBValue();
        if (rslt != null && rslt.Kind != OperandKind.None && rslt.Bytes != null) {
            do {
                var sepCount = MatchRegExp("^\\s*,");
                if (sepCount > 0) {
                    Munch(sepCount); //skip past comma to next value
                    Operand? oprNext = ParseOperandDBValue();
                    if (oprNext != null && oprNext.Kind != OperandKind.None && oprNext.Bytes != null) {
                        rslt.Bytes.AddRange(oprNext.Bytes);
                        //} else {
                        //just loop back and look for a comma and another value (e.g. 2 was missing:  1, , 3)    
                    }
                } else {
                    break;
                }
            } while (true);
            rslt.Text = Source[oldPos..LinePosition];
            rslt.Kind = OperandKind.DBList;
        }
        return rslt;
    }

    private Operand? ParseOperandDBValue() {
        //try imm8
        var rslt = ParseOperandImmByte();
        if (rslt == null || rslt.HasError) {
            //try "string"
            rslt = ParseOperandDBString();
        }
        return rslt;
    }

    private Operand? ParseOperandDBString() {
        SkipSpace();
        var mLength = MatchRegExp("^\\'(\\\\.|[^\\'])+\\'"); //TODO: consider unicode or hex char literals
        if (mLength < 3) {
            return null;
        }
        var match = Munch(mLength);
        match = match[1 .. (match.Length-1)]; //remove laading and trailing quote
        match = dequote(match);
        var matchBytes = new List<byte>();
        foreach (var ch in match) {
            matchBytes.Add((byte)ch);
        }
        return new Operand(match) {
            Kind = OperandKind.DBList,
            Bytes = matchBytes,
            WordValue = 0
        };
    }

    private string dequote(string match) {
        var rslt = "";
        var ix = 0;
        while (ix < match.Length) {
            if (match[ix] == '\\' && ix < match.Length - 1) {
                rslt += match[++ix];
            } else {
                rslt += match[ix];
            }
            ix++;
        }
        return rslt;
    }

    private Operand ParseOperandListDW() {
        var oldPos = LinePosition;
        Operand? rslt = ParseOperandImmWord();
        if (rslt != null && rslt.Kind != OperandKind.None) {
            rslt.Bytes ??= new List<byte>();
            rslt.Bytes.Add((byte)((rslt.WordValue ?? 0) & 0xff));
            rslt.Bytes.Add((byte)((rslt.WordValue ?? 0) >> 8));
            do {
                var sepCount = MatchRegExp("^\\s*,");
                if (sepCount > 0) {
                    Munch(sepCount); //skip past comma to next value
                    Operand? oprNext = ParseOperandImmWord();
                    if (oprNext != null && oprNext.Kind != OperandKind.None) {
                        rslt.Bytes.Add((byte)((oprNext.WordValue ?? 0) & 0xff));
                        rslt.Bytes.Add((byte)((oprNext.WordValue ?? 0) >> 8));
                        //} else {
                        //just loop back and look for a comma and another value (e.g. 2 was missing:  1, , 3)    
                    }
                } else {
                    break;
                }
            } while (true);
            rslt.Text = Source[oldPos..LinePosition];
            rslt.Kind = OperandKind.DWList;
        }
        return rslt;
    }

    private Operand ParseOperandImmWord() {
        var opr = ParseNumericExpression();
        opr.Kind = OperandKind.Imm16;
        if (!opr.HasError && opr.WordValue.HasValue) {
            opr.Bytes = new() {
                (byte)((opr.WordValue ?? 0) & 0xff),
                (byte)((opr.WordValue ?? 0) >> 8)
            };        
        } else {
            opr.Bytes = new() {
                0, 0
            };        
        }
        return opr;
    }
    private Operand ParseOperandImmByte() {
        var opr = ParseNumericExpression();
        opr.Kind = OperandKind.Imm8;
        if (!opr.HasError && opr.WordValue.HasValue) {
            opr.Bytes = new() {
                (byte)(opr.WordValue ?? 0)
            };        
        } else {
            opr.Bytes = new() {
                0
            };        
        }
        return opr;
    }
    private Operand ParseOperandRst() => throw new NotImplementedException();

    /// <summary>
    /// Parse comment.  Really this does nothing.  If ';' found, ignore rest of line, else return so rest of line will be ignored.
    /// </summary>
    public void ParseComment() {
        SkipSpace();
        var found = MatchString(";");
        if (found) {
            Comment = Source[LinePosition..]; //rest of line, including ';'
            LinePosition = Source.Length - 1;
        }
    }

    /// <summary>
    /// Check if current source location matches a literal string; does not change LinePosition
    /// </summary>
    /// <param name="searchString">string to search for</param>
    /// <param name="caseInsensitive">true to make search case-insensitive</param>
    /// <returns></returns>
    public bool MatchString(string searchString, bool caseInsensitive = false) {
        if (Source[LinePosition..].StartsWith(searchString, caseInsensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Look for an string in array matching the next characters in Source.  If found, return string's index and
    /// advance past it.
    /// </summary>
    /// <param name="LookupArray"></param>
    /// <param name="ParsedText"></param>
    /// <returns></returns>
    public int ParseForLookupEntry(string[] LookupArray, ref string ParsedText) {
        SkipSpace();
        var rv = 0;
        foreach (var reg in LookupArray) {
            var matchLength = MatchRegExp(reg + "");
            if (matchLength > 0) {
                ParsedText = Munch(matchLength);
                return rv;
            }
            rv++;
        }
        ParsedText = "";
        return -1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isLeft"></param>
    /// <returns></returns>
    public Operand ParseOperandReg8(bool isLeft = true) {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs8, ref parsedText);
        var rslt = new Operand(//operandModel: isLeft ? OperandModel.R8Left : OperandModel.R8Right,
            operandKind: isLeft ? OperandKind.R8Left : OperandKind.R8Right,
            text: parsedText,
            bytes: null,
            wordValue: 0,
            opcodeModifier: 0,
            hasError: false,
            errorDescription: null);
        if (nWhich >= 0) {
            //rslt.IsResolved = true;
            rslt.WordValue = (ushort)nWhich;
            rslt.OpcodeModifier = (byte)((nWhich & 0x07) << (isLeft ? 3 : 0));
        } else {
            rslt.HasError = true;
            rslt.ErrorDescription = "Unrecognized register name";
        }
        return rslt;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Operand ParseOperandReg16WithSP() {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs16SP, ref parsedText);
        var rslt = new Operand(//operandModel: OperandModel.R16WithSP,
            operandKind: OperandKind.R16WithSP,
            text: parsedText,
            bytes: null,
            wordValue: 0,
            opcodeModifier: 0,
            hasError: false,
            errorDescription: null);
        if (nWhich >= 0) {
            rslt.WordValue = (ushort)nWhich; //this may be used in place of IsResolved to determine validity of operand value.
            rslt.OpcodeModifier = (byte)((nWhich & 0x03) << 4); //this is actual useful operand value, or'ed to opcode
        } else {
            rslt.HasError = true;
            rslt.ErrorDescription = "Unrecognized register pair name";
        }
        return rslt;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Operand ParseOperandReg16WithPSW() {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs16PSW, ref parsedText);
        var rslt = new Operand(//operandModel: OperandModel.R16WithPSW,
            operandKind: OperandKind.R16WithPSW,
            text: parsedText,
            bytes: null,
            wordValue: 0,
            opcodeModifier: 0,
            hasError: false,
            errorDescription: null);
        if (nWhich >= 0) {
            rslt.WordValue = (ushort)nWhich; //this may be used in place of IsResolved to determine validity of operand value.
            rslt.OpcodeModifier = (byte)((nWhich & 0x03) << 4);
        } else {
            rslt.HasError = true;
            rslt.ErrorDescription = "Unrecognized register pair name";
        }
        return rslt;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Operand ParseOperandReg16OnlyBD() {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs16BD, ref parsedText);
        var rslt = new Operand(//operandModel: OperandModel.R16OnlyBD,
            operandKind: OperandKind.R16OnlyBD,
            text: parsedText,
            bytes: null,
            wordValue: 0,
            opcodeModifier: 0,
            hasError: false,
            errorDescription: null);
        if (nWhich >= 0) {
            rslt.WordValue = (ushort)nWhich; //this may be used in place of IsResolved to determine validity of operand value.
            rslt.OpcodeModifier = (byte)((nWhich & 0x03) << 4);
        } else {
            rslt.HasError = true;
            rslt.ErrorDescription = "Unrecognized register pair name";
        }
        return rslt;
    }

    /// <summary>
    /// 
    /// </summary>
    public enum EnumOperatorType {
        Binary,
        Prefix,
        Suffix //not used?
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="precedenceLeval"></param>
    /// <returns></returns>
    public Operand ParseNumericExpression(int precedenceLeval = 0) {
        Operand leftTerm;
        if (precedenceLeval > 5) {  //get a value
            leftTerm = ParseValue();
        } else { // get a value from next precedence level, parse an operator for this level, if found get another value, calculate, and repeat
            leftTerm = ParseNumericExpression(precedenceLeval + 1);
            if (leftTerm.Text == "") {
                return leftTerm;
            }
            var operators = OperatorDef.Operators.Where(op => op.Precedence == precedenceLeval && op.OperatorType == EnumOperatorType.Binary);
            OperatorDef? oper;
            do {
                oper = ParseOperator(operators);
                if (oper == null) {
                    break;
                }
                var rightTerm = ParseNumericExpression(precedenceLeval + 1);
                if (rightTerm.Text == "") { //remember error but continue processing, e.g. DB 2 +   + 3; could skip to next comma or semicolon?
                    leftTerm.Text += " " + oper.Operator + " ";
                    if (!leftTerm.HasError) {
                        leftTerm.ErrorDescription = rightTerm.ErrorDescription;
                        leftTerm.HasError = true;
                    }
                } else {
                    leftTerm.Text += " " + oper.Operator + " " + rightTerm.Text;
                    if (!leftTerm.HasError && rightTerm.HasError) {
                        leftTerm.ErrorDescription = rightTerm.ErrorDescription;
                        leftTerm.HasError = true;
                    }
                    leftTerm.WordValue = oper.Calculate(leftTerm.WordValue, rightTerm.WordValue);

                }

            } while (true);
        }
        return leftTerm;
    }

    public OperatorDef? ParseOperator(IEnumerable<OperatorDef> operators) {
        SkipSpace();
        var rslt = operators.FirstOrDefault(oper => MatchString(oper.Operator, false));
        if (rslt != null) {
            Munch(rslt.Operator.Length);
        }
        return rslt;
    }

    public Operand ParseValue() {
        //return a single value:
        //  a number (decimal or hex)
        //  a symbol
        //  (a parenthesized expression)
        //  prefix value (we'll tolerate unlimited prefixes like ---1)
        Operand? value;
        SkipSpace();

        //number - hex
        value = ParseHexadecimalNumber();
        if (!value.HasError) {
            return value;
        }

        //number - decimal
        value = ParseDecimalNumber();
        if (!value.HasError) {
            return value;
        }

        //symbol
        value = ParseLabelRef();
        if ((value != null) && !value.HasError) {
            return value;
        }

        value = ParseChar();
        if ((value != null) && !value.HasError) {
            return value;
        }

        //$
        var mLen = MatchRegExp("^\\$(?![0-9a-z_])");
        if (mLen == 1) {
            Munch(1);
            value = new Operand(text: "$") { WordValue = CodeGenerator.Instance.MemoryAddress};
            return value;
        }


        //(expr)
        if (MatchString("(")) {
            Munch(1);
            value = ParseNumericExpression();
            if (MatchString(")")) {
                Munch(1);
            } else {
                if (!value.HasError) {
                    value.HasError = true;
                    value.ErrorDescription = "Missing right perenthesis in operand expression.";
                }
            }
            return value;
        }

        //prefix val
        var operators = OperatorDef.Operators.Where(op => op.Precedence == 6 && op.OperatorType == EnumOperatorType.Prefix);
        OperatorDef? oper;
        oper = ParseOperator(operators);
        if (oper != null) {
            value = ParseValue();
            value.WordValue = oper.Calculate(value.WordValue, 0);
            return value;
        }

        //expression error - generic
        value = new Operand(//operandModel: OperandModel.R16WithPSW,
            operandKind: OperandKind.None,
            text: "",
            bytes: null,
            wordValue: null,
            opcodeModifier: 0,
            hasError: true,
            errorDescription: "Error evaluating an operand - valid value not found.");
        return value;
    }


    public Operand ParseDecimalNumber() {
        //TO DO: implement distinct version for byte-sized operands, range 0 to 255
        var MatchLen = MatchRegExp("^[0-9]+(?!h)"); // (?!H)");
        ushort rsltVal;
        if (MatchLen > 0) {
            var strDecimalNum = Munch(MatchLen);
            if (ushort.TryParse(strDecimalNum, System.Globalization.NumberStyles.Integer, null, out rsltVal)) {
                //return result built on rsltVal
                return new Operand(text: strDecimalNum) { WordValue = rsltVal };
            }
            return new Operand(text: "") { HasError = true, ErrorDescription = "Range error?  Could not convert numeric literal to word (0000H - 0FFFFH, or 0-65535)." };
        }
        return new Operand(text: "") { HasError = true, ErrorDescription = "Did not find a valid number" };
    }

    public Operand ParseHexadecimalNumber() {
        //TO DO: implement distinct version for byte-sized operands, range 0 to FF (255)
        //TO DO: implement alternative hex number syntaxes ($ffff, 0xff, &HFFA0)
        var MatchLen = MatchRegExp("^[0-9][0-9ABCDEF]*H");
        ushort rsltVal;
        if (MatchLen > 0) {
            var strHexadecimalNum = Munch(MatchLen);

            if (ushort.TryParse(strHexadecimalNum[0..(strHexadecimalNum.Length - 1)], System.Globalization.NumberStyles.AllowHexSpecifier, null, out rsltVal)) {
                //return result built on rsltVal
                return new Operand(text: strHexadecimalNum) { WordValue = rsltVal };
            }
            return new Operand(text: "") { HasError = true, ErrorDescription = "Range error?  Could not convert numeric literal to word (0-65535)." };
        }
        return new Operand(text: "") { HasError = true, ErrorDescription = "Did not find a valid number" };
    }

    // need parseChar
    public Operand? ParseChar() {
        SkipSpace();
        var mLength = MatchRegExp("^\\'((\\\\.)|[^\\'])\\'"); //TODO: consider unicode or hex char literals
        var match = Munch(mLength);
        if (match.Length < 1) {
            return null;
        }
        match = match[1 .. (match.Length - 1)]; //remove leading and trailing quote
        match = dequote(match);
        var matchBytes = new List<byte>() {
            (byte)match[0] 
        };
        return new Operand(match) {
            Kind = OperandKind.Imm8,
            Bytes = matchBytes,
            WordValue = (byte)match[0]
        };
    }

    public Operand? ParseString() {
        SkipSpace();
        var mLength = MatchRegExp("^\\'(\\\\.)|[^\\'])+\\'"); //TODO: consider unicode or hex char literals
        var match = Munch(mLength);
        match = match[1 .. (match.Length - 1)]; //remove leading and trailing quote
        match = dequote(match);
        var matchBytes = new List<byte>();
        foreach (var ch in match) {
            matchBytes.Add((byte)ch);
        }
        return new Operand(match) {
            Kind = OperandKind.DBList,
            Bytes = matchBytes,
            WordValue = (byte)match[0]
        };
    }


    /// <summary>
    /// Parse a label operand value, as part of an operand expression (e.g. JMP label)
    /// </summary>
    /// <returns>An Operand object, with appropriate error properties as needed, or null if text does not match any valid label syntax.</returns>
    public Operand? ParseLabelRef() {
        var lRef = new LabelReference();
        lRef.ParseLabelReferenceName(Source, ref LinePosition);
        if (lRef.Kind == LabelReferenceKind.None) {
            return null;
        }
        if (lRef.Kind != LabelReferenceKind.Global) {
            lRef.FileName = SourceFileName;
        } else if (lRef.ParentText != null) {
            lRef.FileName = lRef.ParentText; //locate long name?
        }
        var symbol = SymbolTable.Instance.Lookup(lRef);
        if (symbol == null) {
            return null;
        } else {
            return new Operand( 
                operandKind: OperandKind.Imm16,
                text: lRef.FullLabelText ?? "",
                bytes: null,
                wordValue: symbol.WordValue,
                opcodeModifier: 0,
                hasError: false,
                errorDescription: null
            );
        }
    }
}
