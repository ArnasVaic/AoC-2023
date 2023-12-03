using System.Diagnostics.CodeAnalysis;
using ParsecSharp;
using static ParsecSharp.Parser;
using static ParsecSharp.Text;

namespace AoC2023.Days.Day02;

public class Day02 : IDay
{

    public enum Color
    {
        Red,
        Green,
        Blue
    }

    public static readonly Dictionary<Color, int> CubeCount = new()
    {
        { Color.Red, 12 },
        { Color.Green, 13 },
        { Color.Blue, 14 }
    };

    public static readonly HashSet<Color> Colors = [.. CubeCount.Keys];

    [method: SetsRequiredMembers]
    public class Game(int id, IEnumerable<Dictionary<Color, int>> sets)
    {
        public int Id { get; init; } = id;

        public required IEnumerable<Dictionary<Color, int>> Sets { get; init; } = sets;

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

    public static Parser<char, IEnumerable<Game>> CreateInputParser()
    {
        var integer = OneOf("123456789").Append(Many(DecDigit())).ToInt();
        
        // color ::= red | green | blue
        var color = Choice(Colors.Select(c => String($"{c}".ToLower()).Map(_ => c)));
        
        // colorNumber ::= space number space color
        var colorNumber = integer
            .Between(Char(' '))
            .Bind(number => color.Map(color => (color, number)));

        // set ::= colorNumber *( ',' colorNumber )
        var set = colorNumber
            .SeparatedBy(Char(','))
            .Map(pairs => pairs.ToDictionary(x => x.color, x => x.number));

        // game ::= Game space number ':' set *(set ';')
        var game = 

            // Game {id}:
            String("Game ").Right(integer).Left(Char(':'))

            // sets of pairs of cube colors and their counts separated by ';'
            .Bind(id =>
            {
                return set
                    .SeparatedBy(Char(';'))
                    .Map(sets => new Game(id, sets));
            });

        // games ::= game *(newline game)
        var games = game.SeparatedBy(String("\r\n"));

        return games;
    }

    private static IEnumerable<Game> GetGames(string input) =>
        CreateInputParser()
        .Parse(input)
        .Value;

    public string Part1(string input) => 
        GetGames(input)
        .Where(game => game.Ok())
        .Sum(game => game.Id)
        .ToString();

    public string Part2(string input) => 
        GetGames(input)
        .Sum(game => game.MinPower())
        .ToString();
}