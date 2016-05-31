using System;
using System.Collections.Generic;

namespace Tracery
{
	public class Modifiers
	{
		public static IDictionary<string,Func<string,string>> BASE_ENG_MODIFIERS;

		static Modifiers()
		{
			BASE_ENG_MODIFIERS = new Dictionary<string,Func<string,string>>();
			BASE_ENG_MODIFIERS.Add("a", A);
			BASE_ENG_MODIFIERS.Add("capitalize", Capitalize);
			BASE_ENG_MODIFIERS.Add("ed", Ed);
			BASE_ENG_MODIFIERS.Add("s", S);
		}

		private static char CharToLower(char c)
		{
			return c.ToString().ToLower()[0];
		}

		private static bool IsVowel(char c)
		{
			c = CharToLower(c);
			return c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u';
		}

		public static string A(string s)
		{
			if (s.Length > 0)
			{
				// TODO There's a weird exception to the "a"/"an" rules in English:
				// although 'u' is a vowel, words that start with 'u' sometimes take "a"
				// instead of "an". Ideally, we want this function to return:
				//
				// "a unicorn"
				// "a universal truth"
				// "a urologist"
				// "an uninvited guest"
				// "an uncle"
				// "an ugly scene"
				//
				// The common factor here seems to be pronunciation. We use "a" when
				// the word begins with a /y/ sound (as in "unicorn"), and "an" when
				// the word begins with an /uh/ sound (as in "uninvited"). Maybe we can
				// somehow use a pronunciation dictionary to handle this?
				//
				// There are other weird pronunciation-based exceptions too:
				// "a horoscope" vs "an honor"
				// "a NASA spacecraft" vs "an NSA cryptanalyst"

				if (IsVowel(s[0]))
				{
					return "an " + s;
				}
			}

			return "a " + s;
		}

		public static string Capitalize(string s)
		{
			return s.Substring(0,1).ToUpper() + s.Substring(1);
		}

		public static string Ed(string s)
		{
			switch (s[s.Length - 1])
			{
			case 'e':
				return s + "d";
			case 'y':
				if (IsVowel(s[s.Length - 2]))
				{
					return s + "d";
				}
				else
				{
					return s.Substring(0, s.Length - 1) + "ied";
				}
			default:
				return s + "ed";
			}
		}

		public static string S(string s)
		{
			switch (s[s.Length - 1])
			{
			case 's':
			case 'h':
			case 'x':
				return s + "es";
			case 'y':
				if (IsVowel(s[s.Length - 2]))
				{
					return s + "s";
				}
				else
				{
					return s.Substring(0, s.Length - 1) + "ies";
				}
			default:
				return s + "s";
			}
		}
	}
}
