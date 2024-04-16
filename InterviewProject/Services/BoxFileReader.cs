using InterviewProject.Models;

namespace InterviewProject.Services;

public class BoxFileReader : IAsyncEnumerable<IReadOnlyList<Box>>, IDisposable
{
    const string boxStart = "HDR";
    const string contentStart = "LINE";
    private readonly int batchSize;
    private readonly StreamReader streamReader;
    private Box lastBox;

    public BoxFileReader(string filePath, int batchSize)
    {
        this.batchSize = batchSize;
        streamReader = new StreamReader(filePath);
    }

    public async IAsyncEnumerator<IReadOnlyList<Box>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        while (!streamReader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            yield return await ReadNextBatchAsync(cancellationToken);
        }
    }

    public void Dispose()
    {
        streamReader.Dispose();
    }

    private async Task<IReadOnlyList<Box>> ReadNextBatchAsync(CancellationToken cancellationToken)
    {
        List<Box> boxes = new List<Box>(batchSize);
        try
        {
            while (boxes.Count < batchSize && !cancellationToken.IsCancellationRequested)
            {
                if (streamReader.EndOfStream)
                {
                    if (lastBox != null)
                        boxes.Add(lastBox);

                    break;
                } 

                var line = await streamReader.ReadLineAsync();

                if (IsBox(line))
                {
                    if (lastBox != null)
                        boxes.Add(lastBox);

                    lastBox = ParseBox(line);
                }
                else if (IsContent(line))
                {
                    var content = ParseContent(line);
                    lastBox.AddContent(content);
                }
            }

            return boxes;
        }
        catch (Exception ex)
        {
            // Log error
            throw ex;
        }
    }

    private static bool IsBox(string line)
    {
        return !string.IsNullOrEmpty(line) && line.IndexOf(boxStart) == 0;
    }

    private static bool IsContent(string line)
    {
        return !string.IsNullOrEmpty(line) && line.IndexOf(contentStart) == 0;
    }

    private static Box ParseBox(string line)
    {
        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length != 3) throw new Exception("Invalid Box format");

        Box box = new Box();
        box.SupplierIdentifier = words[1];
        box.Identifier = words[2];
        return box;
    }

    private static Content ParseContent(string line)
    {
        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length != 4) throw new Exception("Invalid Content format");

        Content content = new Content();
        content.PoNumber = words[1];
        content.Isbn = words[2];
        content.Quantity = int.Parse(words[3]);
        return content;
    }
}