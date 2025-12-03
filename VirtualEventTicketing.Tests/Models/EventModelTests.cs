using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtualEventTicketing.Models;
using Xunit;

namespace VirtualEventTicketing.Tests.Models
{
    public class EventModelTests
    {
        [Fact]
        public void Event_WithMissingTitle_IsInvalid()
        {
            var ev = new Event
            {
                Title = string.Empty,
                TicketPrice = 10,
                AvailableTickets = 10,
                CategoryId = 1,
                StartDateTime = DateTimeOffset.UtcNow.AddDays(1)
            };

            var ctx = new ValidationContext(ev);
            var results = new List<ValidationResult>();

            var valid = Validator.TryValidateObject(ev, ctx, results, true);

            Assert.False(valid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Event.Title)));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(-1, true)]
        [InlineData(10, false)]
        public void IsSoldOut_ComputesFromAvailableTickets(int available, bool expected)
        {
            var ev = new Event { Title = "Test", AvailableTickets = available, CategoryId = 1, TicketPrice = 0, StartDateTime = DateTimeOffset.UtcNow };
            Assert.Equal(expected, ev.IsSoldOut);
        }
    }
}
