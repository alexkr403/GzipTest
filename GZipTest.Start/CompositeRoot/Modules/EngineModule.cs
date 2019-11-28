using GZipTest.Core.GZipEngine;
using GZipTest.Core.ResourceCalculation;
using GZipTest.Start.Common;
using Ninject.Modules;

namespace GZipTest.Start.CompositeRoot.Modules
{
    public class EngineModule : NinjectModule
    {
        private readonly GzipEngineEnum _gzipEngineType;

        public EngineModule(GzipEngineEnum gzipEngineType)
        {
            _gzipEngineType = gzipEngineType;
        }

        public override void Load()
        {
            if (_gzipEngineType == GzipEngineEnum.Compress)
            {
                Bind<IGzipEngine>()
                    .To<CompressEngine>()
                    //Not Singleton
                    ;

                //привязку IResourceCalculation можно вынести из под условия и определять так как показано ниже, но тогда CompressEngine
                //будет отвечать также за возврат количества ресурсов требуемых к обработке, что на мой взягляд не принесет большого профита
                // а также несколько нарушит Single Responsibility Principle
                /*Bind<IResourceCalculation>()
                    .To<ResourceCalculationCompress>()
                    .WhenInjectedInto<CompressEngine>() <---
                    .InSingletonScope()

                Bind<IResourceCalculation>()
                    .To<ResourceCalculationCompress>()
                    .WhenInjectedInto<DecompressEngine>() <---
                    .InSingletonScope()
                    ;*/
                //поэтому оставлю та как есть

                Bind<IResourceCalculation>()
                    .To<ResourceCalculationCompress>()
                    .InSingletonScope()
                    ;
            }
            else if (_gzipEngineType == GzipEngineEnum.Decompress)
            {
                Bind<IGzipEngine>()
                    .To<DecompressEngine>()
                    //Not Singleton
                    ;

                Bind<IResourceCalculation>()
                    .To<ResourceCalculationDecompress>()
                    .InSingletonScope()
                    ;
            }
        }
    }
}
