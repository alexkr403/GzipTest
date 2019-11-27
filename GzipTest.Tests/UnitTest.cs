using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using GZipTest.Core.GZipEngine;
using GZipTest.Core.InputArgsContainer;
using GZipTest.Core.ResourceCalculation;
using GZipTest.Core.ThdManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GzipTest.Tests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod()
        {
            var toCompressFile = "Bridge.bmp";
            var toDecompressFile = "DecompressedBridge.bmp";

            var resourceCount = 7;
            var outputFileSize = 1048576;

            var inputArgsCompressPropertiesMock = new Mock<IInputArgsCompress>();
            var inputArgsDecompressPropertiesMock = new Mock<IInputArgsDecompress>();
            var resourceCalculationMock = new Mock<IResourceCalculation>();

            try
            {
                CreateSourceFile(toCompressFile);

                inputArgsCompressPropertiesMock.SetupGet(z => z.OutputFileSize).Returns(outputFileSize);
                inputArgsCompressPropertiesMock.SetupGet(z => z.InputFileName).Returns(toCompressFile);
                inputArgsCompressPropertiesMock.SetupGet(z => z.OutputFileName).Returns(toCompressFile);

                inputArgsDecompressPropertiesMock.SetupGet(z => z.InputFileName).Returns(toCompressFile);
                inputArgsDecompressPropertiesMock.SetupGet(z => z.OutputFileName).Returns(toDecompressFile);

                resourceCalculationMock.Setup(z => z.GetCount()).Returns(resourceCount);

                var compressEngine = new CompressEngine(inputArgsCompressPropertiesMock.Object);

                var threadManager = new ThreadManager(
                    compressEngine,
                    resourceCalculationMock.Object
                    );

                threadManager.Start();

                var decompressEngine = new DecompressEngine(inputArgsDecompressPropertiesMock.Object);

                threadManager = new ThreadManager(
                    decompressEngine,
                    resourceCalculationMock.Object
                    );

                threadManager.Start();

                var toCompressFileHash = GetHash(toCompressFile);
                var toDecompressFileHash = GetHash(toDecompressFile);

                Assert.AreEqual(
                    toCompressFileHash.SequenceEqual(toDecompressFileHash),
                    true
                    );
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                try
                {
                    foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
                    {
                        if (file.Contains(toCompressFile, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }
        }

        private void CreateSourceFile(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var resFilestream = assembly.GetManifestResourceStream($"GzipTest.Tests.Resources.{file}"))
            {
                if (resFilestream == null)
                {
                    throw new ArgumentNullException(nameof(resFilestream));
                }

                var bytes = new byte[resFilestream.Length];

                resFilestream.Read(
                    bytes, 
                    0,
                    bytes.Length
                    );

                using (var memoryStream = new MemoryStream(bytes))
                {
                    using (var fileStream = new FileStream(
                        file,
                        FileMode.Create,
                        FileAccess.ReadWrite,
                        FileShare.ReadWrite
                        ))
                    {
                        memoryStream.CopyTo(fileStream);
                    }
                }
            }
        }

        private byte[] GetHash(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    return 
                        md5.ComputeHash(stream);
                }
            }
        }
    }
}
