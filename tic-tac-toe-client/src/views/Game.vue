<template>
  <div class="about">
    <h1>GamePage</h1>
    <div class="block" v-if="!isConnected">
      Connection...
    </div>
    <div class="block" v-if="isConnected && !isFindingOpponent && !isPlaying">
      Connected!
      <button @click="() => send('findGame')">Start game</button>
    </div>
    <div class="block" v-if="isFindingOpponent">
      Wait.. Finding opponent
    </div>
    <div class="block" v-if="game.isOver">
      <div v-if="isWinner">YOU ARE WINNER</div>
      <div v-else-if="!game.isTie && !isWinner">YOU LOSER</div>
      <div v-if="game.isTie">TIE GAME</div>
    </div>
    <div v-if="isPlaying">
      <div>GameId: {{game.id}}</div>
      <div style="display: flex;">
      <div class="block"><span v-if="LocalIsFirstPlayer">[ME]</span> {{game.player1.username}} Symbol: <b>{{game.player1.piece}}</b></div>
      <div class="block" style="margin-left: 16px;" ><p v-if="!LocalIsFirstPlayer">[ME]</p> {{game.player2.username}} Symbol: <b>{{game.player2.piece}}</b></div>
      </div>
      <div style="height: 24px;">
        <span v-if="IsMyTurn">MY TURN!</span>
      </div>

      <div class = "ui">
        <div class="row">
          <input type="text" id= "b1"
                 class="cell"
                 readonly
                 :value="Pieces[0][0]"
                 @click="tryUseTurn(0, 0)"
          >
          <input type="text" id= "b2"
                 class="cell"
                 readonly
                 :value="Pieces[1][0]"
                 @click="tryUseTurn(1, 0)"
          >
          <input type="text" id= "b3"
                 class="cell"
                 readonly
                 :value="Pieces[2][0]"
                 @click="tryUseTurn(2, 0)"
          >
        </div>
        <div class="row">
          <input type="text" id= "b4"
                 class="cell"
                 readonly
                 :value="Pieces[0][1]"
                 @click="tryUseTurn(0, 1)"
          >
          <input type="text" id= "b5"
                 class="cell"
                 readonly
                 :value="Pieces[1][1]"
                 @click="tryUseTurn(1, 1)"
          >
          <input type="text" id= "b6"
                 class="cell"
                 readonly
                 :value="Pieces[2][1]"
                 @click="tryUseTurn(2, 1)"
          >
        </div>
        <div class="row">
          <input type="text" id= "b7"
                 class="cell"
                 readonly
                 :value="Pieces[0][2]"
                 @click="tryUseTurn(0, 2)"
          >
          <input type="text" id= "b8"
                 class="cell"
                 readonly
                 :value="Pieces[1][2]"
                 @click="tryUseTurn(1, 2)"
          >
          <input type="text" id= "b9"
                 class="cell"
                 readonly
                 :value="Pieces[2][2]"
                 @click="tryUseTurn(2, 2)"
          >
        </div>
      </div>

    </div>
  </div>
</template>

<style>
@media (min-width: 1024px) {
  .about {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
  }
}
</style>

<script>

import store from "@/gameLogic/store.js";
import {HubConnection, HubConnectionBuilder, HubConnectionState} from '@microsoft/signalr';

const Player = {
  id: '',
  username: '',
  gameId: '',
  piece: '',
}

export default {
  data: () => {
    return {
      connection: HubConnection,
      isConnected: false,
      signalr: null,
      isFindingOpponent: false,
      isPlaying: false,

      isWinner: false,

      game: {
        isFirstPlayersTurn: false,
        whoseTurn: {
          username: ''
        },
        id: '',
        isOver: false,
        isTie: false,
        player1: Player,
        player2: Player,
        pieces: [['', '', ''], ['', '', ''], ['', '', '']]
      },
    }
  },
  computed: {
    IsMyTurn() {
      return this.game.whoseTurn.username === this.LocalPlayer.username;
    },
    Pieces() {
      return this.game.pieces;
    },
    LocalPlayer() {
      if (!this.isPlaying) return Player;
      if (this.game.player1.username === store.userName) return this.game.player1;
      return this.game.player2;
    },
    LocalIsFirstPlayer() {
      if (!this.isPlaying) return false;
      return (this.game.player1.username === store.userName);
    }
  },
  mounted() {
    if (store.userToken === null) {
      this.$router.push("/")
      return;
    }
    this.connection = new HubConnectionBuilder()
        .withUrl("http://localhost:7271/lobby", { headers: {
          "Authorization": store.userToken,
          "Content-Type": "application/json",
        }})
        .withAutomaticReconnect()
        .build();

    this.connection.on("joiningPlayer", () => {
      console.log("[joiningPlayer] wait opponent")
      this.isFindingOpponent = true;
    })
    this.connection.on("start", (game) => {
      console.log("[start] start game")
      this.isFindingOpponent = false;
      this.isPlaying = true;
      this.parseGame(game)
    })

    // смена хода
    this.connection.on("updateTurn", (game) => {
      console.log("[updateTurn]")
      console.log({...game})
      this.parseGame(game)
    })

    this.connection.on("winner", (winner) => {
      console.log("[winner] " + winner)
      this.isWinner = this.LocalPlayer.username === winner;
    })

    this.start()
  },
  beforeUnmount() {
    if (this.connection !== null) {
      this.connection.stop()
    }
    store.userToken = null;
    store.userName = null;
  },
  methods: {
    async start() {
      try {
        await this.connection.start();
        console.log("SignalR Connected.");
        this.isConnected = true;
      } catch (err) {
        console.log(err);
        setTimeout(this.start, 5000);
      }
    },
    parseGame(game) {
      this.game = {
        ...game,
        pieces: game.board.pieces.$values
      }
      console.log("game updated")
      console.log({...this.game})
    },
    async send(messageName) {
      try {
        await this.connection.invoke(messageName, store.userName);
      } catch (err) {
        console.error(err);
      }
    },
    tryUseTurn(x, y) {
      // if (!this.IsMyTurn) {
      //   console.log("not my turn!")
      //   return;
      // }
      this.sendTurn(x, y)
    },
    async sendTurn(x, y) {
      try {
        console.log(`sending turn ${x} ${y}`)
        await this.connection.invoke("placePiece", x, y);
      } catch (err) {
        console.error(err)
      }
    },

  }
}
</script>

<style scoped>
.ui {
  display: flex;
  flex-direction: column;
  align-items: center;
}
.row {
  display: flex;
}
.cell {
  border: none;
  width: 80px;
  height: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  text-align: center;
  cursor: pointer;
}
.cell:active {
  outline: none;
}
/* 3*3 Grid */
#b1{
  border-bottom: 1px solid gray;
  border-right: 1px solid gray;
}

#b2 {
  border-bottom: 1px solid gray;
  border-right: 1px solid gray;
  border-left: 1px solid gray;
}

#b3 {
  border-bottom: 1px solid gray;
  border-left: 1px solid gray;
}

#b4 {
  border-top: 1px solid gray;
  border-bottom: 1px solid gray;
  border-right: 1px solid gray;
}

#b5 {
  border: 1px solid gray;
}

#b6 {
  border-top: 1px solid gray;
  border-bottom: 1px solid gray;
  border-left: 1px solid gray;
}

#b7 {
  border-top: 1px solid gray;
  border-right: 1px solid gray;
}

#b8 {
  border-top: 1px solid gray;
  border-right: 1px solid gray;
  border-left: 1px solid gray;
}

#b9 {
  border-top: 1px solid gray;
  border-left: 1px solid gray;
}

.block {
  border: 1px solid #c0c0c0;
  border-radius: 12px;
  padding: 6px;
  margin: 6px;
}
</style>