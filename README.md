# 게임 프로토타입

Unity 6.3 LTS 기반의 **2D 탑다운 액션 어드벤처 게임 프로토타입**입니다.

본 프로젝트는 **PIXEL LABS**에서 개발 중인 게임의 핵심 시스템을 구현하고 검증하기 위해 제작되었습니다.

플레이어 이동과 전투, 적 AI, 공간 전환, NPC 대화 및 선택지 시스템을 포함하고 있습니다.

현재는 게임 시스템과 레벨 구조를 테스트하는 단계이며, 그래픽과 일부 오브젝트는 임시 리소스를 사용하고 있습니다.

## 개발환경

- **게임 엔진:** Unity 6.3 LTS (6000.3.27f1)
- **개발 언어:** C#
- **게임 유형:** 2D 탑다운 액션 어드벤처
- **물리 엔진:** Unity Physics 2D
- **입력 시스템:** Unity Input System
- **UI:** Unity UI (uGUI)
- **개발 도구:** Visual Studio Code, Codex
- **버전 관리:** Git, GitHub

프로젝트에서 사용하는 Unity 패키지 및 버전은 [Packages/manifest.json](Packages/manifest.json)에서 확인할 수 있습니다.

정확한 Unity 에디터 버전은 [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)를 참고하세요.

## 시작하기

### 1. 프로젝트 다운로드

GitHub에서 `Code → Download ZIP`을 선택하여 프로젝트를 다운로드하고 압축을 해제합니다.

Git을 사용하는 경우 다음 명령어로 저장소를 복제할 수 있습니다.

```bash
git clone https://github.com/chocho-pixel/GamePrototype.git
```

다운로드한 저장소에서 원하는 개발 버전의 브랜치를 선택하세요.

### 2. Unity 프로젝트 불러오기

1. Unity Hub를 실행합니다.
2. `Projects → Add → Add project from disk`를 선택합니다.
3. 다운로드한 프로젝트 폴더를 지정합니다.
4. Unity 6.3 LTS 환경에서 프로젝트를 실행합니다.

### 3. 게임 실행

프로젝트를 열었다면 다음 씬을 실행합니다.

[Assets/Scenes/Prototype.unity](Assets/Scenes/Prototype.unity)

1. Unity 에디터에서 `Assets/Scenes` 폴더를 엽니다.
2. `Prototype.unity` 씬을 더블클릭합니다.
3. 상단의 Play 버튼을 누릅니다.

프로젝트를 처음 불러올 때 Unity 에셋 임포트가 진행될 수 있습니다.

## 조작 방법

| 입력 | 동작 |
|---|---|
| E | 문 및 NPC 상호작용 |
| Space | 대사 진행 |
| Space (타이핑 중) | 현재 대사 즉시 완성 |
| 마우스 좌클릭 | NPC 대화 선택지 선택 |

플레이어 이동, 공격 및 대시 입력은 현재 프로젝트의 입력 설정을 따릅니다.

입력 관련 파일은 [Assets/InputSystem_Actions.inputactions](Assets/InputSystem_Actions.inputactions)에서 확인할 수 있습니다.

## 구체화된 시스템

### 1. 플레이어와 전투

플레이어는 2D 탑다운 환경에서 이동하고 적과 전투할 수 있습니다.

**플레이어 시스템**

- 캐릭터 이동
- 대시
- 기본 공격
- 체력 및 피해 판정
- 사망 처리
- 게임 재시작

**전투 시스템**

- 플레이어 공격 판정
- 적과 플레이어의 충돌 처리
- 체력 감소 및 사망 판정
- 체력 UI 표시

플레이어 체력은 화면에 하트 형태로 표시됩니다.

**관련 코드**

- [PlayerController.cs](Assets/Scripts/PlayerController.cs) — 플레이어 이동 및 조작
- [PlayerAttack.cs](Assets/Scripts/PlayerAttack.cs) — 플레이어 공격
- [Health.cs](Assets/Scripts/Health.cs) — 체력 및 피해 처리
- [CombatVisual.cs](Assets/Scripts/CombatVisual.cs) — 전투 시각 효과

### 2. 적 AI

몬스터는 플레이어를 탐지하고 추적하는 AI 시스템을 사용합니다.

**주요 기능**

- 플레이어 탐지 및 추적
- A* 기반 경로 탐색
- 장애물을 고려한 이동 경로 계산
- 벽 및 장애물 충돌
- 플레이어와의 전투

현재 테스트용 몬스터는 0번 홀에 배치되어 있습니다.

몬스터는 자신이 속한 공간에서만 활동하며, 플레이어가 다른 방으로 이동해도 따라오지 않습니다.

**관련 코드**

- [EnemyAI.cs](Assets/Scripts/EnemyAI.cs) — 몬스터 행동 및 추적
- [GridPathfinder.cs](Assets/Scripts/GridPathfinder.cs) — A* 경로 탐색
- [Health.cs](Assets/Scripts/Health.cs) — 체력 및 피해 처리

### 3. 맵 및 공간 시스템

현재 맵은 여러 개의 방과 복도로 구성되어 있습니다.

각 방은 독립적인 게임 공간으로 관리되며, 플레이어는 문을 통해 공간을 이동할 수 있습니다.

**주요 기능**

- 독립적인 방 관리
- 벽과 장애물 충돌
- E키 문 상호작용
- 페이드 효과를 이용한 공간 전환
- 방 전환 시 플레이어 위치 재설정
- 카메라 추적
- 공간별 몬스터 활동 제한

문 근처에 접근하면 상호작용 안내가 표시됩니다.

`[E] Open door`

E키를 누르면 화면 전환 효과와 함께 연결된 공간으로 이동합니다.

중앙 야외 영역은 비플레이 공간이며, 플레이어가 접근할 수 없도록 설정되어 있습니다.

현재 레벨 디자인은 구조 검증 단계로, 최종 설계에 맞춘 조정이 예정되어 있습니다.

**관련 코드**

- [PrototypeMap.cs](Assets/Scripts/PrototypeMap.cs) — 맵 구성 및 관리
- [RoomArea.cs](Assets/Scripts/RoomArea.cs) — 독립 공간 관리
- [RoomDoor.cs](Assets/Scripts/RoomDoor.cs) — 문 상호작용
- [RoomTransition.cs](Assets/Scripts/RoomTransition.cs) — 방 전환 처리
- [FollowCamera.cs](Assets/Scripts/FollowCamera.cs) — 카메라 추적
- [Prototype.unity](Assets/Scenes/Prototype.unity) — 게임 씬

### 4. NPC 대화 시스템

1번 로비에는 테스트용 NPC가 배치되어 있습니다.

NPC는 **분홍색 삼각형**으로 표현되며, 상단에는 `NPC` 이름표가 표시됩니다.

플레이어가 NPC 근처에서 E키를 누르면 대화를 시작할 수 있습니다.

**주요 기능**

- NPC 접근 및 상호작용
- 화자 이름 표시
- 타이핑 효과를 이용한 대사 출력
- 스페이스바를 이용한 대사 진행
- 마우스 클릭 기반 선택지
- 대화 중 플레이어 조작 제한
- 대화 종료 후 조작 복구
- 반복 대화

#### 대사 출력

대사는 한 번에 표시되지 않고 글자가 순차적으로 나타나는 타이핑 효과를 사용합니다.

타이핑 중 스페이스바를 누르면 남은 문장이 즉시 표시됩니다.

출력이 완료된 상태에서 다시 스페이스바를 누르면 다음 대사로 진행합니다.

#### 선택지

특정 대사의 출력이 완료되면 선택지가 자동으로 나타납니다.

선택지는 세로 방향으로 정렬되며, 플레이어는 마우스 클릭으로 원하는 항목을 선택할 수 있습니다.

선택이 완료되면 다음 대사가 진행됩니다.

현재는 대화 시스템 검증을 위한 테스트용 대사와 선택지가 적용되어 있습니다.

**관련 파일**

- [Assets/Scripts](Assets/Scripts) — NPC 상호작용 및 대화 시스템 코드
- [Assets/Scenes/Prototype.unity](Assets/Scenes/Prototype.unity) — NPC가 포함된 게임 씬

### 5. 화면과 UI

현재 프로토타입에는 게임플레이에 필요한 기본 UI가 구현되어 있습니다.

**구현된 UI**

- 플레이어 체력 표시
- 게임 오버 및 재시작
- 문 상호작용 안내
- NPC 상호작용 안내
- NPC 대화창
- 화자 이름 표시
- 대화 선택지

대화 UI는 게임플레이와 분리되어 관리되며, 대화가 종료되면 일반적인 조작 상태로 복귀합니다.

**관련 코드**

- [GameHUD.cs](Assets/Scripts/GameHUD.cs) — 게임 HUD
- [GameOverUIFactory.cs](Assets/Scripts/GameOverUIFactory.cs) — 게임 오버 UI
- [Assets/Scripts](Assets/Scripts) — 기타 UI 및 대화 관련 코드

## 프로젝트 구성

```text
GamePrototype/
│
├── Assets/
│   ├── Editor/
│   ├── Scenes/
│   ├── Scripts/
│   └── Settings/
│
├── Packages/
│
├── ProjectSettings/
│
├── AGENTS.md
├── PLAN.md
├── PROJECT.md
├── STATUS.md
│
└── README.md
```

**주요 디렉터리**

| 경로 | 설명 |
|---|---|
| [Assets](Assets) | 게임 리소스 및 주요 개발 파일 |
| [Assets/Scenes](Assets/Scenes) | Unity 씬 |
| [Assets/Scripts](Assets/Scripts) | 게임플레이 C# 코드 |
| [Assets/Editor](Assets/Editor) | 에디터 도구 및 씬 생성 코드 |
| [Assets/Settings](Assets/Settings) | 에셋 및 프로젝트 설정 |
| [Packages](Packages) | Unity 패키지 설정 |
| [ProjectSettings](ProjectSettings) | 엔진 및 프로젝트 설정 |

## 검증상태

현재 Unity 에디터에서 다음 기능의 정상 작동을 확인했습니다.

- 플레이어 이동 및 전투
- 몬스터 생성 및 표시
- 몬스터 추적 및 경로 탐색
- 맵 및 방 전환
- 문 상호작용
- NPC 상호작용
- 대사 타이핑 효과
- 마우스 선택지
- 대화 종료 및 조작 복귀

현재 프로토타입은 기능 검증 단계입니다.

레벨 구조, 그래픽 및 세부 시스템은 실제 게임 개발 과정에서 변경될 수 있습니다.

## 관련 문서와 작업 규칙

프로젝트의 개발 정보 및 작업 지침은 다음 문서를 참고하세요.

- [AGENTS.md](AGENTS.md) — 개발 작업 지침
- [PROJECT.md](PROJECT.md) — 프로젝트 정보 및 주요 설계
- [PLAN.md](PLAN.md) — 개발 계획
- [STATUS.md](STATUS.md) — 구현 현황 및 검증 상태

기존 시스템을 수정하거나 새로운 기능을 추가할 경우, 다른 시스템과의 호환성을 확인해야 합니다.

---

**PIXEL LABS | GamePrototype**

Unity 2D Action Adventure Prototype  
2026
