// See https://aka.ms/new-console-template for more information
using System.CodeDom.Compiler;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace XASM8080;

public class XASMMain {

    static bool UsageNeeded;
    static bool HelpNeeded;
    static bool GenerateCode = true;
    static bool GenerateListing = true;
    static int ErrorLimit = 10;
    public static readonly List<string> InputFileNames = new();
    static int Main(string[] args) {

        ParseArgs(args);


        if (UsageNeeded) {
            PrintTitleAndUsage();
            return 1;
        }
        if (HelpNeeded) {
            PrintHelp();
            return 1;
        }
        //Console.WriteLine("Hello, World!");
        AssembleFiles();
        return 0;
    }

    private static void AssembleFiles() {
        //Assembler.CmdLineArgs = InputFileNames;
        //var asm = new Assembler(inputFileNames);
        Assembler.Instance.Assemble();
    }

    private static void ParseArgs(string[] args) {
        //TODO: consider more switches: case sensitivity, assembler dialect, macros, symbol table (as output file or at end of listing?), overflow tolerance (i.e. MVI A, 01FFH)
        //TODO: look into bin file formats which might include some property info like start address, size, checksum, name, min. peripheral requirements, etc.
        if (args.Count() == 0) {
            UsageNeeded = true;
        } else {
            var argIndex = -1;
            while (argIndex < args.Count()) {
                ++argIndex;
                var arg = args[argIndex];
                if (Regex.IsMatch(arg, "(^\\-h$)|(^\\-\\-help$)", RegexOptions.IgnoreCase)) {
                    HelpNeeded = true;
                    break;
                } else if (Regex.IsMatch(arg, "^\\-(c|\\-code|\\-com|\\-bin|\\-exe)\\+?$", RegexOptions.IgnoreCase)) {
                    GenerateCode = true;
                } else if (Regex.IsMatch(arg, "^\\-(c|\\-code|\\-com|\\-bin|\\-exe)\\-$", RegexOptions.IgnoreCase)) {
                    GenerateCode = false;
                } else if (Regex.IsMatch(arg, "^\\-(l|\\-listing|\\-prn|\\-lst)\\+?$", RegexOptions.IgnoreCase)) {
                    GenerateListing = true;
                } else if (Regex.IsMatch(arg, "^\\-(l|\\-listing|\\-prn|\\-lst)\\-$", RegexOptions.IgnoreCase)) {
                    GenerateListing = false;
                } else if (Regex.IsMatch(arg, "^\\-(e|\\-errors|\\-errorlimit)$", RegexOptions.IgnoreCase) && (arg != args.Last())) {
                    //(\\s*\\=[0-9]*)? nice but unnecessarily difficult for -e=25 instead of -e 25
                    var eLimit = int.Parse(args[++argIndex]); //use TryParse, handle failure
                    ErrorLimit = eLimit;
                } else if (Regex.IsMatch(arg, "^\\-.*$", RegexOptions.IgnoreCase)) {
                    //unrecognized or missing switch name
                    HelpNeeded = true;
                    break;
                } else {
                    //checked everything else; treat as a filename
                    //TODO: verify file exists and can be read; error out if not.
                    InputFileNames.Add(arg);
                }
            }
        }
    }

    private static void PrintHelp() {
        var exeName = Assembly.GetExecutingAssembly().GetName().Name;
        Console.Error.WriteLine($"""
                Usage: {exeName} [-b] [-l] [-e nnn] [-h] filename [,filename...]

                  filename - one or more assembler source files; the first will
                             determine the root of output filenames
                  
                  -c[-]    - generate executable code with .com extension; default
                             on; use -c- to disable
                  
                  -l[-]    - generate listing file with .lst extension; default on;
                             use -l- to disable

                  -e nnn   - stop after nnn errors (default 10)

                  -h       - show this help info
            """);
    }

    private static void PrintTitleAndUsage() {
        var exeName = Assembly.GetExecutingAssembly().GetName().Name;
        var exeVer = Assembly.GetExecutingAssembly().GetName().Version;
        Console.Error.WriteLine($"""
            {exeName} {exeVer}: a cross-assembler for compiling 8080 assembler code under Windows.
            Usage: {exeName} [options] filename [,filename...]
            For more info, use {exeName} -h or {exeName} --help
        """);
    }
}