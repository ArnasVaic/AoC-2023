using ParsecSharp;
using static ParsecSharp.Parser;
using static ParsecSharp.Text;
using static AoC2023.CommonParsers;

namespace AoC2023.Days.Day04;

public class Card(int id, HashSet<int> winning, HashSet<int> numbers)
{
    public int Id { get; } = id;
    
    public IEnumerable<int> WinningIds => Enumerable
        .Range(Id + 1, numbers.Intersect(winning).Count());

    public static int TwoExp(int exp) => exp < 1 ? 0 : 1 << (exp - 1);

    public int Points => TwoExp(numbers.Intersect(winning).Count());
}

public class Solution(IEnumerable<Card> cards) : IData
{
    public string Part1() => cards
        .Select(card => card.Points)
        .Sum()
        .ToString();

    public string Part2()
    {
        var cardCounts = cards.ToDictionary(c => c.Id, c => 1);
        foreach (var card in cards)
            foreach (var id in card.WinningIds)
                cardCounts[id] += cardCounts[card.Id];
        return cardCounts.Values.Sum().ToString();
    }
}

public class Day04 : DayBase<Solution>
{
    protected override Parser<char, Solution> BuildParser()
    {
        var card =          String("Card").Right(Spaces())  // card identifier followed by spaces
            .Right  (       Integer)                        // card id
            .Left   (       Char(':'))                      // separator
            .Bind   (id =>  Many1(Spaces().Right(Integer))  // card numbers
            .Bind   (ws =>  Spaces().Left(Char('|'))        // separator
            .Right  (       Many1(Spaces().Right(Integer))  // winning numbers
            .Left   (       String("\r\n"))
            .Map    (ns =>  new Card(id, ws.ToHashSet(), ns.ToHashSet())))));

        return Many1(card)
            .Map(cards => new Solution(cards));
    }
};