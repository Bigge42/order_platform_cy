import { cpSync, existsSync, mkdirSync, rmSync } from 'node:fs'
import { resolve } from 'node:path'

const outputDir = process.argv[2]

if (!outputDir) {
  console.error('Usage: node scripts/copy-static.mjs <output-dir>')
  process.exit(1)
}

const sourceDir = resolve('static')
const targetDir = resolve(outputDir, 'static')

if (!existsSync(sourceDir)) {
  console.error(`Static directory not found: ${sourceDir}`)
  process.exit(1)
}

mkdirSync(resolve(outputDir), { recursive: true })
rmSync(targetDir, { recursive: true, force: true })
cpSync(sourceDir, targetDir, { recursive: true })
