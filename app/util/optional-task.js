export function startOptionalTask(task, onError = () => {}) {
	Promise.resolve()
		.then(task)
		.catch(onError)
}
