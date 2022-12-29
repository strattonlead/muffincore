if (window.Muffin.Rete.Sockets.AnySocket == null) {
    window.Muffin.Rete.Sockets.AnySocket = new Rete.Socket('Any type');
}

if (window.Muffin.Rete.Sockets.TextSocket == null) {
    window.Muffin.Rete.Sockets.TextSocket = new Rete.Socket('Text');
    window.Muffin.Rete.Sockets.TextSocket.combineWith(window.Muffin.Rete.Sockets.AnySocket);
}
if (window.Muffin.Rete.Sockets.NumberSocket == null) {
    window.Muffin.Rete.Sockets.NumberSocket = new Rete.Socket('Number');
    window.Muffin.Rete.Sockets.NumberSocket.combineWith(window.Muffin.Rete.Sockets.AnySocket);
}
if (window.Muffin.Rete.Sockets.BoolSocket == null) {
    window.Muffin.Rete.Sockets.BoolSocket = new Rete.Socket('Bool');
    window.Muffin.Rete.Sockets.BoolSocket.combineWith(window.Muffin.Rete.Sockets.AnySocket);
}
if (window.Muffin.Rete.Sockets.EventSocket == null) {
    window.Muffin.Rete.Sockets.EventSocket = new Rete.Socket('Event');
    window.Muffin.Rete.Sockets.EventSocket.combineWith(window.Muffin.Rete.Sockets.AnySocket);
}
if (window.Muffin.Rete.Sockets.ErrorSocket == null) {
    window.Muffin.Rete.Sockets.ErrorSocket = new Rete.Socket('Error');
    window.Muffin.Rete.Sockets.ErrorSocket.combineWith(window.Muffin.Rete.Sockets.AnySocket);
}