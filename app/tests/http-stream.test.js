import test from 'node:test'
import assert from 'node:assert/strict'

import { readEventStream } from '../util/http-stream.js'

function createOpenStreamResponse(chunks) {
  const encoder = new TextEncoder()
  let index = 0

  return {
    body: {
      getReader() {
        return {
          read() {
            if (index < chunks.length) {
              const value = encoder.encode(chunks[index])
              index += 1
              return Promise.resolve({ done: false, value })
            }

            return new Promise(() => {})
          },
          cancel() {
            return Promise.resolve()
          }
        }
      }
    }
  }
}

test('readEventStream resolves when a message_end event arrives before the connection closes', async () => {
  const payloads = []
  const response = createOpenStreamResponse([
    'data: {"event":"message","answer":"done"}\n\n',
    'data: {"event":"message_end","metadata":{"usage":{"total_tokens":1}}}\n\n'
  ])

  const completed = await Promise.race([
    readEventStream(response, async(payload) => {
      payloads.push(payload)
    }).then(() => true),
    new Promise((resolve) => setTimeout(() => resolve(false), 50))
  ])

  assert.equal(completed, true)
  assert.deepEqual(
    payloads.map((payload) => payload.event),
    ['message', 'message_end']
  )
})
