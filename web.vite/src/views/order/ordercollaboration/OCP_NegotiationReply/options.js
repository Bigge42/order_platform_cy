// *Author：jxx
// *Contact：461857658@qq.com
// *代码由框架生成,任何更改都可能导致被代码生成器覆盖
export default function(){
    const table = {
        key: 'ReplyID',
        footer: "Foots",
        cnName: '协商回复表',
        name: 'OCP_NegotiationReply',
        newTabEdit: false,
        url: "/OCP_NegotiationReply/",
        sortName: "CreateDate"
    };
    const tableName = table.name;
    const tableCNName = table.cnName;
    const newTabEdit = false;
    const key = table.key;
    const editFormFields = {"NegotiationID":"","ReplyContent":"","ReplyPersonName":"","ReplyPersonPhone":"","ReplyTime":"","ReplyProgress":"","ReplyDeliveryDate":"","Remarks":""};
    const editFormOptions = [[{"title":"协商ID","required":true,"field":"NegotiationID"}],
                              [{"title":"回复内容","field":"ReplyContent"}],
                              [{"title":"回复人名称","field":"ReplyPersonName"}],
                              [{"title":"回复人电话","field":"ReplyPersonPhone"}],
                              [{"title":"回复时间","field":"ReplyTime","type":"datetime"}],
                              [{"title":"回复进度","field":"ReplyProgress"}],
                              [{"title":"回复交期","field":"ReplyDeliveryDate","type":"datetime"}],
                              [{"title":"备注","field":"Remarks"}]];
    const searchFormFields = {"ReplyPersonPhone":"","ReplyTime":"","ReplyProgress":"","ReplyDeliveryDate":"","Remarks":""};
    const searchFormOptions = [[{"title":"回复ID","field":"ReplyID","type":"like"},{"title":"协商ID","field":"NegotiationID","type":"like"},{"title":"回复内容","field":"ReplyContent","type":"like"},{"title":"回复人名称","field":"ReplyPersonName","type":"like"}],[{"title":"回复人电话","field":"ReplyPersonPhone","type":"like"},{"title":"回复时间","field":"ReplyTime","type":"datetime"},{"title":"回复进度","field":"ReplyProgress","type":"like"},{"title":"回复交期","field":"ReplyDeliveryDate","type":"datetime"}],[{"title":"备注","field":"Remarks","type":"like"}]];
    const columns = [{field:'ReplyID',title:'回复ID',type:'long',sort:true,width:80,hidden:true,readonly:true,require:true,align:'left'},
                       {field:'NegotiationID',title:'协商ID',type:'long',sort:true,width:80,require:true,align:'left'},
                       {field:'ReplyContent',title:'回复内容',type:'string',sort:true,width:220,align:'left'},
                       {field:'ReplyPersonName',title:'回复人名称',type:'string',sort:true,width:120,align:'left'},
                       {field:'ReplyPersonPhone',title:'回复人电话',type:'string',sort:true,width:120,align:'left'},
                       {field:'ReplyTime',title:'回复时间',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'ReplyProgress',title:'回复进度',type:'string',sort:true,width:110,align:'left'},
                       {field:'ReplyDeliveryDate',title:'回复交期',type:'datetime',sort:true,width:150,align:'left'},
                       {field:'Remarks',title:'备注',type:'string',sort:true,width:220,align:'left'},
                       {field:'CreateID',title:'创建人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'CreateDate',title:'创建日期',type:'datetime',width:150,align:'left'},
                       {field:'Creator',title:'创建人',type:'string',width:100,align:'left'},
                       {field:'Modifier',title:'修改人',type:'string',width:100,align:'left'},
                       {field:'ModifyDate',title:'修改日期',type:'datetime',width:150,align:'left'},
                       {field:'ModifyID',title:'修改人ID',type:'int',width:80,hidden:true,align:'left'},
                       {field:'NegotiationStatus',title:'协商状态',type:'string',width:120,align:'left'}];
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