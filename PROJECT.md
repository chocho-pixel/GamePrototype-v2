# GamePrototype

Unity 6.3 LTS 기반 Built-in 2D 탑다운 액션 프로토타입입니다. 목표는 고정된 방에서 이동, 회피, 근접 전투와 장애물 회피 추적을 시험하는 것입니다. Windows와 macOS에서 같은 프로젝트를 열 수 있습니다.

- `Assets/Scripts`: 플레이어, 공격, 체력, 적 AI, 격자 경로 탐색, 카메라, UI와 임시 시각화
- `Assets/Editor`: 테스트 씬 자동 생성 도구
- `Assets/Scenes/Prototype.unity`: 생성된 테스트 씬
- `Assets/Settings`: 교체 가능한 임시 그래픽과 바닥 타일
- `Packages`, `ProjectSettings`: Unity 설정

## 실행

1. Unity Hub에서 이 폴더를 추가하고 Unity 6000.3.27f1로 엽니다. Package Manager가 Input System과 2D 패키지를 복원하도록 기다립니다.
2. `Assets/Scenes/Prototype.unity`를 열고 Play를 누릅니다. 씬이 없다면 `Tools > Game Prototype > Create Test Scene`을 실행합니다. 이 명령은 Prototype 씬 및 임시 그래픽을 재생성하므로 해당 파일에 직접 한 변경을 덮어씁니다.
3. WASD 이동, 마우스 왼쪽 클릭 공격, Shift 대시, R 재시작. Editor 또는 개발 빌드에서 F3는 상태 표시를 전환합니다. Scene 뷰 Gizmos로 경로와 적 공격 영역을 볼 수 있습니다.

주요 전투 수치는 각 오브젝트의 Inspector에서 조정합니다. 플레이어 공격은 마우스를 향한 부채꼴이고 적 공격은 한 칸 크기의 4방향 영역입니다. 적과 장애물 배치는 고정입니다. 정식 아트, 애니메이션, 저장, 자동 적 부활은 범위 밖입니다.

## 게임 오버 UI
플레이어 HP가 0이면 전체 화면 오버레이와 GAME OVER, RESTART가 표시됩니다. 버튼과 R키는 같은 씬 재시작을 사용합니다. 기존 Prototype 씬에서는 별도 설정 없이 플레이 시작 시 UI가 만들어집니다. Inspector에서 디자인을 편집할 수 있는 UI를 씬에 저장하려면 Prototype 씬에서 `Tools > Game Prototype > Add Game Over UI to Current Scene`을 실행하고 씬을 저장하세요. 새 테스트 씬 생성기에는 이 UI가 포함됩니다. HUD의 Game Over Panel과 Restart Button 참조를 교체하고 이미지, 폰트, 배치를 수정할 수 있습니다.
