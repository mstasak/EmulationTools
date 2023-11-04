using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XASM8080;

/**
 * Multi-pass assembler, using "standard"-ish Microsoft/Intel syntax.
 * Not too pleased with the first cut, may scrap most and do over.
 * The details of a simple process do add up quickly.  Too big for a reasonable
 * single-file program, should probably make distinct classes for 
 * instruction set, output generator(s), expression evaluation, symbol table.
**/

public class Assembler {

    public static readonly char[] WhitespaceChars = new char[] { ' ', '\t' };
    public SymbolTable SymbolTable = SymbolTable.Instance;
    public CodeGenerator CodeGenerator = CodeGenerator.Instance;
    public List<string> InputFilePaths;

    public DateTime AssemblyStartTime
    {
        get;
        private set;
    }
    public DateTime AssemblyEndTime
    {
        get;
        private set;
    }
    public TimeSpan AssemblyElapsedTime => AssemblyEndTime - AssemblyStartTime;

    public int Pass;
    public int PriorPassUnresolvedSymbolRefs;
    public bool FinalPass;

    //private bool FinalPass = false;
    //private int passErrorCount = 0;
    //private int passUnresolvedSymbols = 0; //symbol value is uncertain; must reach 0 for successful compile
    //private int passUndefinedSymbols = 0; //symbol not known; ok on pass 1 for forward reference


    public string? currentFileShortName;
    public string? currentFileFullPathName;
    public int currentLineNumber; //within file
    public string? MostRecentNormalLineLabel; //for [label].locallabel symbols
    private readonly List<string> FullFilePaths = new();

    ////private string currentLinePart; // label, opcode, operand n, comment
    ////private string? currentLineLabel;
    ////private ushort? currentLineLabelValue;
    ////private string? currentLineInstruction;
    ////private byte? currentLineData;
    ////private string? currentLineOperandText;
    ////private List<byte>? currentLineOperandData;
    ////private string? currentLineComment;
    ////private string? currentLineError;
    //private SourceCodeLine? currentLine;
    //private int passUnresolvedSymbolRefs;


    private static readonly Lazy<Assembler> lazy =
        new(() => new Assembler());

    public static Assembler Instance => lazy.Value;

    public bool EndEncountered {
        get;
        set;
    }

    private Assembler() {
        InputFilePaths = XASMMain.InputFileNames;
        FullFilePaths = new();
        foreach(var fPath in InputFilePaths)
        {
            var fi = new FileInfo(fPath);
            if (fi.Exists && CanOpenText(fi)) {
                FullFilePaths.Add(fi.FullName);
            } else {
            }
        }
    }

    private static bool CanOpenText(FileInfo fi)
    {
        try {
            if (fi.Exists) {
                fi.OpenText().Dispose();
                return true;
            }
        } catch {
        }
        return false;
    }

    public void Assemble() {
        AssemblyStartTime = DateTime.Now;
        Pass = 1;
        PriorPassUnresolvedSymbolRefs = 0;
        FinalPass = false;
        //bool readyForFinalPass = false; //will be set when no issues remain, or last pass made no progress
        do {
            AssemblePass();
            var passUnresolvedSymbolRefs = SymbolTable.Instance.SymbolValueUnresolvedCount();
            //int passUnknownSymbolCount = SymbolTable.UnknownSymbolCount();
            if (Pass > 1) {
                FinalPass = (passUnresolvedSymbolRefs == 0) || (passUnresolvedSymbolRefs >= PriorPassUnresolvedSymbolRefs);
            }
            PriorPassUnresolvedSymbolRefs = passUnresolvedSymbolRefs;
            Pass++;
        } while (!FinalPass);
        if (false && PriorPassUnresolvedSymbolRefs > 0) {
            Pass--;
            DisplayMessage($"Assembly aborted after {Pass} passes.  There are {PriorPassUnresolvedSymbolRefs} unresolvable symbols.");
        } else {
            //begin final pass
            OutputGenerator.OutputStart();

            AssemblePass();
            AssemblyEndTime = DateTime.Now;
            DisplayMessage($"Assembly completed in {Pass} passes.  Elapsed time: {AssemblyElapsedTime.TotalSeconds} secs.");
            OutputGenerator.OutputEnd();
            //CodeGenerator.DisplayOutputStatistics();
        }
    }

    private static void DisplayMessage(string v) {
        Console.WriteLine(v);
    }

    private void AssemblePass() {
        //throw new NotImplementedException();
        //passErrorCount = 0;
        CodeGenerator.Instance.Reset(Pass: Pass, Address: 0, FinalPass: FinalPass);
        EndEncountered = false;
        foreach (var fileName in FullFilePaths) {
            if (EndEncountered) {
                break;
            }
            var fi = new FileInfo(fileName);
            currentFileShortName = fi.Name;
            currentFileFullPathName = fileName;
            currentLineNumber = 1;
            using var inFile = File.OpenText(fileName);
            if (inFile != null) {
                var s = GetLineWithContinuations(inFile);
                while (s != null) {
                    AssembleLine(s);
                    if (EndEncountered) {
                        break;
                    }
                    s = GetLineWithContinuations(inFile);
                    currentLineNumber++;
                }
            }
        }
    }

    private static string? GetLineWithContinuations(StreamReader inFile) {
        var s = inFile.ReadLine();
        if (s != null) {
            while (s.EndsWith("\\") && !inFile.EndOfStream) {
                s = string.Concat(s.AsSpan(0, s.Length - 1), inFile.ReadLine());
            }
        }
        return s;
    }

    private void AssembleLine(string s) {
        var SourceLine = new SourceCodeLine(s, currentFileFullPathName!, currentLineNumber);
        SourceLine.Parse(FinalPass);
    }

}
