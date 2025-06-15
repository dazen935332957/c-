using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GCodeCommand
{
    public string Type { get; set; } // "G" �� "M"
    public int Number { get; set; }
    public Dictionary<string, double> Parameters { get; set; } = new Dictionary<string, double>();
    public string RawLine { get; set; }
}

public static class GCodeParser
{
    // ����һ��G/M����
    public static GCodeCommand ParseLine(string line)
    {
        var cmd = new GCodeCommand { RawLine = line };
        var matches = Regex.Matches(line.ToUpper(), @"([GM])(\d+)|([XYZFIJKRSPQ])([+-]?\d+(\.\d+)?)");

        foreach (Match match in matches)
        {
            if (match.Groups[1].Success) // G��M����
            {
                cmd.Type = match.Groups[1].Value;
                cmd.Number = int.Parse(match.Groups[2].Value);
            }
            else if (match.Groups[3].Success) // ����
            {
                string param = match.Groups[3].Value;
                double value = double.Parse(match.Groups[4].Value);
                cmd.Parameters[param] = value;
            }
        }
        return cmd;
    }

    // ��������G/M����
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