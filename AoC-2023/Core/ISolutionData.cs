namespace AoC2023.Core;

/// <summary>
/// Solution data interface. This is a parsed structure that must contain
/// all necessary information within and must implement two functions
/// which return the answer for each part of the problem.
/// </summary>
public interface ISolutionData
{
    public string SolveFirst();

    public string SolveSecond();
}