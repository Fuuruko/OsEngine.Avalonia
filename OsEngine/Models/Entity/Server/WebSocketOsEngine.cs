/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsEngine.Models.Entity.Server;

public class WebSocket(string url)
{
    private string _url = url;

    private ClientWebSocket Client { get; } = new ClientWebSocket();

    private CancellationTokenSource _cts;

    private object _ctsLocker = new();

    private Task _receiveTask;

    public WebSocketState ReadyState = WebSocketState.Closed;

   // public SslConfiguration

    public bool EmitOnPing = false;

    public void SetProxy(IWebProxy proxy)
    {
        if (proxy != null)
        {
            Client.Options.Proxy = proxy;
        }
    }

    public void SetCertificate(X509Certificate2 certificate)
    {
        Client.Options.ClientCertificates = [certificate];
        Client.Options.RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true;
    }

    public async Task Connect()
    {
        try
        {
            if (string.IsNullOrEmpty(_url))
            {
                throw new InvalidOperationException("URL must be set before connecting.");
            }

            CancellationToken token;

            lock (_ctsLocker)
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                }

                _cts = new CancellationTokenSource();
                token = _cts.Token;
            }

            await Client.ConnectAsync(new Uri(_url), token);

            ReadyState = WebSocketState.Open;

            OnOpen?.Invoke(this, EventArgs.Empty);

            _receiveTask = Task.Run(() => ReceiveLoopAsync(token));

        }
        catch (Exception ex)
        {
            ErrorEventArgs eventArgs = new() { Exception = ex };

            OnError?.Invoke(this, eventArgs);
        }
    }

    public async Task CloseAsync()
    {
        try
        {
            lock (_ctsLocker)
            {
                _cts?.Cancel();
            }

            if (_receiveTask != null)
            {
                try
                {
                    // Give receive loop some time to exit gracefully
                    await Task.WhenAny(_receiveTask, Task.Delay(TimeSpan.FromSeconds(2)));
                }
                catch { /* ignored */ }
            }

            if (Client.State == System.Net.WebSockets.WebSocketState.Open || Client.State == System.Net.WebSockets.WebSocketState.CloseSent)
            {
                try
                {
                    // Timeout for closing operation
                    var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await Client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client initiated close", closeCts.Token);
                }
                catch (Exception) { /* Ignore close errors, client might be disposed already or connection lost */ }
            }

            lock (_ctsLocker)
            {
                _cts?.Dispose();
                _cts = null;
            }
        }
        catch
        {
            // ignore
        }

        ReadyState = WebSocketState.Closed;
    }

    public async Task Send(string message)
    {
        if (Client.State == System.Net.WebSockets.WebSocketState.Open && _cts != null && !_cts.IsCancellationRequested)
        {
            try
            {
                var messageBuffer = Encoding.UTF8.GetBytes(message);
                await Client.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, _cts.Token);
            }
            catch (Exception ex)
            {
                ErrorEventArgs eventArgs = new() { Exception = ex };
                OnError?.Invoke(this, eventArgs);
            }
        }
    }

    public bool Ping()
    {
        try
        {
            if (Client.State == System.Net.WebSockets.WebSocketState.Open 
                && _cts != null 
                && !_cts.IsCancellationRequested)
            {
                try
                {
                    string message = "ping";

                    var messageBuffer = Encoding.UTF8.GetBytes(message);
                    Client.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text,true, _cts.Token);

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            return false;
        }

        return false;
    }

    public void Dispose()
    {
        try
        {
            CloseAsync().GetAwaiter().GetResult();
        }
        catch { /* ignored */ }

        try
        {
            Client.Dispose();
        }
        catch { /* ignored */ }
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        try
        {
            var buffer = new byte[8192 * 2]; // 16KB buffer, adjust as needed

            while (Client.State == System.Net.WebSockets.WebSocketState.Open && !token.IsCancellationRequested)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    var segment = new ArraySegment<byte>(buffer);
                    result = await Client.ReceiveAsync(segment, token);

                    if (token.IsCancellationRequested) break;

                    ms.Write(segment.Array, segment.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                if (token.IsCancellationRequested) break;

                ms.Seek(0, SeekOrigin.Begin);
                byte[] receivedData = ms.ToArray();

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(receivedData);

                    if (EmitOnPing == true
                        &&
                        (message.Contains("ping")
                         || message.Contains("Ping")))
                    {
                        await Send("pong");
                    }
                    else
                    {
                        MessageEventArgs messageValue = new()
                        {
                            Data = message,
                            IsText = true
                        };
                        OnMessage?.Invoke(this, messageValue);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    MessageEventArgs messageValue = new()
                    {
                        RawData = receivedData,
                        IsBinary = true
                    };
                    OnMessage?.Invoke(this, messageValue);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    ReadyState = WebSocketState.Closed;
                    // If server initiates close, acknowledge it.
                    if (Client.State == System.Net.WebSockets.WebSocketState.CloseReceived)
                    {
                        await Client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client acknowledging close", CancellationToken.None);
                    }

                    CloseEventArgs closeEventArgs = new() { Code = result.CloseStatusDescription };

                    if (result.CloseStatus != null)
                    {
                        closeEventArgs.Reason = result.CloseStatus.ToString();
                    }

                    OnClose?.Invoke(this, closeEventArgs);
                    return;
                }
            }
        }
        catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely 
                                            || Client.State != System.Net.WebSockets.WebSocketState.Open)
        {
            ReadyState = WebSocketState.Closed;
            ErrorEventArgs eventArgs = new() { Exception = ex };
            OnError?.Invoke(this, eventArgs);
        }
        catch (OperationCanceledException)
        {
            ReadyState = WebSocketState.Closed;
            // Expected when token is cancelled
        }
        catch (Exception ex)
        {
            ReadyState = WebSocketState.Closed;
            ErrorEventArgs eventArgs = new() { Exception = ex };
            OnError?.Invoke(this, eventArgs);
        }
        finally
        {
            ReadyState = WebSocketState.Closed;
            // Ensure onClose is called if the loop exits for any reason other than explicit dispose
            if (!token.IsCancellationRequested 
                || Client.State != System.Net.WebSockets.WebSocketState.Aborted)
            {
                CloseEventArgs closeEventArgs = new() { Code = "Finally socket closed" };
                OnClose?.Invoke(this, closeEventArgs);
            }
        }
    }

    public event Action<object, EventArgs> OnOpen;

    public event Action<object, CloseEventArgs> OnClose;

    public event Action<object, MessageEventArgs> OnMessage;

    public event Action<object, ErrorEventArgs> OnError;
}

public class CloseEventArgs
{
    public string Code;

    public string Reason;
}

public class MessageEventArgs
{
    public bool IsBinary = false;

    public bool IsText = false;

    public byte[] RawData;

    public string Data;

}

public class ErrorEventArgs
{
    public Exception Exception;
}

public enum WebSocketState
{
    Connecting,
    Open,
    Closing,
    Closed
}
