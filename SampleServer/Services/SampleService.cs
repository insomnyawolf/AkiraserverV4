using System.Diagnostics;

namespace SampleServer
{
    public class SampleService : ISampleService
    {
        private int i;
        private readonly Stopwatch watch;

        public SampleService()
        {
            watch = Stopwatch.StartNew();
        }

        public int RequestNumber()
        {
            i++;
            return i;
        }

        public double RequestPerSecond()
        {
            i++;
            return i / watch.Elapsed.TotalSeconds;
        }

        public void RequestRestart()
        {
            watch.Restart();
        }
    }

    public interface ISampleService
    {
        public int RequestNumber();
        public double RequestPerSecond();
        public void RequestRestart();
    }
}