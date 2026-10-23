using UnityEngine;

public static class StringHelpers
{
    /// 获取时间格式的字符串
    public static string GetTimeFormat(float m, float s)
    {
        return string.Format("{0:00}:{1:00}", m, s);
    }

    public static string GetTimeFormat(int seconds)
    {
        int minutes = seconds > 60 ? seconds / 60 : 0;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    /// 把像 name 这样的变量名转成可显示的文本。
    public static string NicifyVariableName(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        string result = "";
        for (int i = 0; i < str.Length; i++)
        {
            if (i == 0)
            {
                result += char.ToUpper(str[i]);
                continue;
            }
            if (char.IsUpper(str[i]) == true && i != 0)
            {
                result += " ";
            }

            result += str[i];
        }
        return result;
    }
    /// 生成指定长度的随机字符串
    public static string GenerateKey(int length = 7)
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghlmnopqrustowuvwxyz";
        string key = "";
        for (int i = 0; i < length; i++)
        {
            key += chars[Random.Range(0, chars.Length)];
        }
        return key;
    }
}
