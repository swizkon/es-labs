# Event Sourcing

### Story telling with actions and events

Mapping commands to different events based on some logic

```cs {none|1-3|1-12|1-20}
record SetSlideTitle {
  slide_id: uuid, new_name: string
}

class HandleSetSlideTitle(command) {
  var slide = get(command.slide_id)
  var diff = get_levenshtein_distance(slide.name, command.new_name)
  if(diff < typo_limit)
    emit(TitleTypoFixed)
  else
    emit(SlideRenamed)
}

record TitleTypoFixed {
  slide_id: uuid, corrected_title: string
}

record SlideRenamed {
  slide_id: uuid, new_title: string
}
```