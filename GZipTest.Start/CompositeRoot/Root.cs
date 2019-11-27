using GZipTest.Core.InputArgsContainer;
using GZipTest.Core.ThdManager;
using Ninject;

namespace GZipTest.Start.CompositeRoot
{
    public class Workstation : StandardKernel
    {
        public void Init(
            string inputFileName,
            string outputFileName
            )
        {
            Bind<IThreadManager>()
                .To<ThreadManager>()
                //Not Singleton
                ;

            Bind<IInputArgsCompress, IInputArgsDecompress>()
                .To<InputArgs>()
                .InSingletonScope()
                .WithConstructorArgument("inputFileName", inputFileName)
                .WithConstructorArgument("outputFileName", outputFileName)
                ;
        }
    }
}
