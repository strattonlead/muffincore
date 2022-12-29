class EventComponent extends Rete.Component {

    constructor() {
        super("Event");
    }

    builder(node) {
        return node.addControl(new NumberControl(this.editor, 'event')).addOutput(new Rete.Output('event', 'Event', window.Muffin.Rete.Sockets.EventSocket));
    }

    worker(node, inputs, outputs) {
        console.log("Event " + JSON.stringify(node.data.event));
        outputs['event'] = {
            id: node.data.event
        };
    }
}

window.Muffin.Rete.Components.Event = new EventComponent();