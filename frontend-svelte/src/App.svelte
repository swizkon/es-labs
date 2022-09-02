<script>
  import { onMount } from "svelte";
	import { fade, fly } from 'svelte/transition';
	import { flip } from 'svelte/animate';
  import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
  
	import Toast from './Toast.svelte';

  const baseUrl = 'https://localhost:6001'

  let connection = new HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/testHub`)
    .withAutomaticReconnect()
    .build();

  connection.on("BroadcastMessage", data => {
    game = data.message;
    localGame.push(data.message);
    console.log(localGame);
  });
  
connection.on("Broadcast", (data, b, c) => {
    nextUp = {
      title: data,
      description: b,
      nextOccurrence: c
    }
  });

  connection.on("AddItem", (list, title, itemId, timestamp) => {
    nextUp = {
      title: title,
      description: list,
      nextOccurrence: timestamp,
      itemId: itemId
    }
    celebrations = [nextUp, ...celebrations];

    window.pushToast(`Added ${title} (${itemId})`)
  });
  
  connection.on("RemoveItem", (list, itemId, reason, timestamp) => {
    celebrations = celebrations.filter(c => c.itemId !== itemId);
    window.pushToast(`Removed ${list} - ${itemId}`)
  });

  connection.on("Broadcast", (list, itemId, reason, timestamp) => {
    celebrations = celebrations.filter(c => c.itemId !== itemId);
    window.pushToast(`Removed ${list} - ${itemId}`)
  });

  const remove = i => {
    var item = celebrations[i];
    removeItem(item.itemId);
	};
  
  async function start() {
    try {
        await connection.start();
        connection.send("Broadcast", "user", "message");
        console.log(connection.state);
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(connection.state);
        console.log(err);
        setTimeout(() => start(), 5000);
    }
  };

  let localGame = [];
  export let game;

  let listId = 'jonas'
	let itemTitle = 'qux'

  let celebrations = [];
  let nextUp;

  let gamePos = {
    'player1':  {x:0,y:0},
    'player2':  {x:0,y:0}
  };

	function handleMousemove(event) {
    const player = event.target.dataset.player;
    const {clientX, clientY} = event;    

		gamePos[player].x = clientX - event.target.offsetLeft;
		gamePos[player].y = clientY - event.target.offsetTop;
    
    connection.send("playerPosition", player, clientX, clientY);
	}

  onMount(async () => {
    await start();
  });

	async function addItem () {
		const res = await fetch(`${baseUrl}/shoppingList/${listId}`, {
			method: 'POST',
			body: "\"" + itemTitle + "\"",
      headers: {'Content-Type': 'application/json'}
		})
	}

  async function removeItem (itemId) {
    const res = await fetch(`${baseUrl}/shoppingList/${listId}/${itemId}/reason`, {
      method: 'DELETE'
    })
  }
</script>

<style>
  @import url('https://fonts.googleapis.com/css2?family=Bangers&family=Open+Sans&display=swap');

  main {
    text-align: center;
    padding: 1em;
    max-width: 250px;
    margin: 0 auto;
  }

  h1 {
    color: #369;
    text-transform: uppercase;
    font-size: 4em;
    font-weight: 100;
  }

  h1,h2 {
    font-family: 'Bangers';
  }

  @media (min-width: 640px) {
    main {
      max-width: none;
    }
  }
</style>

<main>

  <h2>Player One</h2>


  <div class="jumbotron" data-player="player1" style="width:640px;height:400px;" on:mousemove={handleMousemove}>
    {gamePos['player1'].x} : {gamePos['player1'].y}
  </div>
  
    <h2>Player Two</h2>
  <div class="jumbotron" data-player="player2" style="width:640px;height:400px;" on:mousemove={handleMousemove}>
    {gamePos['player2'].x} : {gamePos['player2'].y}
  </div>

  <div class="jumbotron">
    <h2>Allå, allå</h2>
    {#if nextUp != null}
      <h1 id="next-title">{nextUp.title}</h1>
      <h2>{nextUp.description}</h2>
      <h3 id="num-days">{nextUp.nextOccurrence}</h3>
    {:else}
      <p>Loading...</p>
      <h1 id="next-title">Loading...</h1>
    {/if}
  </div>

  <h3>Future:</h3>

  <div>
    <ul>
      {#each celebrations as c, i (c)}
        <li animate:flip in:fade out:fly={{x:100}}>
          {c.title} {c.description}
          {c.nextOccurrence}
          <button on:click="{() => remove(i)}">remove</button>
        </li>
      {/each}
    </ul>
  </div>

<input bind:value={listId} />
<input bind:value={itemTitle} />
<button type="button" on:click={addItem}>
	Add it.
</button>
  
  <Toast />
</main>
