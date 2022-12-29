class NumberComponent extends Rete.Component {

    constructor() {
        super("Number");
    }

    builder(node) {
        return node.addControl(new NumberControl(this.editor, 'num')).addOutput(new Rete.Output('num', 'Number', window.Muffin.Rete.Sockets.NumberSocket));
    }

    worker(node, inputs, outputs) {
        console.log("Number " + JSON.stringify(node.data.num));
        outputs['num'] = node.data.num;
    }
}

window.Muffin.Rete.Components.Number = new NumberComponent();