using System; // String
using System.IO; // File/FileStream/Path
using System.Collections.Generic; // Dictionary
using Newtonsoft.Json; // Json stuff

using OwlcatModification.Modifications.CompanionPortraitEnabler.Rules; // Rule

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Utility
{
    // Rule Loader
    public static class JsonUtil
    {
        public static Dictionary<string,BodyPartsInfo> BodyPartCache = new Dictionary<string, BodyPartsInfo>();
        public static Dictionary<string,Rule> RuleCache = new Dictionary<string, Rule>();

        public static void resetState()
        {
            BodyPartCache.Clear();
            // No-Op : In the future if we cache the things we load, we would
            // clear the caches here...
        }
        public static Settings LoadSettings(string path)
        {
			if (!File.Exists(path)) return null;
            Settings settings = null;
            try
            {
                settings = JsonConvert.DeserializeObject<Settings>(ReadJson(path));
            }
            catch (Exception e)
            {
                Log.always($"ERROR: Exception thrown reading Settings File [{path}]: {e.Message}");
            }
            return settings;
        }

        public static void SaveSettings(Settings settings,  string path)
        {
            if (null == path) return;
            try
            {
                File.WriteAllText(path, GetSettingsJson(settings));
            } 
            catch (Exception e)
            {
                Log.always($"ERROR: Exception saving [{path}]: {e.Message} {e.StackTrace}");
            }
        }

        public static BodyPartsInfo LoadBodyPartsInfo(string path)
        {
            if (null == path) return null;
            if (BodyPartCache.TryGetValue(path,out BodyPartsInfo cache)) {
                return cache;
            }
			if (!File.Exists(path)) return null;
            BodyPartsInfo bodyPartsInfo = null;
            try
            {
                bodyPartsInfo = JsonConvert.DeserializeObject<BodyPartsInfo>(ReadJson(path));
            }
            catch (Exception e)
            {
                Log.always($"ERROR: Exception thrown reading Body File [{path}]: {e.Message}");
                Log.always($"ERROR: Body File [{path}]: Future requests will be ignored until game restart.");
            }
            BodyPartCache[path] = bodyPartsInfo;
            return bodyPartsInfo;
        }

        public static void SaveBodyPartsInfo(BodyPartsInfo bodyPartsInfo, string name, string path=null)
        {
            try
            {
                if (null == path)
                { 
                    File.WriteAllText($"{name}_body.json", GetBodyPartsInfoJson(bodyPartsInfo));
                }
                else
                { 
                    File.WriteAllText(Path.Combine(path,$"{name}_body.json"), GetBodyPartsInfoJson(bodyPartsInfo));
                }
            } 
            catch (Exception e)
            {
                Log.always($"ERROR: Exception saving [{path}]: {e.Message} {e.StackTrace}");
            }

        }

		public static Rule LoadRule(string path) 
		{ 
            if (null == path) return null;
            if (RuleCache.TryGetValue(path, out Rule cache)) {
                return cache;
            }
			if (!File.Exists(path)) return null;
            Rule rule = null;
            try
            {
                rule = JsonConvert.DeserializeObject<Rule>(ReadJson(path));
            }
            catch (Exception e)
            {
                Log.always($"ERROR: Exception thrown reading Rule File [{path}]: {e.Message}");
                Log.always($"ERROR: Rule File [{path}]: Future requests will be ignored until game restart.");
            }
            RuleCache[path] = rule;
            return rule;
        }

        public static void SaveRule(Rule rule, string path)
        {
            // Unneeded?
        }

        public static string GetBodyPartsInfoJson(BodyPartsInfo bodyPartsInfo)
        {
            return JsonConvert.SerializeObject(
                      bodyPartsInfo,
                      Formatting.Indented,
                      new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                   );
        }

        public static string GetSettingsJson(Settings settings)
        {
            return JsonConvert.SerializeObject(
                      settings,
                      Formatting.Indented,
                      new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                   );
        }

        public static string GetKeyValueMap(Dictionary<String,String> keyValueMap)
        {
            return JsonConvert.SerializeObject(
                      keyValueMap,
                      Formatting.Indented,
                      new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                   );
        } 

        public static string ReadJson(string path)
        {
            string json = null;
            using (FileStream fileStream = File.OpenRead(path))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    json = streamReader.ReadToEnd();
                }
            }
            return json;
        }

    }
}
