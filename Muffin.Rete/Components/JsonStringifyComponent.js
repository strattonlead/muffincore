class JsonStringifyComponent extends Rete.Component {
    constructor() {
        super("JsonStringify");
    }

    builder(node) {
        return node
            .addInput(new Rete.Input('obj', "Object", window.Muffin.Rete.Sockets.AnySocket))
            .addOutput(new Rete.Output('result', 'Text', window.Muffin.Rete.Sockets.TextSocket));
    }

    worker(node, inputs, outputs) {
        outputs['result'] = JSON.stringify(inputs.obj);
    }
}

window.Muffin.Rete.Components.JsonStringify = new JsonStringifyComponent();