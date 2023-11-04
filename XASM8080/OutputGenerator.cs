using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
public class OutputGenerator {

    private static readonly Lazy<OutputGenerator> lazy =
        new(() => new OutputGenerator());

    public static OutputGenerator Instance => lazy.Value;

    private OutputGenerator() {
        //OutputGen = new();
    }

    private static StreamWriter ListingStream;
    private static StreamWriter ErrorStream;
    private static StreamWriter SymbolsStream;
    private static int ErrorLimit = 10;
    private static string? FileBaseName;

    internal static void OutputStart() {
        FileBaseName = Assembler.Instance.InputFilePaths[0];
        var fi = new FileInfo(FileBaseName);
        var fExt = fi.Extension;
        FileBaseName = FileBaseName[0 .. (FileBaseName.Length - fExt.Length)];
        ListingStream = new StreamWriter(FileBaseName + ".lst", false, Encoding.ASCII, 32768);
        ErrorStream = new StreamWriter(FileBaseName + ".err", false, Encoding.ASCII, 32768);
        SymbolsStream = new StreamWriter(FileBaseName + ".xrf", false, Encoding.ASCII, 32768);
    }

    internal static void OutputLine(SourceCodeLine line) {
        ListingLine(line);
    }

    internal static void OutputEnd() {
        SymbolsBody();
        ListingStream.Close();
        ErrorStream.Close();
        SymbolsStream.Close();
    }

    internal static void BinaryOutput() {
    }

    internal static void ListingStart() {
    }
    internal static void ListingEnd() {
    }
    internal static void ListingLine(SourceCodeLine line) {
        //ListingStream.WriteLine("0000: 00 00 00  Label:      Opcode Op1, Op2   ;comment");
        //ListingStream.WriteLine($"{Assembler.Instance.currentLineNumber,5:D} {3:X4}{0xCD,3:X2}{0x12,3:X2} {0x34,3:X2}   Label:      {(line.Instruction?.Mnemonic)??""} Op1, Op2   ;comment");
        //var addr = CodeGenerator.Instance?.MemoryAddress;
        //if (addr != null) {
        //    addr = (ushort)(addr - (line.OutputBytes?.Count ?? 0));
        //}
        var addr = line.startAddr;
        var addrStr = (addr == null) ? "    " : $"{addr:X4}";

        if (line.OutputBytes?.Count > 0) {
            for (var i = 0; i < line.OutputBytes.Count; i += 3) {
                if (i == 0) {
                    ListingStream.Write($"{line.SourceLineNumber,5:D} {addrStr}");
                } else { 
                    ListingStream.Write($"      {addr + i:X4}");
                }
                for (var j = i; j < i + 3; j++) {
                    if (j < line.OutputBytes.Count) {
                        ListingStream.Write($"{line.OutputBytes[j],3:X2}");
                    } else {
                        ListingStream.Write("   ");
                    }
                }
                if (i == 0) {
                    ListingStream.Write(" ");
                    ListingStream.WriteLine(line.Source);
                } else {
                    ListingStream.WriteLine();
                }
            }
        } else {
            ListingStream.WriteLine($"{line.SourceLineNumber,5:D} {addrStr}          {line.Source}");
        }
    }

    internal static void ErrorsStart() {
    }
    internal static void ErrorsEnd() {
    }
    internal static void ErrorLine(SourceCodeLine line) {
    }

    internal static void SymbolsStart() {
    }
    internal static void SymbolsEnd() {
    }
    internal static void SymbolsBody() {
        SymbolsStream.WriteLine("*** SYMBOL TABLE ***");
        SymbolsStream.WriteLine($"{SymbolTable.Instance.SymbolTab.Count} entries.");
        foreach (var kv in SymbolTable.Instance.SymbolTab) {
            var sym = kv.Value;
            SymbolsStream.WriteLine(sym.ToOutputLine());
        }
        SymbolsStream.WriteLine("*** END ***");
    }
}
