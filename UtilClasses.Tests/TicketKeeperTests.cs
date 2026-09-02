using System.Linq;
using UtilClasses.Tasking;
using Xunit;

namespace UtilClasses.Tests
{
    public class TicketKeeperTests
    {
        private static TicketKeeper<string> Keeper() => new();

        [Fact]
        public void GetHandsOutTicketsInTimestampOrder()
        {
            var keeper = Keeper();
            var late = keeper.Set(30, "late");
            var early = keeper.Set(10, "early");

            Assert.True(keeper.TryGetBefore<string>(100, out var first));
            Assert.Same(early, first);
            Assert.True(keeper.TryGetBefore<string>(100, out var second));
            Assert.Same(late, second);
            Assert.False(keeper.TryGetBefore<string>(100, out _));
        }

        [Fact]
        public void RescheduleMovesATicketAndReordersTheQueue()
        {
            var keeper = Keeper();
            var first = keeper.Set(10, "first");
            var second = keeper.Set(20, "second");

            Assert.True(keeper.Reschedule(second, 5));

            Assert.True(keeper.TryGetBefore<string>(100, out var handed));
            Assert.Same(second, handed);
            Assert.Equal(5ul, handed.Timestamp);
            Assert.True(keeper.TryGetBefore<string>(100, out handed));
            Assert.Same(first, handed);
        }

        /// <summary>
        /// The point of moving a ticket rather than replacing it: whoever is awaiting the original
        /// ticket must still be completed when it fires at its new time.
        /// </summary>
        [Fact]
        public void RescheduleKeepsTheTicketsCompletionSource()
        {
            var keeper = Keeper();
            var ticket = keeper.Set(1000, "eventually");
            var task = ticket.Tcs.Task;

            //not due yet
            Assert.False(keeper.AnyBefore(10));

            Assert.True(keeper.Reschedule(ticket, 0));

            Assert.True(keeper.AnyBefore(10));
            Assert.True(keeper.TryGetBefore<string>(10, out var handed));
            handed.Complete();

            Assert.True(task.IsCompletedSuccessfully);
            Assert.Same(ticket, task.Result);
        }

        [Fact]
        public void ReschedulingATicketThatHasBeenHandedOutFails()
        {
            var keeper = Keeper();
            var ticket = keeper.Set(10, "gone");

            Assert.True(keeper.TryGetBefore<string>(100, out _));

            Assert.False(keeper.Reschedule(ticket, 0));
        }

        /// <summary>
        /// Two tickets can carry the same timestamp and item, so rescheduling has to move the
        /// exact instance it was handed and leave its twin where it is.
        /// </summary>
        [Fact]
        public void RescheduleMovesTheInstanceItWasGiven()
        {
            var keeper = Keeper();
            var a = keeper.Set(10, "same");
            var b = keeper.Set(10, "same");
            Assert.NotSame(a, b);

            Assert.True(keeper.Reschedule(b, 1));

            Assert.Equal(1ul, b.Timestamp);
            Assert.Equal(10ul, a.Timestamp);
            Assert.True(keeper.TryGetBefore<string>(100, out var handed));
            Assert.Same(b, handed);
        }
    }
}
