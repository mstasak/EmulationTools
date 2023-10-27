using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
public enum SymbolType {
    AddressGlobal,
    AddressFile,
    AddressLocal,
    AddressBlock,
    //TextString,
    //Boolean
}
public class SymbolDefinition {
    public string Name = "";
    public string? LocalSubLabel;
    public string? BlockName;
    public SymbolType SymbolType;
    public ushort? WordValue;
    public string? TextStringValue;
    public bool? BooleanValue;
    public int? ReferenceCount;
    public string? DeclarationFileName;
    public int? DeclarationLineNumber;
    public int? ResolvedInPass;
    public string UniqueKey => SymbolKey(
            Label: Name,
            LocalSubLabel: LocalSubLabel, 
            FileName: DeclarationFileName, 
            //LineNumber: DeclarationLineNumber, 
            BlockName: BlockName, 
            IsGlobal: SymbolType == SymbolType.AddressGlobal);

    public static string SymbolKey(
        string Label, 
        string? LocalSubLabel, 
        string? FileName, 
        //int? LineNumber, 
        string? BlockName, 
        bool IsGlobal) {

        //build a unique key for a label
        //labelname@filename#linenumber
        //  "normal" file-unique: abc@filename#linenumber
        //  "global" file-unique: $glob1@filename#linenumber
        //  "local" unique under label: normallabel.locallabel@filename#linenumber
        //  "block" unique within block: blockname:locallabel@filename#linenumber
        if (IsGlobal) {
            return '$' + Label + '@' + FileName; // + "#" + LineNumber;
        } else if (!string.IsNullOrEmpty(BlockName)) {
            return BlockName + ':' + Label + '@' + FileName; // + "#" + LineNumber;
        } else if (!string.IsNullOrEmpty(LocalSubLabel)) {
            return Label + '.' + LocalSubLabel + '@' + FileName; // + "#" + LineNumber;
        } else {
            // "normal" label
            return Label + '@' + FileName; // + "#" + LineNumber;
        }
    }

}

public class BlockDefinition {
    public string FileName;
    public string BlockName;
    public int? StartLine;
    public int? EndLine;

    public BlockDefinition(string fileName, string blockName, int? startLine, int? endLine) {
        FileName = fileName;
        BlockName = blockName;
        StartLine = startLine;
        EndLine = endLine;
    }
}