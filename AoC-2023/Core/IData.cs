namespace AoC2023.Core;

/// <summary>
/// Solution data interface. This is a parsed structure that must contain
/// all necessary information within and be able to implement two functions
/// which return the answer for each part of the problem.
/// </summary>
public interface IData
{
    public string Part1();

    public string Part2();
}