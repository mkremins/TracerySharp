using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tracery
{
	public class Parser
	{
		private string rawRule;
		private int pos;
		private Stack<StringBuilder> contexts;

		public Parser(string rawRule)
		{
			this.rawRule = rawRule;
			this.pos = 0;
			this.contexts = new Stack<StringBuilder>();
		}

		public TraceryNode Parse()
		{
			PushContext();
			IList<TraceryNode> nodes = new List<TraceryNode>();

			while (!Eof())
			{
				switch (CurrentChar())
				{
				case '[':
					nodes.Add(ParseAction());
					break;
				case '#':
					nodes.Add(ParseTag());
					break;
				default:
					nodes.Add(ParsePlaintext());
					break;
				}
			}

			return new RuleNode(nodes.ToArray(), PopContext());
		}

		private ActionNode ParseAction()
		{
			PushContext();
			Advance(); // advance past the opening [ character

			StringBuilder builder = new StringBuilder();
			int depth = 1; // the number of [ characters we've encountered, minus the number of ] characters
			bool escapeNext = false;

			while (depth > 0)
			{
				CheckUnexpectedEof();

				char current = CurrentChar();

				if (escapeNext)
				{
					// TODO maybe check that char is a valid escape character?
					builder.Append(current);
					escapeNext = false;
				}
				else
				{
					if (current == '\\')
					{
						escapeNext = true;
					}
					else if (current == '[')
					{
						depth++;
						builder.Append(current);
					}
					else if (current == ']')
					{
						depth--;
						if (depth > 0) // don't add final closing ] of entire action to contents
						{
							builder.Append(current);
						}
					}
					else
					{
						builder.Append(current);
					}
				}

				Advance();
			}

		
			string[] parts = builder.ToString().Split(new char[]{':'}, 2);
			
			Assert.AreEqual(parts.Length, 2); // TODO allow function actions
			string key = parts[0];
			NodeAction action;
			if (parts[1] == "POP")
			{
				action = new PopRulesAction(key);
			}
			else
			{
				TraceryNode[] ruleNodes = parts[1]
					.Split(',') // TODO allow escaping of commas in rules
					.Select((rawRule) => new Parser(rawRule).Parse())
					.ToArray();
				action = new PushRulesAction(key, ruleNodes);
			}
			return new ActionNode(action, PopContext());
		}

		private PlaintextNode ParsePlaintext()
		{
			PushContext();

			StringBuilder builder = new StringBuilder();
			bool escapeNext = false;

			while (!Eof())
			{
				char current = CurrentChar();

				if (escapeNext)
				{
					// TODO maybe check that char is a valid escape character?
					builder.Append(current);
					escapeNext = false;
					Advance();
				}
				else
				{
					if (current == '\\')
					{
						escapeNext = true;
						Advance();
					}
					else if (current == '#' || current == '[')
					{
						break; // unescaped special char – end current plaintext section
					}
					else
					{
						builder.Append(current);
						Advance();
					}
				}
			}

			return new PlaintextNode(builder.ToString(), PopContext());
		}

		private TagNode ParseTag()
		{
			PushContext();
			Advance(); // advance past the opening # character

			// parse a sequence of zero or more preActions
			IList<NodeAction> preActions = new List<NodeAction>();
			while (CurrentChar() == '[')
			{
				ActionNode actionNode = ParseAction();
				preActions.Add(actionNode.action);
			}

			StringBuilder builder = new StringBuilder();
			bool escapeNext = false;

			while (true)
			{
				CheckUnexpectedEof();

				char current = CurrentChar();

				if (escapeNext)
				{
					// TODO maybe check that char is a valid escape character?
					builder.Append(current);
					escapeNext = false;
				}
				else
				{
					if (current == '\\')
					{
						escapeNext = true;
					}
					else if (current == '#')
					{
						Advance();
						break; // unescaped tag-closing character – end current tag
					}
					else
					{
						builder.Append(current);
					}
				}

				Advance();
			}

			// parse the contents of the tag into a TagNode
			// TODO allow modifiers to take parameters
			string tagContent = builder.ToString();
			string[] parts = tagContent.Split('.');
			Assert.IsTrue(parts.Length > 0);
			string key = parts[0];
			string[] modifiers = parts.Skip(1).ToArray();
			return new TagNode(key, modifiers, preActions.ToArray(), PopContext());
		}

		private bool Eof()
		{
			return pos >= rawRule.Length;
		}

		private char CurrentChar()
		{
			return rawRule[pos];
		}

		private void Advance()
		{
			char current = CurrentChar();
			foreach (StringBuilder context in contexts)
			{
				context.Append(current);
			}
			pos++;
		}

		private void CheckUnexpectedEof()
		{
			if (Eof())
			{
				throw new Exception("Encountered unexpected end of rule: " + rawRule);
			}
		}

		private void PushContext()
		{
			contexts.Push(new StringBuilder());
		}

		private string PopContext()
		{
			return contexts.Pop().ToString();
		}
	}
}
