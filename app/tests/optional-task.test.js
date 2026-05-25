import test from 'node:test'
import assert from 'node:assert/strict'

import { startOptionalTask } from '../util/optional-task.js'

test('startOptionalTask returns immediately when the optional task stays pending', async () => {
  let started = false

  startOptionalTask(() => {
    started = true
    return new Promise(() => {})
  })

  await new Promise((resolve) => setTimeout(resolve, 0))

  assert.equal(started, true)
})

test('startOptionalTask forwards optional task failures without throwing synchronously', async () => {
  let captured = ''

  startOptionalTask(
    () => Promise.reject(new Error('suggested questions failed')),
    (error) => {
      captured = error.message
    }
  )

  await new Promise((resolve) => setTimeout(resolve, 0))

  assert.equal(captured, 'suggested questions failed')
})
