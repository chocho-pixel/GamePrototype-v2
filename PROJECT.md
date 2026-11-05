# GamePrototype

Unity 6.3 LTS 기반 Built-in 2D 탑다운 액션 프로토타입입니다. 여러 독립 공간에서 이동, 회피, 근접 전투와 장애물 회피 추적을 시험합니다. Windows와 macOS에서 같은 프로젝트를 열 수 있습니다.

- `Assets/Scripts`: 플레이어, 공격, 체력, 적 AI, 격자 경로 탐색, 카메라, UI와 임시 시각화
- `Assets/Editor`: 테스트 씬 자동 생성 도구
- `Assets/Scenes/Prototype.unity`: 생성된 테스트 씬
- `Assets/Settings`: 교체 가능한 임시 그래픽과 바닥 타일
- `Packages`, `ProjectSettings`: Unity 설정

## 실행

1. Unity Hub에서 이 폴더를 추가하고 Unity 6000.3.27f1로 엽니다. Package Manager가 Input System과 2D 패키지를 복원하도록 기다립니다.
2. `Assets/Scenes/Prototype.unity`를 열고 Play를 누릅니다. 씬이 없다면 `Tools > Game Prototype > Create Test Scene`을 실행합니다. 이 명령은 Prototype 씬 및 임시 그래픽을 재생성하므로 해당 파일에 직접 한 변경을 덮어씁니다.
3. WASD 이동, 마우스 왼쪽 클릭 공격, Shift 대시, 문 가까이에서 E로 이동, R 재시작. Editor 또는 개발 빌드에서 F3는 상태 표시를 전환합니다. Scene 뷰 Gizmos로 경로와 적 공격 영역을 볼 수 있습니다.

1번 로비의 분홍 삼각형 NPC 가까이에서 E로 대화를 시작합니다. Space는 입력 중인 문장을 완성하거나 완성된 문장을 넘깁니다. 선택지는 마우스로 클릭합니다. 테스트 대화는 반복할 수 있으며 문과 NPC가 모두 가까우면 더 가까운 대상과 상호작용합니다.

새 맵은 Prototype 씬을 실행할 때 기존 캐릭터와 HUD를 재사용해 생성됩니다. Scene 뷰에 미리 저장된 맵은 아니며, 테스트 씬 생성기를 다시 실행해도 Play 시 같은 맵이 적용됩니다. 한 타일은 기존 바닥 타일과 같은 1 Unity 단위입니다. 0번 홀에서 시작하며 기존 적도 홀에만 존재합니다. 방과 복도는 하나씩 활성화되고 연결된 문은 E로 전환합니다. 승강기, 비상구, 화장실, 계단의 불명확한 출입 연결은 보류했습니다.

기존 상자 3개도 홀에 배치되고 해당 방에서만 활성화됩니다. 카메라는 방의 경계에 묶이지 않고 플레이어를 부드럽게 따라가며, 실내 바닥 밖과 비플레이 야외 영역은 검은 배경으로 표시됩니다.

주요 전투 수치는 각 오브젝트의 Inspector에서 조정합니다. 플레이어 공격은 마우스를 향한 부채꼴이고 적 공격은 한 칸 크기의 4방향 영역입니다. 정식 아트, 애니메이션, 저장, 자동 적 부활은 범위 밖입니다.

## 적 진단 및 테스트

Prototype 씬을 Play 중 `Tools > Game Prototype > Enemy Diagnostics`를 열고 `Refresh`를 누르면 적과 부모의 활성 상태, 위치, 렌더러, 카메라 및 활성 방을 확인할 수 있습니다. `Copy report`로 결과를 복사할 수 있습니다. 시야 및 렌더러 정보만으로는 실제 픽셀 표시를 보장할 수 없으므로 Game 뷰도 확인하세요.

`Window > General > Test Runner`의 PlayMode 탭에서 `PrototypeEnemyPlayModeTests`를 실행하면 실제 Prototype 씬의 적, 렌더링 및 추적을 검사합니다. 그래픽 장치가 있으면 카메라의 오프스크린 렌더 결과를 `TestResults/PrototypeEnemy.png`에 저장합니다. 렌더링 장치가 없으면 픽셀 검사는 건너뜁니다. 테스트는 현재 씬을 로드하고 교체하므로 저장하지 않은 작업은 먼저 저장하세요.

`DialoguePlayModeTests`는 로비 NPC 배치, 대화 진행, 세 선택지, 입력 잠금 복구와 기존 적을 검사합니다. 배치 테스트는 포커스가 없는 환경을 위해 키 입력이 호출하는 처리 함수와 Unity 포인터 클릭 이벤트를 직접 실행하므로, 실제 키보드와 마우스 동작은 Game 뷰에서 별도로 확인하세요. 대사와 선택지는 `Assets/Resources/TestNpcDialogue.asset`에서 편집합니다. 한국어 표시에는 OS의 한국어 지원 글꼴이 필요합니다.

## 게임 오버 UI
플레이어 HP가 0이면 전체 화면 오버레이와 GAME OVER, RESTART가 표시됩니다. 버튼과 R키는 같은 씬 재시작을 사용합니다. 기존 Prototype 씬에서는 별도 설정 없이 플레이 시작 시 UI가 만들어집니다. Inspector에서 디자인을 편집할 수 있는 UI를 씬에 저장하려면 Prototype 씬에서 `Tools > Game Prototype > Add Game Over UI to Current Scene`을 실행하고 씬을 저장하세요. 새 테스트 씬 생성기에는 이 UI가 포함됩니다. HUD의 Game Over Panel과 Restart Button 참조를 교체하고 이미지, 폰트, 배치를 수정할 수 있습니다.
