using CommandLine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace WafLogProcessor
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
               .MapResult(
                    o => Run(o),
                    err => HandleErrors(err)
               );
        }

        public static int Run(Options opts)
        {
            var files = GetAllFiles(opts.Path, "*.json");

            var map = new Dictionary<string, List<JObject>>();

            foreach (var file in files)
            {
                using (var fileIn = new StreamReader(file))
                {
                    string line;
                    while ((line = fileIn.ReadLine()) != null)
                    {
                        var entry = ParseLine(line);

                        var list = map.GetValueOrDefault(entry.Item1);
                        if (list == null)
                        {
                            map.Add(entry.Item1, list = new List<JObject>());
                        }

                        list.Add(entry.Item2);
                    }
                }
            }

            foreach (var entry in map)
            {
                Console.WriteLine($"Rule: {entry.Key} -> {entry.Value.Count}");

                var file = @$"{opts.OutputDir}\{entry.Key}.tsv";
                using (var fileout = new StreamWriter(file))
                {
                    fileout.WriteLine("Timestamp\tHost\tRequest URI\tClient IP\tDetail Message\tDetail Data");

                    foreach (var obj in entry.Value)
                    {
                        fileout.WriteLine($"{obj["time"]}\t{obj["properties"]["host"]}\t{obj["properties"]["requestUri"]}\t{obj["properties"]["clientIP"]}\t{obj["properties"]["details"]["msg"]}\t{obj["properties"]["details"]["data"]}\t");
                    }
                }

                Console.WriteLine($"\tWrote details to file: {file}");
                Console.WriteLine();
            }

            return 0;
        }

        private static IList<string> GetAllFiles(string path, string extension)
        {
            var files = new List<string>();
            files.AddRange(Directory.EnumerateFiles(path));

            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                files.AddRange(GetAllFiles(dir, extension));
            }

            return files;
        }

        private static (string, JObject) ParseLine(string line)
        {
            var obj = JObject.Parse(line);
            return (obj["properties"]["ruleName"].Value<string>(), obj);
        }

        public static int HandleErrors(IEnumerable<Error> errors)
        {
            Console.Error.WriteLine("Error parsing parameters:");

            foreach (var e in errors)
            {
                Console.Error.WriteLine($"\t{e}");
            }

            return -1;
        }
    }
}
