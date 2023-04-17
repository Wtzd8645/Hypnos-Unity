# ECS設計

- Component:
    最小的資料層

- Entity:
    只包含所有用到的Component
    使用觀察者模式通知Node增減Component
    由於System只更新Component，與Entity不耦合，因此增減Component由System發起，並由Component通知Entity?
    Component加(減)進Entity時，自動增加(減少)Node

- Node:
    作為Entity與System間的膠水
    存有該System使用到的Componet
    不與Entity耦合，需觀察Entity是否還持有該Node需要的Compoent
    分為必要與不必要的Component

    Node同時亦為System內以LinkedList執行的Node，須為class
    並為Circular結構，可快速搜尋到last

- NodeManager:
  - ComonentsInNodesSet: 以Assembly搜尋Node，與其中包含的Component type (Finished)
  - Com -> node map
  - Entity -> hash of coms map
  - node -> hash of coms map
  - com -> hash of coms map

    Node object pool

- System:
    Node以LinkedList方式遞迴更新
    觀察Node是否可被更新

- SystemManager:
    有序更新所有System
    (Update、FixedUpdate怎麼做到完全有序?)

- View:
    觀察Component，更新自身
    (Ex. Unity)

## 問題

若資料須由View層來取怎麼辦
(Ex. Animator相關資訊)
