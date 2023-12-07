using AoC2023.Core;
using ParsecSharp;
using static ParsecSharp.Parser;
using static ParsecSharp.Text;

namespace AoC2023.Days.Day01;

public class Data(string input) : ISolutionData
{
    private static readonly Dictionary<string, int> NumberLut = new()
    {
        { "one", 1 },
        { "two", 2 },
        { "three", 3 },
        { "four", 4 },
        { "five", 5 },
        { "six", 6 },
        { "seven", 7 },
        { "eight", 8 },
        { "nine", 9 },
    };

    public string SolveFirst()
    {
        var number = input
            .Split('\n')
            .Where(line => line != string.Empty)
            .Sum(line =>
            {
                var fst = line.First(char.IsDigit) - '0';
                var snd = line.Last(char.IsDigit) - '0';
                return 10 * fst + snd;
            });
        
        return $"{number}";
    }

    public string SolveSecond()
    {
        var number = input
            .Split('\n')
            .Where(line => line != string.Empty)
            .Sum(line =>
            {
                var strPairs = NumberLut
                    .Keys
                    .Select(key => (NumberLut[key], line.IndexOf(key, StringComparison.Ordinal)))
                    .Where(p => p.Item2 is not -1);

                var strLastPairs = NumberLut
                    .Keys
                    .Select(key => (NumberLut[key], line.LastIndexOf(key, StringComparison.Ordinal)))
                    .Where(p => p.Item2 is not -1);
                
                var numPairs = NumberLut
                    .Values
                    .Select(val => (val, line.IndexOf($"{val}", StringComparison.Ordinal)))
                    .Where(p => p.Item2 is not -1);

                var numLastPairs = NumberLut
                    .Values
                    .Select(val => (val, line.LastIndexOf($"{val}", StringComparison.Ordinal)))
                    .Where(p => p.Item2 is not -1);
                
                var combined = strPairs
                    .Concat(strLastPairs)
                    .Concat(numPairs)
                    .Concat(numLastPairs)
                    .ToList();
                
                var fst = combined.MinBy(x => x.Item2).Item1; 
                var snd = combined.MaxBy(x => x.Item2).Item1;
                
                return 10 * fst + snd;
            });
        
        return $"{number}";
    }
}

public class ParserBuilder : ISolutionDataParserBuilder<Data>
{
    public Parser<char, Data> Build() =>
        Many1(Any()).Map(chars => new Data(string.Concat(chars)));
}