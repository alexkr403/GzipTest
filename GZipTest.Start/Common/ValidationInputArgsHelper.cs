using System;
using System.IO;

namespace GZipTest.Start.Common
{
    public static class ValidationInputArgsHelper
    {
        private const string Decompress = "decompress";
        private const string Compress = "compress";

        public static GzipEngineEnum GetGzipEngineType(string gzipEngine)
        {
            if (gzipEngine == Compress)
            {
                return 
                    GzipEngineEnum.Compress;
            }

            if (gzipEngine == Decompress)
            {
                return 
                    GzipEngineEnum.Decompress;
            }

            Console.WriteLine("Требуется ввести параметр compress/decompress");

            return 
                GzipEngineEnum.NotDefined;
        }

        public static bool ValidateFile(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Файл <{inputFile}> не найден");

                return
                    false;
            }

            return
                true;
        }

        public static void DeleteOutputFile(string outputFile)
        {
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }
}
