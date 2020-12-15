using CommandLine;

namespace WafLogProcessor
{
    public class Options
    {
        [Option('p', "path", Required = true)]
        public string Path { get; set; }

        [Option('o', "output-dir", Required = true)]
        public string OutputDir { get; set; }
    }
}
