import { defineStore } from "pinia";
import { ref } from "vue";

export const useSnackbarStore = defineStore("snackbar", () => {
  const visible = ref(false);
  const text = ref("");
  const timeout = ref(2000);

  function open(newText: string, newTimeout: number = 2000) {
    text.value = newText;
    visible.value = true;
    timeout.value = newTimeout;
  }

  function close() {
    visible.value = false;
  }

  return { visible, text, timeout, open, close };
});
