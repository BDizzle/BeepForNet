using System;
using System.Diagnostics;

namespace beepcore.logging
{
	public class Log
	{
		public Log()
		{
		}

		public bool isDebugEnabled()
		{
#if DEBUG
			return true;
#else
			return false;
#endif
		}

		public bool isTraceEnabled()
		{
#if TRACE
			return true;
#else
			return false;
#endif
		}

		public void debug(string s)
		{
			Debug.WriteLine(s);
		}

		public void debug(string s,Exception e)
		{
			debug(s);
			debug(e.ToString());
		}

		public void error(string s)
		{
			Console.Error.WriteLine("ERROR: " + s);
		}

		public void error(string s,Exception e)
		{
			Console.Error.WriteLine("ERROR: " + s);
			Console.Error.WriteLine(e.ToString());
		}

		public void error(Exception e)
		{
			Console.Error.WriteLine("ERROR: " + e.ToString());
		}

		public void fatal(string s)
		{
			debug(s);
		}

		public void fatal(string s,Exception e)
		{
			Console.Error.WriteLine("FATAL: " + s);
			Console.Error.WriteLine(e.ToString());
		}

		public void info(string s)
		{
			Trace.WriteLine(s);
		}

		public void trace(string s)
		{
			Trace.WriteLine(s);
		}
	}

	public class LogFactory
	{
		public static Log getLog(System.Type t)
		{
			return new Log();
		}
	}
}
