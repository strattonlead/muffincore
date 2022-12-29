class ScriptComponent extends Rete.Component {
    constructor() {
        super("Script");
    }

    builder(node) {
        return node.addControl(new TextAreaControl(this.editor, 'script'))
            .addInput(new Rete.Input('run', "Run", window.Muffin.Rete.Sockets.AnySocket))
            .addOutput(new Rete.Output('success', 'Success', window.Muffin.Rete.Sockets.AnySocket))
            .addOutput(new Rete.Output('error', 'Error', window.Muffin.Rete.Sockets.ErrorSocket));
    }

    worker(node, inputs, outputs) {
        var script = node.data.script;
        console.log("Script " + script);
        try {
            var result = {};
            if (script != null) {
                result = eval(script);
            }
            console.log("Script success");
            outputs['success'] = result;
        } catch (e) {
            console.log("Script error");
            outputs['error'] = e;
        }
        
        var input = inputs['input'] != undefined ? inputs['input'] : node.data.input;
        console.log("Log" + input);
    }
}

window.Muffin.Rete.Components.Script = new ScriptComponent();