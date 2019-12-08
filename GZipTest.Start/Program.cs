using System;
using GZipTest.Core.ThdManager;
using GZipTest.Start.Common;
using GZipTest.Start.CompositeRoot;
using GZipTest.Start.CompositeRoot.Modules;
using Ninject;

namespace GZipTest.Start
{
    //compress [bridge.bmp] [bridge.bmp]
    //decompress [bridge.bmp.gzip] [bridgeDecompressed.bmp]

    class Program
    {
        private const string Postfix = ".gzip";

        private static Workstation _root;

        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {
                    Console.WriteLine(
                        "Ожидаемое количество параметров комндной стрки: 3. " +
                        "Имена исходного и результирующего файлов должны задаваться в командной строке следующим образом: " +
                        "compress/decompress [имя исходного файла] [имя результирующего файла]"
                        );

                    return;
                }

                var gzipEngine = args[0];
                var inputFileName = args[1].TrimStart('[').TrimEnd(']');
                var outputFileName = args[2].TrimStart('[').TrimEnd(']');

                var gzipEngineType = ValidationInputArgsHelper.GetGzipEngineType(gzipEngine);

                if (gzipEngineType == GzipEngineEnum.NotDefined)
                {
                    return;
                }

                if (!ValidationInputArgsHelper.ValidateFile(inputFileName))
                {
                    return;
                }

                if (gzipEngineType == GzipEngineEnum.Compress)
                {
                    outputFileName = outputFileName + Postfix;
                }

                ValidationInputArgsHelper.DeleteOutputFile(outputFileName);

                using (_root = new Workstation())
                {
                    _root.Init(
                        inputFileName,
                        outputFileName
                        );

                    _root.Load(new EngineModule(gzipEngineType));

                    var engine = _root.Get<IThreadManager>();
                    engine.Start();
                }

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке файла. {ex.Message}");

                Environment.Exit(1);
            }
        }
    }
}