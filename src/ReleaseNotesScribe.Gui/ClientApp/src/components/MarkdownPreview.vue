<template>
  <div class="markdown-preview" v-html="renderedHtml"></div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { marked } from 'marked'

const props = defineProps<{
  markdown: string
}>()

const renderedHtml = computed(() => marked(props.markdown) as string)
</script>
