using CourseRush.Core;
using Microsoft.ClearScript.V8;

namespace CourseRush.Auth.Crypto;

public static class RsaWebEncryptor
{
    private static bool _initialized;
    private static readonly V8ScriptEngine V8ScriptEngine = new();
    private static void EnsureInitialized()
    {
        if (_initialized) return;
        V8ScriptEngine.Execute("const window = {};");
        V8ScriptEngine.Execute(V8ScriptEngine.Compile(HttpUtils.GetString(new Uri("https://cas.hnu.edu.cn/cas/js/login/security.js"))));
        _initialized = true;
    }

    public static string Encrypt(string modulus, string exponent, string data)
    {
        EnsureInitialized();
        return V8ScriptEngine.ExecuteCommand($"const key = new window.RSAUtils.getKeyPair(\"{exponent}\", \"\", \"{modulus}\");\n    \n    window.RSAUtils.encryptedString(key,\"{data}\");");
    }
}