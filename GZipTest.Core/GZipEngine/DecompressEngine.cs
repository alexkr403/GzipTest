using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipTest.Core.InputArgsContainer;
using GZipTest.Core.ResourceCalculation;

namespace GZipTest.Core.GZipEngine
{
    public class DecompressEngine : IGzipEngine
    {
        private readonly InputArgs _inputArgs;
        private readonly IBlockInfoCalculation _blockInfoCalculation;

        /// <summary>
        /// Для организации последовательной (корректной) записи массива байт каждого из сжатых блоков в распакованный файл,
        /// с помощью stream.CopyTo()
        /// </summary>
        private int _indexSync = 1;

        /// <summary>
        /// Количество итерраций чтения блоков в сжимаемом файле
        /// </summary>
        private int _index;

        /// <summary>
        /// Для синхронизации чтения сжатых файлов и синхронизации послед. записи массива байт в выходной массив
        /// </summary>
        private object _obj = new object();

        /// <summary>
        /// Уже считано байт для распаковки
        /// </summary>
        private long _totalLenght;

        int _blockLenght;

        public DecompressEngine(
            InputArgs inputArgs,
            IBlockInfoCalculation blockInfoCalculation
            )
        {
            _inputArgs = inputArgs ?? throw new ArgumentNullException(nameof(inputArgs));
            _blockInfoCalculation = blockInfoCalculation ?? throw new ArgumentNullException(nameof(blockInfoCalculation));
        }

        private InputFile GetInputFile(FileStream inputFileStream)
        {
            lock (_obj)
            {
                _blockInfoCalculation.Execute(
                    inputFileStream,
                    ref _totalLenght,
                    ref _blockLenght,
                    ref _index
                    );

                var blockBytes = new byte[_blockLenght];

                inputFileStream.Read(
                    blockBytes,
                    0,
                    _blockLenght
                    );

                return 
                    new InputFile(
                        blockBytes,
                        ++_index
                        );
            }
        }

        public void Execute()
        {
            try
            {
                using (var inputFileStream = new FileStream(
                    _inputArgs.InputFileName,
                    FileMode.Open,
                    FileAccess.Read
                    ))
                {
                    var inputFile = GetInputFile(inputFileStream);

                    using (var memoryStream = new MemoryStream(inputFile.Data))
                    {
                        using (var decompressionStream = new GZipStream(
                            memoryStream,
                            CompressionMode.Decompress
                            ))
                        {
                            while (_indexSync != inputFile.Index)
                            {
                                //организуем корректную дозапись рапакованных частей файла в выходной файл
                                Thread.Sleep(1);
                            }

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
                throw new Exception($"Произошла ошибка при распаковки файла: {ex.Message}");
            }
        }

        public long GetResourceCount()
        {
            long totalLenght = 0;
            var blockLenght = 0;
            var index = 0;

            using (var inputFileStream = new FileStream(
                _inputArgs.InputFileName,
                FileMode.Open
                ))
            {
                while (true)
                {
                    _blockInfoCalculation.Execute(
                        inputFileStream,
                        ref totalLenght,
                        ref blockLenght,
                        ref index
                        );

                    if (blockLenght == 0)
                    {
                        return
                            index;
                    }

                    index++;
                }
            }
        }

        private class InputFile
        {
            public InputFile(
                byte[] data,
                int index
                )
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
                Index = index;
            }

            public byte[] Data { get; }

            public int Index { get; }
        }
    }
}