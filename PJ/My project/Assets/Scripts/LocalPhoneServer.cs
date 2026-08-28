using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class LocalPhoneServer
{
    private HttpListener _listener;
    private Thread _listenerThread;
    private readonly ConcurrentQueue<string> _incomingMessages = new ConcurrentQueue<string>();
    private readonly List<WebSocket> _activeSockets = new List<WebSocket>(); // เพิ่มลิสต์เก็บ WebSocket ที่เชื่อมต่ออยู่
    private volatile bool _running;
    private readonly string _controllerHtml;

    public int Port { get; private set; }

    public LocalPhoneServer(string controllerHtmlContent, int preferredPort = 7777)
    {
        _controllerHtml = controllerHtmlContent;
        Port = preferredPort;
    }

    public void Start()
    {
        _running = true;
        _listener = new HttpListener();

        try
        {
            _listener.Prefixes.Add($"http://+:{Port}/");
            _listener.Start();
        }
        catch (HttpListenerException)
        {
            UnityEngine.Debug.LogWarning(
                "[LocalPhoneServer] ไม่สามารถ bind ทุก interface ได้ (ต้องรันเป็น Admin หรือจอง URL ACL) " +
                "ลอง fallback เป็น localhost เท่านั้น — มือถือเครื่องอื่นจะเชื่อมต่อไม่ได้จนกว่าจะแก้สิทธิ์");
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://localhost:{Port}/");
                _listener.Start();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("[LocalPhoneServer] Start server ไม่สำเร็จ: " + e.Message);
                _running = false;
                return;
            }
        }

        _listenerThread = new Thread(ListenLoop) { IsBackground = true };
        _listenerThread.Start();
    }

    private async void ListenLoop()
    {
        while (_running && _listener.IsListening)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await _listener.GetContextAsync();
            }
            catch
            {
                break;
            }

#pragma warning disable CS4014
            HandleContext(ctx);
#pragma warning restore CS4014
        }
    }

    private async Task HandleContext(HttpListenerContext ctx)
    {
        try
        {
            if (ctx.Request.IsWebSocketRequest)
            {
                HttpListenerWebSocketContext wsCtx = await ctx.AcceptWebSocketAsync(null);

                // เก็บ Socket ไว้ในรายการเพื่อใช้ส่งข้อมูลกลับ
                lock (_activeSockets)
                {
                    _activeSockets.Add(wsCtx.WebSocket);
                }

                await ReceiveLoop(wsCtx.WebSocket);

                // ลบออกเมื่อปิดการเชื่อมต่อ
                lock (_activeSockets)
                {
                    _activeSockets.Remove(wsCtx.WebSocket);
                }
                return;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(_controllerHtml);
            ctx.Response.ContentType = "text/html; charset=utf-8";
            ctx.Response.ContentLength64 = buffer.Length;
            await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            ctx.Response.OutputStream.Close();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("[LocalPhoneServer] HandleContext error: " + e.Message);
        }
    }

    private async Task ReceiveLoop(WebSocket socket)
    {
        var buffer = new byte[2048];
        while (socket.State == WebSocketState.Open)
        {
            try
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closed", CancellationToken.None);
                    break;
                }
                string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                _incomingMessages.Enqueue(msg);
            }
            catch
            {
                break;
            }
        }
    }

    // --- เพิ่มฟังก์ชันสำหรับส่งข้อมูลกลับไปยังโทรศัพท์มือถือ ---
    public async void SendToAll(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        List<WebSocket> socketsCopy;

        lock (_activeSockets)
        {
            socketsCopy = new List<WebSocket>(_activeSockets);
        }

        foreach (var socket in socketsCopy)
        {
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning("[LocalPhoneServer] Send error: " + e.Message);
                }
            }
        }
    }

    public bool TryDequeue(out string message) => _incomingMessages.TryDequeue(out message);

    public void Stop()
    {
        _running = false;
        try { _listener?.Stop(); _listener?.Close(); } catch { }
    }

    public static string GetLocalIPAddress()
    {
        try
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect("8.8.8.8", 65530);
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            }
        }
        catch
        {
            return "127.0.0.1";
        }
    }
}