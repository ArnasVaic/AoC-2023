using ParsecSharp;
using static ParsecSharp.Parser;
using static ParsecSharp.Text;

namespace AoC2023;

public static class CommonParsers
{
    public static readonly Parser<char, int> Integer = 
        OneOf("123456789")
        .Append(Many(DecDigit()))
        .ToInt();

    public static readonly Parser<char, Unit> Spaces =
        Many1(Char(' '))
        .Map(_ => Unit.Instance);
}