import test from 'node:test'
import assert from 'node:assert/strict'

import {
  buildPreviewAssetUrl,
  buildAssistantBlocks,
  extractCopyableAssistantText,
  findAssistantRegenerateSource,
  getMessageFileImageDisplaySrc,
  getMessageFileDebugUrl,
  getMessageFileRenderSrc,
  isSyntheticMarkdownImageId,
  normalizeMessageFeedback,
  shouldCollectFeedbackContent,
  resolveNextMessageFeedback
} from '../util/ai-message.js'

test('getMessageFileDebugUrl prefers preview request url for assistant images', () => {
  const file = {
    id: 'file-1',
    uploadFileId: 'upload-1',
    previewRequestUrl: 'http://localhost:9200/api/AI/PreviewFile?appId=7&fileId=upload-1',
    previewUrl: 'https://remote.example.com/image.png',
    url: 'https://origin.example.com/image.png'
  }

  assert.equal(
    getMessageFileDebugUrl(file),
    'http://localhost:9200/api/AI/PreviewFile?appId=7&fileId=upload-1'
  )
})

test('getMessageFileRenderSrc prefers hydrated local preview path before remote urls', () => {
  const file = {
    localPreviewPath: 'blob:http://localhost:8080/preview-1',
    localPath: '/tmp/image.png',
    previewUrl: 'https://remote.example.com/image.png'
  }

  assert.equal(getMessageFileRenderSrc(file), 'blob:http://localhost:8080/preview-1')
})

test('getMessageFileRenderSrc falls back to remote preview url when no local preview exists', () => {
  const file = {
    previewRequestUrl: 'http://localhost:9200/api/AI/PreviewFile?appId=7&fileId=file-2',
    previewUrl: 'https://remote.example.com/image-2.png'
  }

  assert.equal(getMessageFileRenderSrc(file), 'https://remote.example.com/image-2.png')
  assert.equal(
    getMessageFileDebugUrl(file),
    'http://localhost:9200/api/AI/PreviewFile?appId=7&fileId=file-2'
  )
})

test('getMessageFileImageDisplaySrc hides protected preview urls until a local preview is ready', () => {
  const file = {
    previewUrl: 'http://localhost:9200/api/AI/PreviewFile?appId=7&fileId=file-3'
  }

  assert.equal(getMessageFileImageDisplaySrc(file), '')
  assert.equal(
    getMessageFileImageDisplaySrc({
      ...file,
      localPreviewPath: 'blob:http://localhost:8080/file-3'
    }),
    'blob:http://localhost:8080/file-3'
  )
})

test('buildPreviewAssetUrl wraps signed dify preview paths with backend proxy', () => {
  const result = buildPreviewAssetUrl(
    (url) => `http://localhost:9200/${url}`,
    7,
    '/files/7c82fb6a-2330-45ec-917e-31a6a1c596ff/file-preview?timestamp=1&nonce=abc'
  )

  assert.equal(
    result,
    'http://localhost:9200/api/AI/PreviewAsset?appId=7&assetUrl=%2Ffiles%2F7c82fb6a-2330-45ec-917e-31a6a1c596ff%2Ffile-preview%3Ftimestamp%3D1%26nonce%3Dabc'
  )
})

test('buildAssistantBlocks extracts markdown images and resolves them through preview proxy', () => {
  const blocks = buildAssistantBlocks(
    [
      'Steps below',
      '![image](/files/7c82fb6a-2330-45ec-917e-31a6a1c596ff/file-preview?timestamp=1&nonce=abc)',
      'Done'
    ].join('\n'),
    [],
    {
      resolveAssetUrl(url) {
        if (url.startsWith('/files/7c82fb6a-2330-45ec-917e-31a6a1c596ff/file-preview')) {
          return 'http://localhost:9200/api/AI/PreviewAsset?appId=7&assetUrl=%2Ffiles%2F7c82fb6a-2330-45ec-917e-31a6a1c596ff%2Ffile-preview%3Ftimestamp%3D1%26nonce%3Dabc'
        }
        return url
      }
    }
  )

  assert.equal(blocks.length, 1)
  assert.match(blocks[0].html, /Steps below/)
  assert.match(blocks[0].html, /Done/)
  assert.doesNotMatch(blocks[0].html, /<img/)
  assert.equal(blocks[0].images.length, 1)
  assert.equal(blocks[0].images[0].alt, 'image')
  assert.equal(
    blocks[0].images[0].previewUrl,
    'http://localhost:9200/api/AI/PreviewAsset?appId=7&assetUrl=%2Ffiles%2F7c82fb6a-2330-45ec-917e-31a6a1c596ff%2Ffile-preview%3Ftimestamp%3D1%26nonce%3Dabc'
  )
})

test('buildAssistantBlocks hides incomplete think tags during streaming', () => {
  const blocks = buildAssistantBlocks('Result\n<think>still thinking...</thi', [])

  assert.equal(blocks.length, 2)
  assert.equal(blocks[0].type, 'markdown')
  assert.match(blocks[0].html, /Result/)
  assert.equal(blocks[1].type, 'think')
  assert.equal(blocks[1].pending, true)
  assert.match(blocks[1].html, /still thinking/)
  assert.doesNotMatch(blocks[0].html, /<think/i)
  assert.doesNotMatch(blocks[1].html, /<think/i)
})

test('buildAssistantBlocks keeps think block id stable while streaming content grows', () => {
  const first = buildAssistantBlocks('Result\n<think>a', [])
  const second = buildAssistantBlocks('Result\n<think>analysis', [])

  assert.equal(first[1].type, 'think')
  assert.equal(second[1].type, 'think')
  assert.equal(first[1].id, second[1].id)
})

test('buildAssistantBlocks marks think blocks as completed after the closing tag arrives', () => {
  const blocks = buildAssistantBlocks('Result\n<think>done</think>\nFinal', [])

  assert.equal(blocks[1].type, 'think')
  assert.equal(blocks[1].pending, false)
})

test('isSyntheticMarkdownImageId matches generated markdown image ids only', () => {
  assert.equal(isSyntheticMarkdownImageId('md-image-0-51dffc63'), true)
  assert.equal(isSyntheticMarkdownImageId('file-123'), false)
  assert.equal(isSyntheticMarkdownImageId('7c82fb6a-2330-45ec-917e-31a6a1c596ff'), false)
})

test('normalizeMessageFeedback reads rating from object or string payloads', () => {
  assert.equal(normalizeMessageFeedback(null), null)
  assert.equal(normalizeMessageFeedback(undefined), null)
  assert.equal(normalizeMessageFeedback('like'), 'like')
  assert.equal(normalizeMessageFeedback('dislike'), 'dislike')
  assert.equal(normalizeMessageFeedback({ rating: 'like' }), 'like')
  assert.equal(normalizeMessageFeedback({ rating: 'dislike', content: 'not helpful' }), 'dislike')
  assert.equal(normalizeMessageFeedback({ rating: 'other' }), null)
})

test('resolveNextMessageFeedback toggles off the current rating when clicked again', () => {
  assert.equal(resolveNextMessageFeedback(null, 'like'), 'like')
  assert.equal(resolveNextMessageFeedback(null, 'dislike'), 'dislike')
  assert.equal(resolveNextMessageFeedback('like', 'like'), null)
  assert.equal(resolveNextMessageFeedback('dislike', 'dislike'), null)
  assert.equal(resolveNextMessageFeedback('like', 'dislike'), 'dislike')
  assert.equal(resolveNextMessageFeedback('dislike', 'like'), 'like')
})

test('shouldCollectFeedbackContent only requires content when setting dislike', () => {
  assert.equal(shouldCollectFeedbackContent(null, 'dislike'), true)
  assert.equal(shouldCollectFeedbackContent('like', 'dislike'), true)
  assert.equal(shouldCollectFeedbackContent('dislike', 'dislike'), false)
  assert.equal(shouldCollectFeedbackContent('dislike', 'like'), false)
  assert.equal(shouldCollectFeedbackContent(null, 'like'), false)
  assert.equal(shouldCollectFeedbackContent(null, null), false)
})

test('extractCopyableAssistantText removes think blocks and preserves visible answer text', () => {
  const text = [
    'Final answer:',
    '',
    '<think>This reasoning should not be copied.</think>',
    '',
    '1. Close the program',
    '2. Reload the model'
  ].join('\n')

  assert.equal(
    extractCopyableAssistantText(text),
    ['Final answer:', '', '1. Close the program', '2. Reload the model'].join('\n')
  )
})

test('findAssistantRegenerateSource reuses the nearest preceding user query and files', () => {
  const source = findAssistantRegenerateSource(
    [
      {
        localId: 'user-1',
        role: 'user',
        content: '',
        requestQuery: 'please analyze the attachment',
        files: [{ localId: 'file-1', uploadFileId: 'upload-1', type: 'image' }]
      },
      { localId: 'assistant-1', role: 'assistant', content: 'first answer' },
      { localId: 'assistant-2', role: 'assistant', content: 'second answer' }
    ],
    'assistant-2'
  )

  assert.deepEqual(source, {
    localId: 'user-1',
    query: 'please analyze the attachment',
    displayQuery: '',
    files: [{ localId: 'file-1', uploadFileId: 'upload-1', type: 'image' }]
  })
})

test('findAssistantRegenerateSource returns null when the assistant has no preceding user turn', () => {
  assert.equal(
    findAssistantRegenerateSource([{ localId: 'assistant-1', role: 'assistant', content: 'answer' }], 'assistant-1'),
    null
  )
})
