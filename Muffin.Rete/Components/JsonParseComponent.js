class JsonParseComponent extends Rete.Component {
    constructor() {
        super("JsonParse");
    }

    builder(node) {
        return node
            .addInput(new Rete.Input('text', "Text", window.Muffin.Rete.Sockets.TextSocket))
            .addOutput(new Rete.Output('result', 'Object', window.Muffin.Rete.Sockets.AnySocket));
    }

    worker(node, inputs, outputs) {
        var text = inputs.text;
        outputs['result'] = JSON.parse(text);
    }
}

window.Muffin.Rete.Components.JsonParse = new JsonParseComponent();