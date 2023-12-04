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

    public static readonly Parser<char, Unit> Blanks =
        Many1(Char(' '))
        .Map(_ => Unit.Instance);


    /// <summary>
    /// Parser any variant of a new line.
    /// </summary>
    public static readonly Parser<char, Unit> NL = Parser.Or(
        String("\r\n").Map(_ => Unit.Instance),
        Char('\n').Map(_ => Unit.Instance));
}