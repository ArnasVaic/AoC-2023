using System.Diagnostics;
using System.Reflection;
using AoC2023.Core;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using ParsecSharp;
using static System.AppDomain;
using static System.String;

namespace AoC2023;

public static class Program
{
    private static async Task FetchInputAsync(IConfiguration configuration, int day)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("cookie", configuration["cookie"]);
        var directoryBackup = Directory.GetCurrentDirectory();
        var assemblyPath = Assembly.GetEntryAssembly()!.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
        Directory.SetCurrentDirectory(assemblyDirectory!);

        var inputFileDirectory = configuration["inputFileDirectory"];
        var inputUrl = Format(configuration["inputUrl"]!, day);
        var miniInputUrl =  Format(configuration["miniInputUrl"]!, day);

        var result = await client.GetAsync(inputUrl);
        var content = await result.Content.ReadAsStringAsync();

        var inputPath =             $"{inputFileDirectory}/Day{day:D2}.txt";
        var part1MiniInputPath =    $"{inputFileDirectory}/Day{day:D2}_1.txt";
        var part2MiniInputPath =    $"{inputFileDirectory}/Day{day:D2}_2.txt";

        if(!Directory.Exists(inputFileDirectory))
        {
            Directory.CreateDirectory(inputFileDirectory!);
        }

        Console.WriteLine($"Fetching input from '{inputUrl}'.");

        if(File.Exists(inputPath))
        {
            Console.WriteLine($"Input file '{inputPath}' already exists.");
        }
        else
        {
            if(!result.IsSuccessStatusCode)
            {
                Console.WriteLine($"Could not fetch input: {content}");
            }
            else
            {
                File.WriteAllText(inputPath, content);
            }
        }

        result = await client.GetAsync(miniInputUrl);
        content = await result.Content.ReadAsStringAsync();

        if(!result.IsSuccessStatusCode)
        {
            Console.WriteLine($"Could not fetch mini input: {content}");
        }
        else
        {
            var document = new HtmlDocument();
            document.LoadHtml(content);
            var resultNodes = document
                .DocumentNode
                .SelectNodes("//pre/code");

            if (resultNodes is null)
            {
                Console.WriteLine("Could not extract mini input data.");
            }
            else
            {
                var filenamesWithText = resultNodes
                    .Select(x => x.InnerText)
                    .Zip([part1MiniInputPath, part2MiniInputPath]);

                foreach (var (text, filename) in filenamesWithText)
                {
                    if(File.Exists(filename))
                    {
                        Console.WriteLine($"Input file '{filename}' already exists.");
                    }
                    else
                    {
                        File.WriteAllText(filename, text);
                    }
                }
            }
        }

        Directory.SetCurrentDirectory(directoryBackup);
    }

    private record DayInfo(int Day, int Part, bool Mini);

    private static readonly Func<int, List<DayInfo>> BuildDayInfo = day =>
    [
        new(day, 1, true),
        new(day, 1, false),
        new(day, 2, true),
        new(day, 2, false),
    ];

    private static void HandleDay(IConfiguration configuration, dynamic parser, DayInfo dayInfo)
    {
        var assemblyPath = Assembly.GetEntryAssembly()!.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
        Directory.SetCurrentDirectory(assemblyDirectory!);

        var miniText = dayInfo.Mini ? $"_{dayInfo.Part}" : Empty;
        var inputFileName = $"{configuration["inputFileDirectory"]}/Day{dayInfo.Day:D2}{miniText}.txt";
        var stopwatch = new Stopwatch();
        miniText = dayInfo.Mini ? "mini" : "full";
        try
        {
            var input = File.ReadAllText(inputFileName);
            stopwatch.Start();
            var ans = dayInfo.Part is 1 ? SolveFirst(input, parser) : SolveSecond(input, parser);
            stopwatch.Stop();
            Console.WriteLine($"[{miniText}, Part {dayInfo.Part}, t = {stopwatch.ElapsedMilliseconds}ms]: {ans}");
        }
        catch(Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"[{miniText}, Part {dayInfo.Part}, t = {stopwatch.ElapsedMilliseconds}ms]: {ex.Message}");
        }
    } 

    private static string Solve<TSolutionData>(
        string input,
        Parser<char, TSolutionData> parser,
        Func<TSolutionData, string> getSolution) where TSolutionData : ISolutionData
    {
        var answer = "empty answer";
        var result = parser.Parse(input);
        result.CaseOf(
            failure => answer = failure.Message,
            success => answer = getSolution(success.Value)
        );
        return answer;  
    }

    private static string SolveFirst<TSolutionData>(
        string input,
        Parser<char, TSolutionData> parser) 
        where TSolutionData : ISolutionData =>
        Solve(input, parser, d => d.SolveFirst());

    private static string SolveSecond<TSolutionData>(
        string input,
        Parser<char, TSolutionData> parser) 
        where TSolutionData : ISolutionData =>
        Solve(input, parser, d => d.SolveSecond());

    private static void Run(IConfiguration configuration, int day)
    {
        var @namespace =  $"AoC2023.Days.Day{day:D2}";

        var solutionDataTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.Namespace == @namespace)
            .Where(type => type.GetInterface(nameof(ISolutionData)) != null);

        if(solutionDataTypes.Count() != 1)
        {
            Console.WriteLine($"Exactly one class should implement the ISolutionData interface per namespace");
            return;
        }

        var solutionDataType = solutionDataTypes.First();
        
        var solveFstMi = solutionDataType.GetMethod(nameof(ISolutionData.SolveFirst));
        var solveSndMi = solutionDataType.GetMethod(nameof(ISolutionData.SolveSecond));

        var parserBuilderInterfaceType = typeof(ISolutionDataParserBuilder<>)
            .MakeGenericType(solutionDataType);

        var solutionDataParserBuilderTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.Namespace == @namespace)
            .Where(type => type.GetInterface(parserBuilderInterfaceType.Name) != null);

        if(solutionDataParserBuilderTypes.Count() != 1)
        {
            Console.WriteLine($"Exactly one class should implement the ISolutionData interface per namespace");
            return;
        }

        var solutionDataParserBuilderType = solutionDataParserBuilderTypes.First();

        dynamic solutionDataParserBuilder = Activator.CreateInstance(solutionDataParserBuilderType, [])!;
        var buildParserMethodInfo = solutionDataParserBuilderType.GetMethod("Build");

        if(buildParserMethodInfo is null)
        {
            Console.WriteLine("Could not find method 'Build'.");
            return;
        }

        var parser = buildParserMethodInfo!.Invoke(solutionDataParserBuilder, Array.Empty<object>());
        
        Console.WriteLine($"[Day {day}]");
        foreach(var info in BuildDayInfo(day))
        {
            HandleDay(configuration, parser, info);
        }
    }

    public static async Task Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .Build();

        if(args.Length is 1)
        {
            var day = DateTime.Today.Day;
            if(args[0] is "fetch")
            {
                await FetchInputAsync(configuration, day);
                return;
            }
            else if(args[0] is "solve")
            {
                Run(configuration, day);
                return;
            }
        }
        else if(args.Length is 2 && int.TryParse(args[1], out var day))
        {
            if(args[0] is "fetch")
            {
                await FetchInputAsync(configuration, day);
                return;
            }
            else if(args[0] is "solve")
            {
                Run(configuration, day);
                return;
            }
        }
        
        Run(configuration, 1);

        Console.WriteLine("Usage:");
        Console.WriteLine($"\t{CurrentDomain.FriendlyName} {{ fetch | solve }} [day]");
    }
}