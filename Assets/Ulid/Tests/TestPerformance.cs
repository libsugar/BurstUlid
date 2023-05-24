using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Unity.Burst;
using Unity.Jobs;
using Unity.PerformanceTesting;
using Random = Unity.Mathematics.Random;

namespace LibSugar.Unity.Tests
{

    public class TestPerformance
    {
        #region new
        
        [Test, Performance, Category("New")]
        public void TestNewGuid()
        {
            Measure.Method(() =>
                {
                    for (var i = 0; i < 100; i++)
                    {
                        Guid.NewGuid();
                    }
                })
                .WarmupCount(100)
                .IterationsPerMeasurement(100)
                .MeasurementCount(100)
                .Run();
        }

        [Test, Performance, Category("New")]
        public void TestNew()
        {
            Measure.Method(() =>
                {
                    for (var i = 0; i < 100; i++)
                    {
                        BurstUlid.NewUlid();
                    }
                })
                .WarmupCount(100)
                .IterationsPerMeasurement(100)
                .MeasurementCount(100)
                .Run();
        }

        [Test, Performance, Category("New")]
        public void TestNewCryptoRand()
        {
            Measure.Method(() =>
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        BurstUlid.NewUlidCryptoRand();
                    }
                })
                .WarmupCount(100)
                .IterationsPerMeasurement(100)
                .MeasurementCount(100)
                .Run();
        }

        [Test, Performance, Category("New")]
        public void TestNewOuterRandom()
        {
            Measure.Method(() =>
                {
                    var random = new Random((uint)DateTimeOffset.Now.ToUnixTimeSeconds());
                    for (var i = 0; i < 100; i++)
                    {
                        BurstUlid.NewUlid(ref random);
                    }
                })
                .WarmupCount(100)
                .IterationsPerMeasurement(100)
                .MeasurementCount(100)
                .Run();
        }

        [Test, Performance, Category("New")]
        public void TestNewCySharpUlid()
        {
            Measure.Method(() =>
                {
                    for (var i = 0; i < 100; i++)
                    {
                        Ulid.NewUlid();
                    }
                })
                .WarmupCount(100)
                .IterationsPerMeasurement(100)
                .MeasurementCount(100)
                .Run();
        }

        [Test, Performance, Category("New")]
        public void TestNewJob()
        {
            Measure.Method(() =>
                {
                    var job = new NewJob();
                    job.Run(100);
                })
                .WarmupCount(100)
                .MeasurementCount(100)
                .Run();
        }

        [BurstCompile]
        public struct NewJob : IJobFor
        {
            public void Execute(int index)
            {
                for (var i = 0; i < 100; i++)
                {
                    var ulid = BurstUlid.NewUlid();
                    Eat(ulid);
                }
            }

            // ReSharper disable once UnusedParameter.Local
            [MethodImpl(MethodImplOptions.NoInlining)]
            private void Eat(in BurstUlid _) { }
        }

        [Test, Performance, Category("New")]
        public void TestNewCryptoRandJob()
        {
            Measure.Method(() =>
                {
                    var job = new NewCryptoRandJob();
                    job.Run(100);
                })
                .WarmupCount(100)
                .MeasurementCount(100)
                .Run();
        }

        [BurstCompile]
        public struct NewCryptoRandJob : IJobFor
        {
            public void Execute(int index)
            {
                for (var i = 0; i < 100; i++)
                {
                    var ulid = BurstUlid.NewUlidCryptoRand();
                    Eat(ulid);
                }
            }

            // ReSharper disable once UnusedParameter.Local
            [MethodImpl(MethodImplOptions.NoInlining)]
            private void Eat(in BurstUlid _) { }
        }

        [Test, Performance, Category("New")]
        public void TestNewOuterRandomJob()
        {
            Measure.Method(() =>
                {
                    var job = new NewOuterRandomJob();
                    job.Run(100);
                })
                .WarmupCount(100)
                .MeasurementCount(100)
                .Run();
        }

        [BurstCompile]
        public struct NewOuterRandomJob : IJobFor
        {
            public void Execute(int index)
            {
                var random = Random.CreateFromIndex((uint)index);
                for (var i = 0; i < 100; i++)
                {
                    var ulid = BurstUlid.NewUlid(ref random);
                    Eat(ulid);
                }
            }

            // ReSharper disable once UnusedParameter.Local
            [MethodImpl(MethodImplOptions.NoInlining)]
            private void Eat(in BurstUlid _) { }
        }
        
        #endregion

        #region to_string parse

        [Test, Performance, Category("ToString Parse")]
        public void TestGuidToStringParse()
        {
            Measure.Method(() =>
                {
                    for (var i = 0; i < 100; i++)
                    {
                        var str = Guid.NewGuid().ToString();
                        Guid.Parse(str);
                    }
                })
                .WarmupCount(100)
                .IterationsPerMeasurement(100)
                .MeasurementCount(100)
                .Run();
        }
        
        [Test, Performance, Category("ToString Parse")]
        public void TestToStringParse()
        {
            Measure.Method(() =>
                {
                    for (var i = 0; i < 100; i++)
                    {
                        var str = BurstUlid.NewUlid().ToString();
                        BurstUlid.Parse(str);
                    }
                })
                .WarmupCount(100)
                .IterationsPerMeasurement(100)
                .MeasurementCount(100)
                .Run();
        }
        
        [Test, Performance, Category("ToString Parse")]
        public void TestCySharpToStringParse()
        {
            Measure.Method(() =>
                {
                    for (var i = 0; i < 100; i++)
                    {
                        var str = Ulid.NewUlid().ToString();
                        Ulid.Parse(str);
                    }
                })
                .WarmupCount(100)
                .IterationsPerMeasurement(100)
                .MeasurementCount(100)
                .Run();
        }

        #endregion
    }

}
