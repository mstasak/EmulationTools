using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
public class SymbolTable {

    private static readonly Lazy<SymbolTable> lazy =
        new(() => new SymbolTable());

    public static SymbolTable Instance => lazy.Value;

    private SymbolTable() {
        SymbolTab = new();
        //BlockTab = new();
        //BlockStacks = new();

    }


    public Dictionary<string, SymbolDefinition> SymbolTab; //complex globbed key [$=global]label[.sublabel]@file or blockname:label@filename
    //public Dictionary<string, BlockDefinition> BlockTab; //key=globbed filename.blockname
    //public Dictionary<string, Stack<string>> BlockStacks; //key=filename, stack = list of unique nestable blocknames

    /// <summary>
    /// Return symbol table entry for lbl, or null if not found.
    /// If found in partially resolved state, add info if possible
    /// </summary>
    /// <param name="lbl">Label declaration, parsed from line of source code</param>
    /// <returns></returns>
    public SymbolDefinition? Lookup(LabelDeclaration lbl) {
        var key = lbl.SymbolTableKey();
        if (key == null) {
            return null;
        } else {
            SymbolDefinition rslt = null;
            var found = SymbolTab.TryGetValue(key, out rslt);
            
            var addStub = false;
            if (!found) { //2nd chance for a partial entry to an unresolved global
                var atPos = key.IndexOf('@'); //only forward referenced globals lack filename
                if (atPos >= 0) {
                    var key2 = key[0 .. (atPos + 1)] + "?"; //unresolved, priorly referenced global: "$LABEL5@?"
                    var foundStub = SymbolTab.TryGetValue(key2, out rslt);
                    if (foundStub && !string.IsNullOrEmpty(lbl.FileName) && lbl.FileName != "?") {
                        rslt.DeclarationFileName = lbl.FileName;
                        rslt.DeclarationLineNumber = lbl.LineNumber;
                        SymbolTab.Remove(key2);
                        SymbolTab[key] = rslt;
                    } else {
                        //add unresolved stub entry to SymbolTab
                        addStub = true;
                    }
                } else {
                    //new declaration (static or local)
                    addStub = true;
                }
                if (addStub) {
                    SymbolTab[key] = new SymbolDefinition() {
                        Kind = lbl.Kind switch {
                            LabelDeclarationKind.None => SymbolKind.None,
                            LabelDeclarationKind.Global => SymbolKind.Global,
                            LabelDeclarationKind.Static => SymbolKind.Static,
                            LabelDeclarationKind.Local => SymbolKind.Local,
                            _ => SymbolKind.None
                        },
                        ParentText = lbl.ParentText,
                        LabelText = lbl.LabelText,
                        DeclarationFileName = lbl.FileName,
                        DeclarationLineNumber = lbl.LineNumber,
                        WordValue = null
                    };
                }
            } else {
                //symbol already present with filename; add linenumber if missing
                //rslt.DeclarationFileName ??= lbl.FileName;
                rslt.DeclarationLineNumber ??= lbl.LineNumber;
            }
            return rslt;
        }
    }

    /// <summary>
    /// Return symbol table entry for lbl, or null if not found.
    /// </summary>
    /// <param name="lbl">Label declaration, parsed from line of source code</param>
    /// <returns></returns>
    public SymbolDefinition? Lookup(LabelReference lbl) {
        var key = lbl.SymbolTableKey();
        if (key == null) {
            return null;
        } else {
            SymbolDefinition? rslt;
            if (!SymbolTab.TryGetValue(key, out rslt)) {
                var atPos = key.IndexOf('@'); //only forward referenced globals lack filename
                if (atPos >= 0) {
                    var key2 = key[0 .. (atPos + 1)] + "?"; //unresolved, priorly referenced global: "$LABEL5@?"
                    if (key2 != key) {
                        //rslt = SymbolTab[key2];
                        SymbolTab.TryGetValue(key2, out rslt);
                    }
                }
                if (rslt == null) {
                    rslt = new SymbolDefinition() {
                        Kind = lbl.Kind switch {
                            LabelReferenceKind.None => SymbolKind.None,
                            LabelReferenceKind.Global => SymbolKind.Global,
                            LabelReferenceKind.Static => SymbolKind.Static,
                            LabelReferenceKind.Local => SymbolKind.Local,
                            _ => SymbolKind.None
                        },
                        ParentText = lbl.ParentText,
                        LabelText = lbl.LabelText,
                        DeclarationFileName = lbl.FileName,
                        DeclarationLineNumber = lbl.LineNumber,
                        WordValue = null
                    };
                    SymbolTab[key] = rslt;
                }
            //} else {
                //symbol already present, return as found - cannot add info
            }
            return rslt;
        }
    }

    //public SymbolDefinition Add(SymbolDefinition sym) {
    //    SymbolDefinition? rslt = SymbolTab[sym.ParentText] as SymbolDefinition;
    //    //if (SymbolTab.ContainsKey(sym)) {

    //    //} else {
    //    //    SymbolTab.Add(sym.Name, sym);
    //    //}
    //    return rslt;
    //}

    internal int SymbolValueUnresolvedCount() {
        return SymbolTab.Count(pair => pair.Value.WordValue == null);
    }

    internal int RefSymbolNotFoundCount() {
        return SymbolTab.Count(pair => pair.Value.DeclarationFileName == null);
    }

    //internal static SymbolDefinition ProcessReference(SymbolDefinition sym) {
    //    //if sym exists, update properties from sym and return actual reference
    //    //else create unresolved ref in symbol table and return sym?
    //    throw new NotImplementedException();
    //}
}
