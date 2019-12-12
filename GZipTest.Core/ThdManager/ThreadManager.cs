using System;
using System.Diagnostics;
using System.Threading;
using GZipTest.Core.GZipEngine;

namespace GZipTest.Core.ThdManager
{
    public class ThreadManager : IThreadManager
    {
        private readonly IGzipEngine _gzipEngine;

        /// <summary>
        /// Количество ресурсов, обработку которых можно распараллелить
        /// </summary>
        private readonly int _semaphoreCount;

        /// <summary>
        /// Общее количество ресурсов требующих обработки
        /// </summary>
        private long _resourceCount;

        /// <summary>
        /// Доля одной части сжимаемого файла, от размера всего сжимаемого файла
        /// </summary>
        private double _stake;

        /// <summary>
        /// Количество сжатых блоков (завершенных потоков)
        /// </summary>
        private int _counterCompletedThreads;

        /// <summary>
        /// Для синхронизации
        /// </summary>
        private object _obj = new object();

        public ThreadManager(IGzipEngine gzipEngine)
        {
            _gzipEngine = gzipEngine ?? throw new ArgumentNullException(nameof(gzipEngine));

            _semaphoreCount = Environment.ProcessorCount*16; //при необходимости можно дать возможность через конструктор передавать
        }

        public void Start()
        {
            var stopWatch = Stopwatch.StartNew();

            _resourceCount = _gzipEngine.GetBlockCount();
            _stake = 100d / _resourceCount;

            var semaphore = new SemaphoreSlim(_semaphoreCount);
            var threads = new Thread[_resourceCount];

            for (int i = 0; i < _resourceCount; i++)
            {
                threads[i] = new Thread(StartThread);
                threads[i].Start(semaphore);
            }

            for (int i = 0; i < _resourceCount; i++)
            {
                threads[i].Join();
            }

            Console.Write($"\r100%. Задача успешно завершена. Время выполнения: {stopWatch.Elapsed}");
        }

        private void StartThread(object o)
        {
            try
            {
                var semaphore = o as SemaphoreSlim;
                if (semaphore == null)
                {
                    throw new ArgumentNullException(nameof(semaphore));
                }

                var isCompleted = false;

                while (!isCompleted)
                {
                    if (semaphore.Wait(10))
                    {
                        try
                        {
                            _gzipEngine.Execute();
                        }
                        finally
                        {
                            semaphore.Release();
                            isCompleted = true;
                        }
                    }
                    else
                    {
                        //Тайм-аут для потока
                    }
                }

                lock (_obj)
                {
                    _counterCompletedThreads++;

                    if (_resourceCount > _counterCompletedThreads)
                    {
                        var percent = (_stake * _counterCompletedThreads).ToString("#.##");

                        Console.Write($"\r{percent}% ");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();

                Environment.Exit(1);
            }
        }
    }
}
