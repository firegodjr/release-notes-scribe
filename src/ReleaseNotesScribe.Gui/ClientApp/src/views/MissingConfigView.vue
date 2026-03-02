<template>
  <div>
    <h2 class="section-title">Configuration Required</h2>
    <div class="card" style="margin-bottom: 1rem;">
      <p style="margin-bottom: 1rem;">
        The application requires environment variables to connect to Azure DevOps and Azure OpenAI.
        Create a <code>.env</code> file in the project root with the following variables:
      </p>
      <DataTable :value="configStore.variables" stripedRows>
        <Column field="name" header="Variable" />
        <Column header="Status">
          <template #body="{ data }">
            <span :class="data.isSet ? 'status-set' : 'status-missing'">
              {{ data.isSet ? 'Set' : 'Missing' }}
            </span>
          </template>
        </Column>
        <Column header="Value">
          <template #body="{ data }">
            <span v-if="data.isSet" class="status-set">
              {{ data.displayValue }}
            </span>
            <span v-else class="status-missing">Not set</span>
          </template>
        </Column>
      </DataTable>
    </div>
    <Button label="Retry" icon="pi pi-refresh" @click="retry" />
  </div>
</template>

<script setup lang="ts">
import { useConfigStore } from '../stores/config'
import { useRouter } from 'vue-router'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'

const configStore = useConfigStore()
const router = useRouter()

async function retry() {
  await configStore.fetchStatus()
  if (configStore.isConfigured) {
    router.push('/workitems')
  }
}
</script>
