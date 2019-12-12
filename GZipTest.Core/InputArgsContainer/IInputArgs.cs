namespace GZipTest.Core.InputArgsContainer
{
    /// <summary>
    /// Пользовательские параметры из консоли
    /// </summary>
    public interface IInputArgs
    {
        string InputFileName
        {
            get;
        }

        string OutputFileName
        {
            get;
        }
    }
}
