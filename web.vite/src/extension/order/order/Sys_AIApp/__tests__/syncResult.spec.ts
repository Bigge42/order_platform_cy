/* @vitest-environment jsdom */

import { describe, expect, it } from 'vitest'
import {
  buildSyncResultState,
  getRetrySelectionIds,
  getSyncResultAlertType,
  getSyncResultSummaryText
} from '../syncResult'

describe('Sys_AIApp sync result helpers', () => {
  it('normalizes partial sync results and keeps failed ids for retry', () => {
    const result = buildSyncResultState({
      totalSelected: 3,
      matchedCount: 2,
      inserted: 1,
      updated: 0,
      details: [
        {
          platformAppId: 'app-1',
          appName: 'Alpha',
          success: true,
          action: 'inserted',
          message: 'ok'
        },
        {
          platformAppId: 'app-2',
          appName: 'Beta',
          success: false,
          action: 'failed',
          message: 'token failed'
        },
        {
          platformAppId: 'app-3',
          appName: 'Gamma',
          success: false,
          action: 'missing',
          message: 'not found'
        }
      ]
    })

    expect(result.totalSelected).toBe(3)
    expect(result.successCount).toBe(1)
    expect(result.failedCount).toBe(2)
    expect(result.failedItems.map((item) => item.platformAppId)).toEqual(['app-2', 'app-3'])
    expect(getRetrySelectionIds(result)).toEqual(['app-2', 'app-3'])
    expect(getSyncResultAlertType(result)).toBe('warning')
  })

  it('marks all-failed results as error', () => {
    const result = buildSyncResultState({
      details: [
        {
          platformAppId: 'app-2',
          appName: 'Beta',
          success: false,
          action: 'failed',
          message: 'login failed'
        }
      ]
    })

    expect(result.successCount).toBe(0)
    expect(result.failedCount).toBe(1)
    expect(getSyncResultAlertType(result)).toBe('error')
  })

  it('builds a readable summary for fully successful sync', () => {
    const result = buildSyncResultState({
      totalSelected: 2,
      successCount: 2,
      failedCount: 0,
      inserted: 1,
      updated: 1,
      details: [
        { platformAppId: 'app-1', appName: 'Alpha', success: true, action: 'inserted' },
        { platformAppId: 'app-2', appName: 'Beta', success: true, action: 'updated' }
      ]
    })

    const summary = getSyncResultSummaryText(result)
    expect(summary).toContain('\u6210\u529f 2')
    expect(summary).toContain('\u5931\u8d25 0')
    expect(summary).toContain('\u65b0\u589e 1')
    expect(summary).toContain('\u66f4\u65b0 1')
    expect(summary).not.toContain('AppKey')
  })
})
