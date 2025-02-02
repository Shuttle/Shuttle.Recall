<template>
  <div
    class="lg:w-2/4 md:w-3/4 p-6 mx-auto bg-zinc-800 text-zinc-300 border border-zinc-400 text-lg flex flex-col justify-center items-center">
    <v-progress-circular indeterminate></v-progress-circular>
    <span>{{ $t("messages.retrieving-session") }}</span>
  </div>
</template>

<script lang="ts" setup>
import { useSessionStore } from "@/stores/session";
import { useAlertStore } from "@/stores/alert";
import router from "@/router";

const props = defineProps({
  token: String
});

const sessionStore = useSessionStore();
const alertStore = useAlertStore();

onMounted(() => {
  if (!props.token) {
    return;
  }

  sessionStore.signInExchangeToken(props.token)
    .catch((error) => {
      alertStore.add({
        message: error.toString(),
        type: "error",
        name: "session-exception"
      });
    })
    .finally(() => {
      router.push({ name: "events" });
    });
});
</script>
