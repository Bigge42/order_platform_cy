const LOGIN_REMEMBER_KEY = 'loginRememberInfo'

function getStorageApi(storage) {
  if (storage && typeof storage.getStorageSync === 'function') {
    return storage
  }

  if (typeof uni !== 'undefined') {
    return uni
  }

  return null
}

function normalizeRememberedLogin(payload = {}) {
  const userName = String(payload.userName || payload.username || '').trim()
  const password = String(payload.password || '').trim()
  const rememberPassword = !!payload.rememberPassword

  return {
    userName,
    password,
    rememberPassword
  }
}

export function loadRememberedLogin(storage) {
  const api = getStorageApi(storage)
  if (!api) {
    return normalizeRememberedLogin()
  }

  return normalizeRememberedLogin(api.getStorageSync(LOGIN_REMEMBER_KEY) || {})
}

export function saveRememberedLogin(payload = {}, storage) {
  const api = getStorageApi(storage)
  if (!api) {
    return normalizeRememberedLogin(payload)
  }

  const remembered = normalizeRememberedLogin(payload)
  if (remembered.rememberPassword && remembered.userName && remembered.password) {
    api.setStorageSync(LOGIN_REMEMBER_KEY, remembered)
  } else {
    api.removeStorageSync(LOGIN_REMEMBER_KEY)
  }

  return remembered
}

export function clearRememberedLogin(storage) {
  const api = getStorageApi(storage)
  if (!api) {
    return
  }

  api.removeStorageSync(LOGIN_REMEMBER_KEY)
}

