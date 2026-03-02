<template>
  <div>
    <h2 class="section-title">Generate Release Notes</h2>

    <div class="card" style="margin-bottom: 1rem;">
      <div class="button-row">
        <span class="p-float-label" style="flex: 1; max-width: 300px;">
          <InputText
            id="versionLabel"
            v-model="store.versionLabel"
            style="width: 100%;"
            placeholder="e.g. 4.2"
          />
          <label for="versionLabel">Version label</label>
        </span>
        <Button
          label="Generate"
          icon="pi pi-sparkles"
          :loading="store.generating"
          :disabled="!store.hasSelection || !store.versionLabel.trim()"
          @click="store.generateNotes()"
        />
        <Button
          v-if="store.generatedMarkdown"
          label="Regenerate"
          icon="pi pi-refresh"
          severity="secondary"
          :loading="store.generating"
          @click="store.generateNotes()"
        />
        <Button
          v-if="store.generatedMarkdown"
          label="Copy to Clipboard"
          icon="pi pi-copy"
          severity="secondary"
          @click="copyToClipboard"
        />
      </div>
      <p style="margin-top: 0.5rem; color: var(--color-text-dim); font-size: 0.9rem;">
        {{ store.selectedItems.length }} work item(s) selected
      </p>
    </div>

    <!-- Error -->
    <div v-if="store.error" class="card" style="margin-bottom: 1rem; border-color: #ef4444;">
      <p style="color: #ef4444;">{{ store.error }}</p>
    </div>

    <!-- Loading -->
    <div v-if="store.generating" class="card" style="text-align: center; padding: 3rem;">
      <i class="pi pi-spin pi-spinner" style="font-size: 2rem; color: var(--color-primary);"></i>
      <p style="margin-top: 1rem; color: var(--color-text-dim);">Generating release notes...</p>
    </div>

    <!-- Result -->
    <MarkdownPreview v-if="store.generatedMarkdown && !store.generating" :markdown="store.generatedMarkdown" />

    <!-- Back link -->
    <div style="margin-top: 1rem;">
      <router-link to="/workitems" style="color: var(--color-primary);">
        &larr; Back to Work Items
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useWorkItemsStore } from '../stores/workitems'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import MarkdownPreview from '../components/MarkdownPreview.vue'

const store = useWorkItemsStore()

async function copyToClipboard() {
  await navigator.clipboard.writeText(store.generatedMarkdown)
}
</script>
