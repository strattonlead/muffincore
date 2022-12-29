var VueTextAreaControl = {
    props: ['readonly', 'emitter', 'ikey', 'getData', 'putData'],
    template: '<textarea rows="4" :readonly="readonly" :value="value" @input="change($event)" @dblclick.stop="" @pointerdown.stop="" @pointermove.stop=""/>',
    data() {
        return {
            value: 0,
        }
    },
    methods: {
        change(e) {
            this.value = e.target.value;
            this.update();
        },
        update() {
            if (this.ikey)
                this.putData(this.ikey, this.value)
            this.emitter.trigger('process');
        }
    },
    mounted() {
        this.value = this.getData(this.ikey);
    }
}

class TextAreaControl extends Rete.Control {

    constructor(emitter, key, readonly) {
        super(key);
        this.component = VueTextAreaControl;
        this.props = { emitter, ikey: key, readonly };
    }

    setValue(val) {
        this.vueContext.value = val;
    }
}