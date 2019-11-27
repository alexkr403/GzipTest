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
            GzipEngineEnum gzipEngineType;
            if (gzipEngine == Compress)
            {
                gzipEngineType = GzipEngineEnum.Compress;
            }
            else if (gzipEngine == Decompress)
            {
                gzipEngineType = GzipEngineEnum.Decompress;
            }
            else
            {
                throw new ArgumentException("Требуется ввести параметр compress/decompress");
            }

            return
                gzipEngineType;
        }

        public static void ValidateToCompress(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException(inputFile);
            }
        }

        public static void ValidateToDecompress(
            string inputFile,
            string outputFile,
            string postfix
            )
        {
            if (postfix == null)
            {
                throw new ArgumentNullException(nameof(postfix));
            }

            if (string.Equals(inputFile, outputFile, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Имя входного фыйла {inputFile} совпадает с именем выходного файла {outputFile}");
            }

            if (!inputFile.Contains(postfix))
            {
                throw new ArgumentException("Необходимо указать имя сжатого файла (файл c постфиксом *.prt)");
            }

            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException(inputFile);
            }

            File.Delete(outputFile);
        }
    }
}
