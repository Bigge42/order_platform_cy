export const uploadtxdr = () => {
	plus.nativeUI.pickFile((path) => {
		if (path) {
			// 当成功选择文件后，获取到文件路径，调用上传文件的方法
			this.uploadFile(path);
		} else {
			console.log("用户未选择文件");
		}
	}, {
		// 可以设置一些过滤条件，比如允许选择的文件类型等
		filter: ["*"]
	});
}
