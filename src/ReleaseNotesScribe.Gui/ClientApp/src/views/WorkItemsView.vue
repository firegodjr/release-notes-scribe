<template>
  <div>
    <h2 class="section-title">Work Items</h2>

    <!-- Query controls -->
    <div class="card" style="margin-bottom: 1rem;">
      <div class="button-row" style="margin-bottom: 0.75rem;">
        <span class="p-float-label" style="flex: 1;">
          <InputText
            id="iterationPath"
            v-model="store.iterationPath"
            style="width: 100%;"
            placeholder="e.g. 4.2 or development\4.2, 4.3"
            @keyup.enter="store.queryWorkItems()"
          />
          <label for="iterationPath">Iteration path(s)</label>
        </span>
        <div style="display: flex; align-items: center; gap: 0.5rem;">
          <Checkbox v-model="store.onlyClosed" :binary="true" inputId="onlyClosed" />
          <label for="onlyClosed">Completed only</label>
        </div>
        <Button
          label="Query"
          icon="pi pi-search"
          :loading="store.loading"
          :disabled="!store.iterationPath.trim()"
          @click="store.queryWorkItems()"
        />
      </div>
    </div>

    <!-- Error -->
    <div v-if="store.error" class="card" style="margin-bottom: 1rem; border-color: #ef4444;">
      <p style="color: #ef4444;">{{ store.error }}</p>
    </div>

    <!-- Results -->
    <div v-if="store.items.length > 0">
      <div class="card">
        <div class="button-row" style="margin-bottom: 0.75rem;">
          <Button label="Select All" severity="secondary" size="small" @click="store.selectAll()" />
          <Button label="Deselect All" severity="secondary" size="small" @click="store.deselectAll()" />
          <span style="flex: 1;"></span>
          <span style="color: var(--color-text-dim); font-size: 0.9rem;">
            {{ store.selectedItems.length }} of {{ store.items.length }} selected
          </span>
        </div>

        <DataTable :value="store.items" stripedRows scrollable scrollHeight="500px">
          <Column header="" style="width: 3rem;">
            <template #body="{ data }">
              <Checkbox v-model="data.selected" :binary="true" @change="store.toggleItem(data.id)" />
            </template>
          </Column>
          <Column field="id" header="ID" style="width: 5rem;">
            <template #body="{ data }">
              <span style="color: var(--color-text-dim);">{{ data.id }}</span>
            </template>
          </Column>
          <Column field="workItemType" header="Type" style="width: 10rem;">
            <template #body="{ data }">
              <span :class="['type-tag', typeClass(data.workItemType)]">
                {{ data.workItemType }}
              </span>
            </template>
          </Column>
          <Column field="title" header="Title" />
          <Column field="state" header="State" style="width: 7rem;" />
          <Column field="iterationPath" header="Iteration" style="width: 14rem;">
            <template #body="{ data }">
              <span style="color: var(--color-text-dim);">{{ data.iterationPath }}</span>
            </template>
          </Column>
        </DataTable>

        <div class="button-row" style="margin-top: 1rem; justify-content: flex-end;">
          <Button
            label="Generate Release Notes"
            icon="pi pi-sparkles"
            :disabled="!store.hasSelection"
            @click="goGenerate"
          />
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else-if="!store.loading && store.iterationPath">
      <p style="color: var(--color-text-dim);">No work items found. Try a different iteration path.</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useWorkItemsStore } from '../stores/workitems'
import { useRouter } from 'vue-router'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Checkbox from 'primevue/checkbox'

const store = useWorkItemsStore()
const router = useRouter()

function typeClass(type: string): string {
  const map: Record<string, string> = {
    'Epic': 'epic',
    'Feature': 'feature',
    'User Story': 'user-story',
    'Defect': 'defect',
    'Design Debt': 'design-debt'
  }
  return map[type] ?? ''
}

function goGenerate() {
  store.generationActive = true
  router.push('/generate')
}
</script>
