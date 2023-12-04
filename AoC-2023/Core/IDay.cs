namespace AoC2023.Core;

/// <summary>
/// Specific day solution interface. Each solution has parts 1 &amp; 2.
/// </summary>
public interface IDay
{
    string Part1(string input);

    string Part2(string input);
}