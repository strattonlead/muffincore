class BoolComponent extends Rete.Component {

    constructor() {
        super("Bool");
    }

    builder(node) {
        return node.addControl(new BoolControl(this.editor, 'bool')).addOutput(new Rete.Output('bool', 'Bool', window.Muffin.Rete.Sockets.BoolSocket));
    }

    worker(node, inputs, outputs) {
        console.log("Bool " + JSON.stringify(node.data.bool));
        outputs['bool'] = node.data.num;
    }
}

window.Muffin.Rete.Components.Bool = new BoolComponent();