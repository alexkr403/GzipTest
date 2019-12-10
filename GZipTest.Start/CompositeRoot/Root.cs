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
                ;

            Bind<IInputArgs>()
                .To<InputArgs>()
                .WithConstructorArgument("inputFileName", inputFileName)
                .WithConstructorArgument("outputFileName", outputFileName)
                ;
        }
    }
}
