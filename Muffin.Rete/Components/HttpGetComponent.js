class HttpGetComponent extends Rete.Component {
    constructor() {
        super("HttpGet");
    }

    builder(node) {
        return node.addControl(new TextControl(this.editor, 'url'))
            .addInput(new Rete.Input('action', "Invoke", window.Muffin.Rete.Sockets.EventSocket))
            .addOutput(new Rete.Output('success', 'Success', window.Muffin.Rete.Sockets.AnySocket))
            .addOutput(new Rete.Output('error', 'Error', window.Muffin.Rete.Sockets.ErrorSocket));
    }

    worker(node, inputs, outputs) {
        var url = /*inputs.url != undefined ? inputs.url :*/ node.data.url;

        fetch(url, {
            method: "get"
        }).then(resp => {
            if (resp.status === 200) {
                outputs['success'] = resp.text()
            } else {
                console.log("Status: " + resp.status)
                return Promise.reject("server")
            }
        }).catch(err => {
                outputs['error'] = err;
            });
    }
}

window.Muffin.Rete.Components.HttpGet = new HttpGetComponent();