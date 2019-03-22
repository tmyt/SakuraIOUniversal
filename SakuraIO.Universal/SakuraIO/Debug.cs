namespace SakuraIO
{
    public static class Debug
    {
        public static bool SakuraDebug = false;

        public static void dbg(params string[] args)
        {
            if (!SakuraDebug) return;
            foreach(var s in args) System.Diagnostics.Debug.Write(s);
        }

        public static void dbgln(params string[] args)
        {
            if (!SakuraDebug) return;
            dbg(args);
            dbg("\n");
        }
    }
}
