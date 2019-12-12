using System;

namespace GZipTest.Core.InputArgsContainer
{
    public class InputArgs : IInputArgs
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
