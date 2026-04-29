const splitPattern = /\r?\n\r?\n/

function extractDataBlock(block) {
	if (!block) {
		return ''
	}
	return block
		.split(/\r?\n/)
		.filter(line => line.startsWith('data:'))
		.map(line => line.substring(5).trim())
		.join('\n')
		.trim()
}

async function emitPayload(block, onMessage) {
	const payloadText = extractDataBlock(block)
	if (!payloadText || payloadText === '[DONE]') {
		return
	}

	let payload = null
	try {
		payload = JSON.parse(payloadText)
	} catch (error) {
		return
	}

	await onMessage(payload)
}

export function isStreamSupported() {
	return typeof fetch === 'function' && typeof TextDecoder !== 'undefined'
}

export async function readEventStream(response, onMessage) {
	if (!response || !response.body || !response.body.getReader) {
		throw new Error('当前环境不支持流式响应')
	}

	const reader = response.body.getReader()
	const decoder = new TextDecoder('utf-8')
	let buffer = ''

	while (true) {
		const result = await reader.read()
		if (result.done) {
			break
		}

		buffer += decoder.decode(result.value, { stream: true })
		const parts = buffer.split(splitPattern)
		buffer = parts.pop() || ''

		for (const part of parts) {
			await emitPayload(part, onMessage)
		}
	}

	buffer += decoder.decode()
	if (buffer.trim()) {
		await emitPayload(buffer, onMessage)
	}
}
