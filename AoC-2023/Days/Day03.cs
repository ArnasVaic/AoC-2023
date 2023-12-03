using System.Diagnostics.CodeAnalysis;
using ParsecSharp;
using static ParsecSharp.Parser;
using static ParsecSharp.Text;
using static System.Linq.Enumerable;

namespace AoC2023.Days.Day03;

public abstract record Cell;
public record EmptyCell : Cell;
public record SymbolCell(char Symbol) : Cell;
public record NumberCell(int Number, Guid Id) : Cell;

public readonly struct Coord(int row, int col)
{
    public int Row { get; } = row;
    public int Col { get; } = col;

    public static Coord operator + (Coord lhs, Coord rhs) => 
        new(lhs.Row + rhs.Row, lhs.Col + rhs.Col);

    public bool InsideRect(
        int bottom,
        int top,
        int left,
        int right
    ) => Row >= bottom && Row <= top && Col >= left && Col <= right;
};

[method: SetsRequiredMembers]
public class Schematic(Cell[][] numberGrid) : IData
{
    public int Height => numberGrid.Length;

    public int Width => numberGrid[0].Length;

    Cell At(Coord coord) => numberGrid[coord.Row][coord.Col];

    Cell At(int row, int col) => numberGrid[row][col];

    public Guid CellId(Coord coord) 
    {
        var cell = At(coord);

        if(cell is NumberCell numberCell)
            return numberCell.Id;

        throw new InvalidOperationException("Cell is not a number cell.");
    }

    public int CellNumber(Coord coord) 
    {
        var cell = At(coord);

        if(cell is NumberCell numberCell)
            return numberCell.Number;

        throw new InvalidOperationException("Cell is not a number cell.");
    }

    public bool IsNumber(Coord coord) => At(coord) is NumberCell;

    public bool CoordInsideGrid(Coord coord) =>
        coord.InsideRect(0, Height - 1, 0, Width - 1);

    private static readonly Coord[] NeighborOffsets = [
        new(-1, -1), new(-1, 0), new(-1, 1),
        new( 0, -1),             new( 0, 1),
        new( 1, -1), new( 1, 0), new( 1, 1),
    ];

    private IEnumerable<Coord> GetNeighborCoords(Coord point) => 
        NeighborOffsets
        .Select(offset => point + offset)
        .Where(CoordInsideGrid);

    private List<Coord> GetSymbolCoords(Predicate<char> predicate)
    {
        var symbolCoords = new List<Coord>();
        for(var row = 0; row < Height; ++row)
            for(var col = 0; col < Width; ++col)
                if(At(row, col) is SymbolCell symbol && predicate(symbol.Symbol))
                    symbolCoords.Add(new(row, col));

        return symbolCoords;
    }

    public IEnumerable<Coord> NeighborNumberCellCoords(Coord coord) =>
        GetNeighborCoords(coord)
        .Where(IsNumber)
        .DistinctBy(CellId);

    public string Part1() =>
        // All symbol coordinates
        GetSymbolCoords(_ => true)

        // For each, select neighbor numbers
        .SelectMany(NeighborNumberCellCoords)

        // Sum part numbers
        .Sum(CellNumber)

        .ToString();

    public string Part2() => 
        // '*' symbol coordinates
        GetSymbolCoords('*'.Equals)
        
        // Select coordinates of neighbor cells that are number cells
        .Select(NeighborNumberCellCoords)

        // Gears have only two number cells
        .Where(coords => coords.Count() is 2)

        // Sum all gear part number products
        .Sum(coord => coord.Select(CellNumber).Aggregate((p, q) => p * q))
        .ToString();
}

public class Day03 : DayBase<Schematic>, IDay
{
    protected override Parser<char, Schematic> BuildParser()
    {
        var integer = OneOf("123456789").Append(Many(DecDigit())).ToInt();

        var number = 
            integer
            .Map(num => new NumberCell(num, Guid.NewGuid()) as dynamic);

        var empty = 
            Char('.')
            .Map(_ => new EmptyCell() as dynamic);

        var symbol = 
            Satisfy(c => !"0123456789.\r\n".Contains(c))
            .Map(c => new SymbolCell(c) as dynamic);

        var cell = Choice(number, empty, symbol);

        var line = 
            Many1(cell)
            .Left(String("\r\n"))
            .Map(cells => cells.SelectMany<dynamic, dynamic>(c =>
            {
                return Enumerable.Repeat(c, c is NumberCell numberCell? $"{numberCell.Number}".Length : 1);
            }));
         
        var schematic = 
            Many1(line)
            .Map(lines =>
            {
                // Cast array element type from dynamic to Cell 
                var arrays = lines.Select(l => l
                        .Cast<Cell>().ToArray())
                    .Cast<Cell[]>().ToArray();

                return new Schematic(arrays);
            });

        return schematic;
    }

    public string Part1(string input) => SolvePart1(input);

    public string Part2(string input) => SolvePart2(input);
}