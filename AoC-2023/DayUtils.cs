
using ParsecSharp;

namespace AoC2023;

public interface IData
{
    public string Part1();

    public string Part2();
}

public abstract class DayBase<TData> where TData : IData
{
    protected abstract Parser<char, TData> BuildParser();

    public string SolvePart1(string input)
    {
        var parser = BuildParser();
        var answer = string.Empty;
        parser.Parse(input).CaseOf(
            failure => answer = failure.Message,
            success => answer = success.Value.Part1()
        );
        return answer;
    }

    public string SolvePart2(string input)
    {
        var parser = BuildParser();
        var answer = string.Empty;
        parser.Parse(input).CaseOf(
            failure => answer = failure.Message,
            success => answer = success.Value.Part2()
        );
        return answer;
    }
}