using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using GZipTest.Core.InputArgsContainer;

namespace GZipTest.Core.GZipEngine
{
    public class CompressEngine : IGzipEngine
    {

        /// <summary>
        /// Для организации последовательной (корректной) записи массива байт каждого из сжатых файлов в распакованный файл,
        /// с помощью stream.CopyTo()
        /// </summary>
        private int _indexSync = 1;

        private readonly IInputArgsCompress _inputArgs;

        Dictionary<int, long> _dict = new Dictionary<int, long>();


        /// <summary>
        /// Счетчик для увеличения номера в имени сжатых файлов
        /// </summary>
        private int _outputFileCounter;

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

        private OutputFile GetOutputData(FileStream inputFileStream)
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

                //var temp = outputFileSize + 8;

                var outputFileBytes = new byte[outputFileSize];

                inputFileStream.Read(
                    outputFileBytes,
                    0,
                    outputFileSize
                    );

                //byte[] intBytes = BitConverter.GetBytes(outputFileBytes.LongLength);

                //IEnumerable<byte> res = intBytes.Concat(outputFileBytes);

                //Buffer.BlockCopy(intBytes, 0, outputFileBytes, 0, intBytes.Length);

                //var restored = BitConverter.ToInt64(intBytes, 0);

                _outputFileCounter++;

                return
                    new OutputFile(
                        _outputFileCounter, 
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
                    outputFile = GetOutputData(inputFileStream);
                }

                using (var memoryStream = new MemoryStream())
                {
                    using (var compressionStream = new GZipStream(
                        memoryStream,
                        CompressionMode.Compress,
                        true
                        ))
                    {
                        using (var mStream = new MemoryStream(outputFile.Data))
                        {
                            mStream.CopyTo(compressionStream);
                        }
                    }

                    //compressionStream.Write(
                    //    outputFile.Data,
                    //    0,
                    //    outputFile.Data.Length
                    //    );

                    //while (_indexSync != outputFile.Counter)
                    //{
                    //    //организуем корректную дозапись decompressed частей файла в выходной файл
                    //    Thread.Sleep(1);
                    //}

                    //byte[] intBytes = BitConverter.GetBytes(outputFileBytes.LongLength);

                    //IEnumerable<byte> res = intBytes.Concat(outputFileBytes);

                    //Buffer.BlockCopy(intBytes, 0, outputFileBytes, 0, intBytes.Length);

                    //var restored = BitConverter.ToInt64(intBytes, 0);


                    while (_indexSync != outputFile.Counter)
                    {
                        //организуем корректную дозапись decompressed частей файла в выходной файл
                        Thread.Sleep(1);
                    }

                    using (var outputFileStream = File.Open(
                        _inputArgs.OutputFileName,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.Read
                        ))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        var intBytes = BitConverter.GetBytes(memoryStream.Length);
                        outputFileStream.Write(intBytes, 0, intBytes.Length);

                        memoryStream.CopyTo(outputFileStream);

                        //byte[] f = new byte[memoryStream.Length];
                        //memoryStream.Seek(0, SeekOrigin.Begin);

                        //memoryStream.Read(f, 0, (int)memoryStream.Length);

                        _indexSync++;
                    }
                }


                //using (var inputFileStream1 = new FileStream(
                //        "bridge.bmp.gzip",
                //        FileMode.Open,
                //        FileAccess.Read
                //        ))
                //    {
                //        var intBytes = new byte[886947];

                //        inputFileStream1.Read(intBytes, 0, intBytes.Length);

                //        using (var memoryStream1 = new MemoryStream(intBytes))
                //        {
                //            using (var decompressionStream1 = new GZipStream(
                //                memoryStream1,
                //                CompressionMode.Decompress,
                //                true
                //                ))
                //            {
                //                using (var bigStreamOut = new MemoryStream())
                //                {
                //                    decompressionStream1.CopyTo(bigStreamOut);

                //                    bigStreamOut.Seek(0, SeekOrigin.Begin);

                //                    byte[] g = new byte[999999];
                //                    bigStreamOut.Read(g, 0, g.Length);

                //                    using (var outputFileStream = File.Open(
                //                        "bridgeDecompressed.bmp",
                //                        FileMode.Append,
                //                        FileAccess.Write,
                //                        FileShare.Read
                //                        ))
                //                    {
                //                        bigStreamOut.CopyTo(outputFileStream);
                //                    }
                //                }
                //            }
                //        }
                //    }



                //while (_indexSync != outputFile.Counter)
                //{
                //    //организуем корректную дозапись decompressed частей файла в выходной файл
                //    Thread.Sleep(1);
                //}

                //lock (_obj)
                //{
                //    memoryStream.Seek(0, SeekOrigin.Begin);

                //    using (var outputFileStream = File.Open(
                //        _inputArgs.OutputFileName,
                //        FileMode.Append,
                //        FileAccess.Write,
                //        FileShare.Read
                //        ))
                //    {
                //        memoryStream.CopyTo(outputFileStream);

                //        _dict.Add(_indexSync, outputFileStream.Length);

                //        _indexSync++;
                //    }
                //}

            }
            catch (Exception ex)
            {
                //var error = string.Empty;

                //if (outputFile != null)
                //{
                //    error = $"Ошибка при обработке файла {outputFile.Path}{Environment.NewLine}";
                //}

                //throw new Exception($"{error}{ex.Message}");
            }
        }

        private class OutputFile
        {
            public OutputFile(
                int counter, 
                byte[] data
                )
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
                Counter = counter;
            }

            public int Counter { get; }

            public byte[] Data { get; }
        }
    }
}
