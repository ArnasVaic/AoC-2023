using ParsecSharp;

namespace AoC2023.Core;

/// <summary>
/// Builds a parser for a specific problem.
/// </summary>
/// <typeparam name="TSolutionData">Type of data it parser</typeparam>
public interface ISolutionDataParserBuilder<TSolutionData> where TSolutionData : ISolutionData
{
    /// <summary>
    /// Build the input parser.
    /// </summary>
    /// <returns>Parser</returns>
    Parser<char, TSolutionData> Build();
}