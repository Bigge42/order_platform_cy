import http from './http.js'
export default { 
	getDateTime(){
		return this.getDate(true)
	},
	getDate(time = false) {
		let date = new Date();
		let year = date.getFullYear();
		let month = date.getMonth() + 1;
		let day = date.getDate();

		let datetime = 
			year +
			'-' +
			(month < 10 ? '0' + month : month) +
			'-' +
			(day < 10 ? '0' + day : day);

		if (!time) {
			return datetime;
		}

		let hour = date.getHours();
		let minutes = date.getMinutes();
		let second = date.getSeconds();

		return (
			datetime +
			'' +
			' ' +
			(hour < 10 ? '0' + hour : hour) +
			':' +
			(minutes < 10 ? '0' + minutes : minutes) +
			':' +
			(second < 10 ? '0' + second : second)
		);
	},
	//日期+多少天
	//日期-10天：this.base.date('2023-04-02',-10)
	//当天日期-10天：this.base.date(this.base.getDate(),-10)
	addDays(date, days) {
		if (!days) {
			return date;
		}
		let dateArr = []
		if (typeof date == 'string') {
			dateArr = date.split(' ')
		}
		date = new Date(new Date(date).setDate(new Date(date).getDate() + days));
		var year = date.getFullYear();
		var month = date.getMonth() + 1;
		if (month < 10) {
			month = "0" + month;
		}
		var day = date.getDate();
		if (day < 10) {
			day = "0" + day;
		}
		date = year + "-" + month + "-" + day;
		if (dateArr.length == 1) {
			return date;
		}
		return date + ' ' + dateArr[1];
	},
	//日期+多少月，同上
	addMonth(date, m) {
		date = new Date(
			new Date(date).setMonth(
				new Date(date).getMonth() + (m === undefined ? 1 : m)
			)
		);
		var year = date.getFullYear();
		var month = date.getMonth() + 1;
		if (month < 10) {
			month = "0" + month;
		}
		var day = date.getDate();
		if (day < 10) {
			day = "0" + day;
		}
		return year + "-" + month + "-" + day;
	},
	isEmpty(value, checkArr) {
		if (checkArr) {
			if (Array.isArray(value)) {
				if (!value.length) {
					return true;
				}
			}
		}
		return (value === '' || value === ' ' || value === undefined || value === null || value === 'null')
	},
	getImg(img) {
		return (img || '').split(',')
			.filter(c => {
				return c
			})
			.map(c => {
				return {
					orginUrl: c,//.split('/').pop(),
					url:( c.startsWith('http')?'': http.ipAddress) + c
				}
			})
	}
}
