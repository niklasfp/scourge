using FluentAssertions;
using Scourge.Hurt;

namespace Scourge.Tests
{
    public class CrashalotTests
    {
        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(ArgumentNullException))]
        public void ThrowExceptionMustThrowCorrectException(Type exceptionType)
        {
            Assert.Throws(exceptionType, () => Crashalot.ThrowException(exceptionType, ""));
        }

        [Fact]
        public void ThrowExceptionMustThrowCorrectExceptionWithGenericType()
        {
            var actNull = () => Crashalot.ThrowException<ArgumentNullException>("");
            actNull.Should().Throw<ArgumentNullException>();

            var actArg = () => Crashalot.ThrowException<ArgumentException>("");
            actArg.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        public void ThrowExceptionShouldFailWithNonExceptionTypes(Type aType)
        {
            var act = () => Crashalot.ThrowException(aType, "");
            act.Should().Throw<ArgumentException>().WithMessage($"{aType} is not an exception type.");
        }


        // Cannot test StackOverflow and AsyncVoidThrow from xunit.
    }
}