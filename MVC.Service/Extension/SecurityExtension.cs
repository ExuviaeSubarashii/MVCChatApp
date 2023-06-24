using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MVC.Domain.SupClass;

namespace MVC.Service.Extension
{
    public static class SecurityExtension
    {
        public static string ConvertStringToMD5(this string ClearText)
        {
            // byte[] ByteData = Encoding.ASCII.GetBytes(ClearText);
            // MD5 oMd5 = MD5.Create();
            // byte[] HashData = oMd5.ComputeHash(ByteData);
            // StringBuilder oSb = new StringBuilder();
            // for (int x = 0; x < HashData.Length; x++)
            // {
            //     oSb.Append(HashData[x].ToString("x2"));
            // }
            // return oSb.ToString();
           
             byte[] ByteData = Encoding.ASCII.GetBytes(ClearText);
             MD5 oMd5 = MD5.Create();
             byte[] HashData = oMd5.ComputeHash(ByteData);
             StringBuilder oSb = new StringBuilder();
             for (int x = HashData.Length - 1; x >= 0; x--)
             {
                 oSb.Append(HashData[x].ToString("x2"));
             }
             return oSb.ToString();
        }
    }
}
