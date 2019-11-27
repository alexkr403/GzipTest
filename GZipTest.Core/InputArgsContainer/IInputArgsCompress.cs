namespace GZipTest.Core.InputArgsContainer
{
    /// <summary>
    /// Аргументы, которые относятся к процедуре по сжатию файла
    /// </summary>
    public interface IInputArgsCompress
    {
        /// <summary>
        /// Путь к исходному файлу (файлу для сжатия)
        /// </summary>
        string InputFileName
        {
            get;
        }

        /// <summary>
        /// Путь к сжатым (результирующим) файлам
        /// </summary>
        string OutputFileName
        {
            get;
        }

        /// <summary>
        /// Размер сжатых (результирующих) файлов
        /// </summary>
        int OutputFileSize
        {
            get;
        }
    }
}
