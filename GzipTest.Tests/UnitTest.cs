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
            var compressInputFile = "Bridge.bmp";
            var compressOutputFile = "Bridge.bmp.gzip";
            var decompressFile = "DecompressedBridge.bmp";

            var compressInputArgsMock = new Mock<IInputArgs>();
            var decompressInputArgsMock = new Mock<IInputArgs>();

            try
            {
                CreateSourceFile(compressInputFile);

                compressInputArgsMock.Setup(z => z.InputFileName).Returns(compressInputFile);
                compressInputArgsMock.Setup(z => z.OutputFileName).Returns(compressOutputFile);

                var compressEngine = new CompressEngine(compressInputArgsMock.Object);

                var threadManager = new ThreadManager(compressEngine);
                threadManager.Start();

                /*************************************************************************************/

                decompressInputArgsMock.Setup(z => z.InputFileName).Returns(compressOutputFile);
                decompressInputArgsMock.Setup(z => z.OutputFileName).Returns(decompressFile);

                var blockInfoCalculation = new BlockInfoCalculation();
                var decompressEngine = new DecompressEngine(
                    decompressInputArgsMock.Object,
                    blockInfoCalculation
                    );

                threadManager = new ThreadManager(decompressEngine);
                threadManager.Start();

                var toCompressFileHash = GetHash(compressInputFile);
                var toDecompressFileHash = GetHash(decompressFile);

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
                        if (file.Contains(compressInputFile, StringComparison.OrdinalIgnoreCase))
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
