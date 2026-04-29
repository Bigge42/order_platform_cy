import test from 'node:test'
import assert from 'node:assert/strict'

import {
  DEFAULT_AI_APP_ICON,
  getAiAppInitial,
  isAiAppImageSource,
  normalizeAiApp,
  normalizeAiAppList,
  resolveAiAppGlyphIcon,
  resolveAiAppIcon,
  resolveAiAppImageSrc
} from '../util/ai-app.js'

test('normalizeAiAppList supports rows payloads with uppercase field names', () => {
  const list = normalizeAiAppList({
    status: 0,
    rows: [
      {
        Id: '7',
        AppName: 'TC issue assistant',
        AppType: 'chat',
        Description: 'Answers handbook questions',
        Icon: '   ',
        SortNo: 3
      }
    ]
  })

  assert.deepEqual(list, [
    {
      id: '7',
      appName: 'TC issue assistant',
      appType: 'chat',
      description: 'Answers handbook questions',
      icon: '',
      sortNo: 3
    }
  ])
})

test('normalizeAiAppList preserves camelCase payloads', () => {
  const list = normalizeAiAppList([
    {
      id: '8',
      appName: 'Order query assistant',
      appType: 'chat',
      description: 'Checks delivery progress',
      icon: '/static/icon/36.png',
      sortNo: 0
    }
  ])

  assert.equal(list[0].appName, 'Order query assistant')
  assert.equal(list[0].icon, '/static/icon/36.png')
})

test('normalizeAiApp supports single app payloads', () => {
  const item = normalizeAiApp({
    Id: '9',
    AppName: 'Single app',
    AppType: 'agent',
    Description: 'Single payload',
    Icon: '<img src="/Upload/AI/icon-9.png" />',
    SortNo: '5'
  })

  assert.deepEqual(item, {
    id: '9',
    appName: 'Single app',
    appType: 'agent',
    description: 'Single payload',
    icon: '<img src="/Upload/AI/icon-9.png" />',
    sortNo: 5
  })
})

test('resolveAiAppIcon falls back to the default icon when icon is empty', () => {
  assert.equal(resolveAiAppIcon(''), DEFAULT_AI_APP_ICON)
  assert.equal(resolveAiAppIcon('   '), DEFAULT_AI_APP_ICON)
  assert.equal(resolveAiAppIcon('/static/icon/36.png'), '/static/icon/36.png')
})

test('resolveAiAppIcon extracts the src when icon is stored as an img tag', () => {
  assert.equal(
    resolveAiAppIcon('<img src="/Upload/AI/icon-1.png" alt="icon" />'),
    '/Upload/AI/icon-1.png'
  )
})

test('isAiAppImageSource detects supported image source formats', () => {
  assert.equal(isAiAppImageSource('/static/icon/36.png'), true)
  assert.equal(isAiAppImageSource('static/icon/36.png'), true)
  assert.equal(isAiAppImageSource('/Upload/AI/icon-1.png'), true)
  assert.equal(isAiAppImageSource('Upload/AI/icon-1.png'), true)
  assert.equal(isAiAppImageSource('https://example.com/icon.png'), true)
  assert.equal(isAiAppImageSource('//cdn.example.com/icon.png'), true)
  assert.equal(isAiAppImageSource('data:image/png;base64,abc123'), true)
  assert.equal(isAiAppImageSource('blob:http://localhost/icon-1'), true)
  assert.equal(isAiAppImageSource('🤖'), false)
})

test('resolveAiAppImageSrc preserves direct urls and prefixes relative api paths', () => {
  assert.equal(
    resolveAiAppImageSrc('https://example.com/icon.png', 'http://localhost:9200/'),
    'https://example.com/icon.png'
  )
  assert.equal(
    resolveAiAppImageSrc('/Upload/AI/icon-1.png', 'http://localhost:9200/'),
    'http://localhost:9200/Upload/AI/icon-1.png'
  )
  assert.equal(
    resolveAiAppImageSrc('Upload/AI/icon-1.png', 'http://localhost:9200/'),
    'http://localhost:9200/Upload/AI/icon-1.png'
  )
  assert.equal(resolveAiAppImageSrc('static/icon-1.png', '/'), '/static/icon-1.png')
})

test('resolveAiAppImageSrc keeps default icon for empty values and skips glyph icons', () => {
  assert.equal(resolveAiAppImageSrc('', '/'), DEFAULT_AI_APP_ICON)
  assert.equal(resolveAiAppImageSrc('🤖', 'http://localhost:9200/'), '')
})

test('resolveAiAppGlyphIcon returns glyph icons and ignores image sources', () => {
  assert.equal(resolveAiAppGlyphIcon('🤖'), '🤖')
  assert.equal(resolveAiAppGlyphIcon('助手'), '助手')
  assert.equal(resolveAiAppGlyphIcon('/static/icon-1.png'), '')
  assert.equal(resolveAiAppGlyphIcon('<img src="/Upload/AI/icon-1.png" />'), '')
})

test('getAiAppInitial returns the first visible character of the app name', () => {
  assert.equal(getAiAppInitial(' TC issue assistant'), 'T')
  assert.equal(getAiAppInitial(' 智能助手'), '智')
  assert.equal(getAiAppInitial(''), 'A')
})
