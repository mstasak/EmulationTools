using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static XASM8080.ExpressionParser;

namespace XASM8080;

/**
 * Parse an expression; this is typically an operand for an assembler expression, and is
 * ended by a comma, semicolon, or end of string (optionally preceeded by whitespace).
 * 
 * Order of evaluation (borrowed from C):
 * 1 prefix operators +, -, ~ 
 * 2 *, /, %
 * 3 +, -
 * 4 <<, >>
 * 5 &
 * 6 ^
 * 7 |
 * 
 * at this time, boolean and comparison operators are not supported.
 * 
 * Special symbols:
 * $             current PC
 * 'label'       a file-wide label; no duplicate allowed in file
 * '.label'      local label (refers to most recent occurence of '.label' after any non-local label)
 * extern filename.label  a label from another file; must be unique in this file
 * extern filename.label equ alias  assign a new name alias for an extern file (to prevent duplicate)
 *
 * special expression types:
 * reg8 = a,b,c,d,e,h,l,m
 * reg16 = B,D,H,SP   (also allow BC, DE, HL, SP)
 * reg16s = B,D,H,PSW (also allow BC, DE, HL, AFLAGS)
 * rstnum = 1,2,3,4,5,6,7,8
 * 
 * At this time, string and boolean expressions are not supported.
 * 
 **/

internal enum ExpressionType {
    //    ExprByte,
    ExprWord,
    //ExprDWord,
    //ExprString, //not much use, unless we implement listing control directives: .TITLE "TINY BASIC v. " + VERSION
    //ExprBool, //really not useful unless we want ternary expression operator or #ifdef-type directives
    //ExprFloat,ExprDouble,etc common floating point formats, both historical and modern
    // possibly 8-bit: char, schar, uchar, byte, sbyte, ubyte,
    // possibly: 16-bit: short, sshort, ushort, word, sword, uword
    // possibly: 32-bit: int, sint, uint, dword, sdword, udword
    // possibly: 64-bit: long, slong, ulong, qword, sqword, uqword
    // perhaps some integral types of same bitwidth as common floating point types
    Reg8,     //A,B,C,D,E,H,L,M
    Reg16SP,  //B, D, H, SP
    Reg16PSW, //B, D, H, PSW
    RstVector //0-7 decimal literal
}

internal class ExpressionResult {
    public string InputLine = "";
    public ExpressionType ExprType;
    public List<string>? UnresolvedSymbols;
    public ushort Value;
    public string ExprSource = "";
    public string RemainingInputLine = "";
    public bool IsError = false;
    public string ErrorMessage = "";
}

internal class ExpressionParser {
    internal static string source = "";
    internal static string remainingSource = "";
    internal static string[] regs8 = { "B", "C", "D", "E", "H", "L", "M", "A" };
    internal static string[] regs16SP = { "B", "D", "H", "SP" };
    internal static string[] regs16PSW = { "B", "D", "H", "PSW" };
    internal static string[] rstNums = { "0", "1", "2", "3", "4", "5", "6", "7" };
    //private ExpressionParser(string sourceString, ExpressionType expressionType) {
    //}
    private static string inputLine = "";


    internal static ExpressionResult ParseReg8(string sourceString) {
        ExpressionResult result = new ExpressionResult() {
            InputLine = sourceString,
            ExprType = ExpressionType.Reg8,
            UnresolvedSymbols = new(),
            //Value = 0,
            //ExprSource = string.Empty,
            RemainingInputLine = sourceString
            //IsError = false,
            //ErrorMessage = string.Empty
        };
        inputLine = sourceString;
        var exprText = ParseWord();
        var which = regs8.ToList().IndexOf(exprText);
        if (which == -1) {

            result.IsError = true;
            result.ErrorMessage = $"Invalid register specified: {exprText}";

        } else {
            result.Value = (ushort)which;
        }
        return result;
    }

    internal static ExpressionResult ParseReg16SP(string sourceString) {
        ExpressionResult result = new ExpressionResult() {
            InputLine = sourceString,
            ExprType = ExpressionType.Reg16SP,
            UnresolvedSymbols = new(),
            //Value = 0,
            //ExprSource = string.Empty,
            RemainingInputLine = sourceString
            //IsError = false,
            //ErrorMessage = string.Empty
        };
        inputLine = sourceString;
        var exprText = ParseWord();
        var which = regs16SP.ToList().IndexOf(exprText);
        if (which == -1) {

            result.IsError = true;
            result.ErrorMessage = $"Invalid register specified: {exprText} (expected B, D, H, or SP)";

        } else {
            result.Value = (ushort)which;
        }
        return result;
    }

    internal static ExpressionResult ParseReg16PSW(string sourceString) {
        ExpressionResult result = new ExpressionResult() {
            InputLine = sourceString,
            ExprType = ExpressionType.Reg16PSW,
            UnresolvedSymbols = new(),
            //Value = 0,
            //ExprSource = string.Empty,
            RemainingInputLine = sourceString
            //IsError = false,
            //ErrorMessage = string.Empty
        };
        inputLine = sourceString;
        var exprText = ParseWord();
        var which = regs16PSW.ToList().IndexOf(exprText);
        if (which == -1) {

            result.IsError = true;
            result.ErrorMessage = $"Invalid register specified: {exprText} (expected B, D, H, or PSW)";

        } else {
            result.Value = (ushort)which;
        }
        return result;
    }

    internal static ExpressionResult ParseRstNum(string sourceString) {
        ExpressionResult result = new ExpressionResult() {
            InputLine = sourceString,
            ExprType = ExpressionType.RstVector,
            UnresolvedSymbols = new(),
            //Value = 0,
            //ExprSource = string.Empty,
            RemainingInputLine = sourceString
            //IsError = false,
            //ErrorMessage = string.Empty
        };
        inputLine = sourceString;
        var exprText = ParseWord();
        var which = rstNums.ToList().IndexOf(exprText);
        if (which == -1) {

            result.IsError = true;
            result.ErrorMessage = $"Invalid RST vector specified: {exprText} (expected 0-7)";

        } else {
            result.Value = (ushort)which;
        }
        return result;
    }

    private static string ParseWord() {
        var rslt = "";
        inputLine = inputLine.TrimStart();
        while ((inputLine.Length > 0) && ((inputLine[0] >= 'A' && inputLine[0] <= 'Z')
                                         || (inputLine[0] >= 'a' && inputLine[0] <= 'z')
                                         || (inputLine[0] == '.')
                                         || (inputLine[0] == '_')
                                         || (inputLine[0] == '$')
                                         )) {
            rslt = rslt + inputLine[0];
            inputLine = inputLine[1..];
        }
        return rslt;
    }

    //private static int ParseOperator(string[] operators) {
    //    inputLine = inputLine.TrimStart();
    //    var whichOp = 0;
    //    foreach (var oper in operators) {
    //        if (inputLine.StartsWith(oper)) {
    //            inputLine = inputLine[oper.Length..];
    //            return whichOp;
    //        }
    //        whichOp++;
    //    }

    //    var which = operators.fir

    //    return -1;
    //}

    private static bool ParseChar(char c) {
        var rslt = false;
        if (c != ' ') {
            inputLine = inputLine.TrimStart();
        }
        if (inputLine.Length > 0 && inputLine[0] == c) {
            rslt = true;
            inputLine = inputLine.Substring(1);
        }
        return rslt;
    }

    public class ExpressionOperator {
        public string? operatorString;
        public Func<ushort, ushort, ushort>? calculate;
        public ExpressionOperator(string opString, Func<ushort, ushort, ushort> calcFn) {
            operatorString = opString;
            calculate = calcFn;
        }
        public ExpressionOperator() {
        }
    }

    internal static ExpressionResult ParseNumericExpression(string SourceString, int PrecedenceLevel = 0) {

        //Dictionary<int, string[]> precedenceLevelDefs = new() {
        //    { 5, new string[] { "*", "/", "%" } },
        //    { 4, new string[] { "+", "-" } },
        //    { 3, new string[] { "<<", ">>" } },
        //    { 2, new string[] { "&" } },
        //    { 1, new string[] { "^" } },
        //    { 0, new string[] { "|" } }
        //};
        //public ExpressionOperator[] addsubops = new ExpressionOperator[] {
        //    new ExpressionOperator("+",(ushort a, ushort b) => (ushort)(a + b)) // {operatorString = "+", calculate = ((a, b) => a + b;) },
        //};


        ExpressionOperator[][] binaryOperators = new ExpressionOperator[][] {
            new ExpressionOperator[2]{
                new ExpressionOperator() {operatorString = "+", calculate = (ushort a, ushort b) => (ushort)(a + b) },
                new ExpressionOperator() {operatorString = "", calculate = (ushort a, ushort b) => (ushort)(a + b) }
            },
            //new ExpressionOperator[]{ },
            //new ExpressionOperator[]{ },
            //new ExpressionOperator[]{ },
            //new ExpressionOperator[]{ },
            //new ExpressionOperator[]{ }
        };

        string[] unaryOperators = { "+", "-", "~", "<", ">" };

        ExpressionResult result = new ExpressionResult() {
        InputLine = SourceString,
        ExprType = ExpressionType.ExprWord,
        UnresolvedSymbols = new(),
        //Value = 0,
        //ExprSource = string.Empty,
        RemainingInputLine = SourceString
        //IsError = false,
        //ErrorMessage = string.Empty


        /** ABORT
         * TOO CRAPPY, CAN'T CONTINUE
         * REDOING PARSING IN SourceCodeLine CLASS.
         * sometimes it takes a fresh pass, maybe even several, to code something well :)
         */



    };

    ExpressionResult term1 = ParseNumericExpression(SourceString: result.InputLine, PrecedenceLevel: PrecedenceLevel + 1);
        if (PrecedenceLevel< 6) {
            //look for operator and term2
            //var operators = precedenceLevelDefs[PrecedenceLevel];
            //var whichOp = ParseOperator(operators);
        } else {
            //look for a value or parenthetical expr
        }

        return result;
    }
}
