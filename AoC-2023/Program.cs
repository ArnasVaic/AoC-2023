using System.Xml.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;

namespace AoC2023;

public static class Program
{

    private enum PartNumber
    {
        First = 1,
        Second = 2
    };

    private static int ToInt(PartNumber number) => number switch
    {
        PartNumber.First => 1,
        PartNumber.Second => 2,
        _ => throw new Exception("Part number unavailable")
    };

    private static void HandleDayPart(
        IDay day, 
        Type type, 
        int dayNumber, 
        PartNumber partNumber, 
        bool mini)
    {
        var number = ToInt(partNumber);
        var methodName = $"Part{number}";
        var methodInfo = type.GetMethod(methodName);

        if(methodInfo is null)
        {
            Console.WriteLine($"Could not get method {methodName} info from type {type}.");
            return;
        } 

        var miniText = mini ? $"_{number}" : string.Empty;
        var inputFileName = $"Inputs/Day{dayNumber:D2}{miniText}.txt";

        miniText = mini ? "mini" : "full";
        try
        {
            object[] methodArg = [File.ReadAllText(inputFileName)];


            TextWriter backup = Console.Out;
            if(!mini)
            {
                Console.SetOut(TextWriter.Null);
            }

            var answer = methodInfo.Invoke(day, methodArg);

            Console.SetOut(backup);

            Console.WriteLine($"[{miniText}, Part {number}]: {answer}");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[{miniText}, Part {number}]: {ex.Message}");
        }
    }

    private static int? GetDayNumber(string[] args)
    {
        if (args.Length is 0)
            return DateTime.Now.Day;

        if (int.TryParse(args[0], out var dayNumber))
            return dayNumber;
       
        Console.WriteLine("Not a number");
        return null; 
    }

    public static async Task Main(string[] args)
    {
        var dayNumber = GetDayNumber(args);

        if(!dayNumber.HasValue)
            return;

        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
        
        IConfiguration config = builder.Build();

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("cookie", config["cookie"]);

        var inputDir = config["InputsDirectory"];
        var filename = $"{inputDir}/Day{dayNumber:D2}.txt";

        var mainurl = $"https://adventofcode.com/2023/day/{dayNumber}/input";
        var result = await client.GetAsync(mainurl);
        var content = await result.Content.ReadAsStringAsync();

        if(!File.Exists(filename))
        {
            if(!result.IsSuccessStatusCode)
                Console.WriteLine($"Coud not fetch input for day {dayNumber:D2}: {content}");
            else
                File.WriteAllText(filename, content);
        }

        string[] miniFilenames = [
            $"{inputDir}/Day{dayNumber:D2}_1.txt",
            $"{inputDir}/Day{dayNumber:D2}_2.txt",];

        var miniurl = $"https://adventofcode.com/2023/day/{dayNumber}";
        result = await client.GetAsync(miniurl);
        content = await result.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        
        if(!result.IsSuccessStatusCode)
            Console.WriteLine($"Coud not fetch mini input for day {dayNumber:D2}: {content}");
        else
        {
            doc.LoadHtml(content);
            var resultNodes = doc.DocumentNode.SelectNodes("//pre/code");

            if (resultNodes is null)
                Console.WriteLine("Could not extract mini data.");
            else
            {
                foreach (var (text, fn) in resultNodes.Select(x => x.InnerText).Zip(miniFilenames))
                    if(!File.Exists(fn))
                        File.WriteAllText(fn, text);
            }
        }

        var typeName = $"AoC2023.Days.Day{dayNumber:D2}.Day{dayNumber:D2}";
        var type = Type.GetType(typeName);
            
        if(type is null)
        {
            Console.WriteLine($"Type '{typeName}' does not exist.");
            return;
        }

        dynamic instance = Activator.CreateInstance(type, []);

        if(instance is null)
        {
            Console.WriteLine($"Could not created instance of type '{typeName}'.");
            return;
        }

        HandleDayPart(instance, type, dayNumber.Value, PartNumber.First, true);
        HandleDayPart(instance, type, dayNumber.Value, PartNumber.First, false);
        HandleDayPart(instance, type, dayNumber.Value, PartNumber.Second, true);
        HandleDayPart(instance, type, dayNumber.Value, PartNumber.Second, false);
    }
}