using System;
using System.IO;
using System.Linq;
using GZipTest.Core.InputArgsContainer;

namespace GZipTest.Core.ResourceCalculation
{
    public class ResourceCalculationDecompress: IResourceCalculation
    {
        private readonly IInputArgsDecompress _inputArgs;

        public ResourceCalculationDecompress(IInputArgsDecompress inputArgs)
        {
            _inputArgs = inputArgs ?? throw new ArgumentNullException(nameof(inputArgs));
        }

        public long GetCount()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var filesCount = 
                Directory.GetFiles(currentDirectory)
                .Count(z => z.Contains(_inputArgs.InputFileName));

            return 
                filesCount;
        }
    }
}