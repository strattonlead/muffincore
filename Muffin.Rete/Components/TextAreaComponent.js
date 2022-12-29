class TextAreaComponent extends Rete.Component {

    constructor() {
        super("TextArea");
    }

    builder(node) {
        return node.addControl(new TextAreaControl(this.editor, 'text')).addOutput(new Rete.Output('text', 'Text', window.Muffin.Rete.Sockets.TextSocket));
    }

    worker(node, inputs, outputs) {
        outputs['text'] = node.data.text;
    }
}

window.Muffin.Rete.Components.TextArea = new TextAreaComponent();