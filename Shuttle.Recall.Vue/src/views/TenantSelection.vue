<template>
  <r-container size="small" class="p-4">
    <r-title :title="$t('tenant-selection')"></r-title>
    <div class="flex flex-col gap-2">
      <v-btn v-for="tenant in sessionStore.tenants" v-bind:key="tenant.id" :alt="`${tenant.id} logo`"
        class="py-8 px-4 flex flex-row justify-center items-center gap-2 w-full border cursor-pointer"
        @click="select(tenant)">
        <div v-if="tenant.logoSvg" v-html="tenant.logoSvg" class="v-icon__svg w-8 h-8 mr-4"></div>
        <span>{{ tenant.name }}</span>
      </v-btn>
    </div>
  </r-container>
</template>

<script setup lang="ts">
import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import { useI18n } from "vue-i18n";
import { accessApi } from "@/api";
import type { Tenant } from "@/types/app";
import router from "@/router";

const { t } = useI18n({ useScope: 'global' });
const sessionStore = useSessionStore();

const busy = ref(false);

const select = async (tenant: Tenant) => {
  busy.value = true;

  try {
    const { data: sessionResponse } = await accessApi.patch(`/v1/sessions/tenant`, {
      tenantId: tenant.id
    })

    sessionStore.tenantSelected(sessionResponse);
    router.push({ name: "events" });
  } catch (error: any) {
    useAlertStore().add({
      message: t("exceptions.tenant-selection"),
      type: "error",
      name: "tenant-select-exception"
    });

    sessionStore.signOut();
    router.push({ name: 'sign-in' })
  }
}
</script>
