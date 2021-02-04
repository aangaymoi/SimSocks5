using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using System.Security.Cryptography;
using System.Text;

namespace Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// IsNullOrEmpty
        /// For simple statement
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>bool</returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }

        /// <summary>
        /// ToString from list<string>        
        /// </summary>
        /// <param name="arr">list<string></param>
        /// <param name="separator">separator</param>
        /// <returns>string</returns>
        public static string ToString(this List<string> arr, string separator)
        {
            return  arr.ToArray().ToString(separator);
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <param name="arr">string[]</param>
        /// <param name="separator">string</param>
        /// <returns>string</returns>
        public static string ToString(this string[] arr, string separator)
        {
            return String.Join(separator, arr);
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <param name="arr">int[]</param>
        /// <param name="separator">string</param>
        /// <returns>string</returns>
        public static string ToString(this int[] arr, string separator)
        {
            return arr.Select(x => x.ToString()).ToArray().ToString(separator);
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <param name="arr">long[]</param>
        /// <param name="separator">string</param>
        /// <returns>string</returns>
        public static string ToString(this long[] arr, string separator)
        {
            return arr.Select(x => x.ToString()).ToArray().ToString(separator);
        }

        /// <summary>
        /// RemoveEmpty
        /// </summary>
        /// <param name="arr">string[]</param>
        /// <returns>string[]</returns>
        public static string[] RemoveEmpty(this string[] arr)
        {
            return arr.Where(x => !String.IsNullOrEmpty(x)).ToArray();
        }

        /// <summary>
        /// ToStrings
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>string[]</returns>
        public static string[] ToStrings(this string str)
        {
            if (str.IsNullOrEmpty())
                return new string[0];

            return str.Split("\n").RemoveEmpty();
        }        

        /// <summary>
        /// Split
        /// </summary>
        /// <param name="str">string</param>
        /// <param name="separator">string</param>
        /// <returns>string[]</returns>
        public static string[] Split(this string str, string separator)
        {
            if (str.IsNullOrEmpty())
            {
                return new string[1] { String.Empty };
            }

            return str.Split(new string[] { separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// WithQuote
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>string</returns>
        public static string WithQuote(this string str)
        {
            return String.Format("\"{0}\"", str);
        }

        /// <summary>
        /// PlaceAccent
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>string</returns>
        public static string PlaceAccent(this string str)
        {
            #region -- Way 01 -->
            //if (str.IsNullOrEmpty())
            //{
            //    return String.Empty;
            //}
            //string formD = str.Normalize(System.Text.NormalizationForm.FormD);
            //System.Text.StringBuilder sb = new System.Text.StringBuilder();

            //for (int ich = 0; ich < formD.Length; ich++)
            //{
            //    System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(formD[ich]);
            //    if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
            //    {
            //        sb.Append(formD[ich]);
            //    }
            //}
            //sb = sb.Replace('Đ', 'D').Replace('đ', 'd');
            //return (sb.ToString().Normalize(System.Text.NormalizationForm.FormD)); 
            #endregion

            // Way 02 --->
            if (str.IsNullOrEmpty())
            {
                return String.Empty;
            }

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = str.Normalize(System.Text.NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        /// <summary>        
        /// michael135
        /// </summary>
        /// <param name="pwd">string</param>
        /// <returns>string</returns>
        public static string ToMD5Sha1(this string pwd)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(String.Format("{0}5w89hK55ee3", pwd)));
                SHA1 sha = new SHA1CryptoServiceProvider();
                byte[] result = sha.ComputeHash(data);

                // Create a new Stringbuilder to collect the bytes 
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data  
                // and format each one as a hexadecimal string. 
                for (int i = 0; i < result.Length; i++)
                {
                    sBuilder.Append(result[i].ToString("x02"));
                }

                // Return the hexadecimal string. 
                return sBuilder.ToString();
            }
        }

        public static string ToStringBase16(this byte[] buffer)
        {
            return buffer.Aggregate(string.Empty,
                   (result, item) => (result += item.ToString("X2")));
        }

        public static int ToInt(this string obj)
        {
            int ret = 0;
            int.TryParse(obj, out ret);

            return ret;
        }

        public static long ToLong(this string obj)
        {
            long ret = 0;
            long.TryParse(obj, out ret);

            return ret;
        }
    }
}