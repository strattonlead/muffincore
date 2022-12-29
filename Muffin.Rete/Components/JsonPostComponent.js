class JsonPostComponent extends Rete.Component {
    constructor() {
        super("JsonPost");
    }

    builder(node) {
        return node.addControl(new TextControl(this.editor, 'url'))
            .addInput(new Rete.Input('action', "Invoke", window.Muffin.Rete.Sockets.EventSocket))
            .addInput(new Rete.Input('url', "Url", window.Muffin.Rete.Sockets.TextSocket))
            .addInput(new Rete.Input('body', "Body", window.Muffin.Rete.Sockets.AnySocket))
            .addOutput(new Rete.Output('success', 'Success', window.Muffin.Rete.Sockets.AnySocket))
            .addOutput(new Rete.Output('error', 'Error', window.Muffin.Rete.Sockets.ErrorSocket));
    }

    worker(node, inputs, outputs) {
        var url = inputs.url != undefined ? inputs.url : node.data.url;

        const dataToSend = JSON.stringify(inputs.body);
        fetch(url, {
            method: "post",
            headers: { "Content-Type": "application/json" },
            body: dataToSend
        }).then(resp => {
                if (resp.status === 200) {
                    return resp.json()
                } else {
                    console.log("Status: " + resp.status)
                    return Promise.reject("server")
                }
            })
            .then(dataJson => {
                outputs['success'] = JSON.parse(dataJson)
            })
            .catch(err => {
                outputs['error'] = err;
            });
    }
}

window.Muffin.Rete.Components.JsonPost = new JsonPostComponent();