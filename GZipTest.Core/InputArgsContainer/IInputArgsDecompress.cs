namespace GZipTest.Core.InputArgsContainer
{
    /// <summary>
    /// Аогументы, которые относятся к процедуре по распаковке файла
    /// </summary>
    public interface IInputArgsDecompress
    {
        /// <summary>
        /// Путь к сжатому файлу
        /// </summary>
        string InputFileName
        {
            get;
        }

        /// <summary>
        /// Путь к распакованному (результирующему) файлу
        /// </summary>
        string OutputFileName
        {
            get;
        }
    }
}
