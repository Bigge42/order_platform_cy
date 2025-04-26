<template>
  <div>
    <vol-box
      v-model="model"
      :width="width"
      :padding="0"
      :lazy="true"
      draggable
      :destroyOnClose="true"
    >
      <div style="padding-bottom: 10px">
        <vol-form
          @dicInited="dicInited"
          ref="form"
          :load-key="true"
          :label-width="100"
          :formRules="formOptions"
          :formFields="formFields"
          :select2Count="10000"
        ></vol-form>
      </div>
      <template #footer>
        <div class="btns">
          <el-button @click="model = false">{{ $ts("关闭") }}</el-button>
          <el-button type="primary" @click="save">{{ $ts("保存") }}</el-button>
        </div>
      </template>
    </vol-box>
  </div>
</template>
<script>
import VolForm from "@/components/basic/VolForm.vue";
import VolBox from "@/components/basic/VolBox.vue";
import { defineComponent, ref, reactive, getCurrentInstance } from "vue";
export default defineComponent({
  components: {
    VolBox,
    "vol-form": VolForm,
  },
  props: {
    getTable: {
      type: Function,
      default: () => {
        return "";
      },
    },
  },
  setup(props, { emit }) {
    const width = ref(600);
    const model = ref(false);
    const formOptions = reactive([]);
    const formFields = ref({});

    const show = (options, key) => {
      //console.log(props.getTable())
      //formOptions.value = [];
      formOptions.length = 0;
      formFields.value = {};
      Object.keys(options).forEach((key) => {
        if (!options[key].isEdit) {
          return;
        }

        let item = options[key];
        formFields.value[item.field] =
          options[key].orgVal === undefined || +options[key].orgVal === null
            ? null
            : options[key].orgVal + "";
        let obj = {
          field: item.field,
          title: item.name,
          type: item.editType,
          required: item.require,
          data: [],
        };
        switch (item.editType) {
          case "img":
          case "file":
          case "excel":
            obj.url = `api/${props.getTable()}/upload`;
            obj.multiple = true;
            obj.maxFile = 6;
            break;
          case "select":
          case "radio":
          case "cascader":
            obj.dataKey = item.dropNo;
            break;
          case "selectList":
          case "checkbox":
          case "treeSelect":
            obj.dataKey = item.dropNo;
            formFields.value[item.field] = (formFields.value[item.field] || "").split(
              ","
            );
            break;
          default:
            break;
        }
        formOptions.push([obj]);
      });
      model.value = true;
    };
    const dicInited = (result) => {
      formOptions.forEach((ops) => {
        ops.forEach((item) => {
          if (item.dataKey) {
            let obj = result.find((x) => {
              return x.dicNo == item.dataKey;
            });
            if (obj) {
              item.data = obj.data;
            }
          }
        });
      });
    };
    const form = ref(null);
    const save = () => {
      form.value.validate((valid) => {
        let _form = {};
        Object.keys(formFields.value).forEach((x) => {
          _form[x] = formFields.value[x];
        });
        emit("saveEditForm", _form, () => {
          model.value = false;
        });
      });
    };
    return {
      model,
      show,
      width,
      form,
      formOptions,
      formFields,
      save,
      dicInited,
    };
  },
});
</script>
<style scope>
.btns {
  text-align: center;
}
</style>
