using System.Diagnostics;
using System.Reflection;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
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

    private static void HandleDayPart(
        IConfiguration configuration,
        IDay solver, 
        Type type, 
        int day, 
        int part,
        bool mini)
    {
        var assemblyPath = Assembly.GetEntryAssembly()!.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
        Directory.SetCurrentDirectory(assemblyDirectory!);

        var methodName = $"Part{part}";
        var methodInfo = type.GetMethod(methodName);
        var stopwatch = new Stopwatch();

        if(methodInfo is null)
        {
            Console.WriteLine($"Could not get method {methodName} info from type {type}.");
            return;
        } 

        var miniText = mini ? $"_{part}" : Empty;
        var inputFileName = $"{configuration["inputFileDirectory"]}/Day{day:D2}{miniText}.txt";

        miniText = mini ? "mini" : "full";
        try
        {
            object[] methodArg = [File.ReadAllText(inputFileName)];

            TextWriter backup = Console.Out;
            if(!mini)
            {
                Console.SetOut(TextWriter.Null);
            }

            stopwatch.Start();
            var answer = methodInfo.Invoke(solver, methodArg);
            stopwatch.Stop();

            Console.SetOut(backup);

            Console.WriteLine($"[{miniText}, Part {part}, t = {stopwatch.ElapsedMilliseconds}ms]: {answer}");
        }
        catch(Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"[{miniText}, Part {part}, t = {stopwatch.ElapsedMilliseconds}]: {ex.Message}");
        }
    }

    private static void Run(IConfiguration configuration, int day)
    {
        var typeName = $"AoC2023.Days.Day{day:D2}.Day{day:D2}";
        var type = Type.GetType(typeName);
            
        if(type is null)
        {
            Console.WriteLine($"Type '{typeName}' does not exist.");
            return;
        }

        dynamic instance = Activator.CreateInstance(type, [])!;

        if(instance is null)
        {
            Console.WriteLine($"Could not created instance of type '{typeName}'.");
            return;
        }

        Console.WriteLine($"[Day {day}]");
        HandleDayPart(configuration, instance, type, day, 1, true);
        HandleDayPart(configuration, instance, type, day, 1, false);
        HandleDayPart(configuration, instance, type, day, 2, true);
        HandleDayPart(configuration, instance, type, day, 2, false);
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
        
        Run(configuration, 2);

        Console.WriteLine("Usage:");
        Console.WriteLine($"\t{CurrentDomain.FriendlyName} {{ fetch | run }} [day]");
    }
}