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

    public class SymbolCell(char symbol) : Cell
    {
        public char Symbol { get; } = symbol;
    }

    public class NumberCell(int number, Guid id) : Cell
    {
        public int Number { get; } = number;
        public Guid Id { get; } = id;
    }

    [method: SetsRequiredMembers]
    public class Schematic(Cell[][] numberGrid)
    {
        public required Cell[][] NumberGrid { get; init; } = numberGrid;

        private HashSet<(int Row, int Col)> GetNeighborCoords((int Row, int Col) point) => 
            new List<(int, int)>
            {
                (point.Row - 1, point.Col - 1), (point.Row - 1, point.Col), (point.Row - 1, point.Col + 1),
                (point.Row + 0, point.Col - 1),                             (point.Row + 0, point.Col + 1),
                (point.Row + 1, point.Col - 1), (point.Row + 1, point.Col), (point.Row + 1, point.Col + 1),
            }
            .Where(p => p.Item1 >= 0 && p.Item1 < NumberGrid.Length)
            .Where(p => p.Item2 >= 0 && p.Item2 < NumberGrid[0].Length)
            .ToHashSet();

        private HashSet<(int Row, int Col)> GetSymbolLocations(Predicate<char> predicate)
        {
            var symbolCoords = new HashSet<(int, int)>();
            
            for(var i = 0; i < NumberGrid.Length; ++i)
                for(var j = 0; j < NumberGrid[i].Length; ++j)
                    if(NumberGrid[i][j] is SymbolCell symbol && predicate(symbol.Symbol))
                        symbolCoords.Add((i, j));

            return symbolCoords;
        }

        public int PartSum()
        {
            var uniquePartCoordinates = 
                GetSymbolLocations(_ => true)
                .SelectMany(symbolCoord => 
                    GetNeighborCoords(symbolCoord)
                    .Where(partCoord => NumberGrid[partCoord.Row][partCoord.Col] is NumberCell))
                .DistinctBy(partCoord =>
                {
                    var cell = NumberGrid[partCoord.Row][partCoord.Col] as NumberCell;
                    return cell!.Id;
                });

            return uniquePartCoordinates.Sum(partCoord =>
            {
                var cell = NumberGrid[partCoord.Row][partCoord.Col] as NumberCell;
                return cell!.Number;
            });
        }

        public int GearSum()
        {
            return GetSymbolLocations('*'.Equals)

                // Filter out non gear symbols (count of numbers around it is not 2)
                .Where(symbolCoord =>
                {
                    var neighborCoords = GetNeighborCoords(symbolCoord);

                    var numberNeighborCoords = neighborCoords
                        .Where(c => NumberGrid[c.Row][c.Col] is NumberCell)
                        .DistinctBy(c =>
                        {
                            var cell = NumberGrid[c.Row][c.Col] as NumberCell;
                            return cell!.Id;
                        });

                    return numberNeighborCoords.Count() == 2;
                })

                // Get coordinates of the parts around them
                .Select(symbolCoord => {

                    var neighborCoords = GetNeighborCoords(symbolCoord);

                    var numberNeighborCoords = neighborCoords
                        .Where(c => NumberGrid[c.Row][c.Col] is NumberCell)
                        .DistinctBy(c =>
                        {
                            var cell = NumberGrid[c.Row][c.Col] as NumberCell;
                            return cell!.Id;
                        });

                    return numberNeighborCoords;
                })
 
                // Sum part number products for each gear
                .Sum(partCoords =>
                {
                    // Multiply all part numbers around each gear 
                    var value = partCoords.Select(coord =>
                    {
                        var cell = NumberGrid[coord.Row][coord.Col] as NumberCell;
                        return cell!.Number;
                    }).Aggregate((p, q) => p * q);

                    
                    return value;
                });
        }
    }

    public static int Length(int num) => $"{num}".Length;

    public static Parser<char, Schematic> BuildInputParser()
    {
        var integer = OneOf("123456789").Append(Many(DecDigit())).ToInt();

        var number = integer.Map(num => new NumberCell(num, Guid.NewGuid())  as dynamic);

        var empty = Char('.').Map(_ => new EmptyCell() as dynamic);

        var symbol = Satisfy(c => !"0123456789.\r\n".Contains(c)).Map(c => new SymbolCell(c) as dynamic);


        var cell = Choice(number, empty, symbol);

        var line = Many1(cell)
            .Left(String("\r\n"))
            .Map(cells => cells.SelectMany<dynamic, dynamic>(c =>
            {
                return Enumerable.Repeat(c, c is NumberCell numberCell? Length(numberCell.Number) : 1);
            }));
         
        var lines = Many1(line).Map(lines =>
        {
            // Cast array element type from dynamic to Cell 
            var arrays = lines.Select(l => l
                    .Cast<Cell>().ToArray())
                .Cast<Cell[]>().ToArray();

            return new Schematic(arrays);
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
        var parser = BuildInputParser(); 

        var result = parser.Parse(input);

        string answer = string.Empty;
        
        result.CaseOf(
            failure => answer = failure.Message,
            success =>
            {
                var schematic = success.Value;
                answer = $"{schematic.GearSum()}";
            }
        );

        return answer;
    }
}