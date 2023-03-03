using FluentAssertions;
using Scourge.Hurt;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scourge.Tests
{
    public class ThreadWhipperTests
    {
        [Fact]
        public void ShouldReturnSameProcessorCountAsEnvironment()
        {
            ThreadWhipper.ProcessorCount.Should().Be(Environment.ProcessorCount);
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

                work.ThreadCount.Should().Be(threadCount);
                work.IsRunning.Should().BeTrue();
            }
            finally
            {
                cts.Cancel();
            }

            work.IsRunning.Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldThrowWithInvalidThreadCount(int threadCount)
        {
            var cts = new CancellationTokenSource();
            var act = () => ThreadWhipper.DoNothingWork(threadCount, cts.Token);
            act.Should().Throw<ArgumentOutOfRangeException>()
                .Where(e => e.Message.StartsWith("Invalid number of threads specified."));
        }
    }
}