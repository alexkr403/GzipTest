using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipTest.Core.InputArgsContainer;

namespace GZipTest.Core.GZipEngine
{
    public class DecompressEngine : IGzipEngine
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
        private int count;

        /// <summary>
        /// Для синхронизации чтения сжатых файлов и синхронизации послед. записи массива байт в выходной массив
        /// </summary>
        private object _obj = new object();


        private long _restored = 0;

        public DecompressEngine(IInputArgsDecompress inputArgs)
        {
            _inputArgs = inputArgs ?? throw new ArgumentNullException(nameof(inputArgs));
        }

        private long totalLenght = 0;
        private int partLenght = 0;

        private InputFile GetInputFile(FileStream inputFileStream)
        {
            var byteLinkLenght = 8;

            lock (_obj)
            {
                byte[] intBytes = new byte[byteLinkLenght];

                totalLenght += partLenght;

                inputFileStream.Seek(totalLenght + byteLinkLenght * count, SeekOrigin.Begin);

                inputFileStream.Read(intBytes, 0, byteLinkLenght);

                var res = inputFileStream.Position;

                partLenght = BitConverter.ToInt32(intBytes, 0);

                byte[] partBytes = new byte[partLenght];

                var res1 = inputFileStream.Position;

                inputFileStream.Read(partBytes, 0, partLenght);

                //if (partLenght == 0)
                //{
                //    return
                //        index;
                //}

                count++;

                return 
                    new InputFile(
                        partBytes,
                        count
                        );
            }
        }

        public void Execute()
        {
            InputFile inputFile = null;

            try
            {
                using (var inputFileStream = new FileStream(
                    _inputArgs.InputFileName,
                    FileMode.Open,
                    FileAccess.Read
                    ))
                {
                    inputFile = GetInputFile(inputFileStream);


                    //byte[] intBytes = new byte[8];
                    //byte[] intBytes1 = new byte[8];

                    //inputFileStream.Read(intBytes, 0, 8);
                    //var restored = BitConverter.ToInt64(intBytes, 0);
                    //inputFileStream.Seek(restored + 8, SeekOrigin.Begin);
                    //inputFileStream.Read(intBytes1, 0, 8);
                    //var restored1 = BitConverter.ToInt64(intBytes1, 0);


                    using (var memoryStream = new MemoryStream(inputFile.Data))
                    {
                        using (var decompressionStream = new GZipStream(
                            memoryStream,
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
            }
            catch (Exception ex)
            {
                //var error = string.Empty;

                //if (inputFile != null)
                //{
                //    error = $"Ошибка при обработке файла {inputFile.Path}{Environment.NewLine}";
                //}

                //throw new Exception($"{error}{ex.Message}");
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