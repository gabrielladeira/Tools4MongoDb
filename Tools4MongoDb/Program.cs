using PowerArgs;

namespace Tools4MongoDb
{
    class Program
    {
        static void Main(string[] args)
        {
            Args.InvokeAction<Actions>(args);
        }

        internal class Actions
        {
            [HelpHook, ArgShortcut("-?"), ArgDescription("Shows this help")]
            public bool Help { get; set; }

            [ArgActionMethod, ArgDescription("Copy Documents from a Collection to Another")]
            public void CopyCollection(CopyCollectionArgs args)
            {
                var action = new CopyCollectionAction();
                action.Run(args);
            }
        }
    }
}
