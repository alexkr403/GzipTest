using System;
using System.IO;
using System.IO.Compression;
using GZipTest.Core.InputArgsContainer;

namespace GZipTest.Core.GZipEngine
{
    public class CompressEngine : IEngine
    {
        private readonly IInputArgsCompress _inputArgs;

        /// <summary>
        /// Счетчик для увеличения номера в имени сжатых файлов
        /// </summary>
        private long _outputFileCounter;

        /// <summary>
        /// Количество уже прочитанных байт в сжимаемом файле
        /// </summary>
        private long _readedCountBytes;

        /// <summary>
        /// Для синхронизации чтения исходного (сжимаемого) файла
        /// </summary>
        private object _obj = new object();

        public CompressEngine(IInputArgsCompress inputArgs)
        {
            _inputArgs = inputArgs ?? throw new ArgumentNullException(nameof(inputArgs));
        }

        private OutputFile GetOutputFile(FileStream inputFileStream)
        {
            lock (_obj)
            {
                _readedCountBytes = _inputArgs.OutputFileSize * _outputFileCounter;

                inputFileStream.Seek(
                    _readedCountBytes, 
                    SeekOrigin.Begin
                    );

                var countBytesToRead = inputFileStream.Length - _readedCountBytes;

                var outputFileSize = countBytesToRead > _inputArgs.OutputFileSize
                    ? _inputArgs.OutputFileSize
                    : (int)countBytesToRead;

                var outputFileBytes = new byte[outputFileSize];

                inputFileStream.Read(
                    outputFileBytes,
                    0,
                    outputFileSize
                    );

                _outputFileCounter++;

                return
                    new OutputFile(
                        _inputArgs.OutputFileName + _outputFileCounter, 
                        outputFileBytes
                        );
            }
        }

        public void Execute()
        {
            OutputFile outputFile = null;

            try
            {
                using (var inputFileStream = new FileStream(
                    _inputArgs.InputFileName,
                    FileMode.Open,
                    FileAccess.Read
                    ))
                {
                    outputFile = GetOutputFile(inputFileStream);

                    using (var outputFileStream = new FileStream(
                        outputFile.Path,
                        FileMode.Create,
                        FileAccess.Write
                        ))
                    {
                        using (var compressionStream = new GZipStream(
                            outputFileStream, 
                            CompressionMode.Compress
                            ))
                        {
                            compressionStream.Write(
                                outputFile.Data, 
                                0, 
                                outputFile.Data.Length
                                );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var error = string.Empty;

                if (outputFile != null)
                {
                    error = $"Ошибка при обработке файла {outputFile.Path}{Environment.NewLine}";
                }

                throw new Exception($"{error}{ex.Message}");
            }
        }

        private class OutputFile
        {
            public OutputFile(string path, byte[] data)
            {
                Path = path ?? throw new ArgumentNullException(nameof(path));
                Data = data ?? throw new ArgumentNullException(nameof(data));
            }

            public string Path { get; }

            public byte[] Data { get; }
        }
    }
}
