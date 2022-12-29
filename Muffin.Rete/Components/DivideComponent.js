class DivideComponent extends Rete.Component {
    constructor() {
        super("Divide");
    }

    builder(node) {
        var inp1 = new Rete.Input('dividend', "Dividend", window.Muffin.Rete.Sockets.NumberSocket);
        var inp2 = new Rete.Input('divisor', "Divisor", window.Muffin.Rete.Sockets.NumberSocket);
        var out = new Rete.Output('result', "Result", window.Muffin.Rete.Sockets.NumberSocket);

        inp1.addControl(new NumberControl(this.editor, 'dividend'))
        inp2.addControl(new NumberControl(this.editor, 'divisor'))

        return node
            .addInput(inp1)
            .addInput(inp2)
            .addControl(new NumberControl(this.editor, 'preview', true))
            .addOutput(out);
    }

    worker(node, inputs, outputs) {
        var dividend = inputs.left != undefined ? inputs.dividend : node.data.dividend;
        var divisor = inputs.divisor != undefined ? inputs.divisor : node.data.divisor;

        if (isNaN(n1)) {
            dividend = dividend.Value;
        } else if (dividend instanceof Array && dividend.length && dividend.length > 0) {
            dividend = dividend[0];
        }

        if (isNaN(divisor)) {
            divisor = divisor.Value;
        } else if (divisor instanceof Array && divisor.length && divisor.length > 0) {
            divisor = divisor[0];
        }

        if (divisor == 0) {
            outputs[result] = NaN;
            return;
        }

        var result = dividend / divisor;
        console.log("Divide " + dividend + " / " + divisor + " = " + result);
        this.editor?.nodes?.find(n => n.id == node.id)?.controls?.get('preview')?.setValue(result);
        outputs['result'] = result;
    }
}

window.Muffin.Rete.Components.Divide = new DivideComponent();