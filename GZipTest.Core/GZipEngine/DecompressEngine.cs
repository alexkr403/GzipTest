using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipTest.Core.InputArgsContainer;

namespace GZipTest.Core.GZipEngine
{
    public class DecompressEngine : IEngine
    {
        private readonly IInputArgsDecompress _inputArgs;

        /// <summary>
        /// Для организации последовательной (корректной) записи массива байт каждого из сжатых файлов в распакованный файл,
        /// с помощью stream.CopyTo()
        /// </summary>
        private int _indexSync = 1;

        /// <summary>
        /// Порядковый номер сжатого файла
        /// </summary>
        private int _inputFileNumber;

        /// <summary>
        /// Для синхронизации чтения сжатых файлов и синхронизации послед. записи массива байт в выходной массив
        /// </summary>
        private object _obj = new object();

        public DecompressEngine(IInputArgsDecompress inputArgs)
        {
            _inputArgs = inputArgs ?? throw new ArgumentNullException(nameof(inputArgs));
        }

        private InputFile GetInputFile()
        {
            lock (_obj)
            {
                _inputFileNumber++;

                return
                    new InputFile(
                        _inputArgs.InputFileName + _inputFileNumber,
                        _inputFileNumber
                        );
            }
        }

        public void Execute()
        {
            InputFile inputFile = null;

            try
            {
                inputFile = GetInputFile();

                using (var inputFileStream = new FileStream(
                    inputFile.Path,
                    FileMode.Open
                    ))
                {
                    using (var decompressionStream = new GZipStream(
                        inputFileStream,
                        CompressionMode.Decompress
                        ))
                    {
                        while (_indexSync != inputFile.Index)
                        {
                            //организуем корректную дозапись decompressed частей файла в выходной файл
                            Thread.Sleep(1);
                        }

                        lock (_obj)
                        {
                            using (var outputFileStream = File.Open(
                                _inputArgs.OutputFileName,
                                FileMode.Append,
                                FileAccess.Write,
                                FileShare.Read
                                ))
                            {
                                decompressionStream.CopyTo(outputFileStream);
                            }

                            _indexSync++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var error = string.Empty;

                if (inputFile != null)
                {
                    error = $"Ошибка при обработке файла {inputFile.Path}{Environment.NewLine}";
                }

                throw new Exception($"{error}{ex.Message}");
            }
        }

        private class InputFile
        {
            public InputFile(
                string path,
                int index
                )
            {
                Path = path ?? throw new ArgumentNullException(nameof(path));
                Index = index;
            }

            public string Path { get; }

            public int Index { get; }
        }
    }
}