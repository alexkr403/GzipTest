using System;
using System.IO;
using GZipTest.Core.ThdManager;
using GZipTest.Start.Common;
using GZipTest.Start.CompositeRoot;
using GZipTest.Start.CompositeRoot.Modules;
using Ninject;

namespace GZipTest.Start
{
    //compress [bridge.bmp] [bridge.bmp]
    //decompress [bridge.bmp.prt1] [bridgeDecompressed.bmp]

    class Program
    {
        private const string Postfix = ".prt";

        private static Workstation _root;

        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {
                    throw new ArgumentException(
                        "Ожидаемое количество параметров комндной стрки: 3. " +
                        "Имена исходного и результирующего файлов должны задаваться в командной строке следующим образом: " +
                        "compress/decompress [имя исходного файла] [имя результирующего файла]"
                        );
                }

                var gzipEngine = args[0];
                var inputFileName = args[1].TrimStart('[').TrimEnd(']');
                var outputFileName = args[2].TrimStart('[').TrimEnd(']');

                var gzipEngineType = ValidationInputArgsHelper.GetGzipEngineType(gzipEngine);
                if (gzipEngineType == GzipEngineEnum.Compress)
                {
                    ValidationInputArgsHelper.ValidateToCompress(inputFileName);

                    outputFileName = outputFileName + Postfix;
                }
                else if (gzipEngineType == GzipEngineEnum.Decompress)
                {
                    ValidationInputArgsHelper.ValidateToDecompress(
                        inputFileName,
                        outputFileName,
                        Postfix
                        );

                    var indexPostfix = inputFileName.IndexOf(Postfix, StringComparison.OrdinalIgnoreCase) + Postfix.Length;
                    inputFileName = inputFileName.Remove(indexPostfix);
                }

                using (_root = new Workstation())
                {
                    _root.Init(
                        inputFileName,
                        outputFileName
                        );

                    var engineModule = new EngineModule(gzipEngineType);

                    _root.Load(engineModule);

                    var engine = _root.Get<IThreadManager>();
                    engine.Start();
                }

                Environment.Exit(0);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Параметры командной строки заданы некорректно. {ex.Message}");
                Environment.Exit(1);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Не найден файл: {ex.Message}");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неизвестная ошибка при обработке файла: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}