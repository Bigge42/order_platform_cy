
function checkKey(item, reset) {
	if (item.key || item.dataKey) {
		if (reset) {
			return true
		}
		return item.data && item.data.length == 0;
	}
	return false;
}

export const initDataSource = (formRules, http, dicInitedCallback, reset) => {
	const dicKeys = []
	formRules.forEach(x => {
		if (Array.isArray(x)) {
			dicKeys.push(...x.filter(c => {
				return checkKey(c, reset)
			}).map(m => {
				return m.key || m.dataKey
			}));
		} else if (checkKey(x, reset)) {
			dicKeys.push(x.key || x.dataKey)
		}
	})
	if (!dicKeys.length) {
		return;
	}
	http.post('api/Sys_Dictionary/GetVueDictionary', dicKeys, false).then(result => {
		dicInitedCallback(result);
		result.forEach(res => {
			formRules.forEach(option => {
				if (Array.isArray(option)) {
					option.forEach(x => {
						if ((x.key || x.dataKey) == res.dicNo) {
							x.data = res.data;
						}
					})
				} else if ((option.key || option.dataKey) == res.dicNo) {
					option.data = res.data;
				}
			})
		})
	})
}


const compressImage = async () => {
	uni.compressImage({
		src: '/static/logo.jpg',
		quality: 80,
		success: res => {
			console.log(res.tempFilePath)
		}
	})
}

const getFileName = (file) => {
	let fileName = file.name;
	if (!fileName && file.thumb) {
		try {
			fileName = file.thumb.split('/').pop();
		} catch (e) {
			console.log(e)
		}
	}
	return fileName;
}

export const uploadBefore = async (inFormFields, http, store, option, event) => {
	if (!option.url) {
		uni.showToast({
			icon: "none",
			title: '未配置好url',
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
	let fileListLen = inFormFields[option.field].length
	lists.map((item) => {
		inFormFields[option.field].push({
			...item,
			status: 'uploading',
			message: '上传中'
		})
	})
	for (let i = 0; i < lists.length; i++) {
		// await compressImage(lists[i].url)
		let result = await uploadFilePromise(http, lists[i].url, option.url, store);
		if (!result.status) {
			//lists.splice(i,1)
			inFormFields[option.field].splice(fileListLen, 1)
			uni.showToast({
				icon: "none",
				title: result.message,
				duration: 2000
			})
			return false;
		}
		result = result.data;
		let item = inFormFields[option.field][fileListLen];
		let fileName = getFileName(lists[i]);
		// console.log('url:', result, fileName)
		// console.log('url:', http)
		const url = http.ipAddress + result + fileName;

		console.log('url:' + url)
		inFormFields[option.field].splice(fileListLen, 1, Object.assign(item, {
			status: 'success',
			message: '',
			url: url,
			orginUrl: result + fileName
		}))
		fileListLen++
	}
}
export const uploadFilePromise = (http, url, apiUrl, store, formData) => {
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
			formData: formData,
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

const uploadFile = async (res, formFields, formOption, proxy, isApp) => {
	const files = res.tempFilePaths || res;
	uni.showToast({
		icon: "none",
		title: proxy.$ts('正在上传文件')
	})
	for (let i = 0; i < files.length; i++) {

		const tempFilePath = isApp ? '' : res.tempFiles[i].path;
		const fileName = isApp ? files[i].split('/').pop() : res.tempFiles[i].name;
		//const fileSize = res.tempFiles[i].size;
		// // 可以使用tempFilePath、fileName和fileSize进行进一步处理
		// console.log("临时文件路径：", tempFilePath);
		console.log("文件名:" + fileName);
		//console.log("文件大小：", fileSize)
		// console.log(files[i])
		// return;
		const formData = {
			fileName: fileName
		}

		try {
			let result = await uploadFilePromise(proxy.http, files[i], formOption.url, proxy.$store, formData);
			if (!result.status) {
				proxy.$toast(result.message)
				return;
			}
			//const fileName = getFileName(files[i]);
			// console.log(files[i])
			// console.log(fileName)
			//console.log(result)
			const url = proxy.http.ipAddress + result.data + fileName;

			if (!Array.isArray(formFields[formOption.field])) {
				formFields[formOption.field] = [];
			}

			//console.log('url:' + url)
			formFields[formOption.field].push({
				status: 'success',
				// message: '',
				url: url,
				orginUrl: result.data + fileName
			})
		} catch (e) {
			uni.hideToast()
			console.log(e)
			proxy.$toast(e.message)
			//TODO handle the exception
		}
	}
	uni.hideToast()
	proxy.$toast(proxy.$ts('上传成功'))
}
export const chooseFile = async (formFields, formOption, proxy) => {
	try {
		// #ifdef MP-WEIXIN
		wx.chooseMessageFile({
			count: formOption.maxCount || 1,
			success: (res) => {
				res.tempFilePaths = res.tempFiles.map(x => {
					return x.path;
				})
				console.log(res)
				uploadFile(res, formFields, formOption, proxy);
			}
		});
		return;
		// #endif


		// #ifdef H5
		uni.chooseFile({
			count: formOption.maxCount || 1,
			success: (res) => {
				console.log(res)
				uploadFile(res, formFields, formOption, proxy);
				//console.log(JSON.stringify(res.tempFilePaths));
			}
		})
		return;
		// #endif



		// #ifdef APP-PLUS
		const MediaStore = plus.android.importClass('android.provider.MediaStore');
		const main = plus.android.runtimeMainActivity();
		const Uri = plus.android.importClass('android.net.Uri');

		//视频还不能上传
		plus.io.chooseFile({
			title: '选择文件',
			//filetypes: ['xlsx'], // 允许的文件类型  
			multiple: true, //  formOption.maxCount>1, // 是否允许多选  
		}, (e) => {
			const tempFilePaths = e.files
			console.log(e.files[0])
			if ( e.files[0].split('/').pop().indexOf('.')!==-1) {
				uploadFile([e.files[0]], formFields, formOption, proxy, true);
				return;
			}
		
			// 获取文件的虚拟路径
			const contentUri = decodeURIComponent(e.files[0])
			console.log(contentUri)
			const uri = MediaStore.Files.getContentUri("external");
			// 给系统导入 contentResolver
			plus.android.importClass(main.getContentResolver());
			console.log('uri', uri)
			console.log('selection', "_id=?")
			const arr = contentUri.split(':')
			console.log('selectionArgs', [arr[arr.length - 1]].toString())
			// 通过查询的方式用虚拟路径的id1获取到文件的真实路径
			let cursor = main.getContentResolver().query(uri, ['_data'], "_id=?", [arr[arr.length - 1]],
				null);
			plus.android.importClass(cursor);
			console.log(cursor)
			console.log(cursor.moveToFirst())
			let result;
			if (cursor != null && cursor.moveToFirst()) {
				let column_index = cursor.getColumnIndexOrThrow('_data');
				console.log(column_index)
				result = cursor.getString(column_index) // result即文件的真实路径
				console.log('result', result)
				cursor.close();
			}
			result = 'file://' + result
			console.log(result)
			const fileName = result.split('/').pop();
			// await compressImage(lists[i].url) 
			console.log(fileName)
			// if (fileName) {
			uploadFile([result], formFields, formOption, proxy, true);
			// } else {
			// 	uploadFile([e.files[0]], formFields, formOption, proxy, true);
			// }
		})
		// #endif

	} catch (e) {
		console.log(e)
		proxy.$toast(e.message)
	}
}
