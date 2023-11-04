using System.Text.RegularExpressions;

namespace XASM8080;

public enum LabelReferenceKind {
    None,
    Global,
    Static,
    Local
    //BlockLocal,
    //Block
}

public class LabelReference {
    public int PositionBeforeParse;

    public LabelReferenceKind Kind;
    public string? LabelText;
    public string? ParentText;
    public string? FileName;
    public string? FullLabelText;
    public int? LineNumber;
    public bool IsGlobal => Kind == LabelReferenceKind.Global;
    public bool IsStatic => Kind == LabelReferenceKind.Static;
    public bool IsLocal => Kind == LabelReferenceKind.Local;

    private static readonly Regex reGlobalLabelRefPattern1 = new(@"^\s*global\s+\$?([A-Z][0-9A-Z_]*)",                   RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
    private static readonly Regex reGlobalLabelRefPattern2 = new(@"^\s*\$([A-Z][0-9A-Z_]*)",                              RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
    private static readonly Regex reStaticLabelRefPattern1 = new(@"^\s*static\s+([A-Z][0-9A-Z_]*)",                       RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
    private static readonly Regex reStaticLabelRefPattern2 = new(@"^\s*([A-Z][0-9A-Z_]*)",                                 RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
    private static readonly Regex reLocalLabelRefPattern1  = new(@"^\s*local\s+([A-Z][0-9A-Z_]*)?\.([A-Z][0-9A-Z_]*)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
    private static readonly Regex reLocalLabelRefPattern2  = new(@"^\s*([A-Z][0-9A-Z_]*)?\.([A-Z][0-9A-Z_]*)",          RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

    public void ParseLabelReferenceName(string source, ref int linePosition) {
        Match match; //reused by various label patterns
        PositionBeforeParse = linePosition;
        //parse a global label
        // GLOBAL FNMULWORDS: ...
        // $FNMULWORDS:
        var sourcePart = source[linePosition..];
        match = reGlobalLabelRefPattern1.Match(sourcePart); //GLOBAL ADDR1:
        if (match.Success) {
            Kind = LabelReferenceKind.Global;
            ParentText = null;
            LabelText = match.Groups[1].Value;
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            return;
        }

        match = reGlobalLabelRefPattern2.Match(sourcePart); //$ADDR1:
        if (match.Success) {
            Kind = LabelReferenceKind.Global;
            ParentText = null;
            LabelText = match.Groups[1].Value;
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            return;
        }

        //parse a normal file-unique label
        // STATIC START: ...
        // START: ...
        match = reStaticLabelRefPattern1.Match(sourcePart); //JMP STATIC ADDR1
        if (match.Success) {
            Kind = LabelReferenceKind.Static;
            ParentText = null;
            LabelText = match.Groups[1].Value;
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            return;
        }

        //private static readonly Regex reStaticLabelRefPattern2 = new(@"^\s*([A-Z][0-9A-Z_]*)",
        //RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

        match = reStaticLabelRefPattern2.Match(sourcePart); //JMP ADDR1
        if (match.Success) {
            Kind = LabelReferenceKind.Static;
            ParentText = null;
            LabelText = match.Groups[1].Value;
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            return;
        }

        //parse a local label (parent.sublabel is unique; parent may be omitted, defaults to last normal label
        // LOCAL START.LOOP1: ...
        // START.LOOP1: ...
        // .LOOP1: ...
        match = reLocalLabelRefPattern1.Match(sourcePart); //JMP LOCAL [PARENTLABEL].ADDR1
        if (match.Success) {
            Kind = LabelReferenceKind.Local;
            if (match.Groups.Count > 2) { 
                ParentText = match.Groups[1].Value;
                LabelText = match.Groups[2].Value;
            } else { 
                ParentText = Assembler.Instance.MostRecentNormalLineLabel ?? "";
                LabelText = match.Groups[1].Value;
            }
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            return;
        }

        match = reLocalLabelRefPattern2.Match(sourcePart); //CALL [PARENTLABEL].ADDR1:
        if (match.Success) {
            Kind = LabelReferenceKind.Local;
            if (match.Groups.Count > 2) { 
                ParentText = match.Groups[1].Value;
                LabelText = match.Groups[2].Value;
            } else { 
                ParentText = Assembler.Instance.MostRecentNormalLineLabel ?? "";
                LabelText = match.Groups[1].Value;
            }
            linePosition += match.Value.Length;
            FullLabelText = match.Value;
            return;
        }

        Kind = LabelReferenceKind.None;
        return; //no match
    }

    public string? SymbolTableKey() {
        string? key;
        switch (Kind) {
            case LabelReferenceKind.None:
                key = null;
                break;
            case LabelReferenceKind.Global:
                key = "$" + LabelText +
                    "@" + FileName ?? "?";
                break;
            case LabelReferenceKind.Static:
                key = LabelText +
                    "@" + FileName ?? "?";
                break;
            case LabelReferenceKind.Local:
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