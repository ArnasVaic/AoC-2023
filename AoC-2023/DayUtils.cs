
using ParsecSharp;

namespace AoC2023;

public interface IData
{
    public string Part1();

    public string Part2();
}

public abstract class DayBase<TData> : IDay where TData : IData
{
    protected abstract Parser<char, TData> BuildParser();

    private string Part(string input, Func<IData, string> partFunc)
    {
        var parser = BuildParser();
        var answer = string.Empty;
        parser.Parse(input).CaseOf(
            failure => answer = failure.Message,
            success => answer = partFunc(success.Value)
        );
        return answer;
    }

    public string Part1(string input) => 
        Part(input, data => data.Part1());

    public string Part2(string input) => 
        Part(input, data => data.Part2());
}