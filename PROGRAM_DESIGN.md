# ざっくり設計

[Pegmatite - Chrome Extension](https://chrome.google.com/webstore/detail/pegmatite/jegkfbnfbfnohncpcfcimepibmhlkldo) で表示を変換すれば設計を見れます

※セキュリティなど不安があれば OSS として公開されてるので参考から見てみてください。現状安全性を把握した上で利用しています。

```uml
@startuml

class Character {
  Sensor sensor;
  string Name;
}

class Sensor {
  List<Vector3> Eyes;
  List<Vector3> Edges;
}

Character *-- Sensor

class Playable {
}

class Automonus {
  Sight sight;
  NavMeshAgent agent;
  ICharacterBehaviour behaviour;

  void CheckSight();
}

Character <|-- Playable
Character <|-- Automonus

class Sight {
  List<GameObject> ObjectsInSight;
  List<GameObject> FoundObjects;

  bool FeelSigns;
  bool IsFound;
}

Automonus *-- Sight

interface ICharacterHabit {
  void Behave();
}

Automonus *-- ICharacterHabit

class GoStraightHabit {}

class TrackingHabit {}

ICharacterHabit <|-- GoStraightHabit
ICharacterHabit <|-- TrackingHabit

@enduml
```

# 参考

* [GitHub の Markdown (GFM) でPlantUMLを表示するChrome拡張](https://dev.classmethod.jp/articles/chrome-extension-plantuml-in-github-markdown/)
