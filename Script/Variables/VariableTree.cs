using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayBox
{
	class VariableTree
	{
		public enum DrawingType
		{
			Window,
			MonoBehaviour,
		}

		public string node;
		public VariableTree parent;
		public SortedDictionary<string, VariableTree> children;
		public List<Variable> variables;

		public VariableTree(IEnumerable<Variable> allParams)
		{
			children = new SortedDictionary<string, VariableTree>();
			variables = new List<Variable>();
			parent = null;
			node = string.Empty;

			foreach (Variable param in allParams)
			{
				IEnumerable<string> paramCategories = param.Path;
				VariableTree childCategory = this;
				foreach (string pathNode in paramCategories)
					childCategory = childCategory.GetChild(pathNode);
				childCategory.variables.Add(param);
			}

			SortVariablesRecursively();
		}

		VariableTree(string node, VariableTree parent)
		{
			this.node = node;
			this.parent = parent;
			children = new SortedDictionary<string, VariableTree>();
			variables = new List<Variable>();
		}

		void SortVariablesRecursively()
		{
			variables.Sort(VariableSorting);
			if (children == null)
				return;
			foreach (KeyValuePair<string, VariableTree> nodes in children)
				nodes.Value.SortVariablesRecursively();
		}

		int VariableSorting(Variable x, Variable y) =>
			string.Compare(x.NiceName, y.NiceName, StringComparison.Ordinal);

		VariableTree GetChild(string category)
		{
			if (children == null)
				children = new SortedDictionary<string, VariableTree>();
			if (children.ContainsKey(category))
				return children[category];

			var child = new VariableTree(category, this);
			children.Add(category, child);
			return child;
		}

		public bool IsRoot
		{
			get
			{
				List<string> categories = Path().ToList();
				return categories.Count == 1 && categories.Last() == string.Empty;
			}
		}

		public string CategoryText
		{
			get
			{
				List<string> categories = Path().Reverse().ToList();
				return IsRoot ? string.Empty : string.Join(" / ", categories);
			}
		}

		public IEnumerable<string> Path()
		{
			yield return node;
			if (parent == null)
				yield break;
			foreach (string category in parent.Path())
				yield return category;
		}
	}
}