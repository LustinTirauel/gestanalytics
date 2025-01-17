using UnityEngine;
using System;

namespace GameDataEditor
{
    public class GDECodeGenConstants
    {
		public static string ClassFileDefaultPath = "CustomExtensions/";
		public static string ClassFileNameFormat = "{0}.cs";
		public static string StaticKeysFileName =  "GDEItemKeys.cs";
		public static string DataClassNameFormat = "GDE{0}Data";

		public static string AutoGenMsg = @"// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by the Game Data Editor.
//
//      Changes to this file will be lost if the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------";

		public static string StaticKeyClassHeader = @"using UnityEngine;
using System;

namespace GameDataEditor
{
    public class GDEItemKeys
    {";

        public static string DataClassHeader = @"using UnityEngine;
using System;
using System.Collections.Generic;

using GameDataEditor;

namespace GameDataEditor
{";

        public static string ClassDeclarationFormat = @"public class {0} : IGDEData";
		public static string ClassConstructorsFormat = @"public {0}(string key) : base(key) {{}}
";

        public static string LoadDictMethod = @"public override void LoadFromDict(string dataKey, Dictionary<string, object> dict)
        {
            _key = dataKey;

			if (dict == null)
				return;
			else
			{";

		public static string LoadDictMethodEnd = @"}
		}";

		public static string VariableFormat = @"static string {1}Key = ""{1}"";
		{0} _{1};
        public {0} {1}
        {{
            get {{ return _{1}; }}
        }}";

		public static string LoadVariableFormat = @"dict.TryGet{0}({1}Key, out _{1});";

		public static string LoadVariableListFormat = @"dict.TryGet{0}List({1}Key, out {1});";

		public static string StaticKeyFormat = "public static string {0}_{1} = \"{1}\";";

		public static string OneDListVariableFormat = @"static string {1}Key = ""{1}"";
		public List<{0}>      {1};";

        public static string TempStringKeyDeclaration = "string customDataKey;";
		public static string LoadCustomVariableFormat = @"dict.TryGetString({1}Key, out customDataKey);
				_{1} = new {0}(customDataKey);";

        public static int IndentLevel1 = 4;
        public static int IndentLevel2 = IndentLevel1 * 2;
        public static int IndentLevel3 = IndentLevel1 * 3;
		public static int IndentLevel4 = IndentLevel1 * 4;
    }
}
