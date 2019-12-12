using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipTest.Core.InputArgsContainer;
using GZipTest.Core.SettingsContainer;

namespace GZipTest.Core.GZipEngine
{
    /// <summary>
    /// Для организации сжатия файла
    /// </summary>
    public class CompressEngine : IGzipEngine
    {
        private readonly IInputArgs _inputArgs;

        private readonly Settings _settings;

        /// <summary>
        /// Для организации последовательной (корректной) записи массива байт каждого из блоков в gzip файл
        /// </summary>
        private int _indexSync = 1;

        /// <summary>
        /// Количество итерраций чтения исходного (сжимаемого) файла
        /// </summary>
        private long _index;

        /// <summary>
        /// Длина прочитанных байт исходного (сжимаемого) файла
        /// </summary>
        private long _totalLenght;

        /// <summary>
        /// Для синхронизации чтения исходного (сжимаемого) файла
        /// </summary>
        private object _obj = new object();

        public CompressEngine(
            IInputArgs inputArgs,
            Settings settings
            )
        {
            _inputArgs = inputArgs ?? throw new ArgumentNullException(nameof(inputArgs));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
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
                    _totalLenght = _settings.BlockSize * _index;

                    inputFileStream.Seek(
                        _totalLenght,
                        SeekOrigin.Begin
                        );

                    var countBytesToRead = inputFileStream.Length - _totalLenght;

                    var outputBlockSize = countBytesToRead > _settings.BlockSize
                        ? _settings.BlockSize
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
                        Thread.Sleep(10);
                    }

                    using (var outputFileStream = File.Open(
                        _inputArgs.OutputFileName,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.Read
                        ))
                    {
                        outputBlockMemoryStream.Seek(
                            0, 
                            SeekOrigin.Begin
                            );

                        var blockBytes = BitConverter.GetBytes(outputBlockMemoryStream.Length);

                        outputFileStream.Write(
                            blockBytes, 
                            0, 
                            blockBytes.Length
                            );

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

        public int GetBlockCount()
        {
            var fileLength = new FileInfo(_inputArgs.InputFileName).Length;

            var blockCount = fileLength / _settings.BlockSize;
            blockCount +=
                _settings.BlockSize == fileLength
                    ? 0
                    : 1;

            var blockCountToInt = (int)blockCount;

            CreateOutputFile(blockCountToInt);

            return
                blockCountToInt;
        }

        private void CreateOutputFile(int blockCount)
        {
            using (var outputFileStream = File.Open(
                _inputArgs.OutputFileName,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read
                ))
            {
                var bytes = BitConverter.GetBytes(blockCount);
                outputFileStream.Write(
                    bytes, 
                    0, 
                    bytes.Length
                    );
            }
        }

        private class OutputBlock
        {
            public OutputBlock(
                long index, 
                byte[] data
                )
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
                Index = index;
            }

            public long Index { get; }

            public byte[] Data { get; }
        }
    }
}
