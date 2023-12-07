using AoC2023.Core;
using System.Diagnostics.CodeAnalysis;
using ParsecSharp;
using static ParsecSharp.Parser;
using static ParsecSharp.Text;
using static AoC2023.Utils.CommonParsers;

namespace AoC2023.Days.Day02;

public enum Color
{
    Red,
    Green,
    Blue
}

[method: SetsRequiredMembers]
public class Game(int id, IEnumerable<Dictionary<Color, int>> sets)
{
    public int Id { get; init; } = id;

    public required IEnumerable<Dictionary<Color, int>> Sets { get; init; } = sets;

    public static readonly Dictionary<Color, int> CubeCount = new()
    {
        { Color.Red, 12 },
        { Color.Green, 13 },
        { Color.Blue, 14 }
    };

    public static readonly HashSet<Color> Colors = [.. CubeCount.Keys];

    public int MinPower()
    {
        Dictionary<Color, int> minimum = new()
        {
            {Color.Red, 0},
            {Color.Green, 0}, 
            {Color.Blue, 0}, 
        };
        foreach(var set in Sets)
        {
            foreach(var color in Colors)
            {
                if(set.TryGetValue(color, out int value) && minimum[color] < value)
                    minimum[color] = value;
            }
        }

        return minimum.Values.Aggregate((a, b) => a * b);
    }

    public bool Ok() => Sets
        .All(set => CubeCount.All(pair => !set.ContainsKey(pair.Key) || set[pair.Key] <= pair.Value));
}

public class Data(IEnumerable<Game> games) : ISolutionData
{
    public string SolveFirst() => games
        .Where(game => game.Ok())
        .Sum(game => game.Id)
        .ToString();

    public string SolveSecond() => games
        .Sum(game => game.MinPower())
        .ToString();
}

public class ParserBuilder : ISolutionDataParserBuilder<Data>
{
    public Parser<char, Data> Build()
    {
        // color ::= red | green | blue
        var color = Choice(Game.Colors.Select(c => String($"{c}".ToLower()).Map(_ => c)));
        
        // colorNumber ::= space number space color
        var colorNumber = Integer
            .Between(Char(' '))
            .Bind(number => color.Map(color => (color, number)));

        // set ::= colorNumber *( ',' colorNumber )
        var set = colorNumber
            .SeparatedBy(Char(','))
            .Map(pairs => pairs.ToDictionary(x => x.color, x => x.number));

        // game ::= Game space number ':' set *(set ';')
        var game = 

            // Game {id}:
            String("Game ").Right(Integer).Left(Char(':'))

            // sets of pairs of cube colors and their counts separated by ';'
            .Bind(id =>
            {
                return set
                    .SeparatedBy(Char(';'))
                    .Map(sets => new Game(id, sets));
            });

        // games ::= game *(newline game)
        return game.SeparatedBy(NL).Map(games => new Data(games));
    }
}