import store from '../store/index.js'

var ipAddress;
if (process.env.NODE_ENV === 'development') {
	//本地开发改为这个地址
	ipAddress = "http://localhost:9100/"
} else {
	//发布后的地址
	ipAddress = "http://localhost:9100/"
}
ipAddress = "http://localhost:9100/"
function post(url, data, loading, error) {
	return request(url, 'POST', data, loading, error);
}

async function get(url, data, loading, error) {
	return request(url, 'GET', data, loading, error);
}

function getToken() {
	return store.getters.getToken();
}

function request(url, method, data, loading, error) {
	if (loading) {
		uni.showLoading({
			title: typeof loading == 'boolean' ? "正在处理..." : loading
		})
	}
	if (url.startsWith("/")) {
		url = url.substr(1)
	}
	url = ipAddress + url;
	var _header = {};
	var _token = getToken();
	if (_token) {
		_header['Authorization'] = _token;

	}
	let langType = uni.getStorageSync('pro_lang');

	_header['lang'] = langType;
	_header['serviceId'] = uni.getStorageSync('serviceId');
	return new Promise((reslove, reject) => {
		_header.uapp = '1';
		uni.request({
			url: url,
			method: method,
			data: data,
			header: _header,
			success: (res) => {

				if (loading) {
					uni.hideLoading();
				}
				if (res.statusCode == 500) {
					if (error) {
						error("服务器内部错误");
						return;
					}
					console.log(JSON.stringify(res))
					uni.showToast({
						icon: "none",
						title: "服务器内部错误"
					})
					return;
				}

				if (res.statusCode == 404) {
					if (error) {
						error("未找到请求接口");
						return;
					}
					uni.showToast({
						icon: "none",
						title: "未找到请求接口"
					})
					return
				}
				if (res.statusCode == 202 || res.statusCode == 401) {
					if (res.data && res.data.message && res.data.code != '401') {
						uni.showToast({
							icon: "none",
							title: res.data.message
						})
						return;
					};
					uni.reLaunch({
						url: "/pages/login/login"
					})
					return;
				}

				if (res.header.vol_exp == "1") {
					post('api/User/replaceToken', "POST").then(async result => {
						let userInfo = store.getters.getUserInfo();
						userInfo.token = result.data;
						store.commit('setUserInfo', userInfo);
					});
				}
				reslove(res.data)
			},
			fail: (err) => {
				if (loading) {
					uni.hideLoading();
				}
				if (error) {
					error(err);
					return;
				}
				console.log(JSON.stringify(err))
				if ((err.hasOwnProperty('statusCode') && err.statusCode == 401) ||
					(err.data && err.data.code == 401)) {
					uni.reLaunch({
						url: "/pages/login/login"
					})
					return;
				}
				uni.showToast({
					icon: "none",
					title: "请求接口失败" + JSON.stringify(err)
				})
			}
		});
	})
}



export default {
	get,
	post,
	request,
	ipAddress
}
