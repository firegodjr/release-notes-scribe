import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ConfigVariable } from '../types'
import { api } from '../api/client'

export const useConfigStore = defineStore('config', () => {
  const isConfigured = ref(false)
  const variables = ref<ConfigVariable[]>([])
  const loaded = ref(false)

  async function fetchStatus() {
    const res = await api.getConfigStatus()
    isConfigured.value = res.isConfigured
    variables.value = res.variables
    loaded.value = true
  }

  return { isConfigured, variables, loaded, fetchStatus }
})
