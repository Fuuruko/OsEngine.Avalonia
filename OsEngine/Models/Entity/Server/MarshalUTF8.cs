using System.Runtime.InteropServices;
using System.Text;

namespace OsEngine.Models.Entity.Server;

static class MarshalUtf8
{
    private static UTF8Encoding _utf8 =  new();

    public static nint StringToHGlobalUtf8(string data)
    {
        byte[] dataEncoded = _utf8.GetBytes(data + "\0");

        int size = Marshal.SizeOf(dataEncoded[0]) * dataEncoded.Length;

        nint pData = Marshal.AllocHGlobal(size);

        Marshal.Copy(dataEncoded, 0, pData, dataEncoded.Length);

        return pData;
    }

    public static string PtrToStringUtf8(nint pData)
    {
        // this is just to get buffer length in bytes
        string errStr = Marshal.PtrToStringAnsi(pData);
        int length = errStr.Length;

        byte[] data = new byte[length];
        Marshal.Copy(pData, data, 0, length);

        return _utf8.GetString(data);
    }
}
