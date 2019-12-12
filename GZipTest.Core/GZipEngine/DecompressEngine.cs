using System;
using System.IO;
using System.IO.Compression;
using GZipTest.Core.InputArgsContainer;
using GZipTest.Core.SettingsContainer;

namespace GZipTest.Core.GZipEngine
{
    /// <summary>
    /// Для организации распаковки файла
    /// </summary>
    public class DecompressEngine : IGzipEngine
    {
        private readonly IInputArgs _inputArgs;

        private readonly Settings _settings;

        /// <summary>
        /// Для синхронизации чтения qzip-файла среди множества потоков
        /// </summary>
        private object _obj = new object();

        /// <summary>
        /// Для синхронизации записи данных в распакованный файл
        /// </summary>
        private object _obj2 = new object();

        /// <summary>
        /// Длина прочитанных байт gzip-файла
        /// </summary>
        private long _totalLenght;

        /// <summary>
        /// Длина последнего прочитанного на данный момент блока из gzip-файла
        /// </summary>
        int _blockLenght;

        /// <summary>
        /// Количество итерраций чтения блоков gzip-файла
        /// </summary>
        private int _index;

        public DecompressEngine(
            IInputArgs inputArgs,
            Settings settings
            )
        {
            _inputArgs = inputArgs ?? throw new ArgumentNullException(nameof(inputArgs));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _totalLenght = _settings.ConutBlockLenght;
        }

        private InputFile GetInputFile(FileStream inputFileStream)
        {
            lock (_obj)
            {
                var blockInfoBytes = new byte[_settings.BlockInfoLenght];

                _totalLenght += _blockLenght;

                inputFileStream.Seek(
                    _totalLenght + _settings.BlockInfoLenght * _index,
                    SeekOrigin.Begin
                    );

                inputFileStream.Read(
                    blockInfoBytes, 
                    0,
                    _settings.BlockInfoLenght
                    );

                _blockLenght = BitConverter.ToInt32(
                    blockInfoBytes,
                    0
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
                        _index++
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

                using (var outputBlockMemoryStream = new MemoryStream(inputFile.Data))
                {
                    using (var decompressionStream = new GZipStream(
                        outputBlockMemoryStream,
                        CompressionMode.Decompress,
                        true
                        ))
                    {
                        lock (_obj2)
                        {
                            using (var outputFileStream = File.Open(
                                _inputArgs.OutputFileName,
                                FileMode.OpenOrCreate,
                                FileAccess.Write,
                                FileShare.Read
                                ))
                            {
                                outputFileStream.Seek(
                                    inputFile.Index * _settings.BlockSize,
                                    SeekOrigin.Begin
                                    );

                                decompressionStream.CopyTo(outputFileStream);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при распаковки файла: {ex.Message}");
            }
        }

        public int GetBlockCount()
        {
            using (var inputFileStream = new FileStream(
                _inputArgs.InputFileName,
                FileMode.Open,
                FileAccess.Read
                ))
            {
                inputFileStream.Seek(
                    0,
                    SeekOrigin.Begin
                    );

                var conutBlockLenghtBytes = new byte[_settings.ConutBlockLenght];

                inputFileStream.Read(
                    conutBlockLenghtBytes,
                    0,
                    conutBlockLenghtBytes.Length
                    );

                var blockCountToInt = BitConverter.ToInt32(
                    conutBlockLenghtBytes,
                    0
                    );

                return
                    blockCountToInt;
            }
        }

        private class InputFile
        {
            public InputFile(
                byte[] data,
                long index
                )
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
                Index = index;
            }

            public byte[] Data { get; }

            public long Index { get; }
        }
    }
}