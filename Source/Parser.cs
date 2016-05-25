using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tracery
{
	public class Parser
	{
		private string rawRule;
		private int pos;

		public Parser(string rawRule)
		{
			this.rawRule = rawRule;
			this.pos = 0;
		}

		public TraceryNode Parse()
		{
			IList<TraceryNode> nodes = new List<TraceryNode>();

			while (!Eof())
			{
				switch (rawRule[pos])
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

			return new RuleNode(nodes.ToArray());
		}

		private ActionNode ParseAction()
		{
			pos++; // advance past the opening [ character

			StringBuilder builder = new StringBuilder();
			int depth = 1; // the number of [ characters we've encountered, minus the number of ] characters
			bool escapeNext = false;

			while (depth > 0)
			{
				CheckUnexpectedEof();

				char current = rawRule[pos];

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

				pos++;
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
			return new ActionNode(action);
		}

		private PlaintextNode ParsePlaintext()
		{
			StringBuilder builder = new StringBuilder();
			bool escapeNext = false;

			while (!Eof())
			{
				char current = rawRule[pos];

				if (escapeNext)
				{
					// TODO maybe check that char is a valid escape character?
					builder.Append(current);
					escapeNext = false;
					pos++;
				}
				else
				{
					if (current == '\\')
					{
						escapeNext = true;
						pos++;
					}
					else if (current == '#' || current == '[')
					{
						break; // unescaped special char – end current plaintext section
					}
					else
					{
						builder.Append(current);
						pos++;
					}
				}
			}

			return new PlaintextNode(builder.ToString());
		}

		private TagNode ParseTag()
		{
			pos++; // advance past the opening # character

			// parse a sequence of zero or more preActions
			IList<NodeAction> preActions = new List<NodeAction>();
			while (rawRule[pos] == '[')
			{
				ActionNode actionNode = ParseAction();
				preActions.Add(actionNode.action);
			}

			StringBuilder builder = new StringBuilder();
			bool escapeNext = false;

			while (true)
			{
				CheckUnexpectedEof();

				char current = rawRule[pos];

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
						pos++;
						break; // unescaped tag-closing character – end current tag
					}
					else
					{
						builder.Append(current);
					}
				}

				pos++;
			}

			// parse the contents of the tag into a TagNode
			// TODO allow modifiers to take parameters
			string tagContent = builder.ToString();
			string[] parts = tagContent.Split('.');
			Assert.IsTrue(parts.Length > 0);
			string key = parts[0];
			string[] modifiers = parts.Skip(1).ToArray();
			return new TagNode(key, modifiers, preActions.ToArray());
		}

		private bool Eof()
		{
			return pos >= rawRule.Length;
		}

		private void CheckUnexpectedEof()
		{
			if (Eof())
			{
				throw new Exception("Encountered unexpected end of rule: " + rawRule);
			}
		}
	}
}
