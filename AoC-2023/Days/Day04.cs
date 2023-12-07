using AoC2023.Core;
using ParsecSharp;
using static ParsecSharp.Parser;
using static ParsecSharp.Text;
using static AoC2023.Utils.CommonParsers;

namespace AoC2023.Days.Day04;

public class Card(int id, HashSet<int> winning, HashSet<int> numbers)
{
    public int Id { get; } = id;
    
    public IEnumerable<int> WinningIds => Enumerable
        .Range(Id + 1, numbers.Intersect(winning).Count());

    public static int TwoExp(int exp) => exp < 1 ? 0 : 1 << (exp - 1);

    public int Points => TwoExp(numbers.Intersect(winning).Count());
}

public class SolutionData(IEnumerable<Card> cards) : ISolutionData
{
    public string SolveFirst() => cards
        .Select(card => card.Points)
        .Sum()
        .ToString();

    public string SolveSecond()
    {
        var cardCounts = cards.ToDictionary(c => c.Id, c => 1);
        foreach (var card in cards)
            foreach (var id in card.WinningIds)
                cardCounts[id] += cardCounts[card.Id];
        return cardCounts.Values.Sum().ToString();
    }
}

public class ParserBuilder : ISolutionDataParserBuilder<SolutionData>
{
    private static readonly Parser<char, Card> card =
                        String("Card").Right(Blanks)  // card identifier followed by spaces
        .Right  (       Integer)                      // card id
        .Left   (       Char(':'))                    // separator
        .Bind   (id =>  Many1(Blanks.Right(Integer))  // card numbers
        .Bind   (ws =>  Blanks.Left(Char('|'))        // separator
        .Right  (       Many1(Blanks.Right(Integer))  // winning numbers
        .Left   (       NL)
        .Map    (ns =>  new Card(id, ws.ToHashSet(), ns.ToHashSet())))));

    public Parser<char, SolutionData> Build() => 
        Many1(card).Map(cards => new SolutionData(cards));
}