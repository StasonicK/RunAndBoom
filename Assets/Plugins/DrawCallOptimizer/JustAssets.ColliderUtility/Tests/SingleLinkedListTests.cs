using System.Collections.Generic;
using JustAssets.ColliderUtilityRuntime.Optimization;
using NUnit.Framework;

namespace JustAssets.ColliderUtilityTests.Assets.ColliderUtility.Tests
{
    internal class SingleLinkedListTests
    {
        [Test]
        public void TestCreation_ContainsThreeElements()
        {
            var list = new SingleLinkedList<int>(new List<int>{1,2,3});

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list.First.Value);
            Assert.AreEqual(3, list.Last.Value);
        }

        [Test]
        public void TestInsertion_ContainsFiveElements()
        {
            var list = new SingleLinkedList<int>(new List<int> { 1, 2, 5 });
            var secondList = new SingleLinkedList<int>(new List<int> { 3, 4 });

            list.InsertAt(list.First.Next, secondList.First, secondList.Last);
            secondList.Clear();

            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(1, list.First.Value);
            Assert.AreEqual(5, list.Last.Value);
        }

        [Test]
        public void TestInsertionAtEnd_ContainsFiveElements()
        {
            var list = new SingleLinkedList<int>(new List<int> { 1, 2, 3 });
            var secondList = new SingleLinkedList<int>(new List<int> { 4, 5 });

            list.InsertAt(list.Last, secondList.First, secondList.Last);
            secondList.Clear();

            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(1, list.First.Value);
            Assert.AreEqual(5, list.Last.Value);
        }

        [Test]
        public void TestInsertionAtStart_ContainsFiveElements()
        {
            var list = new SingleLinkedList<int>(new List<int> { 3, 4, 5 });
            var secondList = new SingleLinkedList<int>(new List<int> { 1, 2 });

            list.InsertAt(null, secondList.First, secondList.Last);
            secondList.Clear();

            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(1, list.First.Value);
            Assert.AreEqual(5, list.Last.Value);
        }

        [Test]
        public void TestRemoveInBetween_ContainsThreeElements()
        {
            var list = new SingleLinkedList<int>(new List<int> { 1, 2, 3, 4, 5 });

            list.Remove(3);
            list.Remove(4);

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list.First.Value);
            Assert.AreEqual(5, list.Last.Value);
        }

        [Test]
        public void TestRemoveAtEnd_ContainsThreeElements()
        {
            var list = new SingleLinkedList<int>(new List<int> { 1, 2, 3, 4 });

            list.Remove(4);

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list.First.Value);
            Assert.AreEqual(3, list.Last.Value);
        }

        [Test]
        public void TestRemoveAtStart_ContainsThreeElements()
        {
            var list = new SingleLinkedList<int>(new List<int> { 1, 2, 3, 4 });

            list.Remove(1);

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(2, list.First.Value);
            Assert.AreEqual(4, list.Last.Value);
        }

    }
}
