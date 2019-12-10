using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipTest.Core.InputArgsContainer;

namespace GZipTest.Core.GZipEngine
{
    /// <summary>
    /// Для организации сжатия файла
    /// </summary>
    public class CompressEngine : IGzipEngine
    {
        /// <summary>
        /// блок 1 мб.
        /// </summary>
        private const int BlockSize = 1048576;

        private readonly IInputArgs _inputArgs;

        /// <summary>
        /// Для организации последовательной (корректной) записи массива байт каждого из блоков в gzip файл
        /// </summary>
        private int _indexSync = 1;

        /// <summary>
        /// Количество итерраций чтения исходного (сжимаемого) файла
        /// </summary>
        private int _index;

        /// <summary>
        /// Длина прочитанных байт исходного (сжимаемого) файла
        /// </summary>
        private long _totalLenght;

        /// <summary>
        /// Для синхронизации чтения исходного (сжимаемого) файла
        /// </summary>
        private object _obj = new object();

        public CompressEngine(IInputArgs inputArgs)
        {
            _inputArgs = inputArgs ?? throw new ArgumentNullException(nameof(inputArgs));
        }

        private OutputBlock GetOutputBlock()
        {
            lock (_obj)
            {
                using (var inputFileStream = new FileStream(
                    _inputArgs.InputFileName,
                    FileMode.Open,
                    FileAccess.Read
                    ))
                {
                    _totalLenght = BlockSize * (long)_index;

                    inputFileStream.Seek(
                        _totalLenght,
                        SeekOrigin.Begin
                        );

                    var countBytesToRead = inputFileStream.Length - _totalLenght;

                    var outputBlockSize = countBytesToRead > BlockSize
                        ? BlockSize
                        : (int)countBytesToRead;

                    var outputBlockBytes = new byte[outputBlockSize];

                    inputFileStream.Read(
                        outputBlockBytes,
                        0,
                        outputBlockSize
                        );

                    _index++;

                    return
                        new OutputBlock(
                            _index,
                            outputBlockBytes
                            );
                }
            }
        }

        public void Execute()
        {
            try
            {
                var outputBlock = GetOutputBlock();

                using (var outputBlockMemoryStream = new MemoryStream())
                {
                    using (var compressionStream = new GZipStream(
                        outputBlockMemoryStream,
                        CompressionMode.Compress,
                        true
                        ))
                    {
                        using (var tempMemoryStream = new MemoryStream(outputBlock.Data))
                        {
                            tempMemoryStream.CopyTo(compressionStream);
                        }
                    }

                    while (_indexSync != outputBlock.Index)
                    {
                        //организуем корректную дозапись сжатых частей в выходной файл
                        Thread.Sleep(1);
                    }

                    using (var outputFileStream = File.Open(
                        _inputArgs.OutputFileName,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.Read
                        ))
                    {
                        outputBlockMemoryStream.Seek(0, SeekOrigin.Begin);

                        var linkedListBytes = BitConverter.GetBytes(outputBlockMemoryStream.Length);
                        outputFileStream.Write(linkedListBytes, 0, linkedListBytes.Length);

                        outputBlockMemoryStream.CopyTo(outputFileStream);
                    }

                    _indexSync++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при сжатии файла: {ex.Message}");
            }
        }

        public int GetResourceCount()
        {
            var fileLength = new FileInfo(_inputArgs.InputFileName).Length;

            var resourceCount = fileLength / BlockSize;
            resourceCount +=
                BlockSize == fileLength
                    ? 0
                    : 1;

            return
                (int)resourceCount;
        }

        private class OutputBlock
        {
            public OutputBlock(
                int index, 
                byte[] data
                )
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
                Index = index;
            }

            public int Index { get; }

            public byte[] Data { get; }
        }
    }
}
