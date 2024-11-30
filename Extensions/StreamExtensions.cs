using System.IO;
using System.Threading.Tasks;

namespace elasticsearch.Extensions;

public static class StreamExtensions
{
    public static async Task<string> ReadResponseContent(MemoryStream buffer)
    {
        buffer.Seek(0, SeekOrigin.Begin);
        using var responseContent = new StreamReader(buffer).ReadToEndAsync();
        return await responseContent;
    }

    public static async Task CopyBufferToOriginalStream(MemoryStream buffer, Stream originalStream)
    {
        buffer.Seek(0, SeekOrigin.Begin);
        await buffer.CopyToAsync(originalStream);
    }
}