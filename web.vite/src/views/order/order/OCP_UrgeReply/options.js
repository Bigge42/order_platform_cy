// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'UrgeRecordID',
        footer: "Foots",
        cnName: '催单-回复',
        name: 'OCP_UrgeReply',
        newTabEdit: false,
        url: "/OCP_UrgeReply/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"MaterialCode":"","UrgeID":"","UrgeSubject":"","BusinessType":"","DefaultOwner":"","AssignedOwner":"","PriorityLevel":"","ReplyDeadlineDays":"","UrgeContent":"","AutoGenerateFlag":"","DeliveryDate":"","Progress":"","Remarks":""};
    const editFormOptions = [[{"title":"物料编码","required":true,"field":"MaterialCode"},
                               {"title":"催单ID","required":true,"field":"UrgeID"},
                               {"title":"催单主题","required":true,"field":"UrgeSubject"}],
                              [{"title":"业务类型","required":true,"field":"BusinessType"},
                               {"title":"默认负责人","field":"DefaultOwner"},
                               {"title":"指定负责人","field":"AssignedOwner"}],
                              [{"title":"紧急等级","field":"PriorityLevel"},
                               {"title":"回复时限（天）","field":"ReplyDeadlineDays","type":"number"},
                               {"title":"催单正文","field":"UrgeContent"}],
                              [{"title":"自动生成标识","required":true,"field":"AutoGenerateFlag","type":"number"},
                               {"title":"交货日期","field":"DeliveryDate","type":"datetime"},
                               {"title":"进度","field":"Progress"}],
                              [{"title":"备注","field":"Remarks"}]];
    const searchFormFields = {"UrgeID":"","UrgeSubject":"","BusinessType":"","DefaultOwner":"","AssignedOwner":"","PriorityLevel":"","ReplyDeadlineDays":"","UrgeContent":"","AutoGenerateFlag":"","DeliveryDate":"","Progress":"","Remarks":""};
    const searchFormOptions = [[{"title":"催单记录ID","field":"UrgeRecordID","type":"like"},{"title":"订单计划ID","field":"PlanID","type":"like"},{"title":"物料分类","field":"MaterialCategory","type":"like"},{"title":"物料编码","field":"MaterialCode","type":"like"}],[{"title":"催单ID","field":"UrgeID","type":"like"},{"title":"催单主题","field":"UrgeSubject","type":"like"},{"title":"业务类型","field":"BusinessType","type":"like"},{"title":"默认负责人","field":"DefaultOwner","type":"like"}],[{"title":"指定负责人","field":"AssignedOwner","type":"like"},{"title":"紧急等级","field":"PriorityLevel","type":"like"},{"title":"回复时限（天）","field":"ReplyDeadlineDays","type":"like"},{"title":"催单正文","field":"UrgeContent","type":"like"}],[{"title":"自动生成标识","field":"AutoGenerateFlag","type":"like"},{"title":"交货日期","field":"DeliveryDate","type":"datetime"},{"title":"进度","field":"Progress","type":"like"},{"title":"备注","field":"Remarks","type":"like"}]];
    const columns = [{field:'UrgeRecordID',title:'催单记录ID',type:'int',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'PlanID',title:'订单计划ID',type:'int',sort:true,width:80,require:true,align:'left'},
                       {field:'MaterialCategory',title:'物料分类',type:'string',sort:true,width:120,align:'left'},
                       {field:'MaterialCode',title:'物料编码',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'UrgeID',title:'催单ID',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'UrgeSubject',title:'催单主题',type:'string',sort:true,width:220,require:true,align:'left'},
                       {field:'BusinessType',title:'业务类型',type:'string',sort:true,width:120,require:true,align:'left'},
                       {field:'DefaultOwner',title:'默认负责人',type:'string',sort:true,width:120,align:'left'},
                       {field:'AssignedOwner',title:'指定负责人',type:'string',sort:true,width:120,align:'left'},
                       {field:'PriorityLevel',title:'紧急等级',type:'string',sort:true,width:110,align:'left'},
                       {field:'ReplyDeadlineDays',title:'回复时限（天）',type:'int',sort:true,width:80,align:'left'},
                       {field:'UrgeContent',title:'催单正文',type:'string',sort:true,width:110,align:'left'},
                       {field:'AutoGenerateFlag',title:'自动生成标识',type:'int',sort:true,width:80,require:true,align:'left'},
                       {field:'DeliveryDate',title:'交货日期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'Progress',title:'进度',type:'string',sort:true,width:120,align:'left'},
                       {field:'Remarks',title:'备注',type:'string',sort:true,width:220,align:'left'},
                       {field:'CreateID',title:'创建人Id',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人Id',type:'int',width:80,hidden:true,align:'left'}];
    const detail ={columns:[]};
    const details = [];

    return {
        table,
        key,
        tableName,
        tableCNName,
        newTabEdit,
        editFormFields,
        editFormOptions,
        searchFormFields,
        searchFormOptions,
        columns,
        detail,
        details
    };
}