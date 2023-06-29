using System.Globalization;
using System.Net;
using System.Runtime.Versioning;
using System.Text;

using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

internal class Program
{
    [SupportedOSPlatform("Windows")]
    private static void Main(String[] _)
    {
        Console.Write("Downloading material symbols ");

        var repoDirectoryPath = @"..\..\..";

        var gitignoreFilePath = Path.Combine(repoDirectoryPath, ".gitignore");
        if (!File.Exists(gitignoreFilePath))
        {
            Console.WriteLine("Please run this tool from output build directory");
            return;
        }

        var gitDirectoryPath = Path.Combine(repoDirectoryPath, "git");

        try
        {
            if (Directory.Exists(gitDirectoryPath))
            {
                foreach (var fileInfo in new DirectoryInfo(gitDirectoryPath).EnumerateFiles())
                {
                    fileInfo.Delete();
                }
            }
            else
            {
                Directory.CreateDirectory(gitDirectoryPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Cannot clean output directory: {ex.Message}");
        }

        Console.Write(".");

        var client = new WebClient();

        var ttfFilePath = Path.Combine(gitDirectoryPath, "MaterialSymbols.ttf");
        DownloadFile("ttf", ttfFilePath);

        var txtFilePath = Path.Combine(gitDirectoryPath, "MaterialSymbols.txt");
        DownloadFile("codepoints", txtFilePath);

        var wofFilePath = Path.Combine(gitDirectoryPath, "MaterialSymbols.woff2");
        DownloadFile("woff2", wofFilePath);

        Console.WriteLine();

        void DownloadFile(String urlExtension, String filePath)
        {
            try
            {
                client.DownloadFile($"https://github.com/google/material-design-icons/raw/master/variablefont/MaterialSymbolsRounded%5BFILL%2CGRAD%2Copsz%2Cwght%5D.{urlExtension}", filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Cannot download file: {ex.Message}");
            }

            Console.Write(".");
        }

        Console.Write("Generating images from material symbols ");

        var size = 256;

        var fonts = new FontCollection();
        var fontFamily = fonts.Add(ttfFilePath);

        var font = fontFamily.CreateFont(size, FontStyle.Regular);
        var color = new Color(new Argb32(255, 255, 255, 166));

        var stringBuilder = new StringBuilder(1024, 1024);

        var count = 0;

        using (var reader = new StreamReader(txtFilePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var parts = line.Split(' ');

                var name = FixName(parts[0].Trim());

                var unicode = (Char)UInt32.Parse(parts[1].Trim(), NumberStyles.HexNumber);

                WritePng(name, unicode);

                count++;

                if (0 == (count % 100))
                {
                    Console.Write(".");
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{count:N0} images generated");

        String FixName(String name)
        {
            var length = name.Length;

            stringBuilder.Clear();

            var wasSpace = true;
            var wasDigit = false;

            foreach (var c in name)
            {
                if ('_' == c)
                {
                    stringBuilder.Append(' ');
                    wasSpace = true;
                    wasDigit = false;
                }
                else if (wasSpace)
                {
                    if (Char.IsDigit(c))
                    {
                        wasDigit = true;
                    }

                    stringBuilder.Append(Char.ToUpperInvariant(c));
                    wasSpace = false;
                }
                else
                {
                    stringBuilder.Append(wasDigit ? Char.ToUpperInvariant(c) : c);
                }
            }

            return stringBuilder.ToString();
        }

        void WritePng(String name, Char symbol)
        {
            var text = symbol.ToString();

            var symbolSize = TextMeasurer.Measure(text, new TextOptions(font));
            var location = new PointF(
                (size - symbolSize.Width) / 2F,
                (size - symbolSize.Height) / 2F);

            using (var image = new Image<Rgba32>(size, size, Color.Transparent))
            {
                image.Mutate(x => x.DrawText(text, font, color, location));
                var pngFilePath = Path.Combine(repoDirectoryPath, "ico", $"{name}.png");
                image.SaveAsPng(pngFilePath);
            }
        }
    }
}
