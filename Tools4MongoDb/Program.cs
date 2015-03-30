using PowerArgs;

namespace Tools4MongoDb
{
    class Program
    {
        static void Main(string[] args)
        {
            Args.InvokeAction<ProgramActions>(args);
        }
    }
}
