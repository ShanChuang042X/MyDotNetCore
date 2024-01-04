using NUnit.Framework;
using MyDotNetCoreMVC.Helpers;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyDotNetCoreMVCTests.Helpers
{
    [TestFixture]
    public class SemaphoreManagerTests
    {
        [Test]
        public async Task SemaphoreManager_ShouldPreventRaceCondition()
        {
            var semaphoreManager = new SemaphoreManager();
            int lockId = 9487;
            int numberOfThreads = 10;
            int sharedResource = 0;
            var tasks = new List<Task>();

            //simulate 10 threads trying to access the same resource
            for (int i = 0; i < numberOfThreads; i++)
            {
                int taskId = i;
                var task = Task.Run(async () =>
                {
                    using (await semaphoreManager.AcquireAsync(lockId))
                    {
                        TestContext.WriteLine($"Task {taskId} has acquired the semaphore.");

                        // Critical section
                        int newValue = sharedResource + 1;
                        await Task.Delay(1); // Simulate some work
                        sharedResource = newValue;

                        TestContext.WriteLine($"Task {taskId} is releasing the semaphore. Shared resource value: {sharedResource}");
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            TestContext.WriteLine("All tasks completed.");

            // Assertion
            sharedResource.Should().Be(numberOfThreads);
            TestContext.WriteLine($"Final shared resource value: {sharedResource}");

            // Check if lockId is removed from SemaphoreDict
            semaphoreManager.IsLockIdPresent(lockId).Should().BeFalse();
            TestContext.WriteLine($"LockId {lockId} has been removed from SemaphoreManager.");
        }
    }
}