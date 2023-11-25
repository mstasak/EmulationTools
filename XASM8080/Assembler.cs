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
 * See Dialect.md for special features and limitations.
**/


internal class Assembler {

    internal static readonly char[] WhitespaceChars = new char[] { ' ', '\t' };
    private readonly SymbolTable SymbolTable = SymbolTable.Instance;
    private readonly CodeGenerator CodeGenerator = CodeGenerator.Instance;
    internal DateTime AssemblyStartTime {
        get;
        private set;
    }
    internal DateTime AssemblyEndTime {
        get;
        private set;
    }
    private TimeSpan AssemblyElapsedTime => AssemblyEndTime - AssemblyStartTime;
    private int Pass;
    private int PriorPassUnresolvedSymbolRefs;
    private bool FinalPass;
    private int CurrentPassErrorCount = 0;


    //internal string? currentFileShortName;
    private string? currentFileFullPathName;
    private int currentLineNumber; //within file
    internal string? MostRecentNormalLineLabel; //for [label].locallabel symbols
    private static string FileErrorMsg;
    internal readonly List<string> InputFullFilePaths = new();

    ////private string? currentLineError;
    //private SourceCodeLine? currentLine;

    /// <summary>
    /// private getter to lazily construct instance
    /// </summary>
    private static readonly Lazy<Assembler> lazy =
        new(() => new Assembler());

    /// <summary>
    /// public accessor to Instance singleton
    /// </summary>
    internal static Assembler Instance => lazy.Value;

    /// <summary>
    /// Flag to interrupt assembly of a file as soon as END pseudo-op is encountered
    /// </summary>
    internal bool EndEncountered {
        get;
        set;
    }

    private Assembler() {
        var relativeOrFullNames = XASMMain.InputFileNames;
        InputFullFilePaths = new();
        foreach (var fPath in relativeOrFullNames) {
            var fi = new FileInfo(fPath);
            if (fi.Exists && CanOpenText(fi)) {
                InputFullFilePaths.Add(fi.FullName);
            } else {
                XASMMain.SessionError($"Error, could not find or open input file {fi.FullName} - {FileErrorMsg}.  Aborting.");
                XASMMain.Abort();
            }
        }
    }

    private static bool CanOpenText(FileInfo fi) {
        try {
            if (fi.Exists) {
                fi.OpenText().Dispose();
                return true;
            }
        } catch (Exception ex) {
            FileErrorMsg = ex.Message;
        }
        return false;
    }

    internal void Assemble() {
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
        CodeGenerator.Instance.Reset(); // Pass: Pass, Address: 0, FinalPass: FinalPass);
        EndEncountered = false;
        foreach (var fileName in InputFullFilePaths) {
            if (EndEncountered) {
                break;
            }
            var fi = new FileInfo(fileName);
            //currentFileShortName = fi.Name;
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
