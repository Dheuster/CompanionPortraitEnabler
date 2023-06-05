// System/C# Generic
using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using System.IO;

using Owlcat.Runtime.Core.Logging;          // LogChannel

namespace OwlcatModification.Modifications.CompanionPortraitEnabler
{
	public static class Log
	{
		public static Kingmaker.Modding.OwlcatModification Modification { get; private set; }
		public static LogChannel Logger => Modification.Logger; 
		public static bool debugEnabled = false;
		public static bool traceEnabled = false;

		public static void init(Kingmaker.Modding.OwlcatModification modification)
        {
			Modification = modification;
			always = _alwaysOn;
        }

		public static void setup(bool enableDebug, bool enableTrace)
        {
			if (enableDebug)
            {
				debugEnabled = true;
				debug = _debugOn;
            }
			if (enableTrace)
            {
				traceEnabled = true;
				trace = _traceOn;
            }
        }

		public delegate void LogSomething(string value);

		public static LogSomething trace = _traceOff;
		public static LogSomething debug = _debugOff;
		public static LogSomething always = _alwaysOn;

		public static void _traceOff(string value) { }
		public static void _debugOff(string value) { }
		public static void _traceOn(string value)  { Logger.Log(value); }
		public static void _debugOn(string value)  { Logger.Log(value); }
		public static void _alwaysOn(string value) { Logger.Log(value); }
	}
}