using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace XASM8080;

public enum LabelDeclarationKind {
    None,
    Global,      //GLOBAL [$]ADDR1: ...
                 // or
                 //$ADDR1: ...

    Static,      //STATIC ADDR2:    ;unique within file; internally, name is mangled with '@filename.ext suffix
                 // or
                 //ADDR2: ...

    Local,       //ADDR3: ;A normal file label
                 //LOCAL ADDR3.LOOP1: ... ;a local label, under most recent file label; name is automatically mangled by prepending ADDR2.
                 // or
                 //ADDR3.LOOP1: ... ;optional, more explicit label declaration
                 // or
                 //.LOOP1: ... ;compact local label declaration (implicitly becomes ADDR3.LOOP1 where ADDR3 is nearest STATIC label above loop1)

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

public class LabelDeclaration {
    //public string? ExplicitName;
    public int PositionBeforeParse;

    public LabelDeclarationKind Kind;
    public string? LabelText; //label text, excluding "$" or "." prefix
    public string? ParentText;
    public string? FileName;
    public string? FullLabelText;
    public int? LineNumber;
    public bool IsGlobal => Kind == LabelDeclarationKind.Global;
    public bool IsStatic => Kind == LabelDeclarationKind.Static;
    public bool IsLocal => Kind == LabelDeclarationKind.Local;
    //public string DeclaredInExpandedLine; //for future: when macro expansion increases line numbers below

    private static readonly Regex reGlobalLabelDeclPattern1 = new("^\\s*global\\s+\\$?([a-z][0-9_a-z]*)(:?(?=\\s+\\s*equ)|:)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
    private static readonly Regex reGlobalLabelDeclPattern2 = new("^\\s*\\$([a-z][0-9_a-z]*)(:?(?=\\s+\\s*equ)|:)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
    private static readonly Regex reStaticLabelDeclPattern1 = new("^\\s*static\\s+([a-z][0-9_a-z]*)(:?(?=\\s+\\s*equ)|:)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
    private static readonly Regex reStaticLabelDeclPattern2 = new("^\\s*([a-z][0-9_a-z]*)(:?(?=\\s+\\s*equ)|:)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
    private static readonly Regex reLocalLabelDeclPattern1 = new("^\\s*local\\s+([a-z][0-9_a-z]*)\\.([a-z][0-9_a-z]*)(:?(?=\\s+\\s*equ)|:)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
    private static readonly Regex reLocalLabelDeclPattern2 = new("^\\s*([a-z][0-9_a-z]*)\\.([a-z][0-9_a-z]*)(:?(?=\\s+\\s*equ)|:)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
    private static readonly Regex reLocalLabelDeclPattern3 = new("^\\s*\\.([a-z][0-9_a-z]*)(:?(?=\\s+\\s*equ)|:)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

    public void ParseLabelDeclarationName(string source, ref int linePosition, string fileName, int lineNumber) {
        Match match; //reused by various label patterns
        PositionBeforeParse = linePosition;
        //parse a global label
        // GLOBAL FNMULWORDS: ...
        // $FNMULWORDS:
        match = reGlobalLabelDeclPattern1.Match(source[linePosition..]); //GLOBAL [$]ADDR1:
        if (match.Success) {
            Kind = LabelDeclarationKind.Global;
            ParentText = null;
            LabelText = match.Groups[1].Value.TrimEnd(':');
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            FileName = fileName;
            LineNumber = lineNumber;
            return;
        }

        match = reGlobalLabelDeclPattern2.Match(source[linePosition..]); //$ADDR1:
        if (match.Success) {
            Kind = LabelDeclarationKind.Global;
            ParentText = null;
            LabelText = match.Groups[1].Value.TrimEnd(':');
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            FileName = fileName;
            LineNumber = lineNumber;
            return;
        }

        //parse a normal file-unique label
        // STATIC START: ...
        // START: ...
        match = reStaticLabelDeclPattern1.Match(source[linePosition..]); //STATIC ADDR1:
        if (match.Success) {
            Kind = LabelDeclarationKind.Static;
            ParentText = null;
            LabelText = match.Groups[1].Value.TrimEnd(':');
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            FileName = fileName;
            LineNumber = lineNumber;
            return;
        }

        match = reStaticLabelDeclPattern2.Match(source[linePosition..]); //ADDR1:
        if (match.Success) {
            Kind = LabelDeclarationKind.Static;
            ParentText = null;
            LabelText = match.Groups[1].Value.TrimEnd(':');
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            FileName = fileName;
            LineNumber = lineNumber;
            return;
        }

        //parse a local label (parent.sublabel is unique; parent may be omitted, defaults to last normal label
        // LOCAL START.LOOP1: ...
        // START.LOOP1: ...
        // .LOOP1: ...
        match = reLocalLabelDeclPattern1.Match(source[linePosition..]); //LOCAL ADDR1:
        if (match.Success) {
            Kind = LabelDeclarationKind.Local;
            ParentText = match.Groups[1].Value;
            LabelText = match.Groups[2].Value.TrimEnd(':');
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            FileName = fileName;
            LineNumber = lineNumber;
            return;
        }

        match = reLocalLabelDeclPattern2.Match(source[linePosition..]); //PARENTLABEL.ADDR1:
        if (match.Success) {
            Kind = LabelDeclarationKind.Local;
            ParentText = match.Groups[1].Value;
            LabelText = match.Groups[2].Value.TrimEnd(':');
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            FileName = fileName;
            LineNumber = lineNumber;
            return;
        }

        match = reLocalLabelDeclPattern3.Match(source[linePosition..]); //.ADDR1:
        if (match.Success) {
            Kind = LabelDeclarationKind.Local;
            ParentText = Assembler.Instance.MostRecentNormalLineLabel; //may be null if misused (local with no static label above it)
            LabelText = match.Groups[1].Value;
            linePosition += match.Value.Length;
            FullLabelText = match.Value.TrimEnd(':');
            FileName = fileName;
            LineNumber = lineNumber;
            return;
        }
        Kind = LabelDeclarationKind.None;
        return; //no match
    }
    public string? SymbolTableKey() {
        string? key;
        switch (Kind) {
            case LabelDeclarationKind.None:
                key = null;
                break;
            case LabelDeclarationKind.Global:
                key = "$" + LabelText +
                    "@" + FileName ?? "?";
                break;
            case LabelDeclarationKind.Static:
                key = LabelText +
                    "@" + FileName ?? "?";
                break;
            case LabelDeclarationKind.Local:
                key = ParentText ?? "" +
                    "." + LabelText +
                    "@" + FileName ?? "?";
                break;
            default:
                key = null;
                break;
        }
        return key;
    }
}