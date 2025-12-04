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
            Should.Throw<ArgumentNullException>(actNull);

            var actArg = () => Crashalot.ThrowException<ArgumentException>("");
            Should.Throw<ArgumentException>(actArg);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        public void ThrowExceptionShouldFailWithNonExceptionTypes(Type aType)
        {
            var act = () => Crashalot.ThrowException(aType, "");
            var exception = Should.Throw<ArgumentException>(act);
            exception.Message.ShouldBe($"{aType} is not an exception type.");
        }


        // Cannot test StackOverflow and AsyncVoidThrow from xunit.
    }
}