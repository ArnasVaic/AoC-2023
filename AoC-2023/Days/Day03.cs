using System.Diagnostics.CodeAnalysis;
using ParsecSharp;
using static ParsecSharp.Parser;
using static ParsecSharp.Text;
using static System.Linq.Enumerable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AoC2023.Days;

public class Day03 : IDay
{

    public abstract class Cell
    {

    }

    public class EmptyCell : Cell
    {
    }

    public class SymbolCell : Cell
    {
    }

    public class NumberCell(int number, Guid id) : Cell
    {
        public int Number { get; } = number;
        public Guid Id { get; } = id;
    }

    [method: SetsRequiredMembers]
    public class Schematic(HashSet<(int, int)> symbolCoordinates, Cell[][] numberGrid)
    {
        public required HashSet<(int Row, int Col)> SymbolCoordinates { get; init; } = symbolCoordinates;

        public required Cell[][] NumberGrid { get; init; } = numberGrid;

        private HashSet<(int Row, int Col)> GetNeighborCoords(int row, int col) => 
            new List<(int, int)>
            {
                (row - 1, col - 1), (row - 1, col), (row - 1, col + 1),
                (row + 0, col - 1),                 (row + 0, col + 1),
                (row + 1, col - 1), (row + 1, col), (row + 1, col + 1),
            }
            .Where(p => p.Item1 >= 0 && p.Item1 < NumberGrid.Length)
            .Where(p => p.Item2 >= 0 && p.Item2 < NumberGrid[0].Length)
            .ToHashSet();

        public int PartSum()
        {
            var uniquePartCoordinates = SymbolCoordinates
                .SelectMany(p => GetNeighborCoords(p.Row, p.Col)
                    .Where(p => NumberGrid[p.Row][p.Col] is NumberCell))
                .DistinctBy(p =>
                {
                    var cell = NumberGrid[p.Row][p.Col] as NumberCell;
                    return cell!.Id;
                });

            return uniquePartCoordinates
                .Sum(p =>
                {
                    var cell = NumberGrid[p.Row][p.Col] as NumberCell;
                    return cell!.Number;
                });
        }
    }

    public static int Length(int num) => $"{num}".Length;

    public static Parser<char, Schematic> BuildInputParser()
    {
        var integer = OneOf("123456789").Append(Many(DecDigit())).ToInt();

        var number = integer.Map(num => new NumberCell(num, Guid.NewGuid())  as dynamic);

        var empty = Char('.').Map(_ => new EmptyCell() as dynamic);

        var symbol = Satisfy(c => !"0123456789.\r\n".Contains(c)).Map(_ => new SymbolCell() as dynamic);


        var cell = Choice(number, empty, symbol);

        var line = Many1(cell)
            .Left(String("\r\n"))
            .Map(cells => cells.SelectMany<dynamic, dynamic>(c =>
            {
                return Enumerable.Repeat(c, c is NumberCell numberCell? Length(numberCell.Number) : 1);
            }));
         
        var lines = Many1(line).Map(lines =>
        {
            var arrays = lines
                .Select(l => l
                    .Cast<Cell>()
                    .ToArray())
                .Cast<Cell[]>()
                .ToArray();

            var coords = new HashSet<(int, int)>();

            for(var i = 0; i < arrays.Length; ++i)
            {
                for(var j = 0; j < arrays[i].Length; ++j)
                {
                    if(arrays[i][j] is SymbolCell)
                        coords.Add((i, j));
                }
            }

            return new Schematic(coords, arrays);
        });

        return lines;
    }

    public string Part1(string input)
    {
        var parser = BuildInputParser(); 

        var result = parser.Parse(input);

        string answer = string.Empty;
        
        result.CaseOf(
            failure => answer = failure.Message,
            success =>
            {
                var schematic = success.Value;
                answer = $"{schematic.PartSum()}";
            }
        );

        return answer;
    }

    public string Part2(string input)
    {
        return "";
    }
}