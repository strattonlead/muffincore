class SubComponent extends Rete.Component {
    constructor() {
        super("Sub");
    }

    builder(node) {
        var inp1 = new Rete.Input('left', "Left", window.Muffin.Rete.Sockets.NumberSocket);
        var inp2 = new Rete.Input('right', "Right", window.Muffin.Rete.Sockets.NumberSocket);
        var out = new Rete.Output('result', "Result", window.Muffin.Rete.Sockets.NumberSocket);

        inp1.addControl(new NumberControl(this.editor, 'left'))
        inp2.addControl(new NumberControl(this.editor, 'right'))

        return node
            .addInput(inp1)
            .addInput(inp2)
            .addControl(new NumberControl(this.editor, 'preview', true))
            .addOutput(out);
    }

    worker(node, inputs, outputs) {
        var n1 = inputs['left'].length ? inputs['left'][0] : node.data.num1;
        var n2 = inputs['right'].length ? inputs['right'][0] : node.data.num2;
        var sum = n1 - n2;

        this.editor?.nodes?.find(n => n.id == node.id)?.controls?.get('preview')?.setValue(sum);
        outputs['result'] = sum;
    }
}

window.Muffin.Rete.Components.Sub = new SubComponent();