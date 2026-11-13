using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BackendCarcass.Application.Tests.TestInfrastructure;

//ტესტის SMTP სერვერი 127.0.0.1-ზე: ერთ წერილს იღებს ისე, როგორც ნამდვილი სერვერი STARTTLS-ისა და ავტორიზაციის
//გარეშე, და DATA-ს პასუხში, ნამდვილი სერვერების მსგავსად, წერილის ნომერს აბრუნებს სერვერის რიგში
public sealed class FakeSmtpServer : IDisposable
{
    public const string QueuedResponse = "2.0.0 Ok: queued as FAKE42";

    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);

    private readonly TaskCompletionSource<string> _message =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public FakeSmtpServer()
    {
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _ = ServeAsync();
    }

    public int Port { get; }

    //მიღებული წერილი (DATA-ს შიგთავსი სათაურებით)
    public Task<string> Message => _message.Task;

    public void Dispose()
    {
        _listener.Dispose();
    }

    private async Task ServeAsync()
    {
        using TcpClient client = await _listener.AcceptTcpClientAsync();
        using NetworkStream stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.Latin1, leaveOpen: true);
        await using var writer = new StreamWriter(stream, Encoding.Latin1, leaveOpen: true)
        {
            NewLine = "\r\n", AutoFlush = true
        };

        await writer.WriteLineAsync("220 fake.test ESMTP");
        var data = new StringBuilder();
        bool inData = false;
        while (await reader.ReadLineAsync() is { } line)
        {
            if (inData)
            {
                if (line == ".")
                {
                    inData = false;
                    _message.TrySetResult(data.ToString());
                    await writer.WriteLineAsync("250 " + QueuedResponse);
                }
                else
                {
                    data.AppendLine(line);
                }

                continue;
            }

            string verb = (line.Length < 4 ? line : line[..4]).ToUpperInvariant();
            inData = verb == "DATA";
            string reply = verb switch
            {
                "EHLO" => "250 fake.test",
                "MAIL" or "RCPT" => "250 2.1.0 Ok",
                "DATA" => "354 End data with <CR><LF>.<CR><LF>",
                "QUIT" => "221 2.0.0 Bye",
                _ => "502 5.5.2 Command not recognized"
            };
            await writer.WriteLineAsync(reply);
            if (verb == "QUIT")
            {
                return;
            }
        }
    }
}
