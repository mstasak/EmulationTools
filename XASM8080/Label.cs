namespace XASM8080;

public enum LabelDeclKind {
    Global,      //GLOBAL ADDR1: ...
                 // or
                 //$ADDR1: ...

    File,        //STATIC ADDR2:    ;unique within file; internally, name is mangled with '@filename.ext suffix
                 // or
                 //ADDR2: ...

    LabelLocal,  //ADDR2: ;A normal file label
                 //.LOOP1: ... ;a local label, under most recent file label; name is automatically mangled by prepending ADDR2.
                 // or
                 //ADDR2.LOOP1: ... ;optional, more explicit label declaration

    //future ideas
    //Block,       //@BLOCK1: [BEGIN]
    //             //         ...
    //             //         END BLOCK1
    //             // or
    //             //@BLOCK1: END

    //BlockLocal,  //@BLOCK2: BEGIN
    //             //@OUTERLOOP: ...
    //             //@NESTEDBLOCK: BEGIN
    //             //@INNERLOOP:
    //             //       ...
    //             //       JZ @INNERLOOP ;jump within current block
    //             //       JMP @BLOCK2.OUTERLOOP ;jump to parent block
    //             //NESTEDBLOCK: ENDBLOCK
    //             //       ...
    //             //BLOCK2: ENDBLOCK
}

public enum LabelRefKind {
    Global,
    File,
    LabelLocal,
    BlockLocal,
    Block
}

public class Label {
    public string? explicitName;
    public LabelKind kind;
    public string name = "";
    public string? declaredInFile;
    public int? declaredInLine;
    //public string DeclaredInExpandedLine; //for future: when macro expansion increases line numbers below
    public ushort wordValue;

    public string? parseLabelDeclarationName(string source, ref int linePosition) {
        string? rslt = null;
        var newLinePosition = linePosition;



        if (rslt != null) {
            linePosition = newLinePosition;
        }
        return rslt;
    }

}