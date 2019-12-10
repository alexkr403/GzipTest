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
                    ;
            }
            else if (_gzipEngineType == GzipEngineEnum.Decompress)
            {
                Bind<IGzipEngine>()
                    .To<DecompressEngine>()
                    ;
            }

            Bind<IBlockInfoCalculation>()
                .To<BlockInfoCalculation>()
                .InSingletonScope()
                ;
        }
    }
}
