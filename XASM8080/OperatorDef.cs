using static XASM8080.SourceCodeLine;

namespace XASM8080;
public class OperatorDef {
    public int Precedence;
    public string Operator;
    public EnumOperatorType OperatorType;
    public Func<ushort?, ushort?, ushort?> Calculate;
    public OperatorDef(int precedence, string operatorText, EnumOperatorType operatorType, Func<ushort?, ushort?, ushort?> calculate) {
        Precedence = precedence;
        Operator = operatorText;
        OperatorType = operatorType;
        Calculate = calculate;
    }

    public static OperatorDef[] Operators = {
        new OperatorDef(precedence: 0, operatorText: "|", operatorType: EnumOperatorType.Binary, calculate: BitwiseOrWord),
        //new OperatorDef(precedence: 0, operatorText: "OR", operatorType: EnumOperatorType.Binary, calculate: BitwiseOrWord),
        new OperatorDef(precedence: 1, operatorText: "^", operatorType: EnumOperatorType.Binary, calculate: BitwiseXorWord),
        //new OperatorDef(precedence: 1, operatorText: "XOR", operatorType: EnumOperatorType.Binary, calculate: BitwiseXorWord),
        new OperatorDef(precedence: 2, operatorText: "&", operatorType: EnumOperatorType.Binary, calculate: BitwiseAndWord),
        //new OperatorDef(precedence: 2, operatorText: "AND", operatorType: EnumOperatorType.Binary, calculate: BitwiseAndWord),
        new OperatorDef(precedence: 3, operatorText: "<<", operatorType: EnumOperatorType.Binary, calculate: BitwiseShiftLeftWord),
        //new OperatorDef(precedence: 3, operatorText: "SHL", operatorType: EnumOperatorType.Binary, calculate: BitwiseShiftLeftWord),
        new OperatorDef(precedence: 3, operatorText: ">>", operatorType: EnumOperatorType.Binary, calculate: BitwiseShiftRightWord),
        //new OperatorDef(precedence: 3, operatorText: "SHR", operatorType: EnumOperatorType.Binary, calculate: BitwiseShiftRightWord),
        new OperatorDef(precedence: 4, operatorText: "+", operatorType: EnumOperatorType.Binary, calculate: BitwiseAddWord),
        new OperatorDef(precedence: 4, operatorText: "-", operatorType: EnumOperatorType.Binary, calculate: BitwiseSubtractWord),
        new OperatorDef(precedence: 5, operatorText: "*", operatorType: EnumOperatorType.Binary, calculate: BitwiseMultiplyWord),
        new OperatorDef(precedence: 5, operatorText: "/", operatorType: EnumOperatorType.Binary, calculate: BitwiseDivideWord),
        new OperatorDef(precedence: 5, operatorText: "%", operatorType: EnumOperatorType.Binary, calculate: BitwiseModuloWord),
        //new OperatorDef(precedence: 5, operatorText: "MOD", operatorType: EnumOperatorType.Binary, calculate: BitwiseModuloWord),
        new OperatorDef(precedence: 6, operatorText: "+", operatorType: EnumOperatorType.Prefix, calculate: BitwisePrefixPlus),
        new OperatorDef(precedence: 6, operatorText: "-", operatorType: EnumOperatorType.Prefix, calculate: BitwisePrefixMinus),
        new OperatorDef(precedence: 6, operatorText: "~", operatorType: EnumOperatorType.Prefix, calculate: BitwisePrefixNot),
        new OperatorDef(precedence: 6, operatorText: "<", operatorType: EnumOperatorType.Prefix, calculate: BitwisePrefixLowByte),
        //new OperatorDef(precedence: 6, operatorText: "LO", operatorType: EnumOperatorType.Prefix, calculate: BitwisePrefixLowByte),
        new OperatorDef(precedence: 6, operatorText: ">", operatorType: EnumOperatorType.Prefix, calculate: BitwisePrefixHighByte),
        //new OperatorDef(precedence: 6, operatorText: "HI", operatorType: EnumOperatorType.Prefix, calculate: BitwisePrefixHighByte),
    };

    private static ushort? BitwiseOrWord(ushort? a, ushort? b) {
        if (a == null || b == null) return null;
        return (ushort)(a | b);
    }
    private static ushort? BitwiseXorWord(ushort? a, ushort? b) {
        if (a == null || b == null) return null;
        return (ushort)(a ^ b);
    }
    private static ushort? BitwiseAndWord(ushort? a, ushort? b) {
        if (a == null || b == null) return null;
        return (ushort)(a & b);
    }
    private static ushort? BitwiseShiftLeftWord(ushort? a, ushort? b) {
        if (a == null || b == null) return null;
        return (ushort)(a << b);
    }
    private static ushort? BitwiseShiftRightWord(ushort? a, ushort? b) {
        if (a == null || b == null) return null;
        return (ushort)(a >> b);
    }
    private static ushort? BitwiseAddWord(ushort? a, ushort? b) {
        if (a == null || b == null) return null;
        return (ushort)(a + b);
    }
    private static ushort? BitwiseSubtractWord(ushort? a, ushort? b) {
        if (a == null || b == null) return null;
        return (ushort)(a - b);
    }
    private static ushort? BitwiseMultiplyWord(ushort? a, ushort? b) {
        if (a == null || b == null) return null;
        return (ushort)(a * b);
    }
    private static ushort? BitwiseDivideWord(ushort? a, ushort? b) {
        if (a == null || b == null) return null;
        return (ushort)(a / b);
    }
    private static ushort? BitwiseModuloWord(ushort? a, ushort? b) {
        if (a == null || b == null) return null;
        return (ushort)(a % b);
    }
    private static ushort? BitwisePrefixPlus(ushort? a, ushort? _) {
        if (a == null) return null;
        return (ushort)a;
    }
    private static ushort? BitwisePrefixMinus(ushort? a, ushort? _) {
        if (a == null) return null;
        return (ushort)-a;
    }
    private static ushort? BitwisePrefixNot(ushort? a, ushort? _) {
        if (a == null) return null;
        return (ushort)~a;
    }
    private static ushort? BitwisePrefixLowByte(ushort? a, ushort? _) {
        if (a == null) return null;
        return (ushort)(a & 0xff);
    }
    private static ushort? BitwisePrefixHighByte(ushort? a, ushort? _) {
        if (a == null) return null;
        return (ushort)(a >> 8);
    }

}
