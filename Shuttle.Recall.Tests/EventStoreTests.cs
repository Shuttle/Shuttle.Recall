﻿using System;
using NUnit.Framework;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class EventStoreTests
    {
        [Test]
        public void Should_be_able_to_remove_an_existing_event_stream()
        {
            IEventStore store = new EventStore();
            var aggregate = new Aggregate(Guid.NewGuid());
            var eventStream = store.Get(aggregate.Id);

            for (var i = 0; i < 100000; i++)
            {
                var moveCommand = new MoveCommand
                {
                    Address = string.Format("Address-{0}", i),
                    DateMoved = DateTime.Now
                };

                eventStream.AddEvent(aggregate.Move(moveCommand));

                if (eventStream.ShouldSnapshot(100))
                {
                    eventStream.AddSnapshot(aggregate.State);
                }
            }

            store.SaveEventStream(eventStream);

            eventStream = store.Get(aggregate.Id);

            Assert.IsFalse(eventStream.IsEmpty);

            store.Remove(aggregate.Id);

            eventStream = store.Get(aggregate.Id);

            Assert.IsTrue(eventStream.IsEmpty);
        }

        [Test]
        public void Should_be_able_to_use_event_stream()
        {
            IEventStore store = new EventStore();
            var aggregate = new Aggregate(Guid.NewGuid());
            var eventStream = store.Get(aggregate.Id);

            var moveCommand = new MoveCommand();

            for (var i = 0; i < 100000; i++)
            {
                moveCommand = new MoveCommand
                {
                    Address = string.Format("Address-{0}", i),
                    DateMoved = DateTime.Now
                };

                eventStream.AddEvent(aggregate.Move(moveCommand));
            }

            store.SaveEventStream(eventStream);

            aggregate = new Aggregate(aggregate.Id);
            eventStream = store.Get(aggregate.Id);

            eventStream.Apply(aggregate);

            Assert.AreEqual(moveCommand.Address, aggregate.State.Location.Address);
            Assert.AreEqual(moveCommand.DateMoved, aggregate.State.Location.DateMoved);
        }


        [Test]
        public void Should_be_able_to_use_event_stream_with_ICanSnapshot()
        {
            IEventStore store = new EventStore();
            var aggregate = new AggregateCanSnapshot(Guid.NewGuid());
            var eventStream = store.Get(aggregate.Id);

            eventStream.Apply(aggregate);

            Assert.IsTrue(eventStream.CanSnapshot);

            var moveCommand = new MoveCommand();

            for (var i = 0; i < 100000; i++)
            {
                moveCommand = new MoveCommand
                {
                    Address = string.Format("Address-{0}", i),
                    DateMoved = DateTime.Now
                };

                eventStream.AddEvent(aggregate.Move(moveCommand));
            }

            store.SaveEventStream(eventStream);

            aggregate = new AggregateCanSnapshot(aggregate.Id);
            eventStream = store.Get(aggregate.Id);

            eventStream.Apply(aggregate);

            var snapshot = (AggregateState) eventStream.Snapshot.Data;

            Assert.AreEqual(moveCommand.Address, snapshot.Location.Address);
            Assert.AreEqual(moveCommand.DateMoved, snapshot.Location.DateMoved);
        }

        [Test]
        public void Should_be_able_to_use_event_stream_with_snapshot()
        {
            IEventStore store = new EventStore();
            var aggregate = new Aggregate(Guid.NewGuid());
            var eventStream = store.Get(aggregate.Id);

            var moveCommand = new MoveCommand();

            for (var i = 0; i < 100000; i++)
            {
                moveCommand = new MoveCommand
                {
                    Address = string.Format("Address-{0}", i),
                    DateMoved = DateTime.Now
                };

                eventStream.AddEvent(aggregate.Move(moveCommand));

                if (eventStream.ShouldSnapshot(100))
                {
                    eventStream.AddSnapshot(aggregate.State);
                }
            }

            store.SaveEventStream(eventStream);

            aggregate = new Aggregate(aggregate.Id);
            eventStream = store.Get(aggregate.Id);

            eventStream.Apply(aggregate);

            Assert.AreEqual(moveCommand.Address, aggregate.State.Location.Address);
            Assert.AreEqual(moveCommand.DateMoved, aggregate.State.Location.DateMoved);
        }
    }
}