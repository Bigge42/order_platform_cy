import test from 'node:test'
import assert from 'node:assert/strict'

import {
  clearRememberedLogin,
  loadRememberedLogin,
  saveRememberedLogin
} from '../util/login-remember.js'

function createStorage(initialValue) {
  const store = initialValue === undefined ? new Map() : new Map([['loginRememberInfo', initialValue]])

  return {
    store,
    getStorageSync(key) {
      return store.get(key)
    },
    setStorageSync(key, value) {
      store.set(key, value)
    },
    removeStorageSync(key) {
      store.delete(key)
    }
  }
}

test('loadRememberedLogin returns stored credentials when present', () => {
  const storage = createStorage({
    userName: 'demo',
    password: 'secret',
    rememberPassword: true
  })

  assert.deepEqual(loadRememberedLogin(storage), {
    userName: 'demo',
    password: 'secret',
    rememberPassword: true
  })
})

test('saveRememberedLogin stores credentials only when rememberPassword is enabled', () => {
  const storage = createStorage()

  saveRememberedLogin({
    userName: 'demo',
    password: 'secret',
    rememberPassword: true
  }, storage)

  assert.deepEqual(storage.store.get('loginRememberInfo'), {
    userName: 'demo',
    password: 'secret',
    rememberPassword: true
  })
})

test('saveRememberedLogin clears cached credentials when rememberPassword is disabled', () => {
  const storage = createStorage({
    userName: 'demo',
    password: 'secret',
    rememberPassword: true
  })

  saveRememberedLogin({
    userName: 'demo',
    password: 'secret',
    rememberPassword: false
  }, storage)

  assert.equal(storage.store.has('loginRememberInfo'), false)
})

test('clearRememberedLogin removes cached credentials', () => {
  const storage = createStorage({
    userName: 'demo',
    password: 'secret',
    rememberPassword: true
  })

  clearRememberedLogin(storage)

  assert.equal(storage.store.has('loginRememberInfo'), false)
})

