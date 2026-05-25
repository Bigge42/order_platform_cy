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
	if (!payloadText) {
		return false
	}
	if (payloadText === '[DONE]') {
		return true
	}

	let payload = null
	try {
		payload = JSON.parse(payloadText)
	} catch (error) {
		return false
	}

	await onMessage(payload)
	return payload.event === 'message_end'
}

async function cancelReader(reader) {
	if (!reader || typeof reader.cancel !== 'function') {
		return
	}

	try {
		await reader.cancel()
	} catch (error) {
	}
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
			const completed = await emitPayload(part, onMessage)
			if (completed) {
				await cancelReader(reader)
				return
			}
		}
	}

	buffer += decoder.decode()
	if (buffer.trim()) {
		const completed = await emitPayload(buffer, onMessage)
		if (completed) {
			await cancelReader(reader)
		}
	}
}
