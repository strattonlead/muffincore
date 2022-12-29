class ReplaceComponent extends Rete.Component {
    constructor() {
        super("Replace");
    }

    builder(node) {
        var inp1 = new Rete.Input('text', "Text", window.Muffin.Rete.Sockets.TextSocket);
        var inp2 = new Rete.Input('search', "Search", window.Muffin.Rete.Sockets.TextSocket);
        var inp3 = new Rete.Input('replace', "Replace", window.Muffin.Rete.Sockets.TextSocket);
        var out = new Rete.Output('result', "Result", window.Muffin.Rete.Sockets.TextSocket);

        inp1.addControl(new NumberControl(this.editor, 'text'))
        inp2.addControl(new NumberControl(this.editor, 'search'))
        inp3.addControl(new NumberControl(this.editor, 'replace'))

        return node
            .addInput(inp1)
            .addInput(inp2)
            .addInput(inp3)
            .addControl(new TextControl(this.editor, 'preview', true))
            .addOutput(out);
    }

    worker(node, inputs, outputs) {
        var text = inputs.text != undefined ? inputs.text : node.data.text;
        var search = inputs.search != undefined ? inputs.search : node.data.search;
        var replace = inputs.replace != undefined ? inputs.replace : node.data.replace;

        try {
            result = text.replace(search, replace);
            this.editor?.nodes?.find(n => n.id == node.id)?.controls?.get('preview')?.setValue(result);
            outputs['result'] = result;
        } catch (e){ }
    }
}

window.Muffin.Rete.Components.Replace = new ReplaceComponent();