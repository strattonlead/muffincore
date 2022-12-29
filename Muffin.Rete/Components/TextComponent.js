class TextComponent extends Rete.Component {

    constructor() {
        super("Text");
    }

    builder(node) {
        return node.addControl(new TextControl(this.editor, 'text')).addOutput(new Rete.Output('text', 'Text', window.Muffin.Rete.Sockets.TextSocket));
    }

    worker(node, inputs, outputs) {
        outputs['text'] = node.data.text;
    }
}

window.Muffin.Rete.Components.Text = new TextComponent();