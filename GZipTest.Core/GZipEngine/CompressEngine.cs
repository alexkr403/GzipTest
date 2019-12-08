using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipTest.Core.InputArgsContainer;

namespace GZipTest.Core.GZipEngine
{
    public class CompressEngine : IGzipEngine
    {
        /// <summary>
        /// блок 1 мб.
        /// </summary>
        private const int OutputFileSize = 1048576;

        private readonly InputArgs _inputArgs;

        /// <summary>
        /// Для организации последовательной (корректной) записи массива байт каждого из сжатых блоков
        /// </summary>
        private int _indexSync = 1;

        /// <summary>
        /// Количество итерраций чтения байт в сжимаемом файле
        /// </summary>
        private int _index;

        /// <summary>
        /// Количество прочитанных байт в сжимаемом файле
        /// </summary>
        private long _complitedBytesCount;

        /// <summary>
        /// Для синхронизации чтения исходного (сжимаемого) файла
        /// </summary>
        private object _obj = new object();

        public CompressEngine(InputArgs inputArgs)
        {
            _inputArgs = inputArgs ?? throw new ArgumentNullException(nameof(inputArgs));
        }

        private OutputFileData GetOutputFile()
        {
            lock (_obj)
            {
                using (var inputFileStream = new FileStream(
                    _inputArgs.InputFileName,
                    FileMode.Open,
                    FileAccess.Read
                    ))
                {
                    _complitedBytesCount = OutputFileSize * _index;

                    inputFileStream.Seek(
                        _complitedBytesCount,
                        SeekOrigin.Begin
                        );

                    var countBytesToRead = inputFileStream.Length - _complitedBytesCount;

                    var outputFileSize = countBytesToRead > OutputFileSize
                        ? OutputFileSize
                        : (int)countBytesToRead;

                    var outputFileBytes = new byte[outputFileSize];

                    inputFileStream.Read(
                        outputFileBytes,
                        0,
                        outputFileSize
                        );

                    _index++;

                    return
                        new OutputFileData(
                            _index,
                            outputFileBytes
                            );
                }
            }
        }

        public void Execute()
        {
            try
            {
                var outputFile = GetOutputFile();

                using (var outputFileMemoryStream = new MemoryStream())
                {
                    using (var compressionStream = new GZipStream(
                        outputFileMemoryStream,
                        CompressionMode.Compress,
                        true
                        ))
                    {
                        using (var tempMemoryStream = new MemoryStream(outputFile.Data))
                        {
                            tempMemoryStream.CopyTo(compressionStream);
                        }
                    }

                    while (_indexSync != outputFile.Index)
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
                        outputFileMemoryStream.Seek(0, SeekOrigin.Begin);

                        var linkedListBytes = BitConverter.GetBytes(outputFileMemoryStream.Length);
                        outputFileStream.Write(linkedListBytes, 0, linkedListBytes.Length);

                        outputFileMemoryStream.CopyTo(outputFileStream);
                    }

                    _indexSync++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при сжатии файла: {ex.Message}");
            }
        }

        public long GetResourceCount()
        {
            var fileLength = new FileInfo(_inputArgs.InputFileName).Length;

            var resourceCount = fileLength / OutputFileSize;
            resourceCount +=
                OutputFileSize == fileLength
                    ? 0
                    : 1;

            return
                resourceCount;
        }

        private class OutputFileData
        {
            public OutputFileData(
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
