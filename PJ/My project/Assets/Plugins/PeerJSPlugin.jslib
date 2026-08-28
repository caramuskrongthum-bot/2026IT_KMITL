mergeInto(LibraryManager.library, {
    InitPeerServer: function (gameIdPtr, controllerUrlPtr) {
        var gameId = UTF8ToString(gameIdPtr);
        console.log("[Unity PeerJS] Initializing Host with ID: " + gameId);

        // โหลด PeerJS Library
        var script = document.createElement('script');
        script.src = "https://unpkg.com/peerjs@1.5.2/dist/peerjs.min.js";
        document.head.appendChild(script);

        script.onload = function() {
            // สร้าง Peer Host
            var peer = new Peer(gameId, {
                debug: 1
            });

            peer.on('open', function(id) {
                console.log('[Unity PeerJS] Host READY! ID: ' + id);
            });

            peer.on('connection', function(conn) {
                console.log('[Unity PeerJS] Mobile Connected!');
                conn.on('data', function(data) {
                    SendMessage('PhoneSensorSender', 'OnReceiveMotionData', String(data));
                });
            });

            peer.on('error', function(err) {
                console.error('[Unity PeerJS] Error:', err);
            });
        };
    }
});