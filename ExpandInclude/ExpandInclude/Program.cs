using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExpandInclude
{
    class Program
    {
        static Regex include = new Regex(@"\s*#\s*include""(?<file>.+)"".*");
        static Regex pragmaOnce = new Regex(@"\s*#\s*pragma\s+once.*");
        static void Expantion(Uri path, StringBuilder expanded, HashSet<Uri> included)
        {
            using (var stream = new StreamReader(path.LocalPath))
            {
                while (stream.Peek() >= 0)
                {
                    var line = stream.ReadLine();
                    var imatch = include.Match(line);
                    var pmatch = pragmaOnce.Match(line);
                    if (imatch.Success)
                    {
                        var file = imatch.Groups["file"].Value;
                        var uri = new Uri(path, file);
                        if (!included.Contains(uri))
                        {
                            if (File.Exists(uri.LocalPath))
                            {
                                included.Add(uri);
                                expanded.AppendLine($"// start: {file}");
                                Expantion(uri, expanded, included);
                                expanded.AppendLine($"// end: {file}");
                            }
                            else
                            {
                                throw new FileNotFoundException($@"""{file}"" was not found(in {path})");
                            }
                        }
                    }
                    else if (!pmatch.Success)
                    {
                        expanded.AppendLine(line);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("ExpandInclude.exe input output [ignore_files...]");
                Environment.Exit(1);
            }
            var expanded = new StringBuilder();
            var included = new HashSet<Uri>();
            var root = new Uri(Path.GetFullPath(args[0]));
            foreach (var ignore in args.Skip(2))
            {
                included.Add(new Uri(root, ignore));
            }
            Expantion(root, expanded, included);
            using (var stream = new StreamWriter(args[1]))
            {
                stream.Write(expanded.ToString());
            }
        }
    }
}
