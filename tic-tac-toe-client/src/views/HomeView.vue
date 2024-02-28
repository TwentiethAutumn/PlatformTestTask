<script setup>
import TheWelcome from '../components/TheWelcome.vue'
</script>

<template>
  <main>
    <div style="display: flex; flex-direction: column;">
      <input placeholder="login" v-model="username" />
      <input placeholder="password" v-model="password" />
      <button @click="login">Register</button>
    </div>
  </main>
</template>

<script>

import store from "@/gameLogic/store.js";
import axios from 'axios';

export default {
  data: () => {
    return {
      username: "",
      password: "",
    }
  },
  methods: {
    async fetchData(url, data) {
      return axios.post("http://localhost:7271/api/Users/" + url, data).then(res => res);
    },
    async login() {
      console.log("fetch");

      await this.fetchData("register", { username: this.username, password: this.password });

      let res = await this.fetchData("token", { username: this.username, password: this.password });

      console.log("token: " + res.data);
      store.userToken = res.data;
      store.userName = this.username;

      await this.$router.push("/game")
    },
  }
}
</script>