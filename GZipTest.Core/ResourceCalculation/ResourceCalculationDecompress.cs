using System;
using System.IO;
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
            const int byteLinkLenght = 8;

            long totalLenght = 0;
            int partLenght = 0;
            int count = 0;

            using (var inputFileStream = new FileStream(
                _inputArgs.InputFileName,
                FileMode.Open
                ))
            {
                while (true)
                {
                    var intBytes = new byte[byteLinkLenght];

                    totalLenght += partLenght;

                    inputFileStream.Seek(totalLenght + byteLinkLenght * count, SeekOrigin.Begin);

                    inputFileStream.Read(intBytes, 0, byteLinkLenght);

                    partLenght = BitConverter.ToInt32(intBytes, 0);

                    if (partLenght == 0)
                    {
                        return
                            count;
                    }

                    count++;
                }
            }
        }
    }
}