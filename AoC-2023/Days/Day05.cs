using AoC2023.Core;
using ParsecSharp;
using static ParsecSharp.Text;
using static ParsecSharp.Parser;
using static AoC2023.Utils.CommonParsers;

namespace AoC2023.Days.Day05;

public class SolutionData(
    IEnumerable<long> seeds,
    IEnumerable<IEnumerable<(long Source, long Destination, long Count)>> categoryRanges
) : ISolutionData
{

    public static Func<long, long> BuildFunc(IEnumerable<(long, long, long)> triples) => cat =>
    {
        foreach (var (src, dest, cnt) in triples)
        {
            if(cat >= src && cat <= src + cnt - 1)
            {
                return cat - src + dest;
            }
        }

        return cat;
    };

    public string SolveFirst()
    {
        var converters = categoryRanges.Select(BuildFunc);

        var locations = seeds.Select(s =>
        {
            var current = s;
            foreach (var converter in converters)
            {
                current = converter(current);
            }
            return current;
        });

        var ans = locations.Min();
        return $"{ans}";
    }

    public string SolveSecond()
    {
        throw new NotImplementedException();
    }
}


public class ParserBuilder : ISolutionDataParserBuilder<SolutionData>
{
    public Parser<char, SolutionData> Build()
    {
        var seeds = String("seeds: ")
            .Right(Long.SeparatedBy(Char(' ')))
            .Left(NewLine())
            .Left(NewLine());

        var triple =       Long.Left(Char(' '))
            .Bind   (dst => Long.Left(Char(' '))
            .Bind   (src => Long.Left(NewLine()) 
            .Map    (cnt => (src, dst, cnt))));

        var mapHeader = TakeWhile(c => c != '\n').Left(NewLine());
        var map = mapHeader.Right(Many1(triple));

        return              seeds
            .Bind(sds =>    map.SeparatedBy(NewLine())
            .Map (maps =>   new SolutionData(sds, maps))); 
    }
}