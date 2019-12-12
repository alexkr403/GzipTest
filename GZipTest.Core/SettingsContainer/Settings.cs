namespace GZipTest.Core.SettingsContainer
{
    /// <summary>
    /// Основные настройки для сжатия/распаковки
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Размер вспомагатльного блока. Вспомагатльный блок содержит информацию длине сжимаего блока
        /// </summary>
        public int BlockInfoLenght => 8;

        /// <summary>
        /// Размер вспомогательного блока, в котором хранится информация о количестве блоков в gzip-файле
        /// </summary>
        public int ConutBlockLenght => 4;

        /// <summary>
        /// Длина gzip-блока 1 мб.
        /// </summary>
        public int BlockSize => 1048576;
    }
}
