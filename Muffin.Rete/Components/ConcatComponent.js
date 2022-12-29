class ConcatComponent extends Rete.Component {
    constructor() {
        super("Concat");
    }

    builder(node) {
        var inp1 = new Rete.Input('left', "Left", window.Muffin.Rete.Sockets.TextSocket);
        var inp2 = new Rete.Input('right', "Right", window.Muffin.Rete.Sockets.TextSocket);
        var out = new Rete.Output('result', "Result", window.Muffin.Rete.Sockets.TextSocket);

        inp1.addControl(new NumberControl(this.editor, 'left'))
        inp2.addControl(new NumberControl(this.editor, 'right'))

        return node
            .addInput(inp1)
            .addInput(inp2)
            .addControl(new TextControl(this.editor, 'preview', true))
            .addOutput(out);
    }

    worker(node, inputs, outputs) {
        var n1 = inputs.left != undefined ? inputs.left : node.data.left;
        var n2 = inputs.right != undefined ? inputs.right : node.data.right;

        var result = "" +  n1 + n2;
        console.log("Concat " + n1 + " + " + n2 + " = " + result);
        this.editor?.nodes?.find(n => n.id == node.id)?.controls?.get('preview')?.setValue(result);
        outputs['result'] = result;
    }
}

window.Muffin.Rete.Components.Concat = new ConcatComponent();