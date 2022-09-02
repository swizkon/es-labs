<script>
  import { onMount } from "svelte";
  import { Router, Link, Route } from "svelte-routing";
  import { fade, fly } from "svelte/transition";
  import { flip } from "svelte/animate";
  import { HubConnectionBuilder } from "@microsoft/signalr";
  

  import Toast from "./Toast.svelte";

  const baseUrl = "https://localhost:6001";

  let connection = new HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/testHub`)
    .withAutomaticReconnect()
    .build();

  connection.on("Broadcast", (data, b, c) => {
    nextUp = {
      title: data,
      description: b,
    };
  });

  connection.on("AddItem", (list, title, itemId, timestamp) => {
    const item = {
      title: title,
      description: list,
      itemId: itemId,
    };
    items = [item, ...items];

    window.pushToast(`Added ${title} (to ${itemId})`);
  });

  connection.on("RemoveItem", (list, itemId, reason, timestamp) => {
    items = items.filter((c) => c.itemId !== itemId);
    window.pushToast(`Removed ${list} - ${itemId}`);
  });

  const remove = (i) => {
    var item = items[i];
    removeItem(item.itemId);
  };

  async function start() {
    try {
      await connection.start();
      connection.send("Broadcast", "user", "message");
      console.log("SignalR Connected.", connection.state);
    } catch (err) {
      console.log(connection.state);
      console.log(err);
      setTimeout(() => start(), 5000);
    }
  }

  export let name;

  let listId = "";
  let itemTitle = "qux";

  let items = [];
  let nextUp;

  onMount(async () => {
    await start();
  });

  async function addItem() {
    const res = await fetch(`${baseUrl}/shoppingList/${listId}`, {
      method: "POST",
      body: '"' + itemTitle + '"',
      headers: { "Content-Type": "application/json" },
    });
  }

  async function removeItem(itemId) {
    const res = await fetch(
      `${baseUrl}/shoppingList/${listId}/${itemId}/reason`,
      {
        method: "DELETE",
      }
    );
  }
</script>

<main>

  <h2>{name}</h2>
  <input placeholder="Enter name of list" bind:value={listId} />
  <button type="button" on:click={addItem}> OK </button>
  {#if listId != ""}
    <h1 id="next-title">{listId}</h1>
    <h2>DESC</h2>
    <h3>Add items:</h3>

    <input bind:value={itemTitle} />
    <button type="button" on:click={addItem}> Add </button>

    <div>
      {#each items as c, i (c)}
        <div animate:flip in:fade out:fly={{ x: 10 }}>
          <hr/>
          <h3>
            {c.title}
            {c.description}
          </h3>
          <button on:click={() => remove(i)}>remove</button>
        </div>
      {/each}
    </div>
  {:else}
    <h1>Enter list name...</h1>
  {/if}

  <Toast />
</main>

<style>
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
  
  /*
  h1 {
    color: #ff3e00;
    text-transform: uppercase;
    font-size: 4em;
    font-weight: 100;
    text-shadow: 3px 3px 0em #000;
  }
  */

  @media (min-width: 640px) {
    main {
      max-width: none;
    }
  }
</style>
