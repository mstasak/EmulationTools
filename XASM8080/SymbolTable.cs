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
        BlockTab = new();
        BlockStacks = new();

    }


    public Dictionary<string, SymbolDefinition> SymbolTab; //complex globbed key [$=global]label[.sublabel]@file or blockname:label@filename
    public Dictionary<string, BlockDefinition> BlockTab; //key=globbed filename.blockname
    public Dictionary<string, Stack<string>> BlockStacks; //key=filename, stack = list of unique nestable blocknames

    public SymbolDefinition? Lookup(string name) {
        return SymbolTab[name];
    }

    public SymbolDefinition Add(SymbolDefinition sym) {
        SymbolDefinition? rslt = SymbolTab[sym.Name] as SymbolDefinition;
        //if (SymbolTab.ContainsKey(sym)) {

        //} else {
        //    SymbolTab.Add(sym.Name, sym);
        //}
        return rslt;
    }

    internal int SymbolValueUnresolvedCount() {
        return SymbolTab.Count(pair => pair.Value.WordValue == null);
    }

    internal int RefSymbolNotFoundCount() {
        return SymbolTab.Count(pair => pair.Value.DeclarationFileName == null);
    }

    internal static SymbolDefinition ProcessReference(SymbolDefinition sym) {
        //if sym exists, update properties from sym and return actual reference
        //else create unresolved ref in symbol table and return sym?
        throw new NotImplementedException();
    }
}
