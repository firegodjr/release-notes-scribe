<template>
  <div class="app-container">
    <header class="app-header">
      <h1>Release Notes Scribe</h1>
      <nav v-if="configStore.isConfigured">
        <router-link to="/workitems">Work Items</router-link>
        <router-link
          v-if="workItemsStore.generationActive"
          to="/generate"
        >Generate</router-link>
        <span v-else class="nav-disabled">Generate</span>
      </nav>
    </header>
    <main class="app-main">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import { useConfigStore } from './stores/config'
import { useWorkItemsStore } from './stores/workitems'

const configStore = useConfigStore()
const workItemsStore = useWorkItemsStore()
</script>
