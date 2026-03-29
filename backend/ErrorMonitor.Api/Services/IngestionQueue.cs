using System.Threading.Channels;
using ErrorMonitor.Api.Contracts;

namespace ErrorMonitor.Api.Services;

public record IngestionEnvelope(ErrorIngestRequest Payload);

public interface IIngestionQueue
{
    ValueTask QueueAsync(IngestionEnvelope envelope, CancellationToken ct);
    ValueTask<IngestionEnvelope> DequeueAsync(CancellationToken ct);
}

public class IngestionQueue : IIngestionQueue
{
    private readonly Channel<IngestionEnvelope> _channel = Channel.CreateBounded<IngestionEnvelope>(new BoundedChannelOptions(2000)
    {
        FullMode = BoundedChannelFullMode.DropOldest
    });

    public ValueTask QueueAsync(IngestionEnvelope envelope, CancellationToken ct) => _channel.Writer.WriteAsync(envelope, ct);

    public ValueTask<IngestionEnvelope> DequeueAsync(CancellationToken ct) => _channel.Reader.ReadAsync(ct);
}
