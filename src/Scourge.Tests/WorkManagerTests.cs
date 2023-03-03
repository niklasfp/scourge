using FluentAssertions;
using Scourge.Hurt;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scourge.Tests
{
    public class WorkManagerTests
    {
        [Fact]
        public void ShouldBeAbleToCreateDefaultAndEmptyManager()
        {
            var sut = IWorkManager.CreateDefault();
            sut.GetActiveWork().Should().BeEmpty();
        }

        [Fact]
        public void ShouldBeAbleToCreateWork()
        {
            var sut = IWorkManager.CreateDefault();

            var work = sut.StartWork(cancel => ThreadWhipper.DoNothingWork(1, cancel));
            work.Id.Should().NotBeEmpty();
            work.StartTime.Should().BeBefore(DateTimeOffset.UtcNow);
            work.Work.IsRunning.Should().BeTrue();

            sut.StopWork(work.Id).Should().BeTrue();
            work.Work.IsRunning.Should().BeFalse();
        }


        [Fact]
        public void ShouldBeAbleToGetActiveWork()
        {
            var sut = IWorkManager.CreateDefault();

            var work1 = sut.StartWork(cancel => ThreadWhipper.DoNothingWork(1, cancel));
            var work2 = sut.StartWork(cancel => ThreadWhipper.DoNothingWork(1, cancel));

            sut.GetActiveWork().Should().HaveCount(2);

            sut.StopWork(work1.Id);
            sut.StopWork(work2.Id);
        }

        [Fact]
        public void ShouldIndicateThatNonExistingWorkCannotBeStopped()
        {
            var sut = IWorkManager.CreateDefault();
            sut.StopWork(Guid.NewGuid().ToString("N")).Should().BeFalse();
        }
    }
}