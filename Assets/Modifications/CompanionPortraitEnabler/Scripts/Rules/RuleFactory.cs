using System;
using System.Collections.Generic; // Needed for List, Set, Dictionary, etc...
using System.IO;                  // Path.Combine

using OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions;
using OwlcatModification.Modifications.CompanionPortraitEnabler.Utility;             // JsonUtil

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules
{
	//#################################################################################
	// Basic Components of Rule Engine:
	// --------------------------------------------------------------------------------
	// 1) RuleContext :  A snapshot of the objects/environment that a rule can be
    //                   evaluated against. Exposes the nouns that a rule can be about.
	//
	//                  Example: Snapshot of current NPC.XP and inventory
	//
	// 2) Rule :         List of conditions against values that act as trigger points 
    //                   for the nouns exposed by the Rule Context. 
	//
	//                   Example: if (NPC.XP > 1234) && (inventory.contains("kitkat"))
	//
	// 3) RuleEvaluator: A streamlined/compiled component that takes a rule and a
	//                   rule context and quickly evaluates if the rule is true against
	//                   the given context.
	//
	//                   Example: RuleEvaluator re = new RuleEvaluator(rule)
	//                            if (re.evaluate(ruleContext)) then ...
	// ---------------------------------------------------------------------------------
	// Other Components:
	// ---------------------------------------------------------------------------------
	// 1) RuleFactory: Provides the Rule Creation API. Used typically by devs to
    //                 create rules programatically
	//
	// 2) EventMonitor: Registers for events in order to keep a RuleContext up-to-date
	//                  and relavent for various tracked objects (NPCs). Typically
    //                  tracks when a rule-conext becomes stale and should be
    //                  re-evaluated again. We call this component NPCMonitor.
	//
	//#################################################################################

	public static class RuleFactory
    {
		// No need to cache. Rule searches are requested rarely, mostly only at startup. 


		public static List<Rule> findRules(string portraitsRoot, string subdir, string resourceName, string prefix, Boolean confirmPortraits)
        {
			if (portraitsRoot == null)
            {
                Log.debug($"findRules: portraitsRoot is null. Bailing...");
				return null;
            }
			if (subdir == null)
            {
                Log.debug($"findRules: subdir is null. Bailing...");
				return null;
            }
			if (resourceName == null ) {
                Log.debug($"findRules: resourceName is null. Bailing...");
				return null;
			}
			
			string shortpath = Path.Combine(subdir, resourceName);
			string fullpath = Path.Combine(portraitsRoot, shortpath);

			// string unicodePathWithDirChr = "\\\\?\\" + path;
			string unicodePathWithDirChr = fullpath;
			if (!unicodePathWithDirChr.EndsWith("" + Path.DirectorySeparatorChar)) {
				unicodePathWithDirChr += Path.DirectorySeparatorChar;
			}

            Log.trace($"TRACE: Examinging Path [{unicodePathWithDirChr}]");

			if (!Directory.Exists(unicodePathWithDirChr))
            {
                Log.debug($"Processing  [{fullpath}] : Error : No Access to path");
				return null;
            }
            Log.trace($"TRACE: Path [{unicodePathWithDirChr}] exists");
			string[] subdirs = Directory.GetDirectories(unicodePathWithDirChr);
			if (0 == subdirs.Length)
            {
                Log.debug($"Processing  [{shortpath}] : No rules found");
				return null;
            }
            Log.trace($"TRACE: Path [{shortpath}] has [{subdirs.Length}] sub-directories");
			string prefixUpper = prefix.ToUpper();
			List<Rule> allRules = new List<Rule>();
			foreach (string fullsubdir in subdirs)
            {
				string subdirName = fullsubdir.TrimEnd(Path.DirectorySeparatorChar);
				subdirName = subdirName.Substring(subdirName.LastIndexOf(Path.DirectorySeparatorChar)+1);
                Log.trace($"TRACE:   Found [{fullsubdir}] ({subdirName})");
				if (subdirName.ToUpper().StartsWith(prefixUpper))
                {
		            Log.trace($"TRACE: Scanning subdir [{fullsubdir}]");
					try
					{
						Rule theRule = searchAndLoadPath(fullsubdir, confirmPortraits);
						if (null != theRule)
						{
							theRule.resourceId = resourceName;
							theRule.portraitId = Path.Combine(shortpath, subdirName);
							if (!confirmPortraits)
							{ 
								theRule.fileName   = Path.Combine(fullpath,  subdirName, "body.json");
							}
							Log.trace($"TRACE: rule definition found [{theRule.portraitId}]");
							allRules.Add(theRule);
						}
					} 
					catch (Exception ex) 
					{
						Log.debug($"Error loading [{fullsubdir}] : {ex.Message}");
					}
				} 
				else
                {
					 Log.trace($"TRACE:  [{subdirName.ToUpper()}] does not start with [{prefixUpper}]");
                }
            }
			if (0 == allRules.Count)
            {
				return null;
            }
			return allRules;
        }

		public static Rule searchAndLoadPath(string path, Boolean confirmPortraits)
        {
			if (path == null) return null;

			string unicodePathWithDirChr = path;
			if (!unicodePathWithDirChr.EndsWith("" + Path.DirectorySeparatorChar)) {
				unicodePathWithDirChr += Path.DirectorySeparatorChar;
			}

			// Confirm : "rule.json", "Fulllength.png", "Medium.png", "Small.png" in path

			string rulePath = unicodePathWithDirChr + "rule.json";
			if (!File.Exists(rulePath)) {
				Log.debug($"[{rulePath}] Not Found");
				return null;
            }
			Log.trace($"TRACE: [{rulePath}] Found");

			if (confirmPortraits)
			{ 
				string fullPath = unicodePathWithDirChr + "Fulllength.png";
				if (!File.Exists(fullPath)) {
					Log.debug($"[{fullPath}] Not Found");
					return null;
				}

				Log.trace($"TRACE: [Fulllength.png] Found");

				string medPath  = unicodePathWithDirChr + "Medium.png";
				if (!File.Exists(medPath)) {
					Log.debug($"[{medPath}] Not Found");
					return null;
				}

				Log.trace($"TRACE: [Medium.png] Found");

				string smlPath  = unicodePathWithDirChr + "Small.png";
				if (!File.Exists(smlPath)) {
					Log.debug($"[{smlPath}] Not Found");
					return null;
				}

				Log.trace($"TRACE: [Small.png] Found");
			} 
			else
            {
				string bodyCheck = unicodePathWithDirChr + "body.json";
				if (!File.Exists(bodyCheck)) {
					Log.debug($"[{bodyCheck}] Not Found");
					return null;
				}
				Log.trace($"TRACE: [{bodyCheck}] Found");
            }

			Rule rule = JsonUtil.LoadRule(rulePath);
			if (rule == null) return null;
			Log.trace($"TRACE: Json File [{rulePath}] parsed without error. Creating Rule Evaluator");
			return addRuleEvaluator(path,rulePath,rule);
		}

		public static Rule addRuleEvaluator(string path, string rulePath, Rule rule) 
		{
			if (null == rule)
			{
				Log.debug("Bailing. Rule is invalid (null)");
				return null;
			}
			RuleEvaluator.Builder rbuilder =  RuleEvaluator.Builder.New();
			try 
			{
				Log.trace($"TRACE: [{rule.portraitId}] has [{rule.conditions.Length}] conditions");

				for (int rc = 0; rc < rule.conditions.Length;rc++)
				{ 
					Rule.Condition cond = rule.conditions[rc];
					if (cond.prop == Rule.PropValues.None)
                    {
						Log.debug($"Error loading [{rulePath}] : Missing prop value for condition [{rc+1}]");
						return null;
                    }
					IConditionInstanceBuilder propCondBuilder = CondBuildStart.New().IfProp(cond.prop.ToString());
					if (null == propCondBuilder)
					{
						Log.debug($"Error loading [{rulePath}] : Property [{cond.prop}] is invalid.");
						return null;
					}
					// There are probably better ways to do this, but I'm a java guy, not a C# guy....
					IConditionInstance propCond = null;
					try
					{ 
						switch(cond.cond)
						{
							case Rule.CondValues.eq:
							{ 
								propCond = propCondBuilder.equals(UInt32.Parse(cond.value));
								break;
							}
							case Rule.CondValues.gt:
							{ 
								propCond = propCondBuilder.greaterThan(UInt32.Parse(cond.value));
								break;
							}
							case Rule.CondValues.lt:
							{
								propCond = propCondBuilder.lessThan(UInt32.Parse(cond.value));
								break;
							}
							case Rule.CondValues.gte:
							{
								propCond = propCondBuilder.greaterThanOrEqualTo(UInt32.Parse(cond.value));
								break;
							}
							case Rule.CondValues.lte:
							{
								propCond = propCondBuilder.lessThanOrEqualTo(UInt32.Parse(cond.value));
								break;
							}
							case Rule.CondValues.neq:
							{
								propCond = propCondBuilder.notEqualTo(UInt32.Parse(cond.value));
								break;
							}
							case Rule.CondValues.any:
							{
								string[] values   = cond.value.Split(',');
								string[] filtered = new string[values.Length];
								for (int i = 0; i < values.Length; i++)
								{
									filtered[i] = values[i].Trim().ToUpper();
								}
								propCond = propCondBuilder.MatchesAny(filtered);
								break;
							}
							case Rule.CondValues.all:
							{
								string[] values   = cond.value.Split(',');
								string[] filtered = new string[values.Length];
								for (int i = 0; i < values.Length; i++)
								{
									filtered[i] = values[i].Trim().ToUpper();
								}
								propCond = propCondBuilder.MatchesAll(filtered);
								break;
							}
							case Rule.CondValues.nany:
							{
								string[] values   = cond.value.Split(',');
								string[] filtered = new string[values.Length];
								for (int i = 0; i < values.Length; i++)
								{
									filtered[i] = values[i].Trim().ToUpper();
								}
								propCond = propCondBuilder.DoesNotMatchAny(filtered);
								break;
							}
							case Rule.CondValues.nall:
							{
								string[] values   = cond.value.Split(',');
								string[] filtered = new string[values.Length];
								for (int i = 0; i < values.Length; i++)
								{
									filtered[i] = values[i].Trim().ToUpper();
								}
								propCond = propCondBuilder.DoesNotMatchAll(filtered);
								break;
							}
							default:
							{ 	
								Log.debug($"Error loading [{rulePath}] : Condition [{cond.cond} is invalid.");
								return null;
							}
						}
					} 
					catch (Exception ex)
                    {
						Log.debug($"Error loading [{rulePath}] : Value for property [{cond.prop.ToString()}] is not the right type: {ex.Message}");
						return null;
                    }				
					if (null == propCond) 
					{
						Log.debug("Error loading [" + rulePath + "] : Value [" + cond.value + "] is not compatible with property [" + cond.prop + "]");
						return null;
					}
					if (cond.next == Rule.LogicValues.or)
					{
						Log.trace($"TRACE: Adding prop [{cond.prop}] cond [{cond.cond}] value [{cond.value}] OR");
						rbuilder = rbuilder.Or(propCond);
					}
					else
					{
						Log.trace($"TRACE: Adding prop [{cond.prop}] cond [{cond.cond}] value [{cond.value}] AND");
						rbuilder = rbuilder.And(propCond);
					}
				}

				rule.ruleEvaluator = rbuilder.build();
				Log.trace($"TRACE: RuleEvaluator created...");
				return rule;
			}
			catch (Exception e)
			{
				Log.debug($"addRuleEvaluator : Error loading [{rulePath}] : {e.Message}");
				return null;
			}
		}
	}
}
