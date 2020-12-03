using Engine.Serialization;
using System;
using System.Text.RegularExpressions;

namespace TemplatesDatabase
{
	public struct ProceduralValue
	{
		private static Regex m_regEx = new Regex("\\%([A-Za-z0-9_\\-\\.\\/\\^]+)\\%", RegexOptions.Compiled);

		public string Procedure;

		public object Parse(DatabaseObject context)
		{
			Match match = m_regEx.Match(Procedure);
			if (match.Success && match.Length == Procedure.Length)
			{
				string value = match.Groups[1].Value;
				DatabaseObject databaseObject = ResolveReference(context, value);
				if (databaseObject != null)
				{
					if (databaseObject.Type.SupportsValue)
					{
						return databaseObject.Value;
					}
					return databaseObject.Name;
				}
				return $"%'{value}' not found%";
			}
			return m_regEx.Replace(Procedure, delegate(Match m)
			{
				string value2 = m.Groups[1].Value;
				DatabaseObject databaseObject2 = ResolveReference(context, value2);
				return (databaseObject2 != null) ? (databaseObject2.Type.SupportsValue ? HumanReadableConverter.ConvertToString(databaseObject2.Value) : databaseObject2.Name) : $"%'{value2}' not found%";
			});
		}

		public static DatabaseObject ResolveReference(DatabaseObject context, string reference)
		{
			if (reference.Length == 36 && reference[8] == '-' && reference[13] == '-' && reference[18] == '-' && reference[23] == '-')
			{
				Guid guid = new Guid(reference);
				return context.Database.FindDatabaseObject(guid, null, throwIfNotFound: false);
			}
			if (reference.Contains('/') || reference.Contains('.') || reference.Contains('^'))
			{
				string[] array = reference.Split(new char[1]
				{
					'/'
				}, StringSplitOptions.RemoveEmptyEntries);
				int num = 0;
				while (context != null && num < array.Length)
				{
					string text = array[num];
					if (!(text == "."))
					{
						if (text == "..")
						{
							context = context.NestingParent;
						}
						else if (text.StartsWith("..."))
						{
							string text2 = text.Substring(3);
							if (string.IsNullOrEmpty(text2))
							{
								context = context.NestingRoot;
							}
							else
							{
								while (context != null && context.Type.Name != text2)
								{
									context = context.NestingParent;
								}
							}
						}
						else if (text == "^^")
						{
							context = context.EffectiveInheritanceParent;
						}
						else
						{
							if (!text.StartsWith("^^^"))
							{
								return context.FindEffectiveNestedChild(text, null, directChildrenOnly: true, throwIfNotFound: false);
							}
							string text3 = text.Substring(3);
							if (string.IsNullOrEmpty(text3))
							{
								context = context.EffectiveInheritanceRoot;
							}
							else
							{
								while (context != null && context.Type.Name != text3)
								{
									context = context.EffectiveInheritanceParent;
								}
							}
						}
					}
					num++;
				}
				return context;
			}
			while (context != null)
			{
				DatabaseObject databaseObject = context.FindEffectiveNestedChild(reference, null, directChildrenOnly: true, throwIfNotFound: false);
				if (databaseObject != null)
				{
					return databaseObject;
				}
				context = context.NestingParent;
			}
			return null;
		}
	}
}
