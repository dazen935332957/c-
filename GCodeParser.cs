using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GCodeCommand
{
    public string Type { get; set; } // "G" 或 "M"
    public int Number { get; set; }
    public Dictionary<string, double> Parameters { get; set; } = new Dictionary<string, double>();
    public string RawLine { get; set; }
}

public static class GCodeParser
{
    // 解析一行G/M代码
    public static GCodeCommand ParseLine(string line)
    {
        var cmd = new GCodeCommand { RawLine = line };
        var matches = Regex.Matches(line.ToUpper(), @"([GM])(\d+)|([XYZFIJKRSPQ])([+-]?\d+(\.\d+)?)");

        foreach (Match match in matches)
        {
            if (match.Groups[1].Success) // G或M代码
            {
                cmd.Type = match.Groups[1].Value;
                cmd.Number = int.Parse(match.Groups[2].Value);
            }
            else if (match.Groups[3].Success) // 参数
            {
                string param = match.Groups[3].Value;
                double value = double.Parse(match.Groups[4].Value);
                cmd.Parameters[param] = value;
            }
        }
        return cmd;
    }

    // 解析多行G/M代码
    public static List<GCodeCommand> ParseLines(IEnumerable<string> lines)
    {
        var result = new List<GCodeCommand>();
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith(";") && !trimmed.StartsWith("("))
            {
                result.Add(ParseLine(trimmed));
            }
        }
        return result;
    }
}