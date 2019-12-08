using System;

namespace GZipTest.Core.InputArgsContainer
{
    /// <summary>
    /// Аогументы, которые относятся к процедуре по распаковке файла
    /// </summary>
    public class InputArgs
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
    }
}
