using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipTest.Core.InputArgsContainer;
using GZipTest.Core.ResourceCalculation;

namespace GZipTest.Core.GZipEngine
{
    /// <summary>
    /// Для организации распаковки файла
    /// </summary>
    public class DecompressEngine : IGzipEngine
    {
        private readonly IInputArgs _inputArgs;

        private readonly IBlockInfo _blockInfo;

        /// <summary>
        /// Для организации последовательной (корректной) записи массива байт каждого из блоков в результирующий файл
        /// </summary>
        private int _indexSync = 1;

        /// <summary>
        /// Для синхронизации чтения qzip-файла среди множества потоков
        /// </summary>
        private object _obj = new object();

        /// <summary>
        /// Длина прочитанных байт gzip-файла
        /// </summary>
        private long _totalLenght;

        /// <summary>
        /// Информация по блокам, разбитым при создании gzip-файла
        /// </summary>
        private Queue<Block> _blockQueue;

        public DecompressEngine(
            IInputArgs inputArgs,
            IBlockInfo blockInfoCalculation
            )
        {
            _inputArgs = inputArgs ?? throw new ArgumentNullException(nameof(inputArgs));
            _blockInfo = blockInfoCalculation ?? throw new ArgumentNullException(nameof(blockInfoCalculation));
        }

        private InputFile GetInputFile(FileStream inputFileStream)
        {
            lock (_obj)
            {
                if (_blockQueue == null)
                {
                    throw new ArgumentNullException(nameof(_blockQueue));
                }

                var block = _blockQueue.Dequeue();

                inputFileStream.Seek(
                    _totalLenght + _blockInfo.Lenght * (long)block.Number,
                    SeekOrigin.Begin
                    );

                _totalLenght += block.Size;

                var blockBytes = new byte[block.Size];

                inputFileStream.Read(
                    blockBytes,
                    0,
                    block.Size
                    );

                return
                    new InputFile(
                        blockBytes,
                        block.Number
                        );
            }
        }

        public void Execute()
        {
            try
            {
                InputFile inputFile;

                using (var inputFileStream = new FileStream(
                    _inputArgs.InputFileName,
                    FileMode.Open,
                    FileAccess.Read
                    ))
                {
                    inputFile = GetInputFile(inputFileStream);
                }

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
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при распаковки файла: {ex.Message}");
            }
        }

        public int GetResourceCount()
        {
            _blockQueue = _blockInfo.GetBlockQueue();

            return
                _blockQueue.Count;
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