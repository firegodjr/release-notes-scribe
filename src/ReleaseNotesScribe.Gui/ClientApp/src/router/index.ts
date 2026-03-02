import { createRouter, createWebHistory } from 'vue-router'
import { useConfigStore } from '../stores/config'

import MissingConfigView from '../views/MissingConfigView.vue'
import WorkItemsView from '../views/WorkItemsView.vue'
import GenerationView from '../views/GenerationView.vue'

const routes = [
  { path: '/', redirect: '/workitems' },
  { path: '/missing-config', name: 'missing-config', component: MissingConfigView },
  { path: '/workitems', name: 'workitems', component: WorkItemsView },
  { path: '/generate', name: 'generate', component: GenerationView }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to) => {
  const configStore = useConfigStore()

  // Fetch config status on first navigation
  if (!configStore.loaded) {
    await configStore.fetchStatus()
  }

  if (!configStore.isConfigured && to.name !== 'missing-config') {
    return { name: 'missing-config' }
  }

  if (configStore.isConfigured && to.name === 'missing-config') {
    return { name: 'workitems' }
  }
})

export default router
