using Scourge.Hurt;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scourge.Tests
{
    public class ThreadWhipperTests
    {
        [Fact]
        public void ShouldReturnSameProcessorCountAsEnvironment()
        {
            ThreadWhipper.ProcessorCount.ShouldBe(Environment.ProcessorCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        public void ShouldBeAbleToStartAndStopThreadsA(int threadCount)
        {
            using var cts = new CancellationTokenSource();
            IThreadWork? work = null;
            try
            {
                work = ThreadWhipper.DoNothingWork(threadCount, cts.Token);

                work.ThreadCount.ShouldBe(threadCount);
                work.IsRunning.ShouldBeTrue();
            }
            finally
            {
                cts.Cancel();
            }

            work.IsRunning.ShouldBeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldThrowWithInvalidThreadCount(int threadCount)
        {
            var cts = new CancellationTokenSource();
            var act = () => ThreadWhipper.DoNothingWork(threadCount, cts.Token);
            var exception = Should.Throw<ArgumentOutOfRangeException>(act);
            exception.Message.ShouldStartWith("Invalid number of threads specified.");
        }
    }
}