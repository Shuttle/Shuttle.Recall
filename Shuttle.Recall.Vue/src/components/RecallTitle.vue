<template>
  <div class="flex flex-row align-middle font-semibold text-lg items-start pb-2">
    <div class="grow">
      <div class="flex flex-col">
        <div>{{ props.title }}</div>
        <div v-if="!!props.description" class="text-sm font-normal text-gray-700 dark:text-gray-300">
          {{ props.description }}
        </div>
      </div>
    </div>
    <v-btn v-if="canClose" :icon="mdiClose" @click.stop="click" size="x-small" flat></v-btn>
  </div>
</template>

<script setup lang="ts">
import type { Title } from '@/types/app'
import { useRouter } from 'vue-router'
import { useDrawerStore } from '@/stores/drawer'
import { mdiClose } from '@mdi/js'

const router = useRouter()
const props = defineProps<Title>()

const canClose = computed(() => props.closeDrawer || props.closePath || props.closeClick)

const click = () => {
  if (props.closeDrawer) {
    useDrawerStore().close(false)
    return
  }

  if (props.closePath) {
    router.push(props.closePath)
    return
  }

  if (props.closeClick) {
    props.closeClick()
  }
}
</script>
