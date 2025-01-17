using UnityEngine;
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace GameDataEditor
{
    public static class GenericExtensions
    {
        public static bool IsCloneableType<T>(this T variable)
        {
            return typeof(ICloneable).IsAssignableFrom(variable.GetType());
        }
        
        public static bool IsGenericList<T>(this T variable)
        {
            foreach (Type @interface in variable.GetType().GetInterfaces()) {
                if (@interface.IsGenericType) {
                    if (@interface.GetGenericTypeDefinition() == typeof(IList<>)) {
                        return true;
                    }
                }
            }
            return false;
        }
        
        public static bool IsGenericDictionary<T>(this T variable)
        {
            foreach (Type @interface in variable.GetType().GetInterfaces()) {
                if (@interface.IsGenericType) {
                    if (@interface.GetGenericTypeDefinition() == typeof(IDictionary<,>)) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    
    public static class FlagExtensions
    {
        public static bool IsSet(this Enum variable, Enum flag)
        {
            ulong variableVal = Convert.ToUInt64(variable);
            ulong flagVal = Convert.ToUInt64(flag);
            return (variableVal & flagVal) == flagVal;
        }
    }
    
    public static class FloatExtensions
    {
        public const float TOLERANCE = 0.0001f;
        public static bool NearlyEqual(this float a, float b)
        {
            return Math.Abs(a - b) < TOLERANCE;
        }
    }
    
    public static class ArrayExtensions
    {
        public static bool IsValidIndex(this Array variable, int index)
        {
            return index > -1 && variable != null && index < variable.Length;
        }
    }
    
    public static class ListExtensions
    {
        public static bool IsValidIndex<T>(this List<T> variable, int index)
        {
            return index > -1 && variable != null && index < variable.Count;
        }

        public static MethodInfo DeepCopyMethodInfo = typeof(ListExtensions).GetMethod("DeepCopy");
        public static List<T> DeepCopy<T>(this List<T> variable)
        {
            List<T> newList = new List<T>();
            
            T newEntry = default(T);
            foreach (T entry in variable)
            {
                if (entry == null)
                {
                    newEntry = entry;
                }
                else if (entry.IsCloneableType())
                {
                    newEntry = (T)((ICloneable)(entry)).Clone();
                }
                else if (entry.IsGenericList())
                {
                    Type listType = entry.GetType().GetGenericArguments()[0];
                    MethodInfo deepCopyMethod = DeepCopyMethodInfo.MakeGenericMethod(new Type[] { listType });
                    newEntry = (T)deepCopyMethod.Invoke(entry, new object[] {entry});
                }
                else if (entry.IsGenericDictionary())
                {
                    Type[] genericArgs = entry.GetType().GetGenericArguments();
                    Type keyType = genericArgs[0];
                    Type valueType = genericArgs[1];
                    
                    MethodInfo deepCopyMethod = DictionaryExtensions.DeepCopyMethodInfo.MakeGenericMethod(new Type[] { keyType, valueType });
                    newEntry = (T)deepCopyMethod.Invoke(entry, new object[] {entry});
                }
                else
                {
                    newEntry = entry;
                }
                
                newList.Add(newEntry);
            }
            return newList;
        }
        
        public static List<int> AllIndexesOf<T>(this List<T> variable, T searchValue) 
        {
            List<int> indexes = new List<int>();
            for (int index = 0; index<= variable.Count; index ++) 
            {
                index = variable.IndexOf(searchValue, index);
                if (index == -1)
                    break;
                
                indexes.Add(index);
            }
            return indexes;
        }
    }
    
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Adds the value if the key does not exist, otherwise it updates the value for the given key
        /// </summary>
        /// <returns><c>true</c>, if add or update suceeded, <c>false</c> otherwise.</returns>
        /// <param name="key">Key of the value we are adding or updating.</param>
        /// <param name="value">Value to add or use to set as the current value for the Key.</param>
        public static bool TryAddOrUpdateValue<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, TValue value)
        {
            bool result;
            try
            {
                if (variable.ContainsKey(key))
                {
                    variable[key] = value;
                    result = true;
                }
                else
                    result = variable.TryAddValue(key, value);
            }
            catch
            {
                result = false;
            }
            
            return result;
        }
        
        public static bool TryAddValue<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, TValue value)
        {
            bool result;
            try
            {
                variable.Add(key, value);
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Reads the Value for Key and converts it to a List<object>
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetList<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out List<object> value)
        {
            bool result = true;
            value = null;
            
            try
            {
                TValue temp;
                variable.TryGetValue(key, out temp);
                value = temp as List<object>;
            }
            catch
            {
                result = false;
            }
            
            return result;
        }

        /// <summary>
        /// Reads the value for Key and converts it to a bool
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetBool<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out bool value)
        {
            bool result = true;
            value = false;

            try
            {
                TValue origValue;
                variable.TryGetValue(key, out origValue);
                value = Convert.ToBoolean(origValue);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Reads the value for Key and converts it to a List<bool>
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetBoolList<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out List<bool> value)
        {
            bool result = true;
            value = null;

            try
            {
                List<object> tempList;
                if (variable.TryGetList(key, out tempList))                
                    value = tempList.ConvertAll(obj => Convert.ToBoolean(obj));
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Reads the value for Key and converts it to a string
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetString<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out string value)
        {
            bool result = true;
            value = "";
            
            try
            {
                TValue origValue;
                variable.TryGetValue(key, out origValue);
                value = origValue.ToString();
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Reads the value for Key and converts it to a List<string>
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetStringList<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out List<string> value)
        {
            bool result = true;
            value = null;
            
            try
            {
                List<object> tempList;
                if (variable.TryGetList(key, out tempList))                
                    value = tempList.ConvertAll(obj => obj.ToString());
            }
            catch
            {
                result = false;
            }
            
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a float
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetFloat<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out float value)
        {
            bool result = true;
            value = 0f;
            
            try
            {
                TValue origValue;
                variable.TryGetValue(key, out origValue);
                value = Convert.ToSingle(origValue);
            }
            catch
            {
                result = false;
            }
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a List<float>
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetFloatList<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out List<float> value)
        {
            bool result = true;
            value = null;
            
            try
            {
                List<object> tempList;
                if (variable.TryGetList(key, out tempList))                
                    value = tempList.ConvertAll(obj => Convert.ToSingle(obj));
            }
            catch
            {
                result = false;
            }
            
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a int (Int32)
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetInt<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out int value)
        {
            bool result = true;
            value = 0;
            
            try
            {
                TValue origValue;
                variable.TryGetValue(key, out origValue);
                value = Convert.ToInt32(origValue);
            }
            catch
            {
                result = false;
            }
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a List<int>
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetIntList<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out List<int> value)
        {
            bool result = true;
            value = null;
            
            try
            {
                List<object> tempList;
                if (variable.TryGetList(key, out tempList))                
                    value = tempList.ConvertAll(obj => Convert.ToInt32(obj));
            }
            catch
            {
                result = false;
            }
            
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a Vector2
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetVector2<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out Vector2 value)
        {
            bool result = true;
            value = Vector2.zero;
            
            try
            {
                TValue temp;
                Dictionary<string, object> vectorDict;
                variable.TryGetValue(key, out temp);
                
                vectorDict = temp as Dictionary<string, object>;
                if (vectorDict != null)
                {
                    value.x = Convert.ToSingle(vectorDict["x"]);
                    value.y = Convert.ToSingle(vectorDict["y"]);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a List<Vector2>
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetVector2List<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out List<Vector2> value)
        {
            bool result = true;
            value = null;
            
            try
            {
                List<object> tempList;
                if (variable.TryGetList(key, out tempList))
                {
                    Vector2 vect;
                    value = new List<Vector2>();
                    foreach(object vec2 in tempList)
                    {
                        Dictionary<string, object> vectDict = vec2 as Dictionary<string, object>;

                        vect = new Vector2();

                        if (vectDict != null)
                        {
                            vectDict.TryGetFloat("x", out vect.x);
                            vectDict.TryGetFloat("y", out vect.y);
                        }

                        value.Add(vect);
                    }
                }
            }
            catch
            {
                result = false;
            }
            
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a Vector3
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetVector3<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out Vector3 value)
        {
            bool result = true;
            value = Vector3.zero;
            
            try
            {
                TValue temp;
                Dictionary<string, object> vectorDict;
                variable.TryGetValue(key, out temp);
                
                vectorDict = temp as Dictionary<string, object>;
                if (vectorDict != null)
                {
                    value.x = Convert.ToSingle(vectorDict["x"]);
                    value.y = Convert.ToSingle(vectorDict["y"]);
                    value.z = Convert.ToSingle(vectorDict["z"]);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a List<Vector3>
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetVector3List<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out List<Vector3> value)
        {
            bool result = true;
            value = null;
            
            try
            {
                List<object> tempList;
                if (variable.TryGetList(key, out tempList))
                {
                    Vector3 vect;
                    value = new List<Vector3>();
                    foreach(object vec3 in tempList)
                    {
                        Dictionary<string, object> vectDict = vec3 as Dictionary<string, object>;
                        
                        vect = new Vector3();

                        if (vectDict != null)
                        {
                            vectDict.TryGetFloat("x", out vect.x);
                            vectDict.TryGetFloat("y", out vect.y);
                            vectDict.TryGetFloat("z", out vect.z);
                        }
                        
                        value.Add(vect);
                    }
                }
            }
            catch
            {
                result = false;
            }
            
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a Vector4
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetVector4<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out Vector4 value)
        {
            bool result = true;
            value = Vector4.zero;
            
            try
            {
                TValue temp;
                Dictionary<string, object> vectorDict;
                variable.TryGetValue(key, out temp);
                
                vectorDict = temp as Dictionary<string, object>;
                if (vectorDict != null)
                {
                    value.x = Convert.ToSingle(vectorDict["x"]);
                    value.y = Convert.ToSingle(vectorDict["y"]);
                    value.z = Convert.ToSingle(vectorDict["z"]);
                    value.w = Convert.ToSingle(vectorDict["w"]);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a List<Vector4>
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetVector4List<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out List<Vector4> value)
        {
            bool result = true;
            value = null;
            
            try
            {
                List<object> tempList;
                if (variable.TryGetList(key, out tempList))
                {
                    Vector4 vect;
                    value = new List<Vector4>();
                    foreach(object vec4 in tempList)
                    {
                        Dictionary<string, object> vectDict = vec4 as Dictionary<string, object>;
                        
                        vect = new Vector4();

                        if (vectDict != null)
                        {
                            vectDict.TryGetFloat("x", out vect.x);
                            vectDict.TryGetFloat("y", out vect.y);
                            vectDict.TryGetFloat("z", out vect.z);
                            vectDict.TryGetFloat("w", out vect.w);
                        }
                        
                        value.Add(vect);
                    }
                }
            }
            catch
            {
                result = false;
            }
            
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a Color
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetColor<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out Color value)
        {
            bool result = true;
            value = Color.black;

            try
            {
                TValue temp;
                Dictionary<string, object> colorDict;
                variable.TryGetValue(key, out temp);

                colorDict = temp as Dictionary<string, object>;
                if (colorDict != null)
                {
                    colorDict.TryGetFloat("r", out value.r);
                    colorDict.TryGetFloat("g", out value.g);
                    colorDict.TryGetFloat("b", out value.b);
                    colorDict.TryGetFloat("a", out value.a);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
        
        /// <summary>
        /// Reads the value for Key and converts it to a List<Color>
        /// </summary>
        /// <returns><c>true</c>, if the value was successfully converted, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Converted value</param>
        public static bool TryGetColorList<TKey, TValue>(this Dictionary<TKey, TValue> variable, TKey key, out List<Color> value)
        {
            bool result = true;
            value = null;
            
            try
            {
                List<object> tempList;
                if (variable.TryGetList(key, out tempList))
                {
                    Color col;
                    value = new List<Color>();
                    foreach(object color in tempList)
                    {
                        Dictionary<string, object> colorDict = color as Dictionary<string, object>;
                        
                        col = new Color();
                        if (colorDict != null)
                        {
                            colorDict.TryGetFloat("r", out col.r);
                            colorDict.TryGetFloat("g", out col.g);
                            colorDict.TryGetFloat("b", out col.b);
                            colorDict.TryGetFloat("a", out col.a);
                        }
                        
                        value.Add(col);
                    }
                }
            }
            catch
            {
                result = false;
            }
            
            return result;
        }
		
		public static bool TryGetCustomList<TKey, TValue, T>(this Dictionary<TKey, TValue> variable, TKey key, out List<T> value) where T : IGDEData
		{
			bool result = true;
			value = new List<T>();
			
			try
			{
				List<string> customDataKeys;
				if (variable.TryGetStringList(key, out customDataKeys))
				{
					for (int x=0;  x<customDataKeys.Count;  x++)
						value.Add((T)Activator.CreateInstance(typeof(T), new object[]{ customDataKeys[x] }));
				}
			}
			catch
			{
				result = false;
			}
			
			return result;
		}
        
        public static MethodInfo DeepCopyMethodInfo = typeof(DictionaryExtensions).GetMethod("DeepCopy");
        public static Dictionary<TKey, TValue> DeepCopy<TKey, TValue>(this Dictionary<TKey, TValue> variable)
        {
            Dictionary<TKey, TValue> newDictionary = new Dictionary<TKey, TValue>();
            
            TKey newKey = default(TKey);
            TValue newValue = default(TValue);
            
            foreach (KeyValuePair<TKey, TValue> pair in variable)
            {
                if (pair.Key == null)
                    newKey = pair.Key;
                else if (pair.Key.IsCloneableType())
                {
                    newKey = (TKey)((ICloneable)(pair.Key)).Clone();
                }
                else if (pair.Key.IsGenericList())
                {
                    Type listType = pair.Key.GetType().GetGenericArguments()[0];                   
                    MethodInfo deepCopyMethod = ListExtensions.DeepCopyMethodInfo.MakeGenericMethod(new Type[] { listType });
                    newKey = (TKey)deepCopyMethod.Invoke(pair.Key, new object[] {pair.Key});
                }
                else if (pair.Key.IsGenericDictionary())
                {
                    Type[] genericArgs = pair.Key.GetType().GetGenericArguments();
                    Type keyType = genericArgs[0];
                    Type valueType = genericArgs[1];
                    
                    MethodInfo deepCopyMethod = DeepCopyMethodInfo.MakeGenericMethod(new Type[] { keyType, valueType });
                    newKey = (TKey)deepCopyMethod.Invoke(pair.Key, new object[] {pair.Key});
                }
                else
                    newKey = pair.Key;
                
                if (pair.Value == null)
                    newValue = pair.Value;
                else if (pair.Value.IsCloneableType())
                {
                    newValue = (TValue)((ICloneable)(pair.Value)).Clone();
                }
                else if (pair.Value.IsGenericList())
                {
                    Type listType = pair.Value.GetType().GetGenericArguments()[0];                   
                    MethodInfo deepCopyMethod = ListExtensions.DeepCopyMethodInfo.MakeGenericMethod(new Type[] { listType });
                    newValue = (TValue)deepCopyMethod.Invoke(pair.Value, new object[] {pair.Value});
                }
                else if (pair.Value.IsGenericDictionary())
                {
                    Type[] genericArgs = pair.Value.GetType().GetGenericArguments();
                    Type keyType = genericArgs[0];
                    Type valueType = genericArgs[1];
                    
                    MethodInfo deepCopyMethod = DeepCopyMethodInfo.MakeGenericMethod(new Type[] { keyType, valueType });
                    newValue = (TValue)deepCopyMethod.Invoke(pair.Value, new object[] {pair.Value});
                }
                else
                {
                    newValue = pair.Value;
                }
                
                newDictionary.Add(newKey, newValue);
            }
            return newDictionary;
        }
    }
    
    public static class StringExtensions
    {
		/// <summary>
		/// Returns a new string that hightlights the first instance of substring with html color tag
		/// Ex. "The sky is <color=blue>blue</color>!"
		/// Only supported in Unity 4.0+
		/// </summary>
		/// <returns>A new string formatted with the color tag around the first instance of substring.</returns>
		/// <param name="substring">Substring to highlight</param>
		/// <param name="color">Color to specify in the color tag</param>
		public static string HighlightSubstring(this string variable, string substring, string color)
		{
			string highlightedString = variable;
			
			if (!string.IsNullOrEmpty(substring))
			{
				int index = variable.Replace("Schema:", "       ").IndexOf(substring, StringComparison.CurrentCultureIgnoreCase);
				
				if (index != -1)
					highlightedString = string.Format("{0}<color=#{1}>{2}</color>{3}", 
					                                  variable.Substring(0, index), color, variable.Substring(index, substring.Length), variable.Substring(index+substring.Length));
			}
			
			return highlightedString;
		}

        /// <summary>
        /// Returns the Md5 Sum of a string.
        /// </summary>
        /// <returns>The Md5 sum.</returns>
        public static string Md5Sum(this string strToEncrypt)
        {
            UTF8Encoding ue = new UTF8Encoding();
            byte[] bytes = ue.GetBytes(strToEncrypt);
            
            // encrypt bytes
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);
            
            // Convert the encrypted bytes back to a string (base 16)
            string hashString = "";
            
            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }
            
            return hashString.PadLeft(32, '0');
        }

		/// <summary>
		/// Uppercases the first letter
		/// </summary>
		/// <returns>A copy of the string with the first letter uppercased.</returns>
		/// <param name="s">The string to uppercase.</param>
		public static string UppercaseFirst(this string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			char[] a = s.ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}
    }
    
    public static class ColorExtensions
    {
        public static string ToHexString(this Color32 color)
        {
            return string.Format("{0}{1}{2}", color.r.ToString("x2"), color.g.ToString("x2"), color.b.ToString("x2"));
        }
        
        public static Color ToColor(this string hex)
        {
			return (Color)hex.ToColor32();
        }

		public static Color32 ToColor32(this string hex)
		{
			if (string.IsNullOrEmpty(hex))
				return new Color32();
			
			hex = hex.Replace("#", "");
			
			byte r = byte.Parse(hex.Substring(0,2), NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2,2), NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4,2), NumberStyles.HexNumber);
			
			return new Color32(r, g, b, 1);
		}

        public static bool NearlyEqual(this Color variable, Color other)
        {
            return  variable.r.NearlyEqual(other.r) &&
                    variable.g.NearlyEqual(other.g) &&
                    variable.b.NearlyEqual(other.b);
        }
    }

    public static class VectorExtensions
    {
        public static bool NearlyEqual(this Vector3 variable, Vector3 other)
        {
            return  variable.x.NearlyEqual(other.x) &&
                    variable.y.NearlyEqual(other.y) &&
                    variable.z.NearlyEqual(other.z);
        }
    }
}

