using System;
using System.Threading;
using GZipTest.Core.GZipEngine;
using GZipTest.Core.ResourceCalculation;

namespace GZipTest.Core.ThdManager
{
    public class ThreadManager : IThreadManager
    {
        private readonly IEngine _engine;

        private readonly IResourceCalculation _resourceCalculation;

        /// <summary>
        /// Количество ресурсов, обработку которых можно распараллелить
        /// </summary>
        private readonly int _semaphoreCount;

        /// <summary>
        /// Количество ресурсов требующих обработки
        /// </summary>
        private long _resourceCount;

        /// <summary>
        /// Доля одного части сжимаемого файла, от размера целого сжимаемого файла
        /// </summary>
        private double _stake;

        /// <summary>
        /// Количество сжатых файлов (завершенных потоков)
        /// </summary>
        private int _counterCompletedThreads;

        /// <summary>
        /// Для синхронизации
        /// </summary>
        private object _obj = new object();

        public ThreadManager(
            IEngine engine,
            IResourceCalculation resourceCalculation
            )
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _resourceCalculation = resourceCalculation ?? throw new ArgumentNullException(nameof(resourceCalculation));
           
            _semaphoreCount = Environment.ProcessorCount; //при необходимости можно дать возможность через конструктор передавать
        }

        public void Start()
        {
            _resourceCount = _resourceCalculation.GetCount();
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

            Console.Write("\rЗадача успешно завершена. 100%");
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
                    if (semaphore.Wait(1))
                    {
                        try
                        {
                            _engine.Execute();
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
                        var percent = (_stake * (_counterCompletedThreads)).ToString("#.##");

                        Console.Write($"\r{percent}% ");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
