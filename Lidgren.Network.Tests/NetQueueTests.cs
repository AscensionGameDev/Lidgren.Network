using NUnit.Framework;

namespace Lidgren.Network.Tests
{
    [TestFixture]
    public class NetQueueTests
    {
        [Test]
        public void TestNetQueue()
        {
            NetQueue<int> queue = new NetQueue<int>(4);

            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);

            int[] arr = queue.ToArray();
            if (arr.Length != 3)
                Assert.Fail("NetQueue.ToArray failure");

            if (arr[0] != 1 || arr[1] != 2 || arr[2] != 3)
                Assert.Fail("NetQueue.ToArray failure");

            bool ok;
            int a;

            if (queue.Contains(4))
                Assert.Fail("NetQueue Contains failure");

            if (!queue.Contains(2))
                Assert.Fail("NetQueue Contains failure 2");

            if (queue.Count != 3)
                Assert.Fail("NetQueue failed");

            ok = queue.TryDequeue(out a);
            if (ok == false || a != 1)
                Assert.Fail("NetQueue failure");

            if (queue.Count != 2)
                Assert.Fail("NetQueue failed");

            queue.EnqueueFirst(42);
            if (queue.Count != 3)
                Assert.Fail("NetQueue failed");

            ok = queue.TryDequeue(out a);
            if (ok == false || a != 42)
                Assert.Fail("NetQueue failed");

            ok = queue.TryDequeue(out a);
            if (ok == false || a != 2)
                Assert.Fail("NetQueue failed");

            ok = queue.TryDequeue(out a);
            if (ok == false || a != 3)
                Assert.Fail("NetQueue failed");

            ok = queue.TryDequeue(out a);
            if (ok == true)
                Assert.Fail("NetQueue failed");

            ok = queue.TryDequeue(out a);
            if (ok == true)
                Assert.Fail("NetQueue failed");

            queue.Enqueue(78);
            if (queue.Count != 1)
                Assert.Fail("NetQueue failed");

            ok = queue.TryDequeue(out a);
            if (ok == false || a != 78)
                Assert.Fail("NetQueue failed");

            queue.Clear();
            if (queue.Count != 0)
                Assert.Fail("NetQueue.Clear failed");

            int[] arr2 = queue.ToArray();
            if (arr2.Length != 0)
                Assert.Fail("NetQueue.ToArray failure");
        }
    }
}