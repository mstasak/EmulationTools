using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
public enum SymbolKind {
    None,
    Global,
    Static,
    Local,
    //Block,
    //TextString,
    //Boolean
}
public class SymbolDefinition {
    public SymbolKind Kind;
    public string? ParentText = "";
    public string? LabelText;
    public string? DeclarationFileName;
    public int? DeclarationLineNumber;
    public ushort? WordValue;

    public string? SymbolTableKey() {
        string? key;
        switch (Kind) {
            case SymbolKind.None:
                key = null;
                break;
            case SymbolKind.Global:
                if (DeclarationFileName == null) { 
                    key = null;
                } else {
                    key = "$" + LabelText +
                        "@" + DeclarationFileName ?? "?";
                }
                break;
            case SymbolKind.Static:
                if (DeclarationFileName == null) {
                    key = null;
                } else {
                    key = LabelText +
                    "@" + DeclarationFileName ?? "?";
                }
                break;
            case SymbolKind.Local:
                if (DeclarationFileName == null) {
                    key = null;
                } else {
                    key = ParentText ?? "" +
                    "." + LabelText +
                    "@" + DeclarationFileName ?? "?";
                }
                break;
            default:
                key = null;
                break;
        }
        return key;
    }


    internal string ToOutputLine() {
        var rslt = "";
        var kindStr = Kind switch {
            (SymbolKind.None) => "None",
            (SymbolKind.Global) => "Global",
            (SymbolKind.Static) => "Static",
            (SymbolKind.Local) => "Local",
            _ => "None"
        };
        rslt += ($"{kindStr,-6} ");
        if ((ParentText != null) || (Kind == SymbolKind.Local)) {
            rslt += ParentText + ".";
        }
        rslt += LabelText + "@" + DeclarationFileName + ":" + DeclarationLineNumber;
        if (WordValue != null) {
            rslt = $"{rslt,-70} = {WordValue:X4}";
        } else {
            rslt = $"{rslt,-70} = null (undefined)";
        }
        return rslt;
    }
    //public string? BlockName;
    //public string? TextStringValue; //actual string as expressed in declaration
    //public bool? BooleanValue;
    //public int? ReferenceCount;
    //public int? ResolvedInPass;
    //public string UniqueKey => SymbolKey(
    //        Label: ParentText,
    //        LocalSubLabel: LabelText, 
    //        FileName: DeclarationFileName, 
    //        //LineNumber: DeclarationLineNumber, 
    //        //BlockName: BlockName, 
    //        IsGlobal: Kind == SymbolKind.Global);

    //public static string SymbolKey(
    //    string Label, 
    //    string? LocalSubLabel, 
    //    string? FileName, 
    //    //int? LineNumber, 
    //    string? BlockName, 
    //    bool IsGlobal) {

    //    //build a unique key for a label
    //    //labelname@filename#linenumber
    //    //  "normal" file-unique: abc@filename#linenumber
    //    //  "global" file-unique: $glob1@filename#linenumber
    //    //  "local" unique under label: normallabel.locallabel@filename#linenumber
    //    //  "block" unique within block: blockname:locallabel@filename#linenumber
    //    if (IsGlobal) {
    //        return '$' + Label + '@' + FileName; // + "#" + LineNumber;
    //    } else if (!string.IsNullOrEmpty(BlockName)) {
    //        return BlockName + ':' + Label + '@' + FileName; // + "#" + LineNumber;
    //    } else if (!string.IsNullOrEmpty(LocalSubLabel)) {
    //        return Label + '.' + LocalSubLabel + '@' + FileName; // + "#" + LineNumber;
    //    } else {
    //        // "normal" label
    //        return Label + '@' + FileName; // + "#" + LineNumber;
    //    }
    //}

}

//public class BlockDefinition {
//    public string FileName;
//    public string BlockName;
//    public int? StartLine;
//    public int? EndLine;

//    public BlockDefinition(string fileName, string blockName, int? startLine, int? endLine) {
//        FileName = fileName;
//        BlockName = blockName;
//        StartLine = startLine;
//        EndLine = endLine;
//    }
//}