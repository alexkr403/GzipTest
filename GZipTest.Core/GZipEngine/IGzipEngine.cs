namespace GZipTest.Core.GZipEngine
{
    /// <summary>
    /// Движок обработки файлов
    /// </summary>
    public interface IGzipEngine
    {
        /// <summary>
        /// Запуск процедуры обработки файлов
        /// </summary>
        void Execute();

        /// <summary>
        /// Количество блоков параллельную обработку которых необходимо организовать
        /// </summary>
        int GetBlockCount();
    }
}
