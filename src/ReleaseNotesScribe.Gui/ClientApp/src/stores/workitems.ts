import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { SelectableWorkItem } from '../types'
import { api } from '../api/client'

export const useWorkItemsStore = defineStore('workitems', () => {
  const items = ref<SelectableWorkItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const iterationPath = ref('')
  const onlyClosed = ref(true)
  const versionLabel = ref('')
  const generatedMarkdown = ref('')
  const generating = ref(false)
  const generationActive = ref(false)

  const selectedItems = computed(() => items.value.filter(i => i.selected))
  const hasSelection = computed(() => selectedItems.value.length > 0)

  async function queryWorkItems() {
    loading.value = true
    error.value = null
    items.value = []

    try {
      const res = await api.queryWorkItems({
        iterationPath: iterationPath.value,
        onlyClosed: onlyClosed.value
      })

      items.value = res.items.map(item => ({ ...item, selected: true }))

      // Derive default version label from iteration path input
      const input = iterationPath.value
      versionLabel.value = input.includes('\\')
        ? input.substring(input.lastIndexOf('\\') + 1)
        : input
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to query work items'
    } finally {
      loading.value = false
    }
  }

  function toggleItem(id: number) {
    const item = items.value.find(i => i.id === id)
    if (item) item.selected = !item.selected
  }

  function selectAll() {
    items.value.forEach(i => { i.selected = true })
  }

  function deselectAll() {
    items.value.forEach(i => { i.selected = false })
  }

  async function generateNotes() {
    generating.value = true
    error.value = null

    try {
      const selected = selectedItems.value.map(({ selected: _, ...rest }) => rest)
      const res = await api.generate({
        versionLabel: versionLabel.value,
        items: selected
      })
      generatedMarkdown.value = res.markdown
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to generate release notes'
    } finally {
      generating.value = false
    }
  }

  function resetGeneration() {
    generatedMarkdown.value = ''
    error.value = null
    generationActive.value = false
  }

  return {
    items, loading, error, iterationPath, onlyClosed,
    versionLabel, generatedMarkdown, generating, generationActive,
    selectedItems, hasSelection,
    queryWorkItems, toggleItem, selectAll, deselectAll, generateNotes, resetGeneration
  }
})
