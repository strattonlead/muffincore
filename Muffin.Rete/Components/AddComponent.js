class AddComponent extends Rete.Component {
    constructor() {
        super("Add");
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
        var n1 = inputs.left != undefined ? inputs.left : node.data.left;
        var n2 = inputs.right != undefined ? inputs.right : node.data.right;

        if (isNaN(n1)) {
            n1 = n1.Value;
        } else if (n1 instanceof Array && n1.length && n1.length > 0) {
            n1 = n1[0];
        }

        if (isNaN(n2)) {
            n2 = n2.Value;
        } else if (n2 instanceof Array && n2.length && n2.length > 0) {
            n2 = n2[0];
        }

        var sum = n1 + n2;
        console.log("Add " + n1 + " + " + n2 + " = " + sum);
        this.editor?.nodes?.find(n => n.id == node.id)?.controls?.get('preview')?.setValue(sum);
        outputs['result'] = sum;
    }
}

window.Muffin.Rete.Components.Add = new AddComponent();