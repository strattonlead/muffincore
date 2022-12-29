using Microsoft.Extensions.DependencyInjection;
using Muffin.Deepl;
using Muffin.Deepl.Abstraction;
using System.Text;

namespace Muffin.FileTranslator
{
    public static class FileTranslator
    {
        public static readonly string[] AVAILABLE_LANGUAGE_CODES = new string[] { "BG", "CS", "DA", "DE", "EL", "EN-US", "ES", "ET", "FI", "FR", "HU", "IT", "JA", "LT", "LV", "NL", "PL", "PT-PT", "RO", "RU", "SK", "SL", "SV", "ZH" };

        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddDeeplService("60f9f86c-59a6-2913-1bc1-d8a2580c4256:fx", false);
            var serviceProvider = services.BuildServiceProvider();
            var deeplService = serviceProvider.GetRequiredService<IDeeplService>();

            var inputDir = Path.Combine(AppContext.BaseDirectory, "input");
            if (!Directory.Exists(inputDir))
            {
                Directory.CreateDirectory(inputDir);
            }

            var outputDir = Path.Combine(AppContext.BaseDirectory, "output");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var files = Directory.EnumerateFiles(inputDir).ToArray();
            foreach (var file in files)
            {
                var text = File.ReadAllText(file, Encoding.UTF8);
                var result = deeplService.TranslateAsync(new MatrixRequest()
                {
                    SourceLang = "DE",
                    TargetLangs = AVAILABLE_LANGUAGE_CODES.Except(new string[] { "DE" }).ToArray(),
                    Texts = new string[] { text }
                }).Result;

                foreach (var translated in result)
                {
                    var fileName = Path.GetFileName(file);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    var extension = Path.GetExtension(fileName);

                    File.WriteAllText(Path.Combine(outputDir, $"{fileNameWithoutExtension}.{translated.TargetLang.ToLower()}{extension}".Replace("_de.", "_").Replace("en-us", "").Replace("pt-pt", "pt")), translated.Text);
                }

                File.Delete(file);
            }
        }
    }
}