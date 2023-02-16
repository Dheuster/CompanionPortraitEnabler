// System C#/Generic
using System;
using System.Collections.Generic;

namespace OwlcatModification.Modifications.NPCCustomPortraitEnabler
{
	[Serializable]
	public class ConfigData
	{
		public string Documentation;
		public bool Disabled;
		public bool LogDebug;
		public bool CreateIfMissing;
		public string LastLoadTime;
		public string SubDirectory;
		public string PortraitsFolder;
	}
}
