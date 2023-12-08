using AoC2023.Core;
using ParsecSharp;
using static ParsecSharp.Text;
using static ParsecSharp.Parser;
using static AoC2023.Utils.CommonParsers;

namespace AoC2023.Days.Day05;

public class SolutionData(
    IEnumerable<long> seeds,
    IEnumerable<Func<long , long>> convertCategory
) : ISolutionData
{

    public string SolveFirst()
    {
        var s = seeds;
        return "";
    }

    public string SolveSecond()
    {
        throw new NotImplementedException();
    }
}


public class ParserBuilder : ISolutionDataParserBuilder<SolutionData>
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

    public Parser<char, SolutionData> Build()
    {
        var twoNL = Parser.Repeat(NL, 2);

        var seeds = String("seeds: ")
            .Right(Long.SeparatedBy(Char(' ')));

        var mapline =       Long
            .Bind   (dst => Long
            .Bind   (src => Long
            .Map    (cnt => (src, dst, cnt))));

        var map = 
            TakeWhile(c => c != ':').Right(Char(':')).Right(NL)
            .Right(Many1(mapline).Left(NL))
            .Map(BuildFunc);

        // return seeds.Left(NL)
        //    .Bind(ss => Many1(map).Between(Parser.Repeat(NL, 2))
        //    .Map(fns => new SolutionData(ss, fns)))
        //    .Left(NL);

        return 
            seeds.Left(twoNL)
            .Bind(ss => map
            .Map(fn => new SolutionData(ss, [fn])));
    }
}