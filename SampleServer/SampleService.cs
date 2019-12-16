namespace SampleServer
{
    public class SampleService
    {
        private int i;

        public int RequestNumber()
        {
            i++;
            return i;
        }
    }
}