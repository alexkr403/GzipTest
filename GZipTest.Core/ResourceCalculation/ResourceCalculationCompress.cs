using System;
using System.IO;
using GZipTest.Core.InputArgsContainer;

namespace GZipTest.Core.ResourceCalculation
{
    public class ResourceCalculationCompress: IResourceCalculation
    {
        private readonly IInputArgsCompress _inputArgs;

        public ResourceCalculationCompress(IInputArgsCompress inputArgs)
        {
            _inputArgs = inputArgs ?? throw new ArgumentNullException(nameof(inputArgs));
        }

        public long GetCount()
        {
            var fileLength = new FileInfo(_inputArgs.InputFileName).Length;

            var resourceCount = fileLength / _inputArgs.OutputFileSize;
            resourceCount +=
                _inputArgs.OutputFileSize == fileLength
                    ? 0
                    : 1;

            return
                resourceCount;
        }
    }
}