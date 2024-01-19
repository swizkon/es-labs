---
# try also 'default' to start simple
# theme: the-unnamed
theme: slidev-theme-queenslab

# random image from a curated Unsplash collection by Anthony
# like them? see https://unsplash.com/collections/94734566/slidev
background: https://source.unsplash.com/collection/94734566/1920x1080
# apply any windi css classes to the current slide
class: 'text-center'
# https://sli.dev/custom/highlighters.html
highlighter: shiki
# show line numbers in code blocks
lineNumbers: false
# some information about the slides, markdown enabled
info: |
  ## Slidev Queenslab theme
  Presentation slides for developers.

  Learn more at [Sli.dev](https://sli.dev)
# persist drawings in exports and build
drawings:
  persist: false
# page transition
transition: fade
# use UnoCSS
css: unocss
---

# Exploring the unknown

## Event Sourcing

---
src: ./pages/event-sourcing.md
hide: false
transition: fade
---

---
src: ./pages/crud-sourcing.md
hide: false
transition: fade
---

---
layout: image-right
image: https://source.unsplash.com/collection/94734566/1920x1080
src: ./pages/event-streaming.md
hide: false
transition: fade
---

---
src: ./pages/event-sourcing-example.md
hide: false
transition: fade
---

---
src: ./pages/event-sourcing-example-2.md
hide: false
transition: fade
---

---
src: ./pages/event-storming.md
hide: false
transition: fade
---

---
layout: image-right
image: https://source.unsplash.com/collection/94734566/1920x1080
src: ./pages/projections.md
hide: false
transition: fade
---

---
layout: default
src: ./pages/projections-examples.md
hide: false
transition: fade
---



---
layout: image-right
image: https://source.unsplash.com/collection/94734566/1920x1080
src: ./pages/setting-the-stage.md
hide: false
transition: fade
---


---
transition: fade
---
# Demo-time

Hover on the bottom-left corner to see the navigation's controls panel, [learn more](https://sli.dev/guide/navigation.html)

<div class="relative">
<iframe style="min-height:400px;" width="100%" height="90%" src="http://localhost:3000/sensors/enterandexit">Demo frame</iframe>
</div>

---
layout: default
---

# Table of contents

```
<Toc minDepth="1" maxDepth="1"></Toc>
```

<Toc></Toc>

---
src: ./pages/demo-time.md
hide: false
layout: center
class: text-center
---


---
layout: center
class: text-center
---

# Thank you

for your time

[Code](https://github.com/swizkon/es-labs) · [Queenslab](https://www.queenslab.co/)
