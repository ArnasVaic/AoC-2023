using AoC2023.Core;
using ParsecSharp;
using static ParsecSharp.Text;
using static ParsecSharp.Parser;
using static AoC2023.Utils.CommonParsers;
using System.Runtime.InteropServices;

namespace AoC2023.Days.Day05;

public class SolutionData(
    IEnumerable<long> seeds,
    IEnumerable<Func<long , long>> converters
) : ISolutionData
{

    public string SolveFirst()
    {
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
        var twoNL = Parser.Repeat(NewLine(), 2);

        var seeds = String("seeds: ")
            .Right(Long.SeparatedBy(Char(' ')))
            .Left(twoNL);

        var mapLine =       Long.Left(Char(' '))
            .Bind   (dst => Long.Left(Char(' '))
            .Bind   (src => Long.Left(NL) 
            .Map    (cnt => (src, dst, cnt))));

        var mapHeader = TakeWhile(c => c != '\n').Left(NewLine());

        var map = mapHeader
            .Right(Many1(mapLine))
            .Map(BuildFunc);

        return      seeds
           .Bind(   ss => Parser.Repeat(map, 7) // .Between(NL)
           .Map (   fns => new SolutionData(ss, fns)));
           //.Left(   NL);

        return seeds.Bind(ss => 
            map
            .Map(line =>
        {
            Console.WriteLine(line);
            return new SolutionData(ss, null);
        }));
    }
}