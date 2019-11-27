using GZipTest.Core.GZipEngine;
using GZipTest.Core.ResourceCalculation;
using GZipTest.Start.Common;
using Ninject.Modules;

namespace GZipTest.Start.CompositeRoot.Modules
{
    public class EngineModule : NinjectModule
    {
        private readonly GzipEngineEnum _gzipEngine;

        public EngineModule(GzipEngineEnum gzipEngine)
        {
            _gzipEngine = gzipEngine;
        }

        public override void Load()
        {
            if (_gzipEngine == GzipEngineEnum.Compress)
            {
                Bind<IEngine>()
                    .To<CompressEngine>()
                    //Not Singleton
                    ;

                Bind<IResourceCalculation>()
                    .To<ResourceCalculationCompress>()
                    .InSingletonScope()
                    ;
            }
            else if (_gzipEngine == GzipEngineEnum.Decompress)
            {
                Bind<IEngine>()
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
