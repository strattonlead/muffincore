class LoggerComponent extends Rete.Component {
    constructor() {
        super("Logger");
    }

    builder(node) {
        var inp1 = new Rete.Input('input', "Input", window.Muffin.Rete.Sockets.AnySocket);
        return node.addInput(inp1);
    }

    worker(node, inputs, outputs) {
        var input = inputs['input'] != undefined ? inputs['input'] : node.data.input;
        console.log("Log " + input);
    }
}

window.Muffin.Rete.Components.Logger = new LoggerComponent();