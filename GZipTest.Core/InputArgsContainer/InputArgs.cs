using System;

namespace GZipTest.Core.InputArgsContainer
{
    public class InputArgs :
        IInputArgsCompress,
        IInputArgsDecompress
    {
        public InputArgs(
            string inputFileName,
            string outputFileName
            )
        {
            InputFileName = inputFileName ?? throw new ArgumentNullException(nameof(inputFileName));
            OutputFileName = outputFileName ?? throw new ArgumentNullException(nameof(outputFileName));
        }

        public string InputFileName
        {
            get;
        }

        public string OutputFileName
        {
            get;
        }

        // задаем здесь жестко т.к. в ТЗ не написано, что данный параметр должен быть динамическим
        /// <summary>
        /// Размер сжатых (результирующих) файлов,
        /// </summary>
        public int OutputFileSize => 999999; //8096; //1048576;
    }
}
