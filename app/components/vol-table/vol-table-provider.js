
   

export const initDataSource = (columns, http, dicInitedCallback) => {
	// let dicKeys = columns.filter(x => {
	// 	if (x.bind&&x.bind.key) {
	// 		if (!Array.isArray(x.bind.data)) {
	// 			x.bind.data=[]
	// 		}
	// 		return true;
	// 	}
	// 	return false;
	// }).map(x => {
	// 	return x.bind.key
	// });

	const dicKeys = []
	columns.forEach(x => {
		if (Array.isArray(x)) {
			dicKeys.push(...x.filter(c => {
				return c.bind && c.bind.key
			}).map(m => {
				return m.bind.key;
			}));
		} else if (x.bind && x.bind.key) {
			dicKeys.push(x.bind.key)
		}
	})

	if (!dicKeys.length) {
		return;
	}
	http.post('api/Sys_Dictionary/GetVueDictionary', dicKeys, false).then(result => {
		dicInitedCallback(result);
		result.forEach(res => {
			columns.forEach(option => {
				if (option.bind && option.bind.data) {
					if ((option.bind.key) == res.dicNo) {
						option.bind.data = res.data;
					}
				} else if (Array.isArray(option)) {
					option.forEach(op => {
						if ((op.bind && op.bind.data && op.bind.key) == res.dicNo) {
							op.bind.data = res.data;
						}
					})
				}
			})
		})
	})
}

export const uploadBeofore = async (row, http, store, option, event) => {
	if (!option.url) {
		uni.showToast({
			icon: "none",
			title: '未配置好columns字段的url',
			duration: 2000
		})
		return false;
	}

	// 当设置 mutiple 为 true 时, file 为数组格式，否则为对象格式
	let lists = [];
	if (option.mutiple) {
		lists = [].concat(event.file)
	} else {
		if (Array.isArray(event.file)) {
			lists.push(...event.file)
		} else {
			lists.push(event.file)
		}
	}
	let fileListLen = row[option.field].length
	lists.map((item) => {
		row[option.field].push({
			...item,
			status: 'uploading',
			message: '上传中'
		})
	})
	for (let i = 0; i < lists.length; i++) {

		let result = await uploadFilePromise(http, lists[i].url, option.url, store);
		if (!result.status) {
			//lists.splice(i,1)
			row[option.field].splice(fileListLen, 1)
			uni.showToast({
				icon: "none",
				title: result.message,
				duration: 2000
			})
			return false;
		}
		result = result.data;
		let item = row[option.field][fileListLen];
		let fileName = lists[i].name;
		// console.log('fileName:'+fileName)
		// console.log(lists[i])
		if (!fileName && lists[i].thumb) {
			try {
				let lastIndex = lists[i].thumb.lastIndexOf('/') + 1;
				fileName = lists[i].thumb.substr(lastIndex)
			} catch (e) {
				console.log(e)
				//TODO handle the exception
			}
		}
		console.log('url:', result, fileName)
		console.log('url:', http)
		const url = http.ipAddress + result + fileName;

		console.log('url:' + url)
		row[option.field].splice(fileListLen, 1, Object.assign(item, {
			status: 'success',
			message: '',
			url: url,
			orginUrl: result + fileName
		}))
		fileListLen++
	}
}
const uploadFilePromise = (http, url, apiUrl, store) => {
	return new Promise((resolve, reject) => {
		let a = uni.uploadFile({
			url: http.ipAddress + apiUrl, // 仅为示例，非真实的接口地址
			filePath: url,
			name: 'fileInput',
			header: {
				"uapp": 1,
				"Authorization": store.getters.getToken(),
				"serviceId": uni.getStorageSync('serviceId')
			},
			formData: {},
			success: (res) => {
				setTimeout(() => {
					//console.log(res)
					//resolve(JSON.parse(res.data).data)
					resolve(JSON.parse(res.data))
				}, 200)
			},
			fail(res) {
				uni.showToast({
					icon: "none",
					title: '上传失败',
					duration: 4000
				})
			}
		});
	})
}