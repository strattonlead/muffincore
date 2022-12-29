var VueBoolControl = {
    props: ['readonly', 'emitter', 'ikey', 'getData', 'putData'],
    template: '<input type="checkbox" :readonly="readonly" :value="value" @input="change($event)" @dblclick.stop="" @pointerdown.stop="" @pointermove.stop=""/>',
    data() {
        return {
            value: 0,
        }
    },
    methods: {
        change(e) {
            this.value = e.target.checked;
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

class BoolControl extends Rete.Control {

    constructor(emitter, key, readonly) {
        super(key);
        this.component = VueBoolControl;
        this.props = { emitter, ikey: key, readonly };
    }

    setValue(val) {
        parseBool = (value, defaultValue) => ['true', 'false', true, false].includes(value) && JSON.parse(value) || defaultValue

        this.vueContext.value = parseBool(val, false);
    }

}