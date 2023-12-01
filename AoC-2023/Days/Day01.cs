namespace AoC2023.Days;

public class Day01 : IDay
{
    public string Part1(string filename)
    {
        var number = File
            .ReadAllText(filename)
            .Split('\n')
            .Where(line => line != string.Empty)
            .Sum(line =>
            {
                var fst = line.First(char.IsDigit) - '0';
                var snd = line.Last(char.IsDigit) - '0';
                return 10 * fst + snd;
            });
        
        return $"{number}";
    }

    public string Part2(string filename)
    {
        return "";
    }
}