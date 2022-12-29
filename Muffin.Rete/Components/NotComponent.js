class NotComponent extends Rete.Component {
    constructor() {
        super("Not");
    }

    builder(node) {
        var inp = new Rete.Input('bool', "Input", window.Muffin.Rete.Sockets.BoolSocket);
        var out = new Rete.Output('result', "Result", window.Muffin.Rete.Sockets.BoolSocket);

        inp.addControl(new NumberControl(this.editor, 'bool'))

        return node
            .addInput(inp)
            .addOutput(out);
    }

    worker(node, inputs, outputs) {
        var b = inputs.bool != undefined ? inputs.bool : true;
        outputs['result'] = !b;
    }
}

window.Muffin.Rete.Components.Not = new NotComponent();